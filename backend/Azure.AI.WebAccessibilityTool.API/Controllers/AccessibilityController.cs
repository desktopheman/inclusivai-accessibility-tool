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
    /// Analyzes an image for accessibility issues based on its URL using Azure Computer Vision
    /// </summary>
    /// <param name="input">The input containing the URL of the image to be analyzed.</param>
    /// <returns>A list of WCAG issues related to the image.</returns>
    [HttpPost("imageUrl")]
    public async Task<IActionResult> AnalyzeImage([FromBody] string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("The URL cannot be null or empty.");
            }

            var results = await _analyzer.AnalyzeImageAsync(url);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while analyzing the image: {ex.Message}");            
        }
    }

    /// <summary>
    /// Analyzes HTML content from a URL for accessibility issues and provides recommendations using Azure OpenAI (ChatGPT)
    /// </summary>
    /// <param name="input">URL</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    [HttpPost("urlWithChat")]
    public async Task<IActionResult> AnalyzeHtmlFromUrlWithChat([FromBody] UrlInput input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input?.Url))
            {
                var wcagResult = new AnalysisResult() { Items = new List<AnalysisItem>(), Explanation = "URL is empty" };
                return BadRequest(wcagResult);
            }

            AnalysisInput analysisInput = new AnalysisInput() 
            {
                Type = AnalysisType.URL, 
                URL = input.Url, 
                ExtractURLContent = input.ExtractHtmlContentFromUrl ?? true,
                GetImageDescriptions = input.GetImageDescriptions ?? false
            };

            var result = await _analyzer.AnalyzeWithChatAsync(analysisInput);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var wcagResult = new AnalysisResult() { Items = new List<AnalysisItem>(), Explanation =$"An error occurred while analyzing the HTML content from the URL: {ex.Message}"  };
            return StatusCode(500, wcagResult);
        }
    }

    /// <summary>
    /// Analyzes HTML content from a URL for accessibility issues and provides recommendations using Azure OpenAI (ChatGPT Assistant)
    /// </summary>
    /// <param name="input">URL</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    [HttpPost("urlWithAssistant")]
    public async Task<IActionResult> AnalyzeHtmlFromUrlWithAssistant([FromBody] UrlInput input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input?.Url))
            {
                var wcagResult = new AnalysisResult() { Items = new List<AnalysisItem>(), Explanation = "URL is empty" };
                return BadRequest(wcagResult);
            }

            AnalysisInput analysisInput = new AnalysisInput()
            {
                Type = AnalysisType.URL,
                URL = input.Url,
                ExtractURLContent = input.ExtractHtmlContentFromUrl ?? true,
                GetImageDescriptions = input.GetImageDescriptions ?? false
            };


            var result = await _analyzer.AnalyzeWithAssistantAsync(analysisInput);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var wcagResult = new AnalysisResult() { Items = new List<AnalysisItem>(), Explanation = $"An error occurred while analyzing the HTML content from the URL: {ex.Message}" };
            return StatusCode(500, wcagResult);
        }
    }

    /// <summary>
    /// Analyzes HTML content for accessibility issues and provides recommendations using Azure OpenAI (ChatGPT)
    /// </summary>
    /// <param name="input">The input containing the HTML content to be analyzed.</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    [HttpPost("htmlWithChat")]
    public async Task<IActionResult> AnalyzeHtmlWithChat([FromBody] string htmlInput, bool getImageDescriptions = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(htmlInput))
            {
                var wcagResult = new AnalysisResult() { Items = new List<AnalysisItem>(), Explanation = "HTML content is empty" };
                return BadRequest(wcagResult);
            }

            AnalysisInput analysisInput = new AnalysisInput()
            {
                Type = AnalysisType.HTML,                
                Content = htmlInput,
                GetImageDescriptions = getImageDescriptions
            };

            var result = await _analyzer.AnalyzeWithChatAsync(analysisInput);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while analyzing the HTML content: {ex.Message}");
        }
    }

    /// <summary>
    /// Analyzes HTML content for accessibility issues and provides recommendations using Azure OpenAI (ChatGPT Assistant)
    /// </summary>
    /// <param name="input">The input containing the HTML content to be analyzed.</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    [HttpPost("htmlWithAssistant")]
    public async Task<IActionResult> AnalyzeHtmlWithAssistant([FromBody] string htmlInput, bool getImageDescriptions = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(htmlInput))
            {
                var wcagResult = new AnalysisResult() { Items = new List<AnalysisItem>(), Explanation = "HTML content is empty" };
                return BadRequest(wcagResult);
            }

            AnalysisInput analysisInput = new AnalysisInput()
            {
                Type = AnalysisType.HTML,
                Content = htmlInput,
                GetImageDescriptions = getImageDescriptions
            };

            var result = await _analyzer.AnalyzeWithAssistantAsync(analysisInput);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while analyzing the HTML content: {ex.Message}");
        }
    }

    /// <summary>
    /// Analyzes an uploaded document for accessibility issues using Azure OpenAI (ChatGPT)
    /// </summary>
    /// <param name="file">The file to be analyzed.</param>
    /// <returns>Issues and recommendations for the document's accessibility.</returns>
    [HttpPost("pdfWithChat")]
    public async Task<IActionResult> AnalyzePDFWithChat(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("The uploaded file is empty or missing.");
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            AnalysisInput analysisInput = new AnalysisInput()
            {
                Type = AnalysisType.PDF,
                FileContent = memoryStream.ToArray(),
            };

            var result = await _analyzer.AnalyzeWithChatAsync(analysisInput);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while analyzing the document: {ex.Message}");
        }
    }

    /// <summary>
    /// Analyzes an uploaded document for accessibility issues using Azure OpenAI (ChatGPT Assistant)
    /// </summary>
    /// <param name="file">The file to be analyzed.</param>
    /// <returns>Issues and recommendations for the document's accessibility.</returns>
    [HttpPost("pdfWithAssistant")]
    public async Task<IActionResult> AnalyzePDFWithAssistant(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("The uploaded file is empty or missing.");
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            AnalysisInput analysisInput = new AnalysisInput()
            {
                Type = AnalysisType.PDF,
                FileContent = memoryStream.ToArray(),
            };


            var result = await _analyzer.AnalyzeWithAssistantAsync(analysisInput);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while analyzing the document: {ex.Message}");
        }
    }
}
