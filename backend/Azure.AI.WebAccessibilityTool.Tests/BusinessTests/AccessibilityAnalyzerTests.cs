using System;
using Microsoft.Extensions.Configuration;
using AzureAI.WebAccessibilityTool.Services;
using Xunit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AzureAI.WebAccessibilityTool.Tests.BusinessTests
{
    /// <summary>
    /// Test suite for the <see cref="AccessibilityAnalyzer"/> class.
    /// </summary>
    public class AccessibilityAnalyzerTests
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the test class, setting up configuration for tests.
        /// </summary>
        public AccessibilityAnalyzerTests()
        {
            // Arrange mock configuration
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.Development.json");
                })
                .Build();

            _configuration = host.Services.GetRequiredService<IConfiguration>();
        }

        /// <summary>
        /// Tests that the <see cref="AccessibilityAnalyzer.AnalyzeImage(string)"/> method
        /// returns expected results when provided with a valid image URL.
        /// </summary>
        [Fact]
        public async Task AnalyzeImage_ValidUrl_ReturnsExpectedResults()
        {
            // Arrange
            var analyzer = new AccessibilityAnalyzer(_configuration);
            var testImageUrl = "https://cdn-dynmedia-1.microsoft.com/is/image/microsoftcorp/390938-comparing-models-acc1?resMode=sharp2&op_usm=1.5,0.65,15,0&wid=1600&hei=1272&qlt=100&fmt=png-alpha&fit=constrain";

            // Act
            var results = await analyzer.AnalyzeImageAsync(testImageUrl);

            // Assert
            Assert.NotNull(results); // Ensure the result is not null.            
            Assert.True(results.Count > 0); 
        }

        /// <summary>
        /// Tests that the <see cref="AccessibilityAnalyzer.AnalyzeHtml(string)"/> method
        /// throws an <see cref="ArgumentException"/> when provided with empty HTML content.
        /// </summary>
        [Fact]
        public async Task AnalyzeHtml_EmptyContent_ThrowsException()
        {
            // Arrange
            var analyzer = new AccessibilityAnalyzer(_configuration);
            var emptyHtmlContent = string.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => analyzer.AnalyzeHtmlAsync(emptyHtmlContent));
        }

        /// <summary>
        /// Tests that the <see cref="AccessibilityAnalyzer.AnalyzeHtml(string)"/> method
        /// returns expected results with 0 issues when provided with HTML content without accessibility issues.
        /// </summary>
        [Fact]
        public async Task AnalyzeHtml_ValidContent_ReturnsNoAdvisory()
        {
            // Arrange
            var analyzer = new AccessibilityAnalyzer(_configuration);
            var validHtmlContent = @"<!DOCTYPE html>
                                        <html lang=""en"">
                                        <head>
                                            <meta charset=""UTF-8"">
                                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                            <meta name=""description"" content=""This is a test HTML document demonstrating proper accessibility practices."">
                                            <meta name=""author"" content=""Your Name"">
                                            <title>Accessibility Test</title>
                                            <style>
                                                /* Ensuring sufficient color contrast */
                                                body {
                                                    background-color: #ffffff;
                                                    color: #1a1a1a; /* Dark gray text for good contrast */
                                                    font-family: Arial, sans-serif;
                                                }
                                                a {
                                                    color: #0056b3; /* Blue for links */
                                                    text-decoration: underline;
                                                }
                                                a:focus, a:hover {
                                                    color: #003d80; /* Darker blue for hover/focus state */
                                                }
                                                figcaption {
                                                    color: #4d4d4d; /* Medium gray text for captions with sufficient contrast */
                                                }
                                            </style>
                                        </head>
                                        <body>
                                            <header>
                                                <h1>Accessibility Test Page</h1>
                                            </header>
                                            <main>
                                                <figure>
                                                    <img 
                                                        src=""https://cdn-dynmedia-1.microsoft.com/is/image/microsoftcorp/390938-comparing-models-acc1?resMode=sharp2&op_usm=1.5,0.65,15,0&wid=1600&hei=1272&qlt=100&fmt=png-alpha&fit=constrain"" 
                                                        alt=""A high-quality image showing a computer on a desk with multiple accessories, including a keyboard, mouse, and monitor displaying vivid colors and clear text."" 
                                                        width=""800"" 
                                                        height=""636""
                                                    />
                                                    <figcaption>
                                                        This image demonstrates a modern computer setup, featuring a clear visual presentation of accessories and text on the screen with appropriate contrast.
                                                    </figcaption>
                                                </figure>
                                                <section>
                                                    <p>
                                                        The text on this page has been designed to meet accessibility standards with a contrast ratio above the minimum requirement of 4.5:1 for normal text and 3:1 for large text. This ensures readability for all users.
                                                    </p>
                                                </section>
                                            </main>
                                            <footer>
                                                <p>
                                                    &copy; 2024 Accessibility Test. All rights reserved. 
                                                    <a href=""privacy-policy.html"">Read our Privacy Policy to understand how we handle your data and protect your privacy</a>.
                                                </p>
                                            </footer>
                                        </body>
                                        </html>";

            // Act
            var result = await analyzer.AnalyzeHtmlAsync(validHtmlContent);

            // Assert            
            Assert.NotNull(result);
            Assert.True(result.Items.Where(e => !e.Severity.Equals("Low")).ToList().Count == 0);
        }

        /// <summary>
        /// Tests that the <see cref="AccessibilityAnalyzer.AnalyzeHtml(string)"/> method
        /// returns expected results with 1 or more issues when provided with HTML content with accessibility issues.
        /// </summary>        
        [Fact]
        public async Task AnalyzeHtml_InvalidContent_ReturnsAdvisory()
        {
            // Arrange
            var analyzer = new AccessibilityAnalyzer(_configuration);
            var invalidHtmlContent = "<html><head><title>Test</title></head><body><img src='https://cdn-dynmedia-1.microsoft.com/is/image/microsoftcorp/390938-comparing-models-acc1?resMode=sharp2&op_usm=1.5,0.65,15,0&wid=1600&hei=1272&qlt=100&fmt=png-alpha&fit=constrain' /></body></html>";

            // Act
            var result = await analyzer.AnalyzeHtmlAsync(invalidHtmlContent);

            // Assert            
            Assert.NotNull(result);
            Assert.True(result.Items.Count > 0 && !string.IsNullOrEmpty(result.Explanation)); 
        }
    }
}
