using BD_Assignment.Models;
using BD_Assignment.Repositories.Interfaces;
using BD_Assignment.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BD_Assignment.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IpController : ControllerBase
{
    private readonly IIpLookupService _ipLookup;
    private readonly ICountryBlockRepository _countryRepo;
    private readonly IBlockAttemptLogRepository _logRepo;
    private readonly ILogger<IpController> _logger;
    public IpController(IIpLookupService ipLookup, ICountryBlockRepository countryRepo, IBlockAttemptLogRepository logRepo, ILogger<IpController> logger)
    {
        _ipLookup = ipLookup;
        _countryRepo = countryRepo;
        _logRepo = logRepo;
        _logger = logger;
    }
    [HttpGet("lookup")]
    public async Task<IActionResult> LookupIp([FromQuery] string ipAddress = null)
    {
        ipAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();

        if (!_ipLookup.IsValidIp(ipAddress))
            return BadRequest("Invalid IP address");

        try
        {
            var geoInfo = await _ipLookup.GetCountryFromIpAsync(ipAddress);
            return Ok(geoInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up IP");
            return StatusCode(500, "Error looking up IP");
        }
    }

    [HttpGet("check-block")]
    public async Task<IActionResult> CheckBlock()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        try
        {
            var geoInfo = await _ipLookup.GetCountryFromIpAsync(ipAddress);
            var isBlocked = await _countryRepo.IsCountryBlockedAsync(geoInfo.CountryCode);

            await _logRepo.AddAsync(new BlockAttemptLog
            {
                IpAddress = ipAddress,
                CountryCode = geoInfo.CountryCode,
                IsBlocked = isBlocked,
                UserAgent = userAgent
            });

            return Ok(new { IsBlocked = isBlocked, Country = geoInfo.CountryCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking IP block");
            return StatusCode(500, "Internal server error");
        }
    }
}
