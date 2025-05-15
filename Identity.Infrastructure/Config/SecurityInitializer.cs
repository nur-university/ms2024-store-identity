using Identity.Infrastructure.Persistence.StoredModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Identity.Infrastructure.Config;

internal class SecurityInitializer(ILogger<SecurityInitializer> logger,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    InitializerJsonUser baseConfiguration) : ISecurityInitializer
{
    private readonly ILogger<SecurityInitializer> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly InitializerJsonUser _baseConfiguration = baseConfiguration;

    public async Task Initialize(string permissionJsonFilePath, string securityInitializationJsonFilePath)
    {
        _logger.LogInformation("=========== Starting Application Initialization Check =============");

        var permissions = InitializePermissions(permissionJsonFilePath);

        string initializerJson = "";
        try
        {
            initializerJson = System.IO.File.ReadAllText(securityInitializationJsonFilePath);
            _logger.LogDebug("The file with the menu configuration has been read: {length} chars", initializerJson.Length);
        }
        catch (Exception q)
        {
            _logger.LogError(q, "The menu configuration couldnot be loaded, maybe missing file or malformed");
        }

        InitializerJsonConfig? initJsonObj = JsonConvert.DeserializeObject<InitializerJsonConfig>(initializerJson);
        if (initJsonObj == null)
        {
            _logger.LogError("The configuration for the inisitailization is not well formed, skipping");
            return;
        }

        // Creates the users
        initJsonObj.defaultUser ??=  new InitializerJsonUser();
        initJsonObj.defaultUser.user = _baseConfiguration.user;
        initJsonObj.defaultUser.password = _baseConfiguration.password;
        initJsonObj.defaultUser.email = _baseConfiguration.email;
        initJsonObj.defaultUser.firstName = _baseConfiguration.firstName;
        initJsonObj.defaultUser.lastName = _baseConfiguration.lastName;
        initJsonObj.defaultUser.phone = _baseConfiguration.phone;
        initJsonObj.defaultUser.jobtitle = _baseConfiguration.jobtitle;

        bool errorConnectingDatabase = await InitializeUsers(initJsonObj);

        if (errorConnectingDatabase)
        {
            _logger.LogError("Skipping all initialization because could not connect to database");
            return;
        }

        // Creates the roles              
        await InitializeRoles(initJsonObj);

        await AssignPermissionsToRoles(initJsonObj, permissions);

        await AssignRolesToUsers(initJsonObj);

    }

    private async Task AssignRolesToUsers(InitializerJsonConfig initJsonObj)
    {
        //Asigna los roles a los usuarios en caso que no los tenga
        //Get User admin
        var userJson = initJsonObj.defaultUser;
        
        ApplicationUser? user = await _userManager.FindByNameAsync(userJson.user);

        foreach (var roleJson in userJson.userroles)
        {
            string roleName = roleJson.role.ToString();
            ApplicationRole? identyRole = await _roleManager.FindByNameAsync(roleName);
            IList<ApplicationUser> usersInRole = await _userManager.GetUsersInRoleAsync(identyRole!.Name!);

            if (!usersInRole.Any(x => x.Id == user!.Id!))
            {
                await _userManager.AddToRoleAsync(user!, identyRole.Name!);
                _logger.LogInformation("Added role { RoleName } to User { UserName }", identyRole.Name, user!.UserName);
            }
        }
        
    }

    private Dictionary<string, ApplicationPermission> InitializePermissions(string permissionJsonFilePath)
    {
        string permissionsJson = "";
        try
        {
            permissionsJson = System.IO.File.ReadAllText(permissionJsonFilePath);
            _logger.LogDebug("The file with the permission configuration has been read: {length} chars", permissionsJson.Length);
        }
        catch (Exception q)
        {
            _logger.LogError(q, "The permission configuration could not be loaded, maybe missing file or malformed");
        }

        try
        {
            var permission = ReadPermissions(permissionsJson);
            _logger.LogDebug("Permissions was loaded");
            return permission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Permissions has not been loaded");
        }
        return [];
    }

