using BD_Assignment.Models;
using BD_Assignment.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BD_Assignment.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    private readonly ICountryBlockRepository _repository;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(ICountryBlockRepository repository, ILogger<CountriesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    [HttpPost("block")]
    public async Task<IActionResult> BlockCountry([FromBody] string countryCode)
    {
        if (!IsValidCountryCode(countryCode))
            return BadRequest("Invalid country code");

        var result = await _repository.BlockCountryAsync(countryCode);
        return result ? Ok() : Conflict("Country already blocked");
    }

    [HttpDelete("block/{countryCode}")]
    public async Task<IActionResult> UnblockCountry(string countryCode)
    {
        var result = await _repository.UnblockCountryAsync(countryCode);
        return result ? Ok() : NotFound("Country not blocked");
    }

    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlockedCountries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = null)
    {
        var countries = await _repository.GetAllBlockedAsync(page, pageSize, search);
        return Ok(countries);
    }

    [HttpPost("temporal-block")]
    public async Task<IActionResult> AddTemporalBlock([FromBody] TemporalBlockRequest request)
    {
        if (!IsValidCountryCode(request.CountryCode))
            return BadRequest("Invalid country code");

        if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
            return BadRequest("Duration must be between 1 and 1440 minutes");

        try
        {
            var result = await _repository.AddTemporalBlockAsync(request.CountryCode, request.DurationMinutes);
            return result ? Ok() : Conflict("Country already temporarily blocked");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding temporal block");
            return StatusCode(500, "Internal server error");
        }
    }

    private bool IsValidCountryCode(string code) =>
        code.Length == 2 && code.All(char.IsLetter);
}

