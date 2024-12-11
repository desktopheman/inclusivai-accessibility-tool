using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAI.WebAccessibilityTool.Models;

/// <summary>
/// The input to analyze.
/// </summary>
public class AnalysisInput
{
    /// <summary>
    /// The type of content to analyze.
    /// </summary>
    public AnalysisType Type { get; set; }

    /// <summary>
    /// The content to analyze.
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// The file content to analyze.
    /// </summary>
    public byte[]? FileContent { get; set; }

    /// <summary>
    /// Extract the content from the file.
    /// </summary>
    public bool ExtractFileContent { get; set; } = false;

    /// <summary>
    /// The URL to analyze or the URL of the content to analyze.
    /// </summary>
    public string URL { get; set; } = "";

    /// <summary>
    /// The URL to extract content from.
    /// </summary>
    public bool ExtractURLContent { get; set; } = false;

}

/// <summary>
/// The type of content to analyze.
/// </summary>
public enum AnalysisType
{
    /// <summary>
    /// Analyze a URL.
    /// </summary>
    URL,
    /// <summary>
    /// Analyze a HTML content
    /// </summary>
    HTML,
    /// <summary>
    /// Analyze a PDF content
    /// </summary>
    PDF,
    /// <summary>
    /// Analyze a Word document content
    /// </summary>
    WordDocument
}