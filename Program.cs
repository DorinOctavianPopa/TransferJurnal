using Microsoft.Extensions.Configuration;
using System.Text.Json;
using TransferJurnal;
using Microsoft.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Net.Sockets;

Console.WriteLine("TransferJurnal - SQL Command Executor");
Console.WriteLine("=====================================\n");

try
{
    // Load configuration
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        WriteError("Error: Connection string not found in appsettings.json");
        return;
    }

    WriteInfo("Testing SQL Server connection...\n");
    
    // Test SQL Server connection with diagnostics
    var connectionSuccess = await TestSqlConnectionAsync(connectionString);
    
    if (!connectionSuccess)
    {
        WriteError("\n❌ Cannot proceed without a valid database connection.");
        WriteWarning("Please resolve the connection issues and try again.");
        return;
    }

    WriteSuccess("✓ SQL Server connection successful!\n");

    // Load SQL commands configuration
    var commandsJson = await File.ReadAllTextAsync("commands.json");
    var commandsConfig = JsonSerializer.Deserialize<CommandsConfig>(commandsJson, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (commandsConfig == null || !commandsConfig.Commands.Any())
    {
        WriteError("Error: No commands found in commands.json");
        return;
    }

    // Create SQL command executor
    var executor = new SqlCommandExecutor(connectionString, commandsConfig);
        
    //WriteSuccess("Application initialized successfully!");
    //Console.WriteLine($"Connection string loaded from appsettings.json");
    //Console.WriteLine($"Loaded {commandsConfig.Commands.Count} SQL commands from commands.json\n");

    // List available commands
    //executor.ListAvailableCommands();

    // Check if execution plan exists
    var executionPlanPath = "execution-plan.json";
    if (File.Exists(executionPlanPath))
    {
        WriteInfo($"\n📋 Found execution plan: {executionPlanPath}");
        Console.Write("Do you want to execute the plan? (Y/N): ");
        var response = Console.ReadLine()?.Trim().ToUpper();

        if (response == "Y" || response == "YES")
        {
            // Load and execute the plan
            var planJson = await File.ReadAllTextAsync(executionPlanPath);
            var planConfig = JsonSerializer.Deserialize<ExecutionPlanConfig>(planJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (planConfig != null)
            {
                var engine = new ExecutionEngine(executor, planConfig);
                var result = await engine.ExecuteAsync();

                // Optionally save execution result
                var resultPath = $"execution-result-{DateTime.Now:yyyyMMdd-HHmmss}.json";
                var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(resultPath, resultJson);
                WriteSuccess($"\n✓ Execution result saved to: {resultPath}");
            }
        }
        else
        {
            WriteInfo("\nExecution plan skipped.");
        }
    }
    else
    {
        WriteWarning($"\n⚠ No execution plan found at: {executionPlanPath}");
        WriteInfo("You can create an execution-plan.json file to automate command execution.");
    }

    Console.WriteLine("\n=====================================");
    WriteInfo("Application is ready to execute SQL commands.");
    Console.WriteLine("Commands can be modified in commands.json without recompiling.");
    Console.WriteLine("Connection string can be updated in appsettings.json.");
    Console.WriteLine("Create execution-plan.json to automate command execution.");
}
catch (Exception ex)
{
    WriteError($"\n❌ Unexpected Error: {ex.Message}");
    WriteError($"Stack trace: {ex.StackTrace}");
}

static async Task<bool> TestSqlConnectionAsync(string connectionString)
{
    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        
        // Extract connection details
        var server = builder.DataSource;
        var database = builder.InitialCatalog;
        var isIntegratedSecurity = builder.IntegratedSecurity;
        var userId = builder.UserID;

        WriteHeader("Connection Details:");
        Console.WriteLine($"  Server: {server}");
        Console.WriteLine($"  Database: {database}");
        Console.WriteLine($"  Authentication: {(isIntegratedSecurity ? "Windows Authentication" : $"SQL Authentication (User: {userId})")}");
        Console.WriteLine();

        // Step 1: Test network connectivity
        WriteInfo("🔍 Step 1: Testing network connectivity...");
        var networkOk = await TestNetworkConnectivityAsync(server);
        
        if (!networkOk)
        {
            return false;
        }

        // Step 2: Attempt database connection
        WriteInfo("🔍 Step 2: Attempting database connection...");
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        WriteSuccess($"  ✓ Connected to SQL Server version: {connection.ServerVersion}");
        WriteSuccess($"  ✓ Database '{database}' is accessible");
        
        return true;
    }
    catch (SqlException sqlEx)
    {
        WriteError($"  ❌ SQL Connection Failed: {sqlEx.Message}");
        Console.WriteLine();
        ProvideSqlDiagnosticRecommendations(sqlEx, connectionString);
        return false;
    }
    catch (Exception ex)
    {
        WriteError($"  ❌ Connection Test Failed: {ex.Message}");
        Console.WriteLine();
        ProvideGeneralRecommendations();
        return false;
    }
}

