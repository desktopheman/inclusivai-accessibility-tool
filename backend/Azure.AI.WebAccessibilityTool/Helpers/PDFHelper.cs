using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace AzureAI.WebAccessibilityTool.Helpers
{
    /// <summary>
    /// A helper class that provides methods to extract comprehensive content and structure from PDF files.
    /// </summary>
    public static class PDFHelper
    {
        /// <summary>
        /// Extracts content, structure, metadata, and other details from a PDF file.
        /// </summary>
        /// <param name="pdfContent">The PDF content as a byte array.</param>
        /// <returns>A JSON string containing the extracted data.</returns>
        public static string ExtractPdfContent(byte[] pdfContent)
        {
            var pdfStructure = new Dictionary<string, object>();
            var pages = new List<Dictionary<string, object>>();

            using (var document = PdfDocument.Open(pdfContent))
            {
                // Document-level metadata
                pdfStructure["NumberOfPages"] = document.NumberOfPages;
                pdfStructure["Title"] = document.Information.Title ?? "";
                pdfStructure["Author"] = document.Information.Author ?? "";
                pdfStructure["Subject"] = document.Information.Subject ?? "";
                pdfStructure["Keywords"] = document.Information.Keywords ?? "";
                pdfStructure["CreationDate"] = document.Information.CreationDate ?? "";
                pdfStructure["ModificationDate"] = document.Information.ModifiedDate ?? "";
                pdfStructure["Producer"] = document.Information.Producer ?? "";

                // Extract content for each page
                foreach (var page in document.GetPages())
                {
                    var pageContent = new Dictionary<string, object>
                    {
                        ["PageNumber"] = page.Number,
                        ["Text"] = ExtractText(page),
                        ["Words"] = ExtractWords(page),
                        ["Images"] = ExtractImages(page),
                        ["Annotations"] = ExtractAnnotations(page),
                        ["Dimensions"] = new { page.Width, page.Height }
                    };
                    pages.Add(pageContent);
                }
            }

            pdfStructure["Pages"] = pages;

            return JsonSerializer.Serialize(pdfStructure, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Extracts textual content from a given page.
        /// </summary>
        static string ExtractText(Page page) => page.Text;

        /// <summary>
        /// Extracts individual words from a given page.
        /// </summary>
        static List<object> ExtractWords(Page page)
        {
            var words = new List<object>();
            foreach (var word in page.GetWords())
            {
                words.Add(new
                {
                    word.Text,
                    word.BoundingBox,
                    word.FontName
                });
            }
            return words;
        }

        /// <summary>
        /// Extracts annotations from a given page.
        /// </summary>
        static List<object> ExtractAnnotations(Page page)
        {
            var annotations = new List<object>();
            foreach (var annotation in page.ExperimentalAccess.GetAnnotations())
            {
                annotations.Add(new
                {
                    annotation.Type,
                    annotation.Name,
                    annotation.Action,
                    annotation.Rectangle,
                    annotation.Flags,
                    annotation.QuadPoints,
                    annotation.Content
                });
            }
            return annotations;
        }        

        /// <summary>
        /// Extracts images from a given page (Placeholder).
        /// </summary>
        static List<object> ExtractImages(Page page) => new();
    }
}
