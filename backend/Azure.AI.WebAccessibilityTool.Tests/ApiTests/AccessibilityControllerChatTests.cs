using AzureAI.WebAccessibilityTool.API.Controllers;
using AzureAI.WebAccessibilityTool.API.Models;
using AzureAI.WebAccessibilityTool.Models;
using AzureAI.WebAccessibilityTool.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;

namespace AzureAI.WebAccessibilityTool.Tests.ApiTests;

/// <summary>
/// Test suite for the <see cref="AccessibilityController"/> class.
/// </summary>
public class AccessibilityControllerChatTests
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the test class and sets up mock configuration.
    /// </summary>
    public AccessibilityControllerChatTests()
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
    public async Task AnalyzeImage_InvalidUrl_ReturnsOk()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);        

        // Act
        var result = await controller.AnalyzeImage(GlobalVariables.emptyUrl) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
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

        // Act

        var result = await controller.AnalyzeImage(GlobalVariables.testImageUrl) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        List<string> results = result.Value as List<string> ?? new List<string>();
        Assert.True(results?.Count > 0);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlWithChat"/> method with empty HTML content.
    /// </summary>    
    [Fact]
    public async Task AnalyzeHtmlWithChat_EmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);        

        // Act
        var result = await controller.AnalyzeHtmlWithChat(GlobalVariables.emptyHtmlContent) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlWithChat"/> method with valid HTML content.
    /// </summary>    
    [Fact]
    public async Task AnalyzeHtmlWithChat_ValidContent_ReturnsNoAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);

        // Act
        var result = await controller.AnalyzeHtmlWithChat(GlobalVariables.validHtmlContent) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        AnalysisResult results = result.Value as AnalysisResult ?? new AnalysisResult { Items = new List<AnalysisItem>(), Explanation = "" };
        var lowOrImprovementsResult = results.Items.Where(e => !e.Severity.Equals("Low") && !e.Severity.Equals("Improvement")).ToList();
        Assert.True(lowOrImprovementsResult.Count == 0);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlWithChat"/> method with invalid HTML content.
    /// </summary>    
    [Fact]
    public async Task AnalyzeHtmlWithChat_InvalidContent_ReturnsAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);
        
        // Act
        var result = await controller.AnalyzeHtmlWithChat(GlobalVariables.invalidHtmlContent) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        AnalysisResult results = result.Value as AnalysisResult ?? new AnalysisResult { Items = new List<AnalysisItem>(), Explanation = "" };
        Assert.True(results.Items.Count > 0);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlFromUrlWithChat"/> method with a valid URL.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AnalyzeHtmlFromUrlWithChat_EmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);
        var urlInput = new UrlInput { Url = GlobalVariables.emptyUrl };

        // Act
        var result = await controller.AnalyzeHtmlFromUrlWithChat(urlInput) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlFromUrlWithChat"/> method with a valid URL.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AnalyzeHtmlFromUrlWithChat_ValidContent_ReturnsNoAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);
        var urlInput = new UrlInput { Url = GlobalVariables.validUrl };

        // Act
        var result = await controller.AnalyzeHtmlFromUrlWithChat(urlInput) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        AnalysisResult results = result.Value as AnalysisResult ?? new AnalysisResult { Items = new List<AnalysisItem>(), Explanation = "" };
        var lowOrImprovementsResult = results.Items.Where(e => !e.Severity.Equals("Low") && !e.Severity.Equals("Improvement")).ToList();
        Assert.True(lowOrImprovementsResult.Count == 0);
    }

    /// <summary>
    /// Tests the <see cref="AccessibilityController.AnalyzeHtmlFromUrlWithChat"/> method with a valid URL.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AnalyzeHtmlFromUrlWithChat_InvalidContent_ReturnsAdvisory()
    {
        // Arrange
        var mockAnalyzer = new Mock<AccessibilityAnalyzer>(_configuration);
        var controller = new AccessibilityController(mockAnalyzer.Object);
        var urlInput = new UrlInput { Url = GlobalVariables.invalidUrl };

        // Act
        // Act
        var result = await controller.AnalyzeHtmlFromUrlWithChat(urlInput) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        AnalysisResult results = result.Value as AnalysisResult ?? new AnalysisResult { Items = new List<AnalysisItem>(), Explanation = "" };
        Assert.True(results.Items.Count > 0);
    }
}
