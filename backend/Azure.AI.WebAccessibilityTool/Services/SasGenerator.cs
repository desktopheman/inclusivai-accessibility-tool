using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAI.WebAccessibilityTool.Services;

public static class SasGenerator
{
    public static Uri GenerateSasUri(string accountName, string accountKey, string containerOrShareName, string resourcePath, DateTimeOffset expiresOn)
    {
        var credentials = new Azure.Storage.StorageSharedKeyCredential(accountName, accountKey);
        var blobClient = new BlobClient(new Uri($"https://{accountName}.blob.core.windows.net/{containerOrShareName}/{resourcePath}"), credentials);

        return blobClient.GenerateSasUri(BlobSasPermissions.Read | BlobSasPermissions.Write, expiresOn);
    }
}
