using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Http;

namespace RentCar.Controllers;

/// <summary>
/// Simple login endpoint that returns a JWT access token.
/// It forwards the credentials to the OpenIddict token endpoint (/connect/token)
/// of this same application, using the password flow.
/// Registration is done with the built-in POST /api/account/register endpoint.
/// </summary>
[RemoteService]
[Route("api/auth")]
[IgnoreAntiforgeryToken]
public class AuthController : AbpControllerBase
{
    // Seeded OpenIddict client that allows the password flow (created by the project template)
    private const string ClientId = "RentCar_App";
    private const string Scope = "RentCar";

    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultDto>> LoginAsync([FromBody] LoginInput input)
    {
        var tokenUrl = $"{Request.Scheme}://{Request.Host}/connect/token";

        var client = _httpClientFactory.CreateClient();
        using var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = ClientId,
                ["username"] = input.UserName,
                ["password"] = input.Password,
                ["scope"] = Scope
            }));

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            // Never log the password
            Logger.LogWarning("Failed login attempt for user '{UserName}'. Token endpoint answered: {Response}",
                input.UserName, json);

            // Return 401 with a JSON body directly (throwing an authorization exception would
            // make ABP redirect the client to the HTML login page instead).
            return Unauthorized(new RemoteServiceErrorResponse(
                new RemoteServiceErrorInfo { Message = "Invalid username or password." }));
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new LoginResultDto
        {
            AccessToken = root.GetProperty("access_token").GetString()!,
            TokenType = root.GetProperty("token_type").GetString()!,
            ExpiresIn = root.GetProperty("expires_in").GetInt32()
        };
    }
}

public class LoginInput
{
    [Required]
    public string UserName { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}

public class LoginResultDto
{
    public string AccessToken { get; set; } = default!;

    public string TokenType { get; set; } = default!;

    /// <summary>Lifetime of the token in seconds.</summary>
    public int ExpiresIn { get; set; }
}
