using Identity.Application.Events;
using Identity.Application.Services;
using Identity.Infrastructure.Persistence.StoredModel;
using Joseco.Outbox.Contracts.Model;
using Joseco.Outbox.Contracts.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Nur.Store2025.Security.Config;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Infrastructure.Services;

internal class SecurityService(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager,
    RoleManager<ApplicationRole> _roleManager, JwtOptions _jwtOptions, ILogger<SecurityService> _logger,
    IOutboxService<DomainEvent> outboxService, IUnitOfWork unitOfWork) : ISecurityService
{

    public async Task<Result<string>> Login(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        _logger.LogInformation("{username} is trying to login", username);
        if (user == null)
        {
            _logger.LogWarning("Username {username} is not registered", username);
            user = await _userManager.FindByEmailAsync(username);
            if (user == null)
            {
                _logger.LogWarning("Email {email} is not registered", username);
                return Result.Failure<string>(new Error("Login.Error", "Email or Password incorrect", ErrorType.Problem));
            }
        }

        if (!user.Active)
        {
            _logger.LogWarning("{username} is not active", username);
            return Result.Failure<string>(new Error("Login.Error", "Email or Password incorrect", ErrorType.Problem));
        }
        var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, true);
        if (signInResult.Succeeded)
        {
            _logger.LogInformation("{username} has logged in", username);
            var jwt = await GenerateJwt(user);
            return Result.Success<string>(jwt);
        }
        return Result.Failure<string>(new Error("Login.Error", "Email or Password incorrect", ErrorType.Problem)); ;
    }

    public async Task<Result<Guid>> RegisterUser(string email, string password, string firstname, string lastname)
    {
        _logger.LogInformation("{email} is trying to register", email);
        var newUser = new ApplicationUser(email, firstname, lastname, true, true);

        IdentityResult userCreated = await _userManager.CreateAsync(newUser, password);
        if (!userCreated.Succeeded)
        {
            string errorCode = null;
            string description = null;
            userCreated.Errors
                .ToList()
                .ForEach(error => {
                    _logger.LogError("Error { ErrorCode }: { Description }", error.Code, error.Description);
                    errorCode = errorCode ?? error.Code;
                    description = description ?? error.Description;
                    });

            return Result.Failure<Guid>(new Error($"RegisterUser.Error.{errorCode}", description, ErrorType.Failure));
        }        

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
        
        IdentityResult result = await _userManager.ConfirmEmailAsync(newUser, token);
        if (!result.Succeeded)
        {

            //await _userManager.AddToRolesAsync(newUser, L);

           
        }

        UserCreated userCreatedEvent = new(newUser.Id, newUser.FullName, newUser.FullName);
        OutboxMessage<DomainEvent> outboxMessage = new(userCreatedEvent);

        await outboxService.AddAsync(outboxMessage);
        await unitOfWork.CommitAsync();

        return Result.Success(newUser.Id);



    }

    private async Task<string> GenerateJwt(ApplicationUser user)
    {
        _logger.LogInformation("Generating JWT for user {username}", user.UserName);
        var authClaims = new List<Claim>
                    {
                        new(ClaimTypes.Name, user.FullName),
                        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    };

        var claims = await _userManager.GetClaimsAsync(user);
        HashSet<string> scope = new HashSet<string>();
        foreach (var item in claims)
        {
            scope.Add(item.Type);
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRoleName in userRoles)
        {
            var userRole = await _roleManager.FindByNameAsync(userRoleName);
            var listOfClaims = await _roleManager.GetClaimsAsync(userRole);

            foreach (var item in listOfClaims)
            {
                scope.Add(item.Type);
            }
        }

        string userScope = string.Join(" ", scope);

        authClaims.Add(new Claim("isStaff", user.Staff.ToString()));
        authClaims.Add(new Claim("scope", userScope));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var lifeTime = _jwtOptions.ValidateLifetime ? _jwtOptions.Lifetime : 60 * 24 * 365;

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.ValidateIssuer ? _jwtOptions.ValidIssuer : null,
            audience: _jwtOptions.ValidateAudience ? _jwtOptions.ValidAudience : null,
            claims: authClaims,
            expires: DateTime.Now.AddMinutes(lifeTime),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
