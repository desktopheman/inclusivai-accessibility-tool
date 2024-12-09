using Azure;
using Azure.AI;
using Azure.AI.OpenAI;
using AzureAI.WebAccessibilityTool.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace AzureAI.WebAccessibilityTool.Services;

/// <summary>
/// A service that analyzes images and HTML content for accessibility issues.
/// </summary>
public class AccessibilityAnalyzer
{
    private readonly string visionEndpoint;
    private readonly string visionApiKey;
    private readonly string openAiEndpoint;
    private readonly string openAiApiKey;
    private readonly ComputerVisionClient computerVisionClient;
    private readonly AzureOpenAIClient openAiClient;
    private readonly string azureOpenAIDeploymentName = "gpt-4o"; 

    /// <summary>
    /// Constructor to initialize API keys, endpoints, and SDK clients.
    /// </summary>
    public AccessibilityAnalyzer(IConfiguration configuration)
    {
        visionEndpoint = configuration["AzureServices:ComputerVision:Endpoint"] ?? throw new ArgumentNullException(nameof(visionEndpoint));
        visionApiKey = configuration["AzureServices:ComputerVision:ApiKey"] ?? throw new ArgumentNullException(nameof(visionApiKey));
        openAiEndpoint = configuration["AzureServices:OpenAI:Endpoint"] ?? throw new ArgumentNullException(nameof(openAiEndpoint));
        openAiApiKey = configuration["AzureServices:OpenAI:ApiKey"] ?? throw new ArgumentNullException(nameof(openAiApiKey));

        computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionApiKey));
        computerVisionClient.Endpoint = visionEndpoint;

        openAiClient = new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(openAiApiKey));
    }

    /// <summary>
    /// Analyzes an image to retrieve the description using Azure Computer Vision.
    /// </summary>
    /// <param name="imageUrl">URL of the image.</param>
    /// <returns>A list of strings with the image description.</returns>
    public async Task<List<string>> AnalyzeImageAsync(string ImageUrl)
    {
        try
        {
            if (!Uri.IsWellFormedUriString(ImageUrl, UriKind.Absolute))
            {
                throw new ArgumentException($"Invalid image URL: {0}", ImageUrl);
            }

            ImageAnalysis analysis = await computerVisionClient.AnalyzeImageAsync(ImageUrl, [ VisualFeatureTypes.Description ]);

            // Validate and process response
            var imageDescription = new List<string>();

            if (analysis.Description?.Captions != null)
            {
                foreach (var caption in analysis.Description.Captions)
                {
                    imageDescription.Add(caption.Text);
                }
            }

            return imageDescription;
        }
        catch (Exception ex)
        {
            throw new Exception("Error analyzing image using Azure Computer Vision SDK.", ex);
        }
    }

    /// <summary>
    /// Analyzes HTML content from a URL for accessibility issues using Azure OpenAI.
    /// </summary>
    /// <param name="Url">Url</param>
    /// <returns></returns>/// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    /// <exception cref="ArgumentException">Invalid URL</exception>
    /// <exception cref="Exception">General exception</exception>
    public async Task<WCAGResult> AnalyzeHtmlFromUrlAsync(string Url)
    {
        if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
        {
            throw new ArgumentException($"Invalid URL: {Url}", nameof(Url));
        }

        try
        {
            using HttpClient httpClient = new HttpClient();
            string htmlContent = await httpClient.GetStringAsync(Url);

            return await AnalyzeHtmlAsync(htmlContent);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching HTML content from URL.", ex);
        }
    }

    /// <summary>
    /// Analyzes HTML content for accessibility issues using Azure OpenAI.
    /// </summary>
    /// <param name="HtmlContent">Full HTML content</param>
    /// <returns></returns>/// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    /// <exception cref="ArgumentException">Invalid URL</exception>
    /// <exception cref="Exception">General exception</exception>
    public async Task<WCAGResult> AnalyzeHtmlAsync(string HtmlContent)
    {
        if (string.IsNullOrEmpty(HtmlContent))
        {
            throw new ArgumentException("HTML content cannot be empty.", nameof(HtmlContent));
        }

        try
        {
            var chatClient = openAiClient.GetChatClient(azureOpenAIDeploymentName);

            // Prepare the prompt for the OpenAI model
            string prompt = $@"Analyze the provided HTML content for any WCAG (Web Content Accessibility Guidelines) compliance issues. 
                               The analysis should evaluate the entire HTML content, examining every element comprehensively. 

                                Response requirements:
                                1. Format: The result must be a JSON object only
                                2. Structure: The JSON object must include:
                                    - `issues`: An array of objects, each containing:
                                        - `Element`: The tag name of the HTML element (e.g., ""img"", ""div"", ""p"").
                                        - `ElementAttributes`: An array of objects, each having:
                                            - `Name`: The name of the attribute.
                                            - `Value`: The value of the attribute.
                                        - `Issue`: A brief description of the detected WCAG compliance issue.
                                        - `Severity`: The severity level of the issue (e.g., ""Low"", ""Medium"", ""High"").
                                        - `Recommendation`: A recommended action to resolve the issue.
                                    - `Explanation`: A summary description explaining the context or overarching reasoning for the issues found (this is a single attribute, separate from the array).

                                3. Scope: Only include elements in the JSON response that have one or more detected WCAG issues. Ignore elements without issues.
                                4. Analysis depth: All HTML elements and their attributes must be analyzed, but the response should exclude elements without issues to maintain brevity and relevance.
                                                                                                
                                Additional Information: The elements must be analyzed in the context of the entire HTML content and each element must be evaluated for WCAG compliance.
                                Also, each element, like a img for instance, must be evaluated for its attributes like alt but in this case it should be check if the alt is present and has a minimal description.
                                The same must be handle for hyperlinks (a tag) checking for a minimal description.                                

                                VERY IMPORTANT: The response must have only the JSON object itself. The response MUST NOT has anything else like a block of code or a markdown response. 

                                HTML Content: 
                               {HtmlContent}";

            List<ChatMessage> messages = [new UserChatMessage(ChatMessageContentPart.CreateTextPart(prompt))];
            ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(messages);

            if (chatCompletion.Content.Count() == 0)
                throw new Exception("No response from OpenAI.");

            // Parse the response
            string rawResponse = chatCompletion.Content[0].Text;

            // Parse the response into a JSON object
            using JsonDocument document = JsonDocument.Parse(rawResponse);
            JsonElement root = document.RootElement;

            // Extract issues and explanation from the JSON object
            List<WCAGItem> issues = new List<WCAGItem>();
            foreach (JsonElement issueElement in root.GetProperty("issues").EnumerateArray())
            {
                var attributes = issueElement.GetProperty("ElementAttributes");
                List<ElementAttribute> elementAttributes = new List<ElementAttribute>();
                foreach (JsonElement attribute in attributes.EnumerateArray())
                {
                    elementAttributes.Add(new ElementAttribute
                    {
                        Name = attribute.GetProperty("Name").GetString() ?? "",
                        Value = attribute.GetProperty("Value").GetString() ?? ""
                    });
                }

                WCAGItem item = new WCAGItem
                {
                    Element = issueElement.GetProperty("Element").GetString() ?? "",
                    Issue = issueElement.GetProperty("Issue").GetString() ?? "",
                    Recommendation = issueElement.GetProperty("Recommendation").GetString() ?? "",
                    Severity = issueElement.GetProperty("Severity").GetString() ?? "",
                    Attributes = elementAttributes
                };

                if (item.Element.Equals("img"))
                {
                    if (item.Attributes.Where(e => e.Name.Equals("src")).Count() > 0)
                    {                        
                        string imageUrl = item.Attributes?.Where(e => e.Name.Equals("src"))?.FirstOrDefault()?.Value ?? "";

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            List<string> result = await AnalyzeImageAsync(imageUrl);

                            if (result.Count > 0)
                                item.ImageDescriptionRecommendation = string.Join(", ", result);
                        }
                    }                    
                }

                issues.Add(item);
            }

            string explanation = root.GetProperty("Explanation").GetString() ?? "";

            return (new WCAGResult() { Items = issues, Explanation = explanation });
        }
        catch (Exception ex)
        {
            throw new Exception("Error analyzing HTML content using Azure OpenAI SDK.", ex);
        }
    }
}
