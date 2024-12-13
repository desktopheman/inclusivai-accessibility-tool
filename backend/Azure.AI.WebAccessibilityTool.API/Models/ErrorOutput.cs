namespace AzureAI.WebAccessibilityTool.API.Models;

/// <summary>
/// Error output model
/// </summary>
public class ErrorOutput
{
    /// <summary>
    /// Error code
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public required string Message { get; set; }
}
