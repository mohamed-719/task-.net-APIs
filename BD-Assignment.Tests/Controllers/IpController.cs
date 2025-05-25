using BD_Assignment.Models;
using BD_Assignment.Repositories.Interfaces;
using BD_Assignment.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Net;

namespace BD_Assignment.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IpController : ControllerBase
{
    private readonly IIpLookupService _ipLookupService;
    private readonly ICountryBlockRepository _countryRepository;
    private readonly IBlockAttemptLogRepository _logRepository;
    private readonly ILogger<IpController> _logger;

    public IpController(
        IIpLookupService ipLookupService,
        ICountryBlockRepository countryRepository,
        IBlockAttemptLogRepository logRepository,
        ILogger<IpController> logger)
    {
        _ipLookupService = ipLookupService;
        _countryRepository = countryRepository;
        _logRepository = logRepository;
        _logger = logger;
    }

    /// <summary>
    /// Looks up country information for a given IP address
    /// </summary>
    /// <param name="ipAddress">The IP address to lookup (optional - uses caller IP if omitted)</param>
    /// <returns>Geolocation information including country code, name, and ISP</returns>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(GeolocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress = null)
    {
        try
        {
            ipAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return BadRequest("IP address could not be determined");
            }

            if (!_ipLookupService.IsValidIp(ipAddress))
            {
                return BadRequest("Invalid IP address format");
            }

            var geoInfo = await _ipLookupService.GetCountryFromIpAsync(ipAddress);
            return Ok(geoInfo);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling geolocation API for IP {IpAddress}", ipAddress);
            return StatusCode(StatusCodes.Status502BadGateway, "Geolocation service unavailable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error looking up IP {IpAddress}", ipAddress);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Checks if the caller's IP address is from a blocked country
    /// </summary>
    /// <returns>Block status and country information</returns>
    [HttpGet("check-block")]
    [ProducesResponseType(typeof(BlockCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckBlock()
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return BadRequest("IP address could not be determined");
            }

            var geoInfo = await _ipLookupService.GetCountryFromIpAsync(ipAddress);
            var isBlocked = await _countryRepository.IsCountryBlockedAsync(geoInfo.CountryCode);

            // Log the attempt
            await _logRepository.AddAsync(new BlockAttemptLog
            {
                IpAddress = ipAddress,
                CountryCode = geoInfo.CountryCode,
                IsBlocked = isBlocked,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            });

            return Ok(new BlockCheckResponse
            {
                IsBlocked = isBlocked,
                CountryCode = geoInfo.CountryCode,
                CountryName = geoInfo.CountryName,
                IpAddress = ipAddress
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking IP block status");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
}

public class BlockCheckResponse
{
    public bool IsBlocked { get; set; }
    public string CountryCode { get; set; }
    public string CountryName { get; set; }
    public string IpAddress { get; set; }
}