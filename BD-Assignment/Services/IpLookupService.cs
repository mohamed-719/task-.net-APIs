using BD_Assignment.Models;
using BD_Assignment.Repositories.Interfaces;
using System.Net;
using System.Text.Json;

namespace BD_Assignment.Services;

public interface IIpLookupService
{
    Task<GeolocationResponse> GetCountryFromIpAsync(string ipAddress);
    bool IsValidIp(string ipAddress);
}

public class IpLookupService : IIpLookupService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IpLookupService> _logger;
    private const string BasePath = "/json/";

    public IpLookupService(HttpClient httpClient, ILogger<IpLookupService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public bool IsValidIp(string ipAddress) => IPAddress.TryParse(ipAddress, out _);

    public async Task<GeolocationResponse> GetCountryFromIpAsync(string ipAddress)
    {
        try
        {
            // Free endpoint doesn't require an API key
            var response = await _httpClient.GetAsync($"{BasePath}{ipAddress}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("IP lookup failed with status {StatusCode}: {Error}",
                    response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode(); 
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GeolocationResponse>(content);

            if (result == null)
            {
                throw new Exception("Failed to deserialize geolocation response");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error looking up IP {IpAddress}", ipAddress);
            throw new ServiceException("Geolocation service unavailable", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse geolocation response for IP {IpAddress}", ipAddress);
            throw new ServiceException("Invalid geolocation data received", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error looking up IP {IpAddress}", ipAddress);
            throw;
        }
    }
}

public class ServiceException : Exception
{
    public ServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}