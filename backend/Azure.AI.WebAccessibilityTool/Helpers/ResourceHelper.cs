using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AzureAI.WebAccessibilityTool.Helpers;

public static class ResourceHelper
{
    /// <summary>
    /// Get the content of a resource file
    /// </summary>
    /// <param name="resourceName">Resource file name</param>
    /// <returns>Resource content</returns>    
    public static string GetResourceContent(string resourceName)
    {
        string resourceContent = string.Empty;

        Assembly assembly = Assembly.GetExecutingAssembly();
        if (assembly == null)
            throw new Exception("Error loading the assembly to get the prompt file");

        string resourcePath = $"{assembly.GetName().Name}.Resources.{resourceName}";
        Stream? stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
            throw new Exception("Error loading the prompt resource file.");

        StreamReader reader = new StreamReader(stream);
        if (reader == null)
            throw new Exception("Error reading the prompt resource file.");

        resourceContent = reader.ReadToEnd();

        if (string.IsNullOrEmpty(resourceContent))
            throw new Exception("The prompt resource file is empty");

        return resourceContent;
    }
}
