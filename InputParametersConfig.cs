using System.Text.Json.Serialization;

namespace TransferJurnal;

public class InputParametersConfig
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();

    [JsonPropertyName("comments")]
    public Dictionary<string, object>? Comments { get; set; }
}