using MimeDetective;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAI.WebAccessibilityTool.Helpers;

/// <summary>
/// Helper class for file operations.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Gets the MIME type and file extension from the file content.
    /// </summary>
    /// <param name="fileContent">File conetnt (byte array)</param>
    /// <returns><MimeType and Extension/returns>
    public static (string, string) GetMimeTypeAndExtension(byte[] fileContent)
    {
        var inspector = new ContentInspectorBuilder()
        {
            Definitions = MimeDetective.Definitions.Default.All()
        }.Build();

        var results = inspector.Inspect(fileContent);
        var fileExtensions = results.ByFileExtension();
        var mimeTypes = results.ByMimeType();

        string fileExtension = fileExtensions == null || fileExtensions.Count() == 0 ? "unknown" : fileExtensions.FirstOrDefault()?.Extension ?? "";
        string mimeType = mimeTypes == null || mimeTypes.Count() == 0 ? "application/octet-stream" : mimeTypes.FirstOrDefault()?.MimeType ?? "";

        return (mimeType, fileExtension);
    }

    /// <summary>
    /// Uploads a file to Azure Blob Storage and returns the URL.
    /// </summary>
    /// <param name="fileContent">File content (byte array)</param>
    /// <returns>Blob URL with SAS</returns>
    /// <exception cref="ArgumentException">File content is invalid</exception>
    /// <exception cref="Exception">Error uploading file</exception>
    public static async Task<string> UploadFile(string storageAccountName, string storageAccountKey, string storageContainerName, byte[] fileContent)
    {
        if (fileContent == null || fileContent.Length == 0)
        {
            throw new ArgumentException("File content cannot be empty.", nameof(fileContent));
        }

        try
        {
            var (mimeType, fileExtension) = GetMimeTypeAndExtension(fileContent);
            string fileName = $"{Guid.NewGuid().ToString()}.{fileExtension}";

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