static async Task<bool> TestNetworkConnectivityAsync(string server)
{
    try
    {
        // Parse server and port
        var parts = server.Split(',');
        var serverName = parts[0].Trim();
        var port = parts.Length > 1 ? int.Parse(parts[1]) : 1433;

        // Test 1: Ping server (if not localhost)
        if (!serverName.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
            !serverName.Equals("127.0.0.1") &&
            !serverName.Equals("."))
        {
            Console.WriteLine($"  Testing ping to {serverName}...");
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(serverName, 3000);
            
            if (reply.Status == IPStatus.Success)
            {
                WriteSuccess($"  ✓ Server is reachable (ping: {reply.RoundtripTime}ms)");
            }
            else
            {
                WriteWarning($"  ⚠ Ping failed: {reply.Status}");
                WriteWarning($"    (Server might still be accessible if ICMP is blocked)");
            }
        }
        else
        {
            WriteSuccess($"  ✓ Target is localhost");
        }

        // Test 2: Check if SQL Server port is open
        Console.WriteLine($"  Testing TCP connection to {serverName}:{port}...");
        using var tcpClient = new TcpClient();
        var connectTask = tcpClient.ConnectAsync(serverName, port);
        var timeoutTask = Task.Delay(5000);
        
        var completedTask = await Task.WhenAny(connectTask, timeoutTask);
        
        if (completedTask == connectTask && tcpClient.Connected)
        {
            WriteSuccess($"  ✓ Port {port} is open and accepting connections");
            return true;
        }
        else
        {
            WriteError($"  ❌ Cannot connect to port {port}");
            Console.WriteLine();
            ProvideNetworkDiagnosticRecommendations(serverName, port);
            return false;
        }
    }
    catch (Exception ex)
    {
        WriteError($"  ❌ Network test failed: {ex.Message}");
        Console.WriteLine();
        ProvideNetworkDiagnosticRecommendations(server, 1433);
        return false;
    }
}

static void ProvideSqlDiagnosticRecommendations(SqlException sqlEx, string connectionString)
{
    WriteHeader("📋 DIAGNOSTIC RECOMMENDATIONS:");
    WriteHeader("================================");
    
    switch (sqlEx.Number)
    {
        case 53:
        case -1:
        case 258:
            WriteWarning("❗ Cannot connect to SQL Server instance");
            Console.WriteLine();
            WriteSubHeader("Possible causes:");
            Console.WriteLine("  1. SQL Server service is not running");
            Console.WriteLine("  2. SQL Server is not configured to accept remote connections");
            Console.WriteLine("  3. Firewall is blocking the connection");
            Console.WriteLine("  4. Incorrect server name or instance");
            Console.WriteLine();
            WriteSubHeader("Solutions:");
            WriteSolution("  ✓ Verify SQL Server service is running:");
            Console.WriteLine("    - Open 'services.msc' and check 'SQL Server (MSSQLSERVER)' status");
            WriteSolution("  ✓ Enable TCP/IP protocol:");
            Console.WriteLine("    - Open SQL Server Configuration Manager");
            Console.WriteLine("    - Navigate to: SQL Server Network Configuration > Protocols");
            Console.WriteLine("    - Enable 'TCP/IP' protocol and restart SQL Server");
            WriteSolution("  ✓ Check Windows Firewall:");
            Console.WriteLine("    - Allow port 1433 through Windows Firewall");
            Console.WriteLine("    - Or run: netsh advfirewall firewall add rule name=\"SQL Server\" dir=in action=allow protocol=TCP localport=1433");
            WriteSolution("  ✓ Verify SQL Server Browser service (for named instances):");
            Console.WriteLine("    - Start 'SQL Server Browser' service in services.msc");
            break;

        case 18456:
            WriteWarning("❗ Authentication Failed");
            Console.WriteLine();
            WriteSubHeader("Possible causes:");
            Console.WriteLine("  1. Incorrect username or password");
            Console.WriteLine("  2. SQL Server authentication is disabled");
            Console.WriteLine("  3. Login does not have permission");
            Console.WriteLine();
            WriteSubHeader("Solutions:");
            WriteSolution("  ✓ Verify credentials in appsettings.json");
            WriteSolution("  ✓ Enable SQL Server authentication:");
            Console.WriteLine("    - In SSMS, right-click server > Properties > Security");
            Console.WriteLine("    - Select 'SQL Server and Windows Authentication mode'");
            Console.WriteLine("    - Restart SQL Server service");
            WriteSolution("  ✓ Check if login exists and has proper permissions:");
            WriteCode($"    - CREATE LOGIN [Admin] WITH PASSWORD = 'YourPassword';");
            WriteCode($"    - USE [ESRP]; CREATE USER [Admin] FOR LOGIN [Admin];");
            WriteCode($"    - ALTER ROLE db_owner ADD MEMBER [Admin];");
            break;

        case 4060:
            WriteWarning("❗ Database Does Not Exist");
            Console.WriteLine();
            var builder = new SqlConnectionStringBuilder(connectionString);
            Console.WriteLine($"The database '{builder.InitialCatalog}' was not found on the server.");
            Console.WriteLine();
            WriteSubHeader("Solutions:");
            WriteSolution($"  ✓ Create the database:");
            WriteCode($"    - CREATE DATABASE [{builder.InitialCatalog}];");
            WriteSolution("  ✓ Or update the database name in appsettings.json");
            WriteSolution("  ✓ Verify available databases by connecting without specifying database:");
            WriteCode("    - SELECT name FROM sys.databases;");
            break;

        case 233:
            WriteWarning("❗ Connection Initialization Failed");
            Console.WriteLine();
            WriteSubHeader("Possible causes:");
            Console.WriteLine("  1. TLS/SSL protocol mismatch");
            Console.WriteLine("  2. Encryption settings incompatible");
            Console.WriteLine();
            WriteSubHeader("Solutions:");
            WriteSolution("  ✓ Add 'TrustServerCertificate=True' to connection string (already present)");
            WriteSolution("  ✓ Or add 'Encrypt=False' for local development");
            WriteSolution("  ✓ Ensure SQL Server supports TLS 1.2 or higher");
            break;

        default:
            WriteWarning($"❗ SQL Error {sqlEx.Number}: {sqlEx.Message}");
            Console.WriteLine();
            WriteSubHeader("General troubleshooting steps:");
            WriteSolution("  ✓ Check SQL Server error logs");
            WriteSolution("  ✓ Verify connection string format in appsettings.json");
            WriteSolution("  ✓ Test connection with SQL Server Management Studio (SSMS)");
            WriteSolution($"  ✓ Search for SQL Error {sqlEx.Number} for specific guidance");
            break;
    }
    
    Console.WriteLine();
    WriteInfo("📝 Current Connection String (check appsettings.json):");
    var safeConnectionString = MaskPassword(connectionString);
    Console.WriteLine($"   {safeConnectionString}");
}

