using System.Security.Claims;

namespace SkyPanel.Utils;

public static class RoleUtil
{
    /// <summary>
    /// Allows checking and comparing user roles and permissions, regardless of string casing.
    /// </summary>
    /// <summary>
    /// Checks if a user has a specific role that matches the given parser name.
    /// </summary>
    /// <param name="parserName">The name of the parser/downloader to check against user roles</param>
    /// <param name="user">The user whose roles will be checked</param>
    /// <returns>True if the user has a role matching the parser name (case-insensitive), otherwise false</returns>
    public static bool HasRole(string parserName, ClaimsPrincipal user)
    {   
        var userRoles = user.Claims
            .Where(c => c.Type.Equals(ClaimTypes.Role))
            .Select(c => c.Value)
            .ToList();
        
        return userRoles.Any(role =>
            string.Equals(role, parserName, StringComparison.OrdinalIgnoreCase));
    }
}