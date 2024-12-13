# Backend for Website Accessibility Solution

This is the backend API for the **Website Accessibility Solution**, designed to evaluate and improve website accessibility using Azure AI services. Built with .NET 8, the backend provides scalable and efficient endpoints for analyzing HTML content and generating accessibility insights.

---

## Features

- **Azure OpenAI Integration**:
  - Analyzes HTML content for WCAG issues and provides actionable recommendations.
- **Azure Computer Vision**:
  - Analyzes images for accessibility metadata, such as alt text suggestions.
- **Scalable Deployment**:
  - Built with .NET 8 for modern, high-performance API development.
- **Data Models**:
  - Supports structured analysis and recommendations through extensible models like `WCAGResult`.

---

## Prerequisites

1. **Azure Account**: Ensure you have an active Azure subscription.
2. **Azure Services**:
   - [Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/)
   - [Azure Computer Vision](https://learn.microsoft.com/en-us/azure/cognitive-services/computer-vision/)
3. **Development Environment**:
   - .NET 8 SDK installed.
   - Access to the Azure CLI.

---

## Configuration

Before running the backend, update the `appsettings.json` file with the following:

```json
{
  "AzureServices": {
    "ComputerVision": {
      "Endpoint": "https://<your-computer-vision-endpoint>",
      "ApiKey": "<your-computer-vision-api-key>"
    },
    "OpenAI": {
      "Endpoint": "https://<your-openai-endpoint>",
      "ApiKey": "<your-openai-api-key>"
    }
  }
}
```

---

## Development Setup

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
dotnet run
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
Using the Azure CLI:
```bash
az webapp create --name <app-name> --resource-group <resource-group> \
  --runtime "DOTNET|8" --plan <app-service-plan>"
az webapp deploy --name <app-name> --resource-group <resource-group> --src-path ./publish
```

---

## API Endpoints

### Analyze HTML
**POST** `/api/accessibility/analyze`
- **Request Body**: 
  ```json
  {
    "htmlContent": "<html>...</html>"
  }
  ```
- **Response**:
  ```json
  {
    "issues": [
      {
        "element": "img", 
        "attributes": [
          { "name": "src", "value": "https://example.com/image.png" }
        ], 
        "issue": "Missing alt attribute",
        "severity": "High",
        "recommendation": "Add an alt attribute with a meaningful description.", 
        "imageDescriptionRecommendation": "This image demonstrates a modern computer setup, featuring a clear visual presentation of accessories and text on the screen with appropriate contrast."
      }
    ],
    "explanation": "The HTML contains images without alt attributes, which are essential for screen readers."
  }
  ```

### Analyze Image
**POST** `/api/accessibility/analyze-image`
- **Request Body**:
  ```json
  {
    "imageUrl": "https://example.com/image.jpg"
  }
  ```
- **Response**:
  ```json
  {
    "descriptions": ["A scenic view of mountains during sunset."]
  }
  ```

---

## Project Structure

```plaintext
backend/
├── Azure.AI.WebAccessibilityTool/          # Core library with models and services
│   ├── Models/                             # Data models (e.g., WCAGResult)
│   ├── Services/                           # Business logic for Azure integration
├── Azure.AI.WebAccessibilityTool.API/      # API layer exposing endpoints
│   ├── Controllers/                        # API controllers (e.g., AccessibilityController)
│   └── appsettings.json                    # Azure configuration
└── Azure.AI.WebAccessibilityTool.Tests/    # Unit tests
    ├── ApiTests/                           # API endpoint tests
    ├── BusinessTests/                      # Service logic tests
    └── GlobalUsings.cs                     # Common using directives
```

---

## CI/CD with GitHub Actions

### Backend Deployment Workflow
Automate deployment to Azure App Service using GitHub Actions.

1. **Create a Workflow File**: `.github/workflows/backend-deploy.yml`

2. **Sample Workflow**:
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
      run: dotnet build ./backend/WebAccessibility.Api/WebAccessibility.Api.csproj
    - name: Deploy to Azure
      run: |
        az webapp deploy --resource-group website-accessibility \
                        --name web-accessibility-api \
                        --src-path ./backend/WebAccessibility.Api/bin/Release/net8.0/publish
```

3. **Secrets Configuration**:
   - Add `AZURE_CREDENTIALS` to your repository's secrets with Azure Service Principal credentials.

---

## Authors

- **Fermin Piccolo**
  - [GitHub](https://github.com/frmpiccolo)
  - [LinkedIn](https://www.linkedin.com/in/ferminpiccolo/)

---

> ⚠ **Disclaimer**: This project uses Azure services, which may incur costs. Be sure to review Azure's pricing and monitor your resource usage.
