using BD_Assignment.Repositories.Interfaces;

namespace BD_Assignment.Services;

public class TemporalBlockCleanupService : BackgroundService
{
    private readonly ICountryBlockRepository _repository;
    private readonly ILogger<TemporalBlockCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public TemporalBlockCleanupService(ICountryBlockRepository repository, ILogger<TemporalBlockCleanupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _repository.RemoveExpiredTemporalBlocksAsync();
                _logger.LogInformation("Cleaned up expired temporal blocks");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up temporal blocks");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
