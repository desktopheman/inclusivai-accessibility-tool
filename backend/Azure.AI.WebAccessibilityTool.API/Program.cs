using AzureAI.WebAccessibilityTool.Services;
using Microsoft.OpenApi.Models;

namespace AzureAI.WebAccessibilityTool.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddScoped<AccessibilityAnalyzer>(); // Business service injection

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "InclusivAI: Accessibility Verification Tool",
                Version = "v1",
                Description = "InclusivAI is a powerful tool designed to verify and improve accessibility for specific URLs, HTML, and PDF files. Leveraging Azure AI services, InclusivAI ensures compliance with major accessibility standards",
                Contact = new OpenApiContact
                {
                    Name = "Fermin Piccolo",
                    Email = "frmpiccolo@gmail.com"
                }
            });
        });

        var allowedHosts = builder.Configuration.GetSection("AllowedHosts");
        string allowedHostsString = allowedHosts.Value ?? "";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp",
                policy =>
                {
                    policy.WithOrigins(allowedHostsString)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
        });

        var app = builder.Build();

        app.UseCors("AllowReactApp");

        //if (app.Environment.IsDevelopment())
        //{
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Accessibility Tool API v1");
            c.RoutePrefix = string.Empty;
        });
        //}

        app.UseRouting();
        app.MapControllers();

        app.Run();
    }
}
