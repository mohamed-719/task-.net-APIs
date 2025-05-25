using BD_Assignment.Models;
using BD_Assignment.Repositories.Interfaces;
using System.Collections.Concurrent;
using System.Globalization;

namespace BD_Assignment.Repositories;

public class CountryBlockRepository :  ICountryBlockRepository
{
    private readonly ConcurrentDictionary<string, BlockedCountry> _permanentBlocks = new();
    private readonly ConcurrentDictionary<string, TemporalBlock> _temporalBlocks = new();
    private readonly ConcurrentQueue<BlockAttemptLog> _blockAttemptLogs = new();

    public Task<bool> BlockCountryAsync(string countryCode)
    {
        if (_permanentBlocks.ContainsKey(countryCode) || _temporalBlocks.ContainsKey(countryCode))
            return Task.FromResult(false);

        return Task.FromResult(_permanentBlocks.TryAdd(countryCode, new BlockedCountry
        {
            CountryCode = countryCode,
            CountryName = GetCountryName(countryCode)
        }));
    }

    public Task<bool> UnblockCountryAsync(string countryCode)
    {
        bool removed = _permanentBlocks.TryRemove(countryCode, out _) ||
                      _temporalBlocks.TryRemove(countryCode, out _);
        return Task.FromResult(removed);
    }

    public Task<IEnumerable<BlockedCountry>> GetAllBlockedAsync(int page = 1, int pageSize = 10, string search = null)
    {
        var allBlocks = _permanentBlocks.Values
            .Concat(_temporalBlocks.Values)
            .Where(b => search == null ||
                       b.CountryCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                       b.CountryName.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return Task.FromResult(allBlocks);
    }

    public Task<bool> IsCountryBlockedAsync(string countryCode)
    {
        if (_permanentBlocks.ContainsKey(countryCode))
            return Task.FromResult(true);

        if (_temporalBlocks.TryGetValue(countryCode, out var temporalBlock))
            return Task.FromResult(temporalBlock.ExpiryTime > DateTime.UtcNow);

        return Task.FromResult(false);
    }

    public Task<bool> AddTemporalBlockAsync(string countryCode, int durationMinutes)
    {
        if (durationMinutes < 1 || durationMinutes > 1440)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));

        if (_temporalBlocks.ContainsKey(countryCode))
            return Task.FromResult(false);

        return Task.FromResult(_temporalBlocks.TryAdd(countryCode, new TemporalBlock
        {
            CountryCode = countryCode,
            CountryName = GetCountryName(countryCode),
            ExpiryTime = DateTime.UtcNow.AddMinutes(durationMinutes)
        }));
    }

    public Task RemoveExpiredTemporalBlocksAsync()
    {
        var expired = _temporalBlocks.Where(kvp => kvp.Value.ExpiryTime <= DateTime.UtcNow)
                                   .Select(kvp => kvp.Key)
                                   .ToList();

        foreach (var key in expired)
        {
            _temporalBlocks.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    private string GetCountryName(string countryCode)
    {
        // Implement country name lookup
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .FirstOrDefault(c => c.Name.EndsWith($"-{countryCode}"))?
            .DisplayName ?? countryCode;
    }
}
