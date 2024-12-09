using System.Threading.Tasks;
using AzureAI.WebAccessibilityTool.API.Controllers;
using AzureAI.WebAccessibilityTool.API.Models;
using AzureAI.WebAccessibilityTool.Models;
using AzureAI.WebAccessibilityTool.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace AzureAI.WebAccessibilityTool.Tests.ApiTests;

/// <summary>
/// Test suite for the <see cref="AccessibilityController"/> class.
/// </summary>
public class AccessibilityControllerTests
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the test class and sets up mock configuration.
    /// </summary>
    public AccessibilityControllerTests()
    {
        // Arrange mock configuration
        var host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.Development.json");
                    })
                    .Build();

        _configuration = host.Services.GetRequiredService<IConfiguration>();
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeImage"/> method with a valid URL.
    /// </summary>
    [Fact]
    public async Task AnalyzeImage_ValidUrl_ReturnsOk()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);        
        var controller = new AccessibilityController(mockAnalyzer.Object);
        var testImageUrl = "https://cdn-dynmedia-1.microsoft.com/is/image/microsoftcorp/390938-comparing-models-acc1?resMode=sharp2&op_usm=1.5,0.65,15,0&wid=1600&hei=1272&qlt=100&fmt=png-alpha&fit=constrain";
        var testInput = new UrlInput { Url = testImageUrl };

        // Act
        var result = await controller.AnalyzeImage(testInput) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        List<string> results = result.Value as List<string> ?? new List<string>();
        Assert.True(results?.Count > 0);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtml"/> method with invalid HTML content.
    /// </summary>
    [Fact]
    public async Task AnalyzeHtml_EmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);
        var testInput = new HtmlInput { HtmlContent = string.Empty };

        // Act
        var result = await controller.AnalyzeHtml(testInput) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    /// </summary>
    [Fact]
    public async Task AnalyzeHtml_ValidContent_ReturnsNoAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);

        var validHtmlContent = @"<!DOCTYPE html>
                                        <html lang=""en"">
                                        <head>
                                            <meta charset=""UTF-8"">
                                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                            <meta name=""description"" content=""This is a test HTML document demonstrating proper accessibility practices."">
                                            <meta name=""author"" content=""Your Name"">
                                            <title>Accessibility Test</title>
                                            <style>
                                                /* Ensuring sufficient color contrast */
                                                body {
                                                    background-color: #ffffff;
                                                    color: #1a1a1a; /* Dark gray text for good contrast */
                                                    font-family: Arial, sans-serif;
                                                }
                                                a {
                                                    color: #0056b3; /* Blue for links */
                                                    text-decoration: underline;
                                                }
                                                a:focus, a:hover {
                                                    color: #003d80; /* Darker blue for hover/focus state */
                                                }
                                                figcaption {
                                                    color: #4d4d4d; /* Medium gray text for captions with sufficient contrast */
                                                }
                                            </style>
                                        </head>
                                        <body>
                                            <header>
                                                <h1>Accessibility Test Page</h1>
                                            </header>
                                            <main>
                                                <figure>
                                                    <img 
                                                        src=""https://cdn-dynmedia-1.microsoft.com/is/image/microsoftcorp/390938-comparing-models-acc1?resMode=sharp2&op_usm=1.5,0.65,15,0&wid=1600&hei=1272&qlt=100&fmt=png-alpha&fit=constrain"" 
                                                        alt=""A high-quality image showing a computer on a desk with multiple accessories, including a keyboard, mouse, and monitor displaying vivid colors and clear text."" 
                                                        width=""800"" 
                                                        height=""636""
                                                    />
                                                    <figcaption>
                                                        This image demonstrates a modern computer setup, featuring a clear visual presentation of accessories and text on the screen with appropriate contrast.
                                                    </figcaption>
                                                </figure>
                                                <section>
                                                    <p>
                                                        The text on this page has been designed to meet accessibility standards with a contrast ratio above the minimum requirement of 4.5:1 for normal text and 3:1 for large text. This ensures readability for all users.
                                                    </p>
                                                </section>
                                            </main>
                                            <footer>
                                                <p>
                                                    &copy; 2024 Accessibility Test. All rights reserved. 
                                                    <a href=""privacy-policy.html"">Read our Privacy Policy to understand how we handle your data and protect your privacy</a>.
                                                </p>
                                            </footer>
                                        </body>
                                        </html>";

        var testInput = new HtmlInput { HtmlContent = validHtmlContent };

        // Act
        var result = await controller.AnalyzeHtml(testInput) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        WCAGResult results = result.Value as WCAGResult ?? new WCAGResult { Items = new List<WCAGItem>(), Explanation = "" };
        Assert.True(results.Items.Where(e => !e.Severity.Equals("Low")).ToList().Count == 0);
    }

    /// </summary>
    [Fact]
    public async Task AnalyzeHtml_InvalidContent_ReturnsAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);

        var invalidHtmlContent = "<html><head><title>Test</title></head><body><img src='https://cdn-dynmedia-1.microsoft.com/is/image/microsoftcorp/390938-comparing-models-acc1?resMode=sharp2&op_usm=1.5,0.65,15,0&wid=1600&hei=1272&qlt=100&fmt=png-alpha&fit=constrain' /></body></html>";
        var testInput = new HtmlInput { HtmlContent = invalidHtmlContent };

        // Act
        // Act
        var result = await controller.AnalyzeHtml(testInput) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        WCAGResult results = result.Value as WCAGResult ?? new WCAGResult { Items = new List<WCAGItem>(), Explanation = "" };
        Assert.True(results.Items.Count > 0);
    }
}
