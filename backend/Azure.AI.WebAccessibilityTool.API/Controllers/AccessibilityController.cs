using AzureAI.WebAccessibilityTool.API.Models;
using AzureAI.WebAccessibilityTool.Models;
using AzureAI.WebAccessibilityTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureAI.WebAccessibilityTool.API.Controllers;

/// <summary>
/// A controller that provides endpoints for analyzing images and HTML content for accessibility issues.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccessibilityController : ControllerBase
{
    private readonly AccessibilityAnalyzer _analyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessibilityController"/> class.
    /// </summary>
    /// <param name="analyzer">An instance of <see cref="AccessibilityAnalyzer"/> for processing accessibility analysis.</param>
    public AccessibilityController(AccessibilityAnalyzer analyzer)
    {
        _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
    }
    
    /// <summary>
    /// Analyzes an image for accessibility issues based on its URL.
    /// </summary>
    /// <param name="input">The input containing the URL of the image to be analyzed.</param>
    /// <returns>A list of WCAG issues related to the image.</returns>
    [HttpPost("fromImageUrl")]
    public async Task<IActionResult> AnalyzeImage([FromBody] UrlInput input)
    {
        if (string.IsNullOrWhiteSpace(input?.Url))
        {
            return BadRequest("The URL cannot be null or empty.");
        }

        try
        {
            var results = await _analyzer.AnalyzeImageAsync(input.Url);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while analyzing the image: {ex.Message}");            
        }
    }

    /// <summary>
    /// Analyzes HTML content from a URL for accessibility issues and provides recommendations - using Chat
    /// </summary>
    /// <param name="input">URL</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    [HttpPost("urlWithChat")]
    public async Task<IActionResult> AnalyzeHtmlFromUrlWithChat([FromBody] UrlInput input)
    {
        if (string.IsNullOrWhiteSpace(input?.Url))
        {
            var wcagResult = new WCAGResult() { Items = new List<WCAGItem>(), Explanation = "URL is empty" };
            return BadRequest(wcagResult);
        }
        try
        {
            var result = await _analyzer.AnalyzeHtmlFromUrlWithChatAsync(input.Url, input.ExtractHtmlContentFromUrl ?? true);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var wcagResult = new WCAGResult() { Items = new List<WCAGItem>(), Explanation =$"An error occurred while analyzing the HTML content from the URL: {ex.Message}"  };
            return StatusCode(500, wcagResult);
        }
    }

    /// <summary>
    /// Analyzes HTML content from a URL for accessibility issues and provides recommendations - using Chat
    /// </summary>
    /// <param name="input">URL</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    [HttpPost("urlWithAssistant")]
    public async Task<IActionResult> AnalyzeHtmlFromUrlWithAssistant([FromBody] UrlInput input)
    {
        if (string.IsNullOrWhiteSpace(input?.Url))
        {
            var wcagResult = new WCAGResult() { Items = new List<WCAGItem>(), Explanation = "URL is empty" };
            return BadRequest(wcagResult);
        }
        try
        {
            var result = await _analyzer.AnalyzeHtmlFromUrlWithAssistantAsync(input.Url, input.ExtractHtmlContentFromUrl ?? false);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var wcagResult = new WCAGResult() { Items = new List<WCAGItem>(), Explanation = $"An error occurred while analyzing the HTML content from the URL: {ex.Message}" };
            return StatusCode(500, wcagResult);
        }
    }

    /// <summary>
    /// Analyzes HTML content for accessibility issues and provides recommendations - using Chat
    /// </summary>
    /// <param name="input">The input containing the HTML content to be analyzed.</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    [HttpPost("htmlWithChat")]
    public async Task<IActionResult> AnalyzeHtmlWithChat([FromBody] HtmlInput input)
    {
        if (string.IsNullOrWhiteSpace(input?.HtmlContent))
        {
            var wcagResult = new WCAGResult() { Items = new List<WCAGItem>(), Explanation = "HTML content is empty" };
            return BadRequest(wcagResult);
        }

        try
        {            
            var result = await _analyzer.AnalyzeHtmlWithChatAsync(input.HtmlContent);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while analyzing the HTML content: {ex.Message}");
        }
    }

    /// <summary>
    /// Analyzes HTML content for accessibility issues and provides recommendations - using Assistant
    /// </summary>
    /// <param name="input">The input containing the HTML content to be analyzed.</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    [HttpPost("htmlWithAssistant")]
    public async Task<IActionResult> AnalyzeHtmlWithAssistant([FromBody] HtmlInput input)
    {
        if (string.IsNullOrWhiteSpace(input?.HtmlContent))
        {
            var wcagResult = new WCAGResult() { Items = new List<WCAGItem>(), Explanation = "HTML content is empty" };
            return BadRequest(wcagResult);
        }

        try
        {
            var result = await _analyzer.AnalyzeHtmlWithAssistantAsync(input.HtmlContent);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while analyzing the HTML content: {ex.Message}");
        }
    }
}