    private async Task<bool> InitializeUsers(InitializerJsonConfig initJsonObj)
    {

        // Creates the users
        var userJson = initJsonObj.defaultUser;
        
        string userName = userJson.user;
        if (string.IsNullOrWhiteSpace(userName))
        {
            _logger.LogError("User entry in initializer json config file is not well formed, skipping user");
            return true;
        }

        ApplicationUser? user = null;
        try
        {
            user = await _userManager.FindByNameAsync(userName);
        }
        catch (Exception q)
        {
            _logger.LogError(q, "Error obtaining users from the database");
            return true;
        }

        if (user == null)
        {
            // User doesn't exist so we create it
            user = new ApplicationUser(userJson.user, userJson.firstName, userJson.lastName, true, true);
            user.Email = userJson.email;
            user.EmailConfirmed = true;

            IdentityResult userCreated = await _userManager.CreateAsync(user, userJson.password.ToString());
            if (!userCreated.Succeeded)
            {
                _logger.LogError("Didn't create user {username}", user.UserName);
                return true; 
            }
            _logger.LogInformation("Created user {username}", user.UserName);
        }
        
        return false;
    }


    private async Task InitializeRoles(InitializerJsonConfig initJsonObj)
    {
        try
        {
            IdentityResult result;

            foreach (var role in initJsonObj.roles)
            {
                string rolName = role.role;
                if (string.IsNullOrWhiteSpace(rolName))
                {
                    _logger.LogError("The role name was not set or empty in Initializer Json Config file, skipping");
                    continue;
                }

                if (_roleManager.Roles.Any(x => x.Name == rolName))
                {
                    _logger.LogInformation("The role {rolName} is already created", rolName);
                    continue;
                }
                result = await _roleManager.CreateAsync(new ApplicationRole() { Name = rolName });
                if (result.Succeeded)
                    _logger.LogInformation("The new role {rolName} has been created", rolName);
                else
                    _logger.LogError("Error in the Identity module, first: {error}", string.Join(" | ", result.Errors));
            }
        }
        catch (Exception q)
        {
            _logger.LogError(q, "Error initializing the new roles ");
        }
    }

    private async Task AssignPermissionsToRoles(InitializerJsonConfig initJsonObj, Dictionary<string, ApplicationPermission> permissions)
    {
        // Assigns permissions to roles
        foreach (var role in initJsonObj.roles)
        {
            if(role.role == null)
            {
                continue;
            } 
            foreach (var permissionJson in role.permissions)
            {
                string permissionNmonic = permissionJson.permission.ToString();
                ApplicationPermission ap = permissions[permissionNmonic];
                string? rolName = role.role;

                ApplicationRole? objRole = await _roleManager.FindByNameAsync(rolName);
                IList<Claim> claimsInRole = await _roleManager.GetClaimsAsync(objRole!);
                if (claimsInRole.Any(x => x.Value.Equals(ap.Mnemonic.ToString())))
                    continue;
                
                Claim claim = new Claim(ap.Mnemonic, ap.Mnemonic.ToString());
                await _roleManager.AddClaimAsync(objRole!, claim);
                _logger.LogInformation("Added claim {claimName}", ap.Mnemonic);
            }
        }
    }

    /// <summary>
    /// Reads all permissions in JSON format to memory as a list of ApplicationPermission
    /// </summary>
    /// <param name="permissionsJson"></param>
    private static Dictionary<string, ApplicationPermission> ReadPermissions(string? permissionsJson)
    {
        List<PermissionJsonConfig>? permissionsJsonObject = JsonConvert.DeserializeObject<List<PermissionJsonConfig>>(permissionsJson);

        if (permissionsJsonObject == null)
            throw new ArgumentException("Could not read the permissions in the JSON configuration file");

        Dictionary<string, ApplicationPermission> permissions = [];
        foreach (var permission in permissionsJsonObject)
        {
            ApplicationPermission objPermission = new(permission.Mnemonic, permission.Name,
                permission.Description);

            permissions.Add(objPermission.Mnemonic, objPermission);
        }

        return permissions;
    }
}
