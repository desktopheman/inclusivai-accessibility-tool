# InclusivAI Backend

This repository contains the backend API for InclusivAI, a tool designed to verify and improve web accessibility. Built with **.NET 8**, the backend provides scalable, efficient endpoints to analyze HTML and PDF content, leveraging Azure AI services.

---

> ⚠ **Disclaimer**: This project uses Azure services, which may incur costs. Be sure to review Azure's pricing and monitor your resource usage to avoid unexpected charges.

---

## Table of Contents

1. [Features](#features)
2. [Prerequisites](#prerequisites)
3. [Configuration](#configuration)
4. [Local Development](#local-development)
5. [Deployment](#deployment)
6. [API Endpoints](#api-endpoints)
7. [Project Structure](#project-structure)
8. [CI/CD with GitHub Actions](#cicd-with-github-actions)
9. [Authors](#authors)

---

## Features

- **HTML and PDF Accessibility Analysis**:
  - Analyze HTML content for WCAG, ADA, and Section 508 compliance.
  - Validate PDF content against PDF/UA standards.
- **AI-Driven Insights**:
  - Integrates with **Azure OpenAI** to provide recommendations for accessibility issues.
  - Uses **Azure Computer Vision** to extract alt text suggestions for images.
- **Extensible Architecture**:
  - Modular and scalable architecture built with .NET 8.
- **Logging and Reports**:
  - Pre-configured structure for generating reports and logs.

---

## Prerequisites

1. **Azure Account**: Active Azure subscription.
2. **Azure Services**:
   - [Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/)
   - [Azure Computer Vision](https://learn.microsoft.com/en-us/azure/cognitive-services/computer-vision/)
3. **Development Environment**:
   - .NET 8 SDK.
   - Azure CLI installed.

---

## Configuration

Before running the backend, configure your Azure services in `appsettings.json` and `appsettings.Development.json`:

```json
{
  "AzureServices": {
    "ComputerVision": {
      "Endpoint": "https://<your-computer-vision-endpoint>.cognitiveservices.azure.com/",
      "ApiKey": "<your-computer-vision-key>"
    },
    "OpenAI": {
      "Endpoint": "https://<your-openai-endpoint>.openai.azure.com/",
      "ApiKey": "<your-openai-key>",
      "DeploymentName": "<your-openai-deployment-name>",
      "AssistantId": "<your-openai-assistant-id>"
    },
    "Storage": {
      "AccountName": "<your-storage-account-name>",
      "AccountKey": "<your-storage-account-key>",
      "ContainerName": "<your-storage-container-name>"
    }
  },
  "AllowedHosts": "*",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

---

## Local Development

### 1. Clone the Repository

```bash
git clone https://github.com/your-repo-name.git
cd backend
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Run the Backend

```bash
dotnet run --project AzureAI.WebAccessibilityTool.API
```

### 4. Run Unit Tests

```bash
dotnet test
```

---

## Deployment

### 1. Publish the Backend

Generate a production-ready build:

```bash
dotnet publish -c Release -o ./publish
```

### 2. Deploy to Azure App Service

```bash
az webapp create --name <app-name> --resource-group <resource-group> \
  --runtime "DOTNET|8" --plan <app-service-plan>
az webapp deploy --name <app-name> --resource-group <resource-group> --src-path ./publish
```

---

## API Endpoints

### Analyze HTML
**POST** `/api/accessibility/analyze`

**Request Body**:
```json
{
  "htmlContent": "<html>...</html>"
}
```

**Response**:
```json
{
  "issues": [
    {
      "element": "img",
      "issue": "Missing alt attribute",
      "recommendation": "Add alt text for better accessibility."
    }
  ],
  "summary": "Found 3 critical accessibility issues."
}
```

### Analyze Image
**POST** `/api/accessibility/analyze-image`

**Request Body**:
```json
{
  "imageUrl": "https://example.com/image.png"
}
```

**Response**:
```json
{
  "suggestedAltText": "A modern office setup with a laptop and desk accessories."
}
```

---

## Project Structure

```plaintext
backend/
├── src/
│   ├── AzureAI.WebAccessibilityTool/      # Core library
│   │   ├── Helpers/                       # Helper classes
│   │   │   ├── AzureBlob.cs
│   │   │   ├── FileHelper.cs
│   │   │   ├── HtmlHelper.cs
│   │   │   ├── PdfHelper.cs
│   │   │   ├── ResourceHelper.cs
│   │   │   └── SasGenerator.cs
│   │   ├── Models/                        # Data models
│   │   │   ├── AnalysisInput.cs
│   │   │   └── AnalysisResult.cs
│   │   ├── Resources/                     # Prompt templates
│   │   │   ├── Prompt_HTML.txt
│   │   │   └── Prompt_PDF.txt
│   │   └── Services/                      # Business logic
│   │       └── AccessibilityAnalyzer.cs
│   ├── AzureAI.WebAccessibilityTool.API/  # API layer
│   │   ├── Controllers/                   # API endpoints
│   │   │   └── AccessibilityController.cs
│   │   ├── Models/                        # API-specific models
│   │   │   ├── ErrorOutput.cs
│   │   │   ├── HtmlInput.cs
│   │   │   └── UrlInput.cs
│   │   ├── appsettings.json               # Configuration
│   │   ├── appsettings.Development.json   # Dev configuration
│   │   └── Program.cs                     # Main entry point
├── tests/
│   └── AzureAI.WebAccessibilityTool.Tests # Unit tests
│       ├── ApiTests/                      # API tests
│       ├── BusinessTests/                 # Service tests
│       └── GlobalUsings.cs                # Common usings
└── AzureAI.WebAccessibilityTool.sln       # Solution file
```

---

## CI/CD with GitHub Actions

Automate deployment to Azure App Service.

**Workflow File**: `.github/workflows/backend-deploy.yml`

```yaml
name: Deploy Backend
on:
  push:
    branches:
      - main
jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Build
        run: dotnet build ./src/AzureAI.WebAccessibilityTool.API/AzureAI.WebAccessibilityTool.API.csproj
      - name: Deploy to Azure
        run: |
          az webapp deploy --resource-group inclusivai-resources \
                          --name inclusivai-backend-api \
                          --src-path ./src/AzureAI.WebAccessibilityTool.API/bin/Release/net8.0/publish
```

---

## Authors

- **Fermin Piccolo**
  - [GitHub](https://github.com/frmpiccolo)
  - [LinkedIn](https://www.linkedin.com/in/ferminpiccolo/)


---

> ⚠ **Disclaimer**: This project uses Azure services, which may incur costs. Be sure to review Azure's pricing and monitor your resource usage to avoid unexpected charges.

---