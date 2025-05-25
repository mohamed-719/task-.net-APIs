using BD_Assignment.Models;

namespace BD_Assignment.Repositories.Interfaces;

public interface ICountryBlockRepository
{
    Task<bool> BlockCountryAsync(string countryCode);
    Task<bool> UnblockCountryAsync(string countryCode);
    Task<IEnumerable<BlockedCountry>> GetAllBlockedAsync(int page = 1, int pageSize = 10, string search = null);
    Task<bool> IsCountryBlockedAsync(string countryCode);
    Task<bool> AddTemporalBlockAsync(string countryCode, int durationMinutes);
    Task RemoveExpiredTemporalBlocksAsync();
}
