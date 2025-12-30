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

        if (parameters != null)
        {
            foreach (var param in config.Parameters)
            {
                if (parameters.ContainsKey(param.Name))
                {
                    command.Parameters.AddWithValue(param.Name, parameters[param.Name]);
                }
            }
        }

        return command;
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
