// BD-Assignment.Tests/Repositories/CountryBlockRepositoryTests.cs
using BD_Assignment.Models;
using BD_Assignment.Repositories;
using Xunit;

namespace BD_Assignment.Tests.Repositories;

public class CountryBlockRepositoryTests
{
    private readonly CountryBlockRepository _repository = new();

    [Fact]
    public async Task BlockCountry_ShouldAddNewCountry()
    {
        // Act
        var result = await _repository.BlockCountryAsync("US");

        // Assert
        Assert.True(result);
        Assert.True(await _repository.IsCountryBlockedAsync("US"));
    }

    [Fact]
    public async Task BlockCountry_ShouldRejectDuplicate()
    {
        // Arrange
        await _repository.BlockCountryAsync("GB");

        // Act
        var result = await _repository.BlockCountryAsync("GB");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UnblockCountry_ShouldRemoveExisting()
    {
        // Arrange
        await _repository.BlockCountryAsync("FR");

        // Act
        var result = await _repository.UnblockCountryAsync("FR");

        // Assert
        Assert.True(result);
        Assert.False(await _repository.IsCountryBlockedAsync("FR"));
    }

    [Fact]
    public async Task TemporalBlock_ShouldExpire()
    {
        // Arrange - block for 1 minute
        await _repository.AddTemporalBlockAsync("DE", 1);

        // Act & Assert - immediately after adding
        Assert.True(await _repository.IsCountryBlockedAsync("DE"));

        // Wait 1 minute (in real tests, consider using mocked time)
        await Task.Delay(TimeSpan.FromMinutes(1.1));

        // Should be expired now
        Assert.False(await _repository.IsCountryBlockedAsync("DE"));
    }
}