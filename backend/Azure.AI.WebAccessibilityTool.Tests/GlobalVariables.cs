using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAI.WebAccessibilityTool.Tests
{
    public static class GlobalVariables
    {        
        public readonly static string invalidUrl = "https://www.w3.org/WAI/demos/bad/before/home.html";
        public readonly static string emptyUrl = string.Empty;
        public readonly static string validUrl = "https://www.w3.org/WAI/demos/bad/after/home.html";

        public readonly static string testImageUrl = "https://www.w3.org/WAI/demos/bad/after/img/oldenburgstudentviolin34.jpg";
        
        public readonly static string invalidHtmlContent = $"<html><head><title>Test</title></head><body><img src='{testImageUrl}' /></body></html>";        
        public readonly static string emptyHtmlContent = string.Empty;
        public readonly static string validHtmlContent = @"<!DOCTYPE html>
                                                            <html lang='en'>
                                                            <head>
                                                                <meta charset='UTF-8'>
                                                                <meta name='viewport'  content='width=device-width, initial-scale=1.0'>
                                                                <meta name='description'  content='This is a test HTML document demonstrating proper accessibility practices.'>
                                                                <meta name='author'  content='Your Name'>
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
                                                                            src='https://www.w3.org/WAI/demos/bad/after/img/oldenburgstudentviolin34.jpg'  
                                                                            alt='A high-quality image showing a computer on a desk with multiple accessories, including a keyboard, mouse, and monitor displaying vivid colors and clear text.'  
                                                                            width='800'  
                                                                            height='636' 
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
                                                                        <a href='privacy-policy.html'>Read our Privacy Policy to understand how we handle your data and protect your privacy</a>.
                                                                    </p>
                                                                </footer>
                                                            </body>
                                                            </html>";
    }
}
