using AzureAI.WebAccessibilityTool.Services;
using Microsoft.OpenApi.Models;

namespace AzureAI.WebAccessibilityTool.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure services
        builder.Services.AddControllers();
        builder.Services.AddScoped<AccessibilityAnalyzer>(); // Business service injection

        // Configure Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Web Accessibility Tool API",
                Version = "v1",
                Description = "API for analyzing website accessibility using Azure AI services.",
                Contact = new OpenApiContact
                {
                    Name = "Fermin Piccolo",
                    Email = "frmpiccolo@gmail.com"
                }
            });
        });

        var app = builder.Build();

        // Configure middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Accessibility Tool API v1");
                c.RoutePrefix = string.Empty; // Swagger UI root
            });
        }

        app.UseRouting();
        app.MapControllers();

        app.Run();
    }
}
