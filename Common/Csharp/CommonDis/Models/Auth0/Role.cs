namespace CommonDis.Models.Auth0;

public class Role(string id, string name, string description)
{
    public string id { get; } = id;
    public string name { get; } = name;
    public string description { get; } = description;
}