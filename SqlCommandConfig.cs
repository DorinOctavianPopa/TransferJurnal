namespace TransferJurnal;

public class SqlCommandConfig
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
    public List<SqlParameterConfig> Parameters { get; set; } = new();
}

public class SqlParameterConfig
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class CommandsConfig
{
    public List<SqlCommandConfig> Commands { get; set; } = new();
}
