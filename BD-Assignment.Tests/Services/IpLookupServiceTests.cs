using BD_Assignment.Models;
using BD_Assignment.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BD_Assignment.Tests.Services;

public class IpLookupServiceTests
{
    [Fact]
    public async Task GetCountryFromIpAsync_ReturnsValidResponse()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                @"{
                    ""ip"": ""8.8.8.8"",
                    ""country_code"": ""US"",
                    ""country_name"": ""United States"",
                    ""isp"": ""Google LLC""
                }")
        };

        mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new System.Uri("https://ipapi.co/")
        };

        var loggerMock = new Mock<ILogger<IpLookupService>>();

        // Now only 2 dependencies (HttpClient and ILogger)
        var service = new IpLookupService(httpClient, loggerMock.Object);

        // Act
        var result = await service.GetCountryFromIpAsync("8.8.8.8");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("US", result.CountryCode);
        Assert.Equal("United States", result.CountryName);
        Assert.Equal("Google LLC", result.ISP);
    }

    [Fact]
    public async Task GetCountryFromIpAsync_ThrowsOnApiError()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new System.Uri("https://ipapi.co/")
        };

        var loggerMock = new Mock<ILogger<IpLookupService>>();
        var service = new IpLookupService(httpClient, loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.GetCountryFromIpAsync("8.8.8.8"));
    }
}