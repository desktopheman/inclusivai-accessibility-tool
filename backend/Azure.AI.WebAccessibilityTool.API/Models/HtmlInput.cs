namespace AzureAI.WebAccessibilityTool.API.Models;

/// <summary>
/// Represents the input model for analyzing HTML content.
/// </summary>
public class HtmlInput
{
    /// <summary>
    /// Gets or sets the HTML content to be analyzed.
    /// </summary>
    public required string HtmlContent { get; set; }
}
