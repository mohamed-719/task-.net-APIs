using BD_Assignment.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BD_Assignment.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LogsController : ControllerBase
{
    private readonly IBlockAttemptLogRepository _logRepository;
    private readonly ILogger<LogsController> _logger;

    public LogsController(
        IBlockAttemptLogRepository logRepository,
        ILogger<LogsController> logger)
    {
        _logRepository = logRepository;
        _logger = logger;
    }
    [HttpGet("blocked-attempts")]
    public async Task<IActionResult> GetBlockedAttempts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var logs = await _logRepository.GetAllAsync(page, pageSize);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blocked attempts");
            return StatusCode(500, "Internal server error");
        }
    }
}
