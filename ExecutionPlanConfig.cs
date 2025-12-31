namespace TransferJurnal;

public class ExecutionPlanConfig
{
    public ExecutionPlan ExecutionPlan { get; set; } = new();
    public GlobalSettings GlobalSettings { get; set; } = new();
}

public class ExecutionPlan
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool ContinueOnError { get; set; } = false;
    public bool OutputResults { get; set; } = true;
    public List<CommandExecution> Commands { get; set; } = new();
}

public class CommandExecution
{
    public string CommandName { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public OutputOptions OutputOptions { get; set; } = new();
    public OutputMapping OutputMapping { get; set; } = new();
}

public class OutputOptions
{
    public bool DisplayResults { get; set; } = true;
    public int MaxRows { get; set; } = 100;
    public bool ExportToFile { get; set; } = false;
    public string ExportPath { get; set; } = string.Empty;
}

public class OutputMapping
{
    public bool StoreResults { get; set; } = false;
    public string ResultKey { get; set; } = string.Empty;
}

public class ParameterDefinition
{
    public string Type { get; set; } = "static"; // static, fromPreviousCommand, expression, aggregate
    public object? Value { get; set; }
    public string SourceCommand { get; set; } = string.Empty;
    public string SourceColumn { get; set; } = string.Empty;
    public int SourceRow { get; set; } = 0;
    public string Transform { get; set; } = string.Empty; // uppercase, lowercase, trim, etc.
    public string Expression { get; set; } = string.Empty;
    public string AggregateFunction { get; set; } = string.Empty; // count, sum, avg, min, max
    public string InputKey { get; set; } = string.Empty;  // Add this line
}

public class GlobalSettings
{
    public int Timeout { get; set; } = 30;
    public string LogLevel { get; set; } = "Info";
    public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
    public ErrorHandling ErrorHandling { get; set; } = new();
}

public class ErrorHandling
{
    public bool ContinueOnError { get; set; } = false;
    public bool LogErrors { get; set; } = true;
    public bool EmailOnError { get; set; } = false;
}