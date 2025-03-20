using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DownloadOrchestrator.Models;

[JsonObject]
public class DisSecretModel
{
    [JsonInclude]
    public Dictionary<string, DisSecret> Secrets { get; set; }
}