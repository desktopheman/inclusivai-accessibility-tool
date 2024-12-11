using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Assistants;
using AzureAI.WebAccessibilityTool.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using MimeDetective;
using System.Reflection;

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
    private readonly AssistantsClient assistantClient;

    private readonly string azureOpenAIDeploymentName;
    private readonly string openAIAssistantId;

    private readonly string storageAccountName;
    private readonly string storageAccountKey;
    private readonly string storageContainerName;

    /// <summary>
    /// Constructor to initialize API keys, endpoints, and SDK clients.
    /// </summary>
    public AccessibilityAnalyzer(IConfiguration configuration)
    {
        visionEndpoint = configuration["AzureServices:ComputerVision:Endpoint"] ?? throw new ArgumentNullException(nameof(visionEndpoint));
        visionApiKey = configuration["AzureServices:ComputerVision:ApiKey"] ?? throw new ArgumentNullException(nameof(visionApiKey));
        
        openAiEndpoint = configuration["AzureServices:OpenAI:Endpoint"] ?? throw new ArgumentNullException(nameof(openAiEndpoint));
        openAiApiKey = configuration["AzureServices:OpenAI:ApiKey"] ?? throw new ArgumentNullException(nameof(openAiApiKey));
        
        azureOpenAIDeploymentName = configuration["AzureServices:OpenAI:DeploymentName"] ?? throw new ArgumentNullException(nameof(azureOpenAIDeploymentName));
        openAIAssistantId = configuration["AzureServices:OpenAI:AssistantId"] ?? throw new ArgumentNullException(nameof(openAIAssistantId));

        storageAccountName = configuration["AzureServices:Storage:AccountName"] ?? throw new ArgumentNullException(nameof(storageAccountName));
        storageAccountKey = configuration["AzureServices:Storage:AccountKey"] ?? throw new ArgumentNullException(nameof(storageAccountKey));
        storageContainerName = configuration["AzureServices:Storage:ContainerName"] ?? throw new ArgumentNullException(nameof(storageContainerName));

        computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionApiKey));
        computerVisionClient.Endpoint = visionEndpoint;

        openAiClient = new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(openAiApiKey));
        assistantClient = new AssistantsClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiApiKey));
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

            using HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync(ImageUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<string>();

            ImageAnalysis analysis = await computerVisionClient.AnalyzeImageAsync(ImageUrl, [VisualFeatureTypes.Description]);

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
    /// Analyzes HTML content from a URL for accessibility issues using Azure OpenAI (Chat)
    /// </summary>
    /// <param name="Url">Url</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    /// <exception cref="ArgumentException">Invalid URL</exception>
    /// <exception cref="Exception">General exception</exception>
    public async Task<WCAGResult> AnalyzeHtmlFromUrlWithChatAsync(string Url, bool extractHtmlFromUrl = false)
    {
        if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
        {
            throw new ArgumentException($"Invalid URL: {Url}", nameof(Url));
        }

        try
        {
            if (extractHtmlFromUrl)
            {
                using HttpClient httpClient = new HttpClient();
                string htmlContent = await httpClient.GetStringAsync(Url);
                string checkedHtmlContent = CheckAndFixHtmlContent(Url, htmlContent);

                return await AnalyzeHtmlWithChatAsync(checkedHtmlContent, Url);
            }
            else
            {
                return await AnalyzeHtmlWithChatAsync(Url);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching HTML content from URL.", ex);
        }
    }

    /// <summary>
    /// Analyzes HTML content from a URL for accessibility issues using Azure OpenAI (Assistant)
    /// </summary>
    /// <param name="Url">Url</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    /// <exception cref="ArgumentException">Invalid URL</exception>
    /// <exception cref="Exception">General exception</exception>
    public async Task<WCAGResult> AnalyzeHtmlFromUrlWithAssistantAsync(string Url, bool extractHtmlFromUrl = false)
    {
        if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
        {
            throw new ArgumentException($"Invalid URL: {Url}", nameof(Url));
        }

        try
        {
            if (extractHtmlFromUrl)
            {
                using HttpClient httpClient = new HttpClient();
                string htmlContent = await httpClient.GetStringAsync(Url);
                string checkedHtmlContent = CheckAndFixHtmlContent(Url, htmlContent);

                return await AnalyzeHtmlWithAssistantAsync(checkedHtmlContent, Url);
            }
            else
            {
                return await AnalyzeHtmlWithAssistantAsync(Url);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching HTML content from URL.", ex);
        }
    }

    /// <summary>
    /// Analyzes a document for accessibility issues using Azure OpenAI.
    /// </summary>
    /// <param name="fileContent">File content (byte array)</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<WCAGResult> AnalyzeDocumentlWithChatAsync(byte[] fileContent)
    {
        if (fileContent == null || fileContent.Length == 0)
        {
            throw new ArgumentException("File content cannot be empty.", nameof(fileContent));
        }

        string fileUrl = await UploadFile(fileContent);
        return await AnalyzeHtmlWithChatAsync(fileUrl); 
    }

    /// <summary>
    /// Analyzes a document for accessibility issues using Azure OpenAI (Assistant)
    /// </summary>
    /// <param name="fileContent">File content (byte array)</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<WCAGResult> AnalyzeDocumentlWithAssistantAsync(byte[] fileContent)
    {
        if (fileContent == null || fileContent.Length == 0)
        {
            throw new ArgumentException("File content cannot be empty.", nameof(fileContent));
        }

        string fileUrl = await UploadFile(fileContent);
        return await AnalyzeHtmlWithAssistantAsync(fileUrl);
    }

    /// <summary>
    /// Analyzes HTML content for accessibility issues using Azure OpenAI.
    /// </summary>
    /// <param name="HtmlContent">Full HTML content</param>
    /// <param name="sourceUrl">Full HTML content</param>
    /// <param name="fileContent">Full HTML content</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    /// <exception cref="ArgumentException">Invalid URL</exception>
    /// <exception cref="Exception">General exception</exception>
    public async Task<WCAGResult> AnalyzeHtmlWithChatAsync(string UrlOrHtmlContent, string? sourceUrl = "")
    {
        if (string.IsNullOrEmpty(UrlOrHtmlContent))
        {
            throw new ArgumentException("HTML content cannot be empty.", nameof(UrlOrHtmlContent));
        }

        try
        {                                   
            var chatClient = openAiClient.GetChatClient(azureOpenAIDeploymentName);

            // Prepare the prompt for the OpenAI model
            string promptResource = "AzureAI.WebAccessibilityTool.Resources.Prompt.txt";

            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly == null)
                throw new Exception("Error loading the assembly to get the prompt file");

            Stream? stream = assembly.GetManifestResourceStream(promptResource);
            if (stream == null)
                throw new Exception("Error loading the prompt resource file.");

            StreamReader reader = new StreamReader(stream);
            if (reader == null)            
                throw new Exception("Error reading the prompt resource file.");

            string basePrompt = reader.ReadToEnd();                

            if (string.IsNullOrEmpty(basePrompt))
                throw new Exception("The prompt resource file is empty");

            string prompt = reader.ReadToEnd().Replace("#UrlOrHtmlContent", UrlOrHtmlContent);                        

            List <ChatMessage> messages = [new UserChatMessage(ChatMessageContentPart.CreateTextPart(prompt))];
            ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(messages);

            if (chatCompletion.Content.Count() == 0)
                throw new Exception("No response from OpenAI.");

            // Parse the response
            string rawResponse = chatCompletion.Content[0].Text;

            string url = CheckAbsolteUrl(UrlOrHtmlContent);
            return await ParseResponse(rawResponse, string.IsNullOrEmpty(url) ? sourceUrl : url);
        }
        catch (Exception ex)
        {
            throw new Exception("Error analyzing the content using Azure OpenAI.", ex);
        }
    }

    /// <summary>
    /// Analyzes HTML content for accessibility issues using Azure OpenAI (Assistant)
    /// </summary>
    /// <param name="HtmlContent">Full HTML content</param>
    /// <returns>Issues and explanation on how to resolve WCAG issues in the HTML content.</returns>
    /// <exception cref="ArgumentException">Invalid URL</exception>
    /// <exception cref="Exception">General exception</exception>
    public async Task<WCAGResult> AnalyzeHtmlWithAssistantAsync(string UrlOrHtmlContent, string? sourceUrl = "")
    {
        if (string.IsNullOrEmpty(UrlOrHtmlContent))
        {
            throw new ArgumentException("HTML content cannot be empty.", nameof(UrlOrHtmlContent));
        }

        try
        {
            var chatAssistance = await assistantClient.GetAssistantAsync(openAIAssistantId);

            Response<AssistantThread> threadResponse = await assistantClient.CreateThreadAsync();
            AssistantThread thread = threadResponse.Value;

            Response<ThreadMessage> messageResponse = await assistantClient.CreateMessageAsync(thread.Id, MessageRole.User, UrlOrHtmlContent);
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

            string url = CheckAbsolteUrl(UrlOrHtmlContent);
            return await ParseResponse(rawResponse, string.IsNullOrEmpty(url) ? sourceUrl : url);
        }
        catch (Exception ex)
        {
            throw new Exception("Error analyzing the content using Azure OpenAI (Assistant).", ex);
        }
    }

    /// <summary>
    /// Parses the response from Azure OpenAI into a WCAGResult object.
    /// </summary>
    /// <param name="rawResponse">JSON content</param>
    /// <param name="url">Base URL</param>
    /// <returns></returns>
    private async Task<WCAGResult> ParseResponse(string rawResponse, string? url = "")
    {
        // Parse the response into a JSON object
        using JsonDocument document = JsonDocument.Parse(rawResponse);
        JsonElement root = document.RootElement;
        
        if (root.TryGetProperty("error", out JsonElement errorElement))
        {
            if (errorElement.ValueKind == JsonValueKind.String)
                return new WCAGResult() { Items = new List<WCAGItem>(), Explanation = errorElement.GetString() ?? "" };
        }

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
                Source = issueElement.GetProperty("Source").GetString() ?? "",
                Details = issueElement.GetProperty("Details").GetString() ?? "",
                Attributes = elementAttributes
            };

            if (item.Element.Equals("img") || item.Element.Equals("source"))
            {
                // Lista de atributos relacionados à origem de imagens
                var imageAttributes = new[] { "src", "data-cfsrc", "srcset" };

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
                                // Analisa a imagem
                                List<string> result = await AnalyzeImageAsync(fullImageUrl);

                                if (result.Count > 0)
                                {
                                    item.ImageDescriptionRecommendation = string.Join(", ", result);
                                }
                            }
                        }
                    }
                }
            }

            issues.Add(item);
        }

        string explanation = root.GetProperty("Explanation").GetString() ?? "";
        var response = new WCAGResult() { Items = issues, Explanation = explanation };

        return response;
    }

    /// <summary>
    /// Checks and fixes the HTML content by making all image-related URLs absolute.
    /// </summary>
    /// <param name="url">Source URL</param>
    /// <param name="htmlContent">HTML content</param>
    /// <returns>Checked and fixed HTML content</returns>
    /// <exception cref="ArgumentException"></exception>
    private string CheckAndFixHtmlContent(string url, string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Base URL cannot be null or empty.", nameof(url));
        }

        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            throw new ArgumentException("HTML content cannot be null or empty.", nameof(htmlContent));
        }

        try
        {
            var baseUri = new Uri(url);            
            var htmlDoc = new HtmlDocument
            {
                OptionFixNestedTags = true, 
                OptionAutoCloseOnEnd = true,
                OptionCheckSyntax = true 
            };

            htmlDoc.LoadHtml(htmlContent);

            // Process <img> tags
            var imgTags = htmlDoc.DocumentNode.SelectNodes("//img");
            if (imgTags != null)
            {
                foreach (var imgTag in imgTags)
                {
                    // Process src attribute
                    var srcValue = imgTag.GetAttributeValue("src", string.Empty);
                    if (!string.IsNullOrEmpty(srcValue) && !Uri.TryCreate(srcValue, UriKind.Absolute, out _))
                    {
                        var absoluteUrl = new Uri(baseUri, srcValue).ToString();
                        imgTag.SetAttributeValue("src", absoluteUrl);
                    }

                    // Process data-cfsrc attribute
                    var dataCfsrcValue = imgTag.GetAttributeValue("data-cfsrc", string.Empty);
                    if (!string.IsNullOrEmpty(dataCfsrcValue) && !Uri.TryCreate(dataCfsrcValue, UriKind.Absolute, out _))
                    {
                        var absoluteUrl = new Uri(baseUri, dataCfsrcValue).ToString();
                        imgTag.SetAttributeValue("data-cfsrc", absoluteUrl);
                    }

                    // Process srcset attribute
                    var srcsetValue = imgTag.GetAttributeValue("srcset", string.Empty);
                    if (!string.IsNullOrEmpty(srcsetValue))
                    {
                        var updatedSrcset = ProcessSrcsetAttribute(baseUri, srcsetValue);
                        imgTag.SetAttributeValue("srcset", updatedSrcset);
                    }
                }
            }

            // Process <source> tags inside <picture>
            var sourceTags = htmlDoc.DocumentNode.SelectNodes("//picture/source[@srcset]");
            if (sourceTags != null)
            {
                foreach (var sourceTag in sourceTags)
                {
                    var srcsetValue = sourceTag.GetAttributeValue("srcset", string.Empty);
                    if (!string.IsNullOrEmpty(srcsetValue))
                    {
                        var updatedSrcset = ProcessSrcsetAttribute(baseUri, srcsetValue);
                        sourceTag.SetAttributeValue("srcset", updatedSrcset);
                    }
                }
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }
        catch (Exception ex)
        {
            throw new Exception("Error processing HTML content.", ex);
        }
    }

    /// <summary>
    /// Processes the srcset attribute to convert all relative URLs to absolute.
    /// </summary>
    /// <param name="baseUri">Base URI</param>
    /// <param name="srcsetValue">The srcset attribute value</param>
    /// <returns>Updated srcset value with absolute URLs</returns>
    private string ProcessSrcsetAttribute(Uri baseUri, string srcsetValue)
    {
        var parts = srcsetValue.Split(',');
        var updatedParts = new List<string>();

        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            var spaceIndex = trimmedPart.LastIndexOf(' ');

            if (spaceIndex > 0)
            {
                // Separate URL and descriptor (e.g., "image.jpg 1x")
                var urlPart = trimmedPart.Substring(0, spaceIndex).Trim();
                var descriptorPart = trimmedPart.Substring(spaceIndex).Trim();

                if (!Uri.TryCreate(urlPart, UriKind.Absolute, out _))
                {
                    // Make URL absolute
                    urlPart = new Uri(baseUri, urlPart).ToString();
                }

                updatedParts.Add($"{urlPart} {descriptorPart}");
            }
            else
            {
                // Handle URLs without descriptors
                if (!Uri.TryCreate(trimmedPart, UriKind.Absolute, out _))
                {
                    trimmedPart = new Uri(baseUri, trimmedPart).ToString();
                }

                updatedParts.Add(trimmedPart);
            }
        }

        return string.Join(", ", updatedParts);
    }

    /// <summary>
    /// Checks if the URL is absolute and returns it.
    /// </summary>
    /// <param name="UrlOrHtmlContent">Url or HTML content to check</param>
    /// <returns>The absolute URL or an empty string</returns>
    private string CheckAbsolteUrl(string UrlOrHtmlContent)
    {
        string url = string.Empty;

        if (Uri.IsWellFormedUriString(UrlOrHtmlContent, UriKind.Absolute))
        {
            Uri? siteUri;

            if (Uri.TryCreate(UrlOrHtmlContent, UriKind.Absolute, out siteUri))
            {
                url = siteUri.ToString();
            }
        }

        return url;
    }

    /// <summary>
    /// Uploads a file to Azure Blob Storage and returns the URL.
    /// </summary>
    /// <param name="fileContent">File content (byte array)</param>
    /// <returns>Blob URL with SAS</returns>
    /// <exception cref="ArgumentException">File content is invalid</exception>
    /// <exception cref="Exception">Error uploading file</exception>
    private async Task<string> UploadFile(byte[] fileContent)
    {
        if (fileContent == null || fileContent.Length == 0)
        {
            throw new ArgumentException("File content cannot be empty.", nameof(fileContent));
        }

        try
        {
            var inspector = new ContentInspectorBuilder()
            {
                Definitions = MimeDetective.Definitions.Default.All()
            }.Build();

            var results = inspector.Inspect(fileContent);
            var fileExtensions = results.ByFileExtension();
            string fileExtension = fileExtensions == null || fileExtensions.Count() == 0 ? "unknown" : fileExtensions.FirstOrDefault()?.Extension ?? "";
            string fileName = $"{Guid.NewGuid()}.{fileExtension}";

            AzureBlob azureBlob = new AzureBlob(storageAccountName, storageAccountKey, storageContainerName);
            await azureBlob.UploadFileWithSdkAsync(fileContent, fileName);

            string blobUrl = SasGenerator.GenerateSasUri(storageAccountName,
                                                         storageAccountKey,
                                                         storageContainerName,
                                                         fileName,
                                                         DateTimeOffset.UtcNow.AddHours(1)).ToString();

            return blobUrl;
        }
        catch (Exception ex)
        {
            throw new Exception("Error uploading file.", ex);
        }
    }
}
