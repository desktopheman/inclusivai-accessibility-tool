using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Assistants;
using AzureAI.WebAccessibilityTool.Models;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using System.Text;
using AzureAI.WebAccessibilityTool.Helpers;
using SkiaSharp;
using System.Linq.Expressions;

namespace AzureAI.WebAccessibilityTool.Services;

/// <summary>
/// Service to analyze content for accessibility issues using Azure Cognitive Services and Azure OpenAI.
/// </summary>
public class AccessibilityAnalyzer
{
    private readonly string visionEndpoint;
    private readonly string visionApiKey;
    private readonly string openAiEndpoint;
    private readonly string openAiApiKey;

    private readonly ImageAnalysisClient imageAnalysisClient;
    private readonly AzureOpenAIClient openAiClient;
    private readonly AssistantsClient assistantClient;    

    private readonly string azureOpenAIDeploymentName;
    private readonly string webAssistantId;
    private readonly string pdfAssistantId;

    private readonly string storageAccountName;
    private readonly string storageAccountKey;
    private readonly string storageContainerName;

    /// <summary>
    /// Constructor to initialize API keys, endpoints, and SDK clients.
    /// </summary>
    public AccessibilityAnalyzer(IConfiguration configuration)
    {
        try
        {
            visionEndpoint = configuration["AzureServices:ComputerVision:Endpoint"] ?? throw new ArgumentNullException(nameof(visionEndpoint));
            visionApiKey = configuration["AzureServices:ComputerVision:ApiKey"] ?? throw new ArgumentNullException(nameof(visionApiKey));

            openAiEndpoint = configuration["AzureServices:OpenAI:Endpoint"] ?? throw new ArgumentNullException(nameof(openAiEndpoint));
            openAiApiKey = configuration["AzureServices:OpenAI:ApiKey"] ?? throw new ArgumentNullException(nameof(openAiApiKey));

            azureOpenAIDeploymentName = configuration["AzureServices:OpenAI:DeploymentName"] ?? throw new ArgumentNullException(nameof(azureOpenAIDeploymentName));
            webAssistantId = configuration["AzureServices:OpenAI:WebAssistantId"] ?? throw new ArgumentNullException(nameof(webAssistantId));
            pdfAssistantId = configuration["AzureServices:OpenAI:PDFAssistantId"] ?? throw new ArgumentNullException(nameof(pdfAssistantId));

            storageAccountName = configuration["AzureServices:Storage:AccountName"] ?? throw new ArgumentNullException(nameof(storageAccountName));
            storageAccountKey = configuration["AzureServices:Storage:AccountKey"] ?? throw new ArgumentNullException(nameof(storageAccountKey));
            storageContainerName = configuration["AzureServices:Storage:ContainerName"] ?? throw new ArgumentNullException(nameof(storageContainerName));

            imageAnalysisClient = new ImageAnalysisClient(new Uri(visionEndpoint), new AzureKeyCredential(visionApiKey));

            openAiClient = new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(openAiApiKey));
            assistantClient = new AssistantsClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiApiKey));
        }
        catch (Exception ex)
        {
            throw new Exception("Error initializing the Accessibility Analyzer service.", ex);
        }
    }

    /// <summary>
    /// Analyzes an image to retrieve the description using Azure Computer Vision.
    /// </summary>
    /// <param name="imageUrl">URL of the image.</param>
    /// <returns>A list of strings with the image description.</returns>
    public async Task<List<string>> AnalyzeImageAsync(string imageUrl)
    {        
        try
        {                        
            if (string.IsNullOrEmpty(HtmlHelper.CheckAbsolteUrl(imageUrl)))
                throw new ArgumentException($"Invalid URL: {imageUrl}", nameof(imageUrl));

            using HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound || !response.IsSuccessStatusCode)
                throw new Exception($"Error retrieving the image from {imageUrl}.");

            if (response.Content.Headers.ContentType?.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == false)
                throw new Exception($"The URL {imageUrl} does not point to an image.");

            // 6Mb limit
            if (response.Content.Headers.ContentLength > 6 * 1024 * 1024)
                throw new Exception("The image is too large.");

            using var imageStream = await response.Content.ReadAsStreamAsync();
            using var skBitmap = SKBitmap.Decode(imageStream);

            // Dimensions limits
            if (skBitmap.Width < 50 || skBitmap.Height < 50 || skBitmap.Width > 10240 || skBitmap.Height > 10240)
                throw new Exception("The image dimensions are out of the limits (50x50 to 10240x10240 pixels).");

            ImageAnalysisResult analysis = await imageAnalysisClient.AnalyzeAsync(new Uri(imageUrl),
                                                                                  VisualFeatures.DenseCaptions,
                                                                                  new ImageAnalysisOptions { GenderNeutralCaption = true });

            // Processa a resposta
            var imageDescription = new List<string>();

            if (analysis.DenseCaptions != null)            
                foreach (var caption in analysis.DenseCaptions.Values)                
                    imageDescription.Add(caption.Text);

            return imageDescription;
        }
        catch (Exception ex)
        {
            throw new Exception("Error analyzing the image using Azure Computer Vision.", ex);
        }
    }


    /// <summary>
    /// Analyzes content for accessibility issues using Azure OpenAI.
    /// </summary>
    /// <param name="input">Analysis input parameters</param>
    /// <returns>Analysis result object</returns>    
    public async Task<AnalysisResult> AnalyzeWithChatAsync(AnalysisInput input)
    {
        try
        {
            (string prompt, string sourceUrl) = await CreatePrompt(input);

            var chatClient = openAiClient.GetChatClient(azureOpenAIDeploymentName);

            List<ChatMessage> messages = [new UserChatMessage(ChatMessageContentPart.CreateTextPart(prompt))];
            ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(messages);

            if (chatCompletion.Content.Count() == 0)
                throw new Exception("No response from OpenAI.");

            string rawResponse = chatCompletion.Content[0].Text;
            return await ParseResponse(rawResponse, sourceUrl, input.GetImageDescriptions);
        }
        catch (Exception ex)
        {
            throw new Exception("Error analyzing the content using Azure OpenAI.", ex);
        }
    }

    /// <summary>
    /// Analyzes content for accessibility issues using Azure OpenAI (Assistant).
    /// </summary>
    /// <param name="input">Analysis input parameters</param>
    /// <returns>Analysis result object</returns>    
    public async Task<AnalysisResult> AnalyzeWithAssistantAsync(AnalysisInput input)
    {
        try
        {
            (string prompt, string sourceUrl) = await CreatePrompt(input, true);

            string assistantId = "";

            if (input.Type == AnalysisType.PDF)
                assistantId = pdfAssistantId;
            else
                assistantId = webAssistantId;

            var chatAssistance = await assistantClient.GetAssistantAsync(assistantId);

            Response<AssistantThread> threadResponse = await assistantClient.CreateThreadAsync();
            AssistantThread thread = threadResponse.Value;

            Response<ThreadMessage> messageResponse = await assistantClient.CreateMessageAsync(thread.Id, MessageRole.User, prompt);
            ThreadMessage message = messageResponse.Value;

            Response<ThreadRun> runResponse = await assistantClient.CreateRunAsync(thread.Id, new CreateRunOptions(chatAssistance.Value.Id) { });
            ThreadRun run = runResponse.Value;

            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                runResponse = await assistantClient.GetRunAsync(thread.Id, runResponse.Value.Id);
            }
            while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

            Response<PageableList<ThreadMessage>> afterRunMessagesResponse = await assistantClient.GetMessagesAsync(thread.Id);
            IReadOnlyList<ThreadMessage> messages = afterRunMessagesResponse.Value.Data;
            string rawResponse = "";

            foreach (ThreadMessage threadMessage in messages)
            {
                if (!string.IsNullOrEmpty(rawResponse))
                    break;

                foreach (MessageContent contentItem in threadMessage.ContentItems)
                {
                    if (contentItem is MessageTextContent textItem)
                    {
                        try
                        {
                            using (JsonDocument jsonDocument = JsonDocument.Parse(textItem.Text))
                            {
                                rawResponse = textItem.Text;
                                break;
                            }
                        }
                        catch (JsonException)
                        {
                            continue;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(rawResponse))
                throw new Exception("No response from OpenAI.");

            return await ParseResponse(rawResponse, sourceUrl, input.GetImageDescriptions);
        }
        catch (Exception ex)
        {
            throw new Exception("Error analyzing the content using Azure OpenAI (Assistant).", ex);
        }
    }

    /// <summary>
    /// Creates a prompt for the analysis based on the input type.  
    /// </summary>
    /// <param name="input">Analysis input parameters</param>
    /// <returns>Prompt ready for the use case</returns>    
    private async Task<(string, string)> CreatePrompt(AnalysisInput input, bool useAssistant = false)
    {
        try
        {
            string contentToAnalyze = string.Empty;
            string contentURL = string.Empty;
            string resourceFileName = "Prompt_HTML.txt";

            switch (input.Type)
            {
                case AnalysisType.URL:
                    string url = input.URL;

                    if (string.IsNullOrEmpty(HtmlHelper.CheckAbsolteUrl(url)))
                        throw new ArgumentException($"Invalid URL: {url}", nameof(url));

                    if (input.ExtractURLContent)
                    {
                        using HttpClient httpClient = new HttpClient();
                        contentToAnalyze = HtmlHelper.CheckAndFixHtmlContent(url, await httpClient.GetStringAsync(url));
                    }
                    else
                    {
                        contentToAnalyze = url;
                    }

                    contentURL = url;                    
                    break;

                case AnalysisType.PDF:
                case AnalysisType.WordDocument:
                    if (input.ExtractFileContent)
                        if (input.FileContent == null || input.FileContent.Length == 0)
                            throw new ArgumentException("File content cannot be empty.", nameof(input.FileContent));
                        else
                            contentToAnalyze = PDFHelper.ExtractPdfContent(input.FileContent);
                    
                    break;

                default:
                    if (string.IsNullOrEmpty(input.Content))
                        throw new ArgumentException("HTML content cannot be empty.", nameof(input.Content));

                    contentURL = input.URL;
                    contentToAnalyze = string.IsNullOrEmpty(contentURL) ? input.Content : HtmlHelper.CheckAndFixHtmlContent(contentURL, input.Content);                    
                    break;
            }

            if (string.IsNullOrEmpty(contentToAnalyze))            
                throw new ArgumentException("HTML content cannot be empty.", nameof(contentToAnalyze));            

            if (input.Type == AnalysisType.PDF || input.Type == AnalysisType.WordDocument)
            {
                return (contentToAnalyze, "");
            }
            else
            {
                if (useAssistant)
                {
                    return (contentToAnalyze, contentURL);
                }
                else
                {
                    string basePrompt = ResourceHelper.GetResourceContent(resourceFileName);
                    return (basePrompt.Replace("#CONTENT", contentToAnalyze), contentURL);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating the prompt for Azure OpenAI.", ex);
        }
    }

    /// <summary>
    /// Parses the response from Azure OpenAI into a WCAGResult object.
    /// </summary>
    /// <param name="rawResponse">JSON content</param>
    /// <param name="url">Base URL</param>
    /// <returns></returns>
    private async Task<AnalysisResult> ParseResponse(string rawResponse, string? url = "", bool? getImageDescriptions = false)
    {
        try
        {
            // Parse the response into a JSON object
            using JsonDocument document = JsonDocument.Parse(rawResponse);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("error", out JsonElement errorElement))
            {
                if (errorElement.ValueKind == JsonValueKind.String)
                    throw new Exception($"Error analyzing the content using Azure OpenAI. Error: {errorElement.GetString() ?? ""}");
            }

            // Extract issues and explanation from the JSON object
            List<AnalysisItem> issues = new List<AnalysisItem>();
            
            JsonElement issuesElement;
            root.TryGetProperty("issues", out issuesElement);

            if (issuesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement issueElement in issuesElement.EnumerateArray())
                {
                    JsonElement attributes;
                    root.TryGetProperty("ElementAttributes", out attributes);

                    List<ElementAttribute> elementAttributes = new List<ElementAttribute>();

                    if (attributes.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement attribute in attributes.EnumerateArray())
                        {
                            elementAttributes.Add(new ElementAttribute
                            {
                                Name = TryToGetPropertyValye(issueElement, "Name"),
                                Value = TryToGetPropertyValye(issueElement, "Value")
                            });
                        }
                    }

                    AnalysisItem item = new AnalysisItem() { 
                        Element = TryToGetPropertyValye(issueElement, "Element"), 
                        Issue = TryToGetPropertyValye(issueElement, "Issue"),
                        Recommendation = TryToGetPropertyValye(issueElement, "Recommendation"), 
                        Severity = TryToGetPropertyValye(issueElement, "Severity"),
                        Source = TryToGetPropertyValye(issueElement, "Source"),
                        Details = TryToGetPropertyValye(issueElement, "Details"),
                        Attributes = elementAttributes
                    };

                    if (getImageDescriptions == true)
                    {
                        if (item.Element.Equals("img") || item.Element.Equals("source"))
                        {
                            // Lista de atributos relacionados à origem de imagens
                            var imageAttributes = new[] { "src", "href", "data-cfsrc", "srcset" };

                            // Itera sobre os atributos da tag
                            foreach (var attribute in item.Attributes.Where(e => imageAttributes.Contains(e.Name)))
                            {
                                string imageUrl = attribute.Value ?? "";

                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    if (Uri.IsWellFormedUriString(imageUrl, UriKind.RelativeOrAbsolute))
                                    {
                                        string fullImageUrl = "";
                                        Uri? imageUri;

                                        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out imageUri))
                                        {
                                            // Se a URL for absoluta, usa diretamente
                                            fullImageUrl = imageUri.ToString();
                                        }
                                        else if (Uri.TryCreate(imageUrl, UriKind.Relative, out imageUri))
                                        {
                                            // Se a URL for relativa, cria uma URL absoluta com base na URL base
                                            if (!string.IsNullOrEmpty(url))
                                            {
                                                if (Uri.TryCreate(url, UriKind.Absolute, out var baseUri))
                                                {
                                                    Uri fullImageUri = new Uri(baseUri, imageUrl);
                                                    fullImageUrl = fullImageUri.ToString();
                                                }
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(fullImageUrl))
                                        {
                                            try
                                            {
                                                List<string> result = await AnalyzeImageAsync(fullImageUrl);

                                                if (result.Count > 0)
                                                {
                                                    item.ImageDescriptionRecommendation = string.Join(", ", result);
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    issues.Add(item);
                }
            }

            string explanation = TryToGetPropertyValye(root, "Explanation");
            var response = new AnalysisResult() { Items = issues, Explanation = explanation };

            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Error parsing the response from Azure OpenAI.", ex);
        }
    }

    /// <summary>
    /// Try to get an JSON element value
    /// </summary>
    /// <param name="element">JSON Element</param>
    /// <param name="key">Element key</param>
    /// <returns>Element value</returns>
    private string TryToGetPropertyValye(JsonElement element, string key)
    {
        JsonElement itemElement;
        element.TryGetProperty(key, out itemElement);

        if (itemElement.ValueKind == JsonValueKind.String)
            return itemElement.GetString() ?? string.Empty;
        else 
            return string.Empty;
    }
}
