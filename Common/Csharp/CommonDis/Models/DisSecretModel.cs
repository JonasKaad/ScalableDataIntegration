using System.Text.Json.Serialization;

namespace CommonDis.Models;

public class DisSecretModel
{
    [JsonInclude]
    public Dictionary<string, DisSecret> Secrets { get; set; }
}