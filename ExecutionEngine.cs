using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TransferJurnal;

public class ExecutionEngine
{
    private readonly SqlCommandExecutor _executor;
    private readonly ExecutionPlanConfig _config;
    private readonly Dictionary<string, DataTable> _storedResults = new();

    public ExecutionEngine(SqlCommandExecutor executor, ExecutionPlanConfig config)
    {
        _executor = executor;
        _config = config;
    }

    public async Task<ExecutionResult> ExecuteAsync()
    {
        var result = new ExecutionResult
        {
            StartTime = DateTime.Now,
            PlanName = _config.ExecutionPlan.Name
        };

        WriteHeader($"\n🚀 Executing Plan: {_config.ExecutionPlan.Name}");
        if (!string.IsNullOrEmpty(_config.ExecutionPlan.Description))
        {
            WriteInfo($"   {_config.ExecutionPlan.Description}");
        }
        Console.WriteLine();

        var enabledCommands = _config.ExecutionPlan.Commands.Where(c => c.Enabled).ToList();
        
        WriteInfo($"📊 Total commands to execute: {enabledCommands.Count}");
        Console.WriteLine();

        for (int i = 0; i < enabledCommands.Count; i++)
        {
            var command = enabledCommands[i];
            var commandResult = await ExecuteCommandAsync(command, i + 1, enabledCommands.Count);
            result.CommandResults.Add(commandResult);

            if (!commandResult.Success && !_config.ExecutionPlan.ContinueOnError)
            {
                WriteError($"\n⚠ Execution stopped due to error in command: {command.CommandName}");
                break;
            }
        }

        result.EndTime = DateTime.Now;
        result.Duration = result.EndTime - result.StartTime;
        
        PrintExecutionSummary(result);
        
        return result;
    }

    private async Task<CommandResult> ExecuteCommandAsync(CommandExecution command, int current, int total)
    {
        var commandResult = new CommandResult
        {
            CommandName = command.CommandName,
            StartTime = DateTime.Now
        };

        try
        {
            WriteInfo($"[{current}/{total}] Executing: {command.CommandName}");
            if (!string.IsNullOrEmpty(command.Description))
            {
                Console.WriteLine($"        Description: {command.Description}");
            }

            // Resolve parameters with chaining support
            var resolvedParameters = await ResolveParametersAsync(command.Parameters);

            if (resolvedParameters.Any())
            {
                Console.WriteLine($"        Parameters: {string.Join(", ", resolvedParameters.Select(p => $"{p.Key}={p.Value}"))}");
            }

            // Execute command
            var dataTable = await _executor.ExecuteQueryAsync(command.CommandName, resolvedParameters);

            commandResult.Success = true;
            commandResult.RowsAffected = dataTable?.Rows.Count ?? 0;

            // Store results if configured
            if (command.OutputMapping.StoreResults && !string.IsNullOrEmpty(command.OutputMapping.ResultKey))
            {
                if (dataTable != null)
                {
                    _storedResults[command.OutputMapping.ResultKey] = dataTable;
                    WriteSuccess($"        ✓ Results stored as: {command.OutputMapping.ResultKey}");
                }
            }

            if (command.OutputOptions.DisplayResults && dataTable != null)
            {
                DisplayResults(dataTable, command.OutputOptions.MaxRows);
            }

            if (command.OutputOptions.ExportToFile && !string.IsNullOrEmpty(command.OutputOptions.ExportPath))
            {
                ExportResults(dataTable, command.OutputOptions.ExportPath);
            }

            WriteSuccess($"        ✓ Success - {commandResult.RowsAffected} rows affected\n");
        }
        catch (Exception ex)
        {
            commandResult.Success = false;
            commandResult.ErrorMessage = ex.Message;
            WriteError($"        ❌ Error: {ex.Message}\n");
        }

        commandResult.EndTime = DateTime.Now;
        commandResult.Duration = commandResult.EndTime - commandResult.StartTime;

        return commandResult;
    }

    private async Task<Dictionary<string, object>> ResolveParametersAsync(Dictionary<string, object> parameters)
    {
        var resolved = new Dictionary<string, object>();

        foreach (var param in parameters)
        {
            var value = await ResolveParameterValueAsync(param.Key, param.Value);
            resolved[param.Key] = value;
        }

        return resolved;
    }

    private async Task<object> ResolveParameterValueAsync(string paramName, object paramValue)
    {
        // If it's a simple value (not a complex object), return as is
        if (paramValue is not JsonElement jsonElement)
        {
            return paramValue;
        }

        // Check if it's a parameter definition object
        if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("type", out var typeProperty))
        {
            var paramDef = JsonSerializer.Deserialize<ParameterDefinition>(jsonElement.GetRawText());
            if (paramDef == null)
            {
                return ConvertJsonElement(jsonElement);
            }

            return paramDef.Type.ToLower() switch
            {
                "static" => ConvertJsonElement(JsonSerializer.SerializeToElement(paramDef.Value)),
                "frompreviouscommand" => ResolveFromPreviousCommand(paramDef),
                "expression" => await ResolveExpressionAsync(paramDef.Expression),
                "aggregate" => ResolveAggregate(paramDef),
                _ => throw new ArgumentException($"Unknown parameter type: {paramDef.Type}")
            };
        }

