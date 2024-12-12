using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAI.WebAccessibilityTool.Helpers
{
    public static class HtmlHelper
    {
        /// <summary>
        /// Checks if the URL is absolute and returns it.
        /// </summary>
        /// <param name="urlToCheck">URL to check</param>
        /// <returns>The absolute URL or an empty string</returns>
        public static string CheckAbsolteUrl(string urlToCheck)
        {
            string url = string.Empty;

            if (Uri.IsWellFormedUriString(urlToCheck, UriKind.Absolute))
            {
                Uri? siteUri;

                if (Uri.TryCreate(urlToCheck, UriKind.Absolute, out siteUri))
                {
                    url = siteUri.ToString();
                }
            }

            return url;
        }

        /// <summary>
        /// Checks and fixes the HTML content by making all image-related URLs absolute.
        /// </summary>
        /// <param name="url">Source URL</param>
        /// <param name="htmlContent">HTML content</param>
        /// <returns>Checked and fixed HTML content</returns>    
        public static string CheckAndFixHtmlContent(string url, string htmlContent)
        {
            if (string.IsNullOrEmpty(CheckAbsolteUrl(url)))
            {
                throw new ArgumentException($"Invalid URL: {url}", nameof(url));
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

                        // Process href attribute
                        var hrefValue = imgTag.GetAttributeValue("href", string.Empty);
                        if (!string.IsNullOrEmpty(srcValue) && !Uri.TryCreate(srcValue, UriKind.Absolute, out _))
                        {
                            var absoluteUrl = new Uri(baseUri, srcValue).ToString();
                            imgTag.SetAttributeValue("href", hrefValue);
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
        private static string ProcessSrcsetAttribute(Uri baseUri, string srcsetValue)
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
    }
}
