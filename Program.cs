using Microsoft.Extensions.Configuration;
using System.Text.Json;
using TransferJurnal;

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
        Console.WriteLine("Error: Connection string not found in appsettings.json");
        return;
    }

    // Load SQL commands configuration
    var commandsJson = await File.ReadAllTextAsync("commands.json");
    var commandsConfig = JsonSerializer.Deserialize<CommandsConfig>(commandsJson, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (commandsConfig == null || !commandsConfig.Commands.Any())
    {
        Console.WriteLine("Error: No commands found in commands.json");
        return;
    }

    // Create SQL command executor
    var executor = new SqlCommandExecutor(connectionString, commandsConfig);

    Console.WriteLine("Application initialized successfully!");
    Console.WriteLine($"Connection string loaded from appsettings.json");
    Console.WriteLine($"Loaded {commandsConfig.Commands.Count} SQL commands from commands.json\n");

    // List available commands
    executor.ListAvailableCommands();

    Console.WriteLine("\n=====================================");
    Console.WriteLine("Application is ready to execute SQL commands.");
    Console.WriteLine("Commands can be modified in commands.json without recompiling.");
    Console.WriteLine("Connection string can be updated in appsettings.json.");
}
catch (Exception ex)
{
    Console.WriteLine($"\nError: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
