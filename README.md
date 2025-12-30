# TransferJurnal

A flexible .NET console application for executing SQL commands against SQL Server with externalized configuration.

## Features

- **Externalized Connection String**: SQL Server connection string stored in `appsettings.json`
- **Configurable SQL Commands**: All SQL commands (queries, updates, stored procedures) stored in `commands.json`
- **No Code Changes Required**: Modify SQL commands and connection string without recompiling
- **Parameter Support**: Full support for parameterized queries
- **Multiple Command Types**: Support for SELECT queries, UPDATE/INSERT statements, and stored procedures

## Project Structure

```
TransferJurnal/
├── Program.cs                  # Main application entry point
├── SqlCommandExecutor.cs       # SQL command execution logic
├── SqlCommandConfig.cs         # Configuration models
├── appsettings.json           # Connection string configuration
├── commands.json              # SQL commands storage
└── TransferJurnal.csproj      # Project file
```

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server (local or remote instance)

## Configuration

### Connection String (`appsettings.json`)

Update the connection string in `appsettings.json` to point to your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### SQL Commands (`commands.json`)

Define your SQL commands in `commands.json`. Each command has:
- **name**: Unique identifier for the command
- **type**: Command type (`query`, `nonquery`, or `storedprocedure`)
- **sql**: The SQL statement or stored procedure name
- **parameters**: Array of parameters with name and type

Example command configurations:

```json
{
  "commands": [
    {
      "name": "GetAllRecords",
      "type": "query",
      "sql": "SELECT * FROM TableName",
      "parameters": []
    },
    {
      "name": "GetRecordById",
      "type": "query",
      "sql": "SELECT * FROM TableName WHERE Id = @Id",
      "parameters": [
        {
          "name": "@Id",
          "type": "Int"
        }
      ]
    },
    {
      "name": "UpdateRecord",
      "type": "nonquery",
      "sql": "UPDATE TableName SET Column1 = @Value1 WHERE Id = @Id",
      "parameters": [
        {
          "name": "@Value1",
          "type": "String"
        },
        {
          "name": "@Id",
          "type": "Int"
        }
      ]
    },
    {
      "name": "ExecuteStoredProcedure",
      "type": "storedprocedure",
      "sql": "sp_ProcedureName",
      "parameters": [
        {
          "name": "@Param1",
          "type": "String"
        }
      ]
    }
  ]
}
```

## Building the Project

```bash
dotnet build
```

## Running the Application

```bash
dotnet run
```

The application will:
1. Load the connection string from `appsettings.json`
2. Load SQL commands from `commands.json`
3. Display all available commands
4. Be ready to execute SQL commands

## Usage Example

To use the SQL command executor in your code:

```csharp
// Execute a query
var parameters = new Dictionary<string, object>
{
    { "@Id", 1 }
};
var result = await executor.ExecuteQueryAsync("GetRecordById", parameters);

// Execute a non-query (INSERT/UPDATE/DELETE)
var updateParams = new Dictionary<string, object>
{
    { "@Value1", "NewValue" },
    { "@Id", 1 }
};
var rowsAffected = await executor.ExecuteNonQueryAsync("UpdateRecord", updateParams);
```

## Modifying SQL Commands

To add, modify, or remove SQL commands:

1. Open `commands.json`
2. Edit the commands array
3. Save the file
4. Restart the application (no recompilation needed)

## Security Notes

- **Never commit sensitive credentials**: Use environment variables or secure configuration management for production
- **Use parameterized queries**: Always use parameters to prevent SQL injection
- **Connection string security**: Consider encrypting connection strings in production environments

## Dependencies

- **Microsoft.Data.SqlClient** (5.1.5): SQL Server data provider
- **Microsoft.Extensions.Configuration** (8.0.0): Configuration framework
- **Microsoft.Extensions.Configuration.Json** (8.0.0): JSON configuration provider

## Architecture

The application follows a clean architecture with:

1. **Configuration Layer**: Manages application settings and SQL command definitions
2. **Executor Layer**: `SqlCommandExecutor` handles SQL command execution
3. **Application Layer**: `Program.cs` orchestrates configuration loading and command execution

## Contributing

When contributing, please ensure:
- All SQL commands are parameterized
- Configuration files are properly structured
- Code follows existing naming conventions

## License

[Add your license information here]
