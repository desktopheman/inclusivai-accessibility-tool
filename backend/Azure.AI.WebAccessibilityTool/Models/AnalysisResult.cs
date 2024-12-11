
namespace AzureAI.WebAccessibilityTool.Models;

/// <summary>
/// The result of a Web Content Accessibility Guidelines (WCAG) evaluation.
/// </summary>
public class AnalysisResult
{
    /// <summary>
    /// The list of items that were evaluated.
    /// </summary>
    public required List<AnalysisItem> Items { get; set; }

    /// <summary>
    /// The explanation of the evaluation.
    /// </summary>
    public required string Explanation { get; set; }
}

public class AnalysisItem
{
    /// <summary>
    /// The element that was evaluated.
    /// </summary>
    public required string Element { get; set; }

    /// <summary>
    /// The list of attributes of the element that was evaluated.
    /// </summary>
    public required List<ElementAttribute> Attributes { get; set; }

    /// <summary>
    /// The Web Content Accessibility Guidelines (WCAG) rule that was evaluated.
    /// </summary>
    public required string Issue { get; set; }

    /// <summary>
    /// The description of the issue that was found.
    /// </summary>
    public required string Recommendation { get; set; }

    /// <summary>
    /// Description recommendation for the image in case alt is missing
    /// </summary>
    public string? ImageDescriptionRecommendation { get; set; }

    /// <summary>
    /// The severity of the issue that was found.
    /// </summary>
    public required string Severity { get; set; }

    /// <summary>
    /// The source of the issue analysis: WCAG 2.1 (Web Content Accessibility Guidelines), ADA (Americans with Disabilities Act), and Section 508 (Rehabilitation Act)
    /// </summary>
    public required string Source { get; set; }

    /// <summary>
    /// The details of the issue that was found.
    /// </summary>
    public required string Details { get; set; }
}


/// <summary>
/// Represents an attribute of an HTML element.
/// </summary>
public class ElementAttribute
{
    public required string Name { get; set; }
    public string? Value { get; set; } = ""; 
}