using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using SkyPanel.Components.Models.Auth0;
using SkyPanel.Utils;

namespace SkyPanel.Components.Pages;

public partial class RoleManagement
{
    private readonly ILogger<RoleManagement> _logger;
    
    public RoleManagement (ILogger<RoleManagement> logger)
    {
        _logger = logger;
    }

    [CascadingParameter] 
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    
    private List<User> _users = new();
    private List<User> _filteredUsers => _users
        .Where(u => string.IsNullOrWhiteSpace(_searchString) ||
                    u.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
        .ToList();

    private string _searchString = "";
    private User? _selectedUser;
    private List<Role> _userRoles = new();
    private List<Role> _originalUserRoles = new();
    private List<Role> _allRoles = new();
    private List<Role> _assignedRoles => _userRoles.ToList();
    private List<Role> _availableRoles => _allRoles
        .Where(r => _userRoles.All(ur => ur.id != r.id))
        .ToList();
    
    private bool _isLoadingUsers = true;
    private bool _isLoadingRoles = false;
    private bool _rolesChanged = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadUsers();
        await LoadAllRoles();
    }

    private async Task LoadUsers()
    {
        _isLoadingUsers = true;
        _users = await OrchestratorClient.GetUsers();
        _isLoadingUsers = false;
    }

    private async Task LoadAllRoles()
    {
        _allRoles = await OrchestratorClient.GetRoles();
    }

    private async Task SelectUser(User user)
    {
        _selectedUser = user;
        await LoadUserRoles();
    }

    private async Task LoadUserRoles()
    {
        if (_selectedUser == null) return;

        _isLoadingRoles = true;
        _userRoles = await OrchestratorClient.GetUserRoles(_selectedUser.UserId);
        _originalUserRoles = new List<Role>(_userRoles);
        _rolesChanged = false;
        _isLoadingRoles = false;
    }

    private void AddRole(Role role)
    {
        if (_userRoles.All(r => r.id != role.id))
        {
            _userRoles.Add(role);
            UpdateRolesChangedState();
        }
    }

    private void RemoveRole(Role role)
    {
        if (_userRoles.Any(r => r.id == role.id))
        {
            _userRoles.RemoveAll(r => r.id == role.id);
            UpdateRolesChangedState();
        }
    }
    private void UpdateRolesChangedState()
    {
        // Check if the current roles are different from the original roles
        if (_userRoles.Count != _originalUserRoles.Count)
        {
            _rolesChanged = true;
            return;
        }

        // Check if all roles in _userRoles exist in _originalUserRoles with the same id
        foreach (var role in _userRoles)
        {
            if (_originalUserRoles.All(r => r.id != role.id))
            {
                _rolesChanged = true;
                return;
            }
        }

        // Check if all roles in _originalUserRoles exist in _userRoles with the same id
        foreach (var role in _originalUserRoles)
        {
            if (_userRoles.All(r => r.id != role.id))
            {
                _rolesChanged = true;
                return;
            }
        }

        // If we get here, the collections have the same roles (by id)
        _rolesChanged = false;
    }

    private async Task SaveUserRoles()
    {
        if (_selectedUser == null) return;
        bool success = true;
        
        // Find roles that need to be added
        var rolesToAddObjects = _userRoles
            .Where(r => _originalUserRoles.All(or => or.id != r.id))
            .ToList();

        var rolesToAdd = rolesToAddObjects.Select(r => r.id).ToArray();
        var auditRoleNamesToAdd = rolesToAddObjects.Select(r => r.name).ToArray();

        // Find roles that need to be removed
        var rolesToRemoveObjects = _originalUserRoles
            .Where(r => _userRoles.All(ur => ur.id != r.id))
            .ToList();

        var rolesToRemove = rolesToRemoveObjects.Select(r => r.id).ToArray();
        var auditRoleNamesToRemove = rolesToRemoveObjects.Select(r => r.name).ToArray();
        
        if (rolesToAdd.Length > 0)
        {
            var addRoleData = new RoleData { roles = rolesToAdd };
            var addSuccess = await OrchestratorClient.UpdateUserRoles(_selectedUser.UserId, addRoleData);
            if (!addSuccess)
            {
                Snackbar.Add("Failed to add roles", Severity.Error);
                success = false;
            }
        }
        
        // Remove roles if needed
        if (rolesToRemove.Length > 0)
        {
            var removeRoleData = new RoleData { roles = rolesToRemove };
            var removeSuccess = await OrchestratorClient.RemoveUserRole(_selectedUser.UserId, removeRoleData);
            if (!removeSuccess)
            {
                Snackbar.Add("Failed to remove roles", Severity.Error);
                success = false;
            }
        }
        if (success)
        {
            
            var authState = await AuthenticationStateTask;
            var authUser = authState.User;
            var user = RoleUtil.GetUserEmail(authUser);
            
            // Detailed entry for adding roles
            foreach (var role in auditRoleNamesToAdd)
            {
                _logger.LogInformation( "[AUDIT] {User} added role {Role} to {TargetUser}", user, role, _selectedUser.Email);
            }
            
            // Detailed entry for removing roles
            foreach (var role in auditRoleNamesToRemove)
            {
                _logger.LogInformation( "[AUDIT] {User} removed role {Role} from {TargetUser}", user, role, _selectedUser.Email);
            }
            
            Snackbar.Add("User roles updated successfully", Severity.Success);
            _originalUserRoles = new List<Role>(_userRoles);
            _rolesChanged = false;
            StateHasChanged();
        }
    }
}