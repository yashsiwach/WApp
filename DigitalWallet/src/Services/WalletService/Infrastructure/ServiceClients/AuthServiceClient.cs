using System.Net.Http.Headers;
using System.Text.Json;
using WalletService.Application.Interfaces;

namespace WalletService.Infrastructure.ServiceClients;

/// <summary>
/// HTTP client implementation of IAuthServiceClient that calls the AuthService REST API.
/// </summary>
public class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<AuthServiceClient> _logger;

    /// <summary>
    /// Initializes the client with a pre-configured HttpClient and logger.
    /// </summary>
    public AuthServiceClient(HttpClient http, ILogger<AuthServiceClient> logger)
    {
        _http   = http;
        _logger = logger;
    }

    /// <summary>
    /// Queries the AuthService user lookup endpoint and returns the user's ID for the given email.
    /// </summary>
    public async Task<Guid?> GetUserIdByEmailAsync(string email, string bearerToken)
    {
        try
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await _http.GetAsync($"/api/auth/users/lookup?email={Uri.EscapeDataString(email)}");

            if (!response.IsSuccessStatusCode)return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var userIdStr = doc.RootElement
                .GetProperty("data")
                .GetProperty("userId")
                .GetString();

            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve userId for email {Email}", email);
            return null;
        }
    }
}