static void ProvideNetworkDiagnosticRecommendations(string server, int port)
{
    WriteHeader("📋 NETWORK DIAGNOSTIC RECOMMENDATIONS:");
    WriteHeader("=======================================");
    Console.WriteLine();
    Console.WriteLine("The SQL Server port is not accessible. Possible causes:");
    Console.WriteLine();
    Console.WriteLine("  1. SQL Server service is not running");
    WriteSolution("     Solution: Open 'services.msc' and start 'SQL Server (MSSQLSERVER)'");
    Console.WriteLine();
    Console.WriteLine("  2. TCP/IP protocol is disabled");
    WriteSolution("     Solution: Enable TCP/IP in SQL Server Configuration Manager");
    Console.WriteLine();
    Console.WriteLine("  3. SQL Server is not listening on the default port");
    WriteSolution($"     Solution: Verify SQL Server port configuration (expected: {port})");
    Console.WriteLine();
    Console.WriteLine("  4. Firewall is blocking the connection");
    WriteSolution($"     Solution: Allow TCP port {port} through Windows Firewall");
    Console.WriteLine();
    Console.WriteLine("  5. Incorrect server name");
    Console.WriteLine($"     Current: {server}");
    WriteSolution("     Try: localhost, 127.0.0.1, .\\SQLEXPRESS, or your machine name");
    Console.WriteLine();
    WriteSubHeader("Quick verification commands:");
    WriteCode($"  netstat -an | findstr :{port}  (Check if port is listening)");
    WriteCode($"  telnet {server} {port}         (Test port connectivity)");
}

static void ProvideGeneralRecommendations()
{
    WriteHeader("📋 GENERAL RECOMMENDATIONS:");
    WriteHeader("============================");
    Console.WriteLine();
    WriteSolution("  ✓ Verify SQL Server is installed and running");
    WriteSolution("  ✓ Check connection string in appsettings.json");
    WriteSolution("  ✓ Ensure .NET application has network access to SQL Server");
    WriteSolution("  ✓ Try connecting with SQL Server Management Studio first");
    WriteSolution("  ✓ Check Windows Event Viewer for SQL Server errors");
}

static string MaskPassword(string connectionString)
{
    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = "********";
        }
        return builder.ConnectionString;
    }
    catch
    {
        return connectionString;
    }
}

// Color console helper methods
static void WriteSuccess(string message)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void WriteError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void WriteWarning(string message)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void WriteInfo(string message)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void WriteHeader(string message)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void WriteSubHeader(string message)
{
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void WriteSolution(string message)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void WriteCode(string message)
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine(message);
    Console.ResetColor();
}
