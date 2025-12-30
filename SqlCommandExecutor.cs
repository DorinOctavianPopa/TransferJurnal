using Microsoft.Data.SqlClient;
using System.Data;

namespace TransferJurnal;

public class SqlCommandExecutor
{
    private readonly string _connectionString;
    private readonly CommandsConfig _commandsConfig;

    public SqlCommandExecutor(string connectionString, CommandsConfig commandsConfig)
    {
        _connectionString = connectionString;
        _commandsConfig = commandsConfig;
    }

    public async Task<DataTable?> ExecuteQueryAsync(string commandName, Dictionary<string, object>? parameters = null)
    {
        var commandConfig = GetCommandConfig(commandName);
        if (commandConfig == null)
        {
            throw new InvalidOperationException($"Command '{commandName}' not found in configuration.");
        }

        using var connection = new SqlConnection(_connectionString);
        using var command = CreateCommand(connection, commandConfig, parameters);

        await connection.OpenAsync();

        if (commandConfig.Type.ToLower() == "query")
        {
            using var adapter = new SqlDataAdapter((SqlCommand)command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }
        else
        {
            await command.ExecuteNonQueryAsync();
            return null;
        }
    }

    public async Task<int> ExecuteNonQueryAsync(string commandName, Dictionary<string, object>? parameters = null)
    {
        var commandConfig = GetCommandConfig(commandName);
        if (commandConfig == null)
        {
            throw new InvalidOperationException($"Command '{commandName}' not found in configuration.");
        }

        using var connection = new SqlConnection(_connectionString);
        using var command = CreateCommand(connection, commandConfig, parameters);

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    private SqlCommandConfig? GetCommandConfig(string commandName)
    {
        return _commandsConfig.Commands.FirstOrDefault(c => c.Name == commandName);
    }

    private SqlCommand CreateCommand(SqlConnection connection, SqlCommandConfig config, Dictionary<string, object>? parameters)
    {
        var command = new SqlCommand(config.Sql, connection);

        if (config.Type.ToLower() == "storedprocedure")
        {
            command.CommandType = CommandType.StoredProcedure;
        }
        else
        {
            command.CommandType = CommandType.Text;
        }

        if (config.Parameters.Any())
        {
            foreach (var paramConfig in config.Parameters)
            {
                if (parameters == null || !parameters.ContainsKey(paramConfig.Name))
                {
                    throw new ArgumentException($"Required parameter '{paramConfig.Name}' not provided for command '{config.Name}'.");
                }

                var parameter = command.Parameters.Add(paramConfig.Name, GetSqlDbType(paramConfig.Type));
                var paramValue = parameters[paramConfig.Name];
                parameter.Value = paramValue ?? DBNull.Value;
            }
        }

        return command;
    }

    private SqlDbType GetSqlDbType(string type)
    {
        return type.ToLower() switch
        {
            "int" => SqlDbType.Int,
            "string" => SqlDbType.NVarChar,
            "datetime" => SqlDbType.DateTime,
            "decimal" => SqlDbType.Decimal,
            "bit" => SqlDbType.Bit,
            "bigint" => SqlDbType.BigInt,
            "uniqueidentifier" => SqlDbType.UniqueIdentifier,
            _ => throw new ArgumentException($"Unsupported parameter type: {type}. Supported types are: Int, String, DateTime, Decimal, Bit, BigInt, UniqueIdentifier.")
        };
    }

    public void ListAvailableCommands()
    {
        Console.WriteLine("\nAvailable SQL Commands:");
        Console.WriteLine("=======================");
        foreach (var cmd in _commandsConfig.Commands)
        {
            Console.WriteLine($"- {cmd.Name} ({cmd.Type})");
            if (cmd.Parameters.Any())
            {
                Console.WriteLine($"  Parameters: {string.Join(", ", cmd.Parameters.Select(p => $"{p.Name}:{p.Type}"))}");
            }
        }
    }
}
