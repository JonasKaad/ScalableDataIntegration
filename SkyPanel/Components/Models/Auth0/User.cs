using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkyPanel.Components.Models.Auth0;

public class User
{
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("identities")]
    public List<Identity> Identities { get; set; }
    
    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }
    
    [JsonPropertyName("picture")]
    public string Picture { get; set; }
    
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("last_login")]
    public DateTime LastLogin { get; set; }
    
    [JsonPropertyName("last_ip")]
    public string LastIp { get; set; }
    
    [JsonPropertyName("logins_count")]
    public int LoginsCount { get; set; }
}

public class Identity
{
    [JsonPropertyName("connection")]
    public string Connection { get; set; }
    
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("provider")]
    public string Provider { get; set; }
    
    [JsonPropertyName("isSocial")]
    public bool IsSocial { get; set; }
}