        // Default conversion
        return ConvertJsonElement(jsonElement);
    }

    private object ResolveFromPreviousCommand(ParameterDefinition paramDef)
    {
        if (string.IsNullOrEmpty(paramDef.SourceCommand))
        {
            throw new ArgumentException("SourceCommand must be specified for fromPreviousCommand type");
        }

        if (!_storedResults.TryGetValue(paramDef.SourceCommand, out var sourceTable))
        {
            throw new InvalidOperationException($"No stored results found for command: {paramDef.SourceCommand}");
        }

        if (sourceTable.Rows.Count == 0)
        {
            throw new InvalidOperationException($"No rows in stored results for command: {paramDef.SourceCommand}");
        }

        if (paramDef.SourceRow >= sourceTable.Rows.Count)
        {
            throw new ArgumentException($"SourceRow {paramDef.SourceRow} is out of range. Available rows: {sourceTable.Rows.Count}");
        }

        if (!sourceTable.Columns.Contains(paramDef.SourceColumn))
        {
            throw new ArgumentException($"Column '{paramDef.SourceColumn}' not found in results from command: {paramDef.SourceCommand}");
        }

        var value = sourceTable.Rows[paramDef.SourceRow][paramDef.SourceColumn];

        // Apply transformation if specified
        if (!string.IsNullOrEmpty(paramDef.Transform) && value != DBNull.Value)
        {
            value = ApplyTransformation(value.ToString() ?? string.Empty, paramDef.Transform);
        }

        WriteInfo($"        📎 Resolved {paramDef.SourceCommand}.{paramDef.SourceColumn}[{paramDef.SourceRow}] = {value}");
        
        return value;
    }

    private async Task<object> ResolveExpressionAsync(string expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            throw new ArgumentException("Expression cannot be empty");
        }

        // Pattern: {commandName.columnName[rowIndex]}
        var pattern = @"\{([^.]+)\.([^\[]+)\[(\d+)\]\}";
        var matches = Regex.Matches(expression, pattern);

        var resolvedExpression = expression;

        foreach (Match match in matches)
        {
            var commandName = match.Groups[1].Value;
            var columnName = match.Groups[2].Value;
            var rowIndex = int.Parse(match.Groups[3].Value);

            if (!_storedResults.TryGetValue(commandName, out var sourceTable))
            {
                throw new InvalidOperationException($"No stored results found for command: {commandName}");
            }

            if (rowIndex >= sourceTable.Rows.Count)
            {
                throw new ArgumentException($"Row index {rowIndex} is out of range for command: {commandName}");
            }

            if (!sourceTable.Columns.Contains(columnName))
            {
                throw new ArgumentException($"Column '{columnName}' not found in results from command: {commandName}");
            }

            var value = sourceTable.Rows[rowIndex][columnName];
            resolvedExpression = resolvedExpression.Replace(match.Value, value?.ToString() ?? string.Empty);
        }

        // Process CONCAT function
        if (resolvedExpression.StartsWith("CONCAT(", StringComparison.OrdinalIgnoreCase))
        {
            var concatPattern = @"CONCAT\((.*)\)";
            var concatMatch = Regex.Match(resolvedExpression, concatPattern, RegexOptions.IgnoreCase);
            if (concatMatch.Success)
            {
                var parts = concatMatch.Groups[1].Value.Split(new[] { ", " }, StringSplitOptions.None);
                var result = string.Join("", parts.Select(p => p.Trim(' ', '\'')));
                WriteInfo($"        🔗 Resolved expression: {expression} = {result}");
                return result;
            }
        }

        WriteInfo($"        🔗 Resolved expression: {expression} = {resolvedExpression}");
        return resolvedExpression;
    }

    private object ResolveAggregate(ParameterDefinition paramDef)
    {
        if (string.IsNullOrEmpty(paramDef.SourceCommand))
        {
            throw new ArgumentException("SourceCommand must be specified for aggregate type");
        }

        if (!_storedResults.TryGetValue(paramDef.SourceCommand, out var sourceTable))
        {
            throw new InvalidOperationException($"No stored results found for command: {paramDef.SourceCommand}");
        }

        if (!sourceTable.Columns.Contains(paramDef.SourceColumn))
        {
            throw new ArgumentException($"Column '{paramDef.SourceColumn}' not found in results from command: {paramDef.SourceCommand}");
        }

        var column = sourceTable.Columns[paramDef.SourceColumn];
        var values = sourceTable.AsEnumerable()
            .Select(row => row[paramDef.SourceColumn])
            .Where(v => v != DBNull.Value)
            .ToList();

        object result = paramDef.AggregateFunction.ToLower() switch
        {
            "count" => values.Count,
            "sum" => values.Sum(v => Convert.ToDecimal(v)),
            "avg" => values.Average(v => Convert.ToDecimal(v)),
            "min" => values.Min(),
            "max" => values.Max(),
            _ => throw new ArgumentException($"Unknown aggregate function: {paramDef.AggregateFunction}")
        };

        WriteInfo($"        📊 Aggregate {paramDef.AggregateFunction.ToUpper()}({paramDef.SourceCommand}.{paramDef.SourceColumn}) = {result}");
        
        return result;
    }

    private string ApplyTransformation(string value, string transform)
    {
        return transform.ToLower() switch
        {
            "uppercase" => value.ToUpper(),
            "lowercase" => value.ToLower(),
            "trim" => value.Trim(),
            "trimstart" => value.TrimStart(),
            "trimend" => value.TrimEnd(),
            _ => value
        };
    }

    private object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt32(out var intValue) ? intValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => DBNull.Value,
            _ => element.ToString()
        };
    }

    private void DisplayResults(DataTable? dataTable, int maxRows)
    {
        if (dataTable == null || dataTable.Rows.Count == 0)
        {
            WriteWarning("        No results returned");
            return;
        }

        Console.WriteLine();
        WriteSubHeader($"        Results ({Math.Min(dataTable.Rows.Count, maxRows)} of {dataTable.Rows.Count} rows):");
        
        // Display column headers
        var headers = string.Join(" | ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName.PadRight(15)));
        Console.WriteLine($"        {headers}");
        Console.WriteLine($"        {new string('-', headers.Length)}");

        // Display rows
        var rowsToDisplay = Math.Min(dataTable.Rows.Count, maxRows);
        for (int i = 0; i < rowsToDisplay; i++)
        {
            var row = dataTable.Rows[i];
            var values = string.Join(" | ", row.ItemArray.Select(v => (v?.ToString() ?? "NULL").PadRight(15)));
            Console.WriteLine($"        {values}");
        }

        if (dataTable.Rows.Count > maxRows)
        {
            WriteWarning($"        ... and {dataTable.Rows.Count - maxRows} more rows");
        }
        Console.WriteLine();
    }

    private void ExportResults(DataTable? dataTable, string exportPath)
    {
        if (dataTable == null) return;

        try
        {
            var directory = Path.GetDirectoryName(exportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new StreamWriter(exportPath);
            
            // Write headers
            writer.WriteLine(string.Join(",", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));

            // Write rows
            foreach (DataRow row in dataTable.Rows)
            {
                writer.WriteLine(string.Join(",", row.ItemArray.Select(v => $"\"{v}\"")));
            }

            WriteSuccess($"        ✓ Exported to: {exportPath}");
        }
        catch (Exception ex)
        {
            WriteError($"        ❌ Export failed: {ex.Message}");
        }
    }

    private void PrintExecutionSummary(ExecutionResult result)
    {
        Console.WriteLine();
        WriteHeader("═══════════════════════════════════════");
        WriteHeader("           EXECUTION SUMMARY           ");
        WriteHeader("═══════════════════════════════════════");
        
        Console.WriteLine($"Plan Name:        {result.PlanName}");
        Console.WriteLine($"Start Time:       {result.StartTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"End Time:         {result.EndTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Duration:         {result.Duration.TotalSeconds:F2} seconds");
        Console.WriteLine($"Total Commands:   {result.CommandResults.Count}");
        
        var successful = result.CommandResults.Count(c => c.Success);
        var failed = result.CommandResults.Count(c => !c.Success);
        
        WriteSuccess($"✓ Successful:     {successful}");
        if (failed > 0)
        {
            WriteError($"❌ Failed:         {failed}");
        }
        else
        {
            Console.WriteLine($"❌ Failed:         {failed}");
        }

        Console.WriteLine();
        WriteSubHeader("Command Details:");
        foreach (var cmd in result.CommandResults)
        {
            var status = cmd.Success ? "✓" : "❌";
            var color = cmd.Success ? ConsoleColor.Green : ConsoleColor.Red;
            
            Console.ForegroundColor = color;
            Console.Write($"  {status} ");
            Console.ResetColor();
            
            Console.WriteLine($"{cmd.CommandName} - {cmd.Duration.TotalMilliseconds:F0}ms - {cmd.RowsAffected} rows");
            
            if (!cmd.Success && !string.IsNullOrEmpty(cmd.ErrorMessage))
            {
                WriteError($"     Error: {cmd.ErrorMessage}");
            }
        }

        WriteHeader("═══════════════════════════════════════");
    }

    // Color helper methods
    private static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void WriteHeader(string message)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void WriteSubHeader(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}

public class ExecutionResult
{
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public List<CommandResult> CommandResults { get; set; } = new();
}

public class CommandResult
{
    public string CommandName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int RowsAffected { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
}       