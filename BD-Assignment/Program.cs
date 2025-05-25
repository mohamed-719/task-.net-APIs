
using BD_Assignment.Repositories.Interfaces;
using BD_Assignment.Repositories;
using BD_Assignment.Services;
using Microsoft.OpenApi.Models;

namespace BD_Assignment;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Country Block API",
                Version = "v1",
                Description = "API for managing blocked countries and IP validation"
            });
        });
        builder.Services.AddHealthChecks();
        builder.Services.AddHttpClient<IIpLookupService, IpLookupService>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://ipapi.co");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        // Register repositories
        builder.Services.AddSingleton<ICountryBlockRepository, CountryBlockRepository>();
        builder.Services.AddSingleton<IBlockAttemptLogRepository, BlockAttemptLogRepository>();

        // Register services
        builder.Services.AddHttpClient<IIpLookupService, IpLookupService>(client =>
        {
            client.BaseAddress = new Uri("https://ipapi.co");
            client.Timeout = TimeSpan.FromSeconds(10);
        }); builder.Services.AddHostedService<TemporalBlockCleanupService>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();
        app.MapHealthChecks("/health");
        app.Run();
    }
}
