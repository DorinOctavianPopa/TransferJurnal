# TransferJurnal

<div align="center">

**A powerful .NET console application for executing SQL commands with externalized configuration, advanced execution planning, and parameter chaining capabilities.**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Compatible-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [Configuration Guide](#-configuration-guide)
- [Usage Examples](#-usage-examples)
- [Execution Plans](#-execution-plans)
- [Advanced Features](#-advanced-features)
- [Troubleshooting](#-troubleshooting)
- [FAQ](#-faq)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

**TransferJurnal** is a flexible SQL command executor that separates SQL logic from application code. It's perfect for:

- 🔄 **Data Migration**: Transfer data between databases with configurable execution plans
- 📊 **ETL Workflows**: Extract, transform, and load data with parameter chaining
- 🧪 **Database Testing**: Execute test queries without writing C# code
- 🔧 **Database Maintenance**: Run routine maintenance tasks with scheduled execution
- 📈 **Reporting**: Generate reports by executing pre-configured queries
- 🔍 **Data Validation**: Validate data across tables with chained queries

### Why TransferJurnal?

- ✅ **Zero Code Changes**: Modify SQL commands without recompiling
- ✅ **Parameter Chaining**: Use results from one query as parameters for another
- ✅ **Execution Plans**: Automate complex multi-step database operations
- ✅ **Type Safety**: Full type support for SQL parameters
- ✅ **Connection Diagnostics**: Built-in connection troubleshooting
- ✅ **Result Export**: Export query results to files

---

## ✨ Key Features

### 🔌 Externalized Configuration
- Connection strings stored in `appsettings.json`
- SQL commands defined in `commands.json`
- Execution workflows in `execution-plan.json`
- No recompilation needed for changes

### 🔗 Parameter Chaining
- Pass results from one command to another
- Support for expressions and transformations
- Aggregate functions (SUM, AVG, COUNT, MIN, MAX)
- Complex value expressions with CONCAT

### 📋 Execution Plans
- Multi-step workflow automation
- Conditional execution with error handling
- Result storage and retrieval
- Export results to CSV files
- Enable/disable individual steps

### 🛡️ Security & Safety
- Parameterized queries prevent SQL injection
- Built-in connection diagnostics
- Secure credential handling
- TrustServerCertificate support

### 🎨 Rich Console Output
- Color-coded status messages
- Real-time execution feedback
- Detailed error diagnostics
- Execution summary reports

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     TransferJurnal                          │
│                    Console Application                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
       ┌───────────────┴────────────────┐
       │                                │
       ▼                                ▼
┌──────────────┐              ┌─────────────────┐
│   Program    │              │ ExecutionEngine │
│   (Main)     │──────────────│  (Orchestrator) │
└──────┬───────┘              └────────┬────────┘
       │                               │
       │                               │
       ▼                               ▼
┌──────────────────┐          ┌────────────────────┐
│SqlCommandExecutor│          │ Parameter Resolver │
│  (SQL Engine)    │          │ (Chain Handler)    │
└─────────┬────────┘          └──────────┬─────────┘
          │                              │
          └──────────┬───────────────────┘
                     │
                     ▼
          ┌─────────────────────┐
          │   SQL Server DB     │
          └─────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                  Configuration Files                    │
├─────────────────────────────────────────────────────────┤
│  appsettings.json  │  commands.json  │ execution-plan   │
│  (Connection)      │  (SQL Commands) │ (Workflows)      │
└─────────────────────────────────────────────────────────┘
```

### Project Structure

```
TransferJurnal/
├── Program.cs                  # Main entry point with connection diagnostics
├── SqlCommandExecutor.cs       # Core SQL command execution engine
├── SqlCommandConfig.cs         # Command configuration models
├── ExecutionEngine.cs          # Workflow orchestration and parameter chaining
├── ExecutionPlanConfig.cs      # Execution plan models
├── Examples.cs                 # Code usage examples
├── appsettings.json           # Database connection configuration
├── commands.json              # SQL command definitions
├── execution-plan.json        # Workflow automation plans
└── TransferJurnal.csproj      # Project file with dependencies
```

---

## 🚀 Getting Started

### Prerequisites

- ✅ [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- ✅ SQL Server (local, remote, or SQL Express)
- ✅ SQL Server Management Studio (SSMS) - optional but recommended

### Quick Start (5 minutes)

1. **Clone the repository**
   ```bash
   git clone https://github.com/DorinOctavianPopa/TransferJurnal.git
   cd TransferJurnal
   ```

2. **Configure your database connection**
   
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=YourDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
     }
   }
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

The application will:
- ✅ Test your SQL Server connection with detailed diagnostics
- ✅ Load all SQL commands from `commands.json`
- ✅ Check for execution plans
- ✅ Be ready to execute your workflows

---

## ⚙️ Configuration Guide

### 1. Connection String (`appsettings.json`)

Configure your SQL Server connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=True;Encrypt=False;"
  }
}
```

#### Connection String Options

| Parameter | Description | Example |
|-----------|-------------|---------|
| `Server` | SQL Server instance name | `localhost`, `.\SQLEXPRESS`, `192.168.1.100` |
| `Database` | Database name | `ESRP`, `MyDatabase` |
| `User Id` | SQL authentication username | `sa`, `admin` |
| `Password` | SQL authentication password | `P@ssw0rd.2025` |
| `TrustServerCertificate` | Trust SSL certificate | `True` (for local dev) |
| `Encrypt` | Use encrypted connection | `False` (for local dev) |
| `Integrated Security` | Use Windows auth | `True` (removes User Id/Password) |

**Windows Authentication Example:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDatabase;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### 2. SQL Commands (`commands.json`)

Define reusable SQL commands:

```json
{
  "commands": [
    {
      "name": "GetAllEmployees",
      "type": "query",
      "sql": "SELECT EmployeeId, FirstName, LastName, DepartmentId, Salary FROM Employees",
      "parameters": []
    },
    {
      "name": "GetEmployeeById",
      "type": "query",
      "sql": "SELECT * FROM Employees WHERE EmployeeId = @EmployeeId",
      "parameters": [
        {
          "name": "@EmployeeId",
          "type": "Int"
        }
      ]
    },
    {
      "name": "InsertEmployee",
      "type": "nonquery",
      "sql": "INSERT INTO Employees (FirstName, LastName, DepartmentId, Salary) VALUES (@FirstName, @LastName, @DepartmentId, @Salary)",
      "parameters": [
        {
          "name": "@FirstName",
          "type": "String"
        },
        {
          "name": "@LastName",
          "type": "String"
        },
        {
          "name": "@DepartmentId",
          "type": "Int"
        },
        {
          "name": "@Salary",
          "type": "Decimal"
        }
      ]
    },
    {
      "name": "UpdateEmployeeSalary",
      "type": "nonquery",
      "sql": "UPDATE Employees SET Salary = @NewSalary WHERE EmployeeId = @EmployeeId",
      "parameters": [
        {
          "name": "@NewSalary",
          "type": "Decimal"
        },
        {
          "name": "@EmployeeId",
          "type": "Int"
        }
      ]
    },
    {
      "name": "DeleteEmployee",
      "type": "nonquery",
      "sql": "DELETE FROM Employees WHERE EmployeeId = @EmployeeId",
      "parameters": [
        {
          "name": "@EmployeeId",
          "type": "Int"
        }
      ]
    },
    {
      "name": "GetDepartmentStats",
      "type": "storedprocedure",
      "sql": "sp_GetDepartmentStatistics",
      "parameters": [
        {
          "name": "@DepartmentId",
          "type": "Int"
        },
        {
          "name": "@Year",
          "type": "Int"
        }
      ]
    }
  ]
}
```

#### Command Types

| Type | Description | Use Case |
|------|-------------|----------|
| `query` | SELECT statements | Retrieve data from database |
| `nonquery` | INSERT/UPDATE/DELETE | Modify data |
| `storedprocedure` | Execute stored procedures | Complex operations |

#### Supported Parameter Types

| Type | SQL Type | .NET Type | Example |
|------|----------|-----------|---------|
| `Int` | INT | int | `123` |
| `String` | NVARCHAR | string | `"John Doe"` |
| `DateTime` | DATETIME | DateTime | `"2025-12-31T10:00:00"` |
| `Decimal` | DECIMAL | decimal | `1234.56` |
| `Bit` | BIT | bool | `true` or `false` |
| `BigInt` | BIGINT | long | `9223372036854775807` |
| `UniqueIdentifier` | UNIQUEIDENTIFIER | Guid | `"550e8400-e29b-41d4-a716-446655440000"` |

### 3. Execution Plans (`execution-plan.json`)

Create automated workflows:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "description": "Execution plan for automated SQL command workflows",
  "executionPlan": {
    "name": "Employee Data Migration",
    "description": "Migrate employee data with salary calculations",
    "continueOnError": false,
    "outputResults": true,
    "commands": [
      {
        "commandName": "GetAllEmployees",
        "enabled": true,
        "description": "Retrieve all employee records",
        "parameters": {},
        "outputOptions": {
          "displayResults": true,
          "maxRows": 100,
          "exportToFile": true,
          "exportPath": "./exports/employees.csv"
        },
        "outputMapping": {
          "storeResults": true,
          "resultKey": "allEmployees"
        }
      },
      {
        "commandName": "GetEmployeeById",
        "enabled": true,
        "description": "Get first employee details",
        "parameters": {
          "@EmployeeId": {
            "type": "fromPreviousCommand",
            "sourceCommand": "allEmployees",
            "sourceColumn": "EmployeeId",
            "sourceRow": 0
          }
        },
        "outputOptions": {
          "displayResults": true,
          "maxRows": 10
        },
        "outputMapping": {
          "storeResults": true,
          "resultKey": "selectedEmployee"
        }
      },
      {
        "commandName": "UpdateEmployeeSalary",
        "enabled": true,
        "description": "Update salary with 10% increase",
        "parameters": {
          "@EmployeeId": {
            "type": "fromPreviousCommand",
            "sourceCommand": "selectedEmployee",
            "sourceColumn": "EmployeeId",
            "sourceRow": 0
          },
          "@NewSalary": {
            "type": "expression",
            "expression": "{selectedEmployee.Salary[0]} * 1.1"
          }
        },
        "outputOptions": {
          "displayResults": true
        }
      }
    ]
  },
  "globalSettings": {
    "timeout": 30,
    "logLevel": "Info",
    "timestampFormat": "yyyy-MM-dd HH:mm:ss",
    "errorHandling": {
      "continueOnError": false,
      "logErrors": true,
      "emailOnError": false
    }
  }
}
```

---

## 💡 Usage Examples

### Example 1: Simple Query Execution

**Scenario**: Retrieve all records from a table

1. **Define the command** in `commands.json`:
```json
{
  "name": "GetAllProducts",
  "type": "query",
  "sql": "SELECT ProductId, ProductName, Price, StockQuantity FROM Products",
  "parameters": []
}
```

2. **Run the application**:
```bash
dotnet run
```

The application will automatically execute the execution plan if available.

### Example 2: Parameterized Query

**Scenario**: Get a specific product by ID

1. **Define the command**:
```json
{
  "name": "GetProductById",
  "type": "query",
  "sql": "SELECT * FROM Products WHERE ProductId = @ProductId",
  "parameters": [
    {
      "name": "@ProductId",
      "type": "Int"
    }
  ]
}
```

2. **Use in execution plan**:
```json
{
  "commandName": "GetProductById",
  "enabled": true,
  "parameters": {
    "@ProductId": 101
  }
}
```

### Example 3: Data Insertion

**Scenario**: Insert a new order

1. **Define the command**:
```json
{
  "name": "InsertOrder",
  "type": "nonquery",
  "sql": "INSERT INTO Orders (CustomerId, OrderDate, TotalAmount) VALUES (@CustomerId, @OrderDate, @TotalAmount)",
  "parameters": [
    {
      "name": "@CustomerId",
      "type": "Int"
    },
    {
      "name": "@OrderDate",
      "type": "DateTime"
    },
    {
      "name": "@TotalAmount",
      "type": "Decimal"
    }
  ]
}
```

2. **Execute with parameters**:
```json
{
  "commandName": "InsertOrder",
  "enabled": true,
  "parameters": {
    "@CustomerId": 5,
    "@OrderDate": "2025-12-31T10:00:00",
    "@TotalAmount": 1299.99
  }
}
```

### Example 4: Update with Parameter Chaining

**Scenario**: Update a product's price based on previous query

1. **Execution plan**:
```json
{
  "commands": [
    {
      "commandName": "GetProductById",
      "enabled": true,
      "parameters": {
        "@ProductId": 101
      },
      "outputMapping": {
        "storeResults": true,
        "resultKey": "currentProduct"
      }
    },
    {
      "commandName": "UpdateProductPrice",
      "enabled": true,
      "description": "Increase price by 15%",
      "parameters": {
        "@ProductId": {
          "type": "fromPreviousCommand",
          "sourceCommand": "currentProduct",
          "sourceColumn": "ProductId",
          "sourceRow": 0
        },
        "@NewPrice": {
          "type": "expression",
          "expression": "{currentProduct.Price[0]} * 1.15"
        }
      }
    }
  ]
}
```

### Example 5: Stored Procedure Execution

**Scenario**: Execute a complex stored procedure

1. **Define the command**:
```json
{
  "name": "GenerateMonthlyReport",
  "type": "storedprocedure",
  "sql": "sp_GenerateMonthlyReport",
  "parameters": [
    {
      "name": "@Year",
      "type": "Int"
    },
    {
      "name": "@Month",
      "type": "Int"
    },
    {
      "name": "@DepartmentId",
      "type": "Int"
    }
  ]
}
```

2. **Execute**:
```json
{
  "commandName": "GenerateMonthlyReport",
  "enabled": true,
  "parameters": {
    "@Year": 2025,
    "@Month": 12,
    "@DepartmentId": 5
  },
  "outputOptions": {
    "displayResults": true,
    "exportToFile": true,
    "exportPath": "./reports/december-2025.csv"
  }
}
```

### Example 6: Programmatic Usage

Use TransferJurnal as a library in your C# code:

```csharp
using TransferJurnal;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

// Load commands
var commandsJson = await File.ReadAllTextAsync("commands.json");
var commandsConfig = JsonSerializer.Deserialize<CommandsConfig>(commandsJson);

// Create executor
var executor = new SqlCommandExecutor(connectionString, commandsConfig);

// Example 1: Simple query
var allProducts = await executor.ExecuteQueryAsync("GetAllProducts");
Console.WriteLine($"Found {allProducts.Rows.Count} products");

// Example 2: Parameterized query
var parameters = new Dictionary<string, object>
{
    { "@ProductId", 101 }
};
var product = await executor.ExecuteQueryAsync("GetProductById", parameters);

// Example 3: Insert with parameters
var insertParams = new Dictionary<string, object>
{
    { "@CustomerId", 5 },
    { "@OrderDate", DateTime.Now },
    { "@TotalAmount", 1299.99m }
};
var rowsAffected = await executor.ExecuteNonQueryAsync("InsertOrder", insertParams);
Console.WriteLine($"Inserted {rowsAffected} rows");

// Example 4: Update
var updateParams = new Dictionary<string, object>
{
    { "@ProductId", 101 },
    { "@NewPrice", 29.99m }
};
await executor.ExecuteNonQueryAsync("UpdateProductPrice", updateParams);

// Example 5: Handle null values
var paramsWithNull = new Dictionary<string, object>
{
    { "@OptionalField", null! },  // Converted to DBNull.Value
    { "@RequiredField", "Value" }
};
await executor.ExecuteNonQueryAsync("UpdateWithNulls", paramsWithNull);

// Example 6: List available commands
executor.ListAvailableCommands();
```

---

## 📋 Execution Plans

Execution plans enable complex multi-step workflows with parameter chaining.

### Basic Execution Plan Structure

```json
{
  "executionPlan": {
    "name": "Plan Name",
    "description": "What this plan does",
    "continueOnError": false,
    "outputResults": true,
    "commands": [
      // ... command definitions
    ]
  },
  "globalSettings": {
    "timeout": 30,
    "logLevel": "Info"
  }
}
```

### Parameter Types in Execution Plans

#### 1. Static Parameters
```json
"parameters": {
  "@CustomerId": 123,
  "@Name": "John Doe",
  "@IsActive": true
}
```

#### 2. From Previous Command
```json
"parameters": {
  "@EmployeeId": {
    "type": "fromPreviousCommand",
    "sourceCommand": "allEmployees",
    "sourceColumn": "EmployeeId",
    "sourceRow": 0,
    "transform": "uppercase"
  }
}
```

**Available Transforms**: `uppercase`, `lowercase`, `trim`, `trimstart`, `trimend`

#### 3. Expression (String Concatenation)
```json
"parameters": {
  "@FullName": {
    "type": "expression",
    "expression": "CONCAT({employees.FirstName[0]}, ' ', {employees.LastName[0]})"
  }
}
```

#### 4. Aggregate Functions
```json
"parameters": {
  "@AverageSalary": {
    "type": "aggregate",
    "sourceCommand": "allEmployees",
    "sourceColumn": "Salary",
    "aggregateFunction": "avg"
  }
}
```

**Available Functions**: `count`, `sum`, `avg`, `min`, `max`

### Complete Workflow Example

**Scenario**: Employee salary adjustment workflow

```json
{
  "executionPlan": {
    "name": "Salary Adjustment Workflow",
    "description": "Calculate and apply salary adjustments based on department averages",
    "continueOnError": false,
    "outputResults": true,
    "commands": [
      {
        "commandName": "GetAllEmployees",
        "enabled": true,
        "description": "Step 1: Load all employees",
        "parameters": {},
        "outputOptions": {
          "displayResults": true,
          "maxRows": 100,
          "exportToFile": true,
          "exportPath": "./exports/employees-before.csv"
        },
        "outputMapping": {
          "storeResults": true,
          "resultKey": "allEmployees"
        }
      },
      {
        "commandName": "CalculateDepartmentAverage",
        "enabled": true,
        "description": "Step 2: Calculate average salary",
        "parameters": {
          "@DepartmentId": {
            "type": "fromPreviousCommand",
            "sourceCommand": "allEmployees",
            "sourceColumn": "DepartmentId",
            "sourceRow": 0
          }
        },
        "outputMapping": {
          "storeResults": true,
          "resultKey": "avgSalary"
        }
      },
      {
        "commandName": "UpdateEmployeeSalary",
        "enabled": true,
        "description": "Step 3: Update salaries below average",
        "parameters": {
          "@EmployeeId": {
            "type": "fromPreviousCommand",
            "sourceCommand": "allEmployees",
            "sourceColumn": "EmployeeId",
            "sourceRow": 0
          },
          "@NewSalary": {
            "type": "aggregate",
            "sourceCommand": "avgSalary",
            "sourceColumn": "AverageSalary",
            "aggregateFunction": "avg"
          }
        },
        "outputOptions": {
          "displayResults": true
        }
      },
      {
        "commandName": "GetAllEmployees",
        "enabled": true,
        "description": "Step 4: Verify changes",
        "parameters": {},
        "outputOptions": {
          "displayResults": true,
          "maxRows": 100,
          "exportToFile": true,
          "exportPath": "./exports/employees-after.csv"
        }
      }
    ]
  },
  "globalSettings": {
    "timeout": 60,
    "logLevel": "Info",
    "timestampFormat": "yyyy-MM-dd HH:mm:ss",
    "errorHandling": {
      "continueOnError": false,
      "logErrors": true,
      "emailOnError": false
    }
  }
}
```

### Execution Plan Features

| Feature | Description |
|---------|-------------|
| **Step Control** | Enable/disable individual steps with `enabled` flag |
| **Error Handling** | Continue on error or stop execution |
| **Result Storage** | Store query results for use in subsequent steps |
| **Result Export** | Export results to CSV files |
| **Max Rows** | Limit displayed rows while processing all data |
| **Descriptions** | Add documentation to each step |
| **Execution Summary** | Detailed report of execution time and results |

---

## 🚀 Advanced Features

### Connection Diagnostics

TransferJurnal includes comprehensive connection diagnostics:

- ✅ Network connectivity testing (ping, TCP port check)
- ✅ SQL Server version detection
- ✅ Authentication validation
- ✅ Database existence verification
- ✅ Detailed error messages with solutions
- ✅ Firewall and configuration recommendations

**Example Output:**
```
Connection Details:
  Server: localhost
  Database: ESRP
  Authentication: SQL Authentication (User: Admin)

🔍 Step 1: Testing network connectivity...
  ✓ Target is localhost
  Testing TCP connection to localhost:1433...
  ✓ Port 1433 is open and accepting connections

🔍 Step 2: Attempting database connection...
  ✓ Connected to SQL Server version: 15.00.2000
  ✓ Database 'ESRP' is accessible
```

### Result Export

Export query results to CSV files:

```json
{
  "outputOptions": {
    "displayResults": true,
    "maxRows": 100,
    "exportToFile": true,
    "exportPath": "./exports/results.csv"
  }
}
```

The CSV file includes:
- Column headers
- All rows (not limited by maxRows)
- Proper CSV escaping for special characters

### Execution Summary

After plan execution, get a detailed summary:

```
═══════════════════════════════════════
           EXECUTION SUMMARY
═══════════════════════════════════════
Plan Name:        Employee Data Migration
Start Time:       2025-12-31 10:00:00
End Time:         2025-12-31 10:00:15
Duration:         15.23 seconds
Total Commands:   4
✓ Successful:     4
❌ Failed:         0

Command Details:
  ✓ GetAllEmployees - 1250ms - 150 rows
  ✓ GetEmployeeById - 45ms - 1 rows
  ✓ UpdateEmployeeSalary - 89ms - 1 rows
  ✓ GetAllEmployees - 1180ms - 150 rows
═══════════════════════════════════════
```

### Error Handling

Configure error handling behavior:

```json
{
  "executionPlan": {
    "continueOnError": true  // Continue executing even if a command fails
  },
  "globalSettings": {
    "errorHandling": {
      "continueOnError": true,
      "logErrors": true,
      "emailOnError": false
    }
  }
}
```

---

## 🔧 Troubleshooting

### Common Issues and Solutions

#### 1. Cannot Connect to SQL Server

**Error**: `Cannot connect to SQL Server instance`

**Solutions**:
1. ✅ Verify SQL Server service is running:
   - Open `services.msc`
   - Check `SQL Server (MSSQLSERVER)` status
   - Start if stopped

2. ✅ Enable TCP/IP protocol:
   - Open SQL Server Configuration Manager
   - Navigate to: SQL Server Network Configuration > Protocols
   - Enable `TCP/IP` protocol
   - Restart SQL Server

3. ✅ Check Windows Firewall:
   - Allow port 1433 through Windows Firewall
   - Or run: `netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433`

4. ✅ Verify server name:
   - Try: `localhost`, `127.0.0.1`, `.\SQLEXPRESS`, or your machine name

#### 2. Authentication Failed

**Error**: `Login failed for user`

**Solutions**:
1. ✅ Verify credentials in `appsettings.json`
2. ✅ Enable SQL Server authentication:
   - In SSMS, right-click server > Properties > Security
   - Select "SQL Server and Windows Authentication mode"
   - Restart SQL Server service
3. ✅ Check if login exists:
   ```sql
   CREATE LOGIN [Admin] WITH PASSWORD = 'YourPassword';
   USE [YourDatabase];
   CREATE USER [Admin] FOR LOGIN [Admin];
   ALTER ROLE db_owner ADD MEMBER [Admin];
   ```

#### 3. Database Does Not Exist

**Error**: `Cannot open database`

**Solutions**:
1. ✅ Create the database:
   ```sql
   CREATE DATABASE [YourDatabase];
   ```
2. ✅ Or update database name in `appsettings.json`
3. ✅ List available databases:
   ```sql
   SELECT name FROM sys.databases;
   ```

#### 4. Command Not Found

**Error**: `Command 'CommandName' not found in configuration`

**Solutions**:
1. ✅ Verify command name matches exactly (case-sensitive)
2. ✅ Check `commands.json` syntax
3. ✅ Ensure command is in the `commands` array
4. ✅ Restart application after modifying `commands.json`

#### 5. Parameter Missing

**Error**: `Required parameter '@ParamName' not provided`

**Solutions**:
1. ✅ Check parameter name matches exactly (including `@`)
2. ✅ Verify parameter is defined in execution plan
3. ✅ For chained parameters, ensure source command was executed and stored results

#### 6. Column Not Found in Results

**Error**: `Column 'ColumnName' not found in results`

**Solutions**:
1. ✅ Verify column name in SQL query
2. ✅ Check if source command returned any rows
3. ✅ Ensure `storeResults: true` and `resultKey` is set
4. ✅ Verify `sourceRow` index is valid (0-based)

---

## ❓ FAQ

### General Questions

**Q: Do I need to recompile after changing SQL commands?**  
A: No! Changes to `commands.json`, `appsettings.json`, and `execution-plan.json` are loaded at runtime.

**Q: Can I use Windows Authentication instead of SQL Authentication?**  
A: Yes! Use `Integrated Security=True` in your connection string and remove `User Id` and `Password`.

**Q: How do I handle NULL values?**  
A: Use `null` in execution plan parameters or `null!` in C# code. It will be automatically converted to `DBNull.Value`.

**Q: Can I execute multiple execution plans?**  
A: Currently, the application executes `execution-plan.json` if present. To run different plans, rename them or modify `Program.cs`.

### Configuration Questions

**Q: What SQL Server versions are supported?**  
A: SQL Server 2012 and later, including SQL Express and Azure SQL Database.

**Q: Can I connect to a remote SQL Server?**  
A: Yes! Use the server's IP address or hostname in the connection string: `Server=192.168.1.100;...`

**Q: How do I encrypt the connection string?**  
A: Use .NET User Secrets for development or Azure Key Vault for production.

### Execution Plan Questions

**Q: Can I loop through all rows from a previous command?**  
A: Currently, you specify `sourceRow` index. For bulk operations, consider using set-based SQL commands.

**Q: How do I debug parameter chaining issues?**  
A: Enable `displayResults: true` and check console output for resolved parameter values.

**Q: Can I use transaction support?**  
A: Transactions are not currently supported in execution plans. Each command is auto-committed.

### Performance Questions

**Q: How many commands can I chain together?**  
A: No hard limit, but consider memory usage for large result sets. Use `maxRows` to limit displayed data.

**Q: Can I execute commands in parallel?**  
A: Currently, commands execute sequentially. Parallel execution may be added in future versions.

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

### Areas for Contribution

- 📝 Documentation improvements
- 🐛 Bug fixes
- ✨ New features (transaction support, parallel execution, etc.)
- 🧪 Test coverage
- 🌐 Localization

### Contribution Guidelines

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/AmazingFeature`
3. **Commit your changes**: `git commit -m 'Add some AmazingFeature'`
4. **Push to the branch**: `git push origin feature/AmazingFeature`
5. **Open a Pull Request**

### Code Standards

- ✅ Use meaningful variable names
- ✅ Add XML documentation comments for public methods
- ✅ Follow existing code style and formatting
- ✅ All SQL commands must be parameterized
- ✅ Include examples for new features
- ✅ Update README.md for significant changes

### Testing

Before submitting:
- ✅ Test with SQL Server locally
- ✅ Verify connection diagnostics work
- ✅ Test parameter chaining scenarios
- ✅ Ensure no sensitive data in commits

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with [.NET 8.0](https://dotnet.microsoft.com/)
- Uses [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient)
- Inspired by the need for flexible database operations without code changes

---

## 📞 Support

Having issues? Here's how to get help:

1. 📖 Check the [Troubleshooting](#-troubleshooting) section
2. 💬 Review [FAQ](#-faq)
3. 🐛 [Open an issue](https://github.com/DorinOctavianPopa/TransferJurnal/issues)
4. 📧 Contact the maintainers

---

<div align="center">

**Made with ❤️ by [Dorin Octavian Popa](https://github.com/DorinOctavianPopa)**

⭐ Star this repo if you find it helpful!

</div>
