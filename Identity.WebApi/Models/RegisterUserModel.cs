namespace Identity.WebApi.Models;

public record RegisterUserModel(string Email, string Password, string FirstName, string LastName);
