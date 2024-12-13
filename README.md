# Website Accessibility Solution with AI

This repository provides a comprehensive solution for evaluating and improving website accessibility using Azure's AI services. It combines a robust backend built with .NET 8 and a modern frontend built with React to analyze website accessibility and generate actionable insights.

---

> ⚠ **Disclaimer**: This project uses Azure services, which may incur costs. Be sure to review Azure's pricing and monitor your resource usage to avoid unexpected charges.

---

## Project Scope

The solution aims to streamline the process of identifying and addressing website accessibility issues by leveraging cutting-edge Azure AI services. The key features include:

- **Azure OpenAI Integration**: Processes natural language for accessibility recommendations.
- **Azure Computer Vision**: Analyzes visual content to detect potential accessibility barriers.
- **Data Storage**: Uses Azure Blob Storage to manage accessibility reports and logs.
- **Frontend Hosting**: Leverages Azure Static Web Apps for a responsive, interactive user interface.
- **Backend Deployment**: Operates using Azure App Service for scalable API handling.

---

## Azure Resource Setup

Below are the Azure services and the step-by-step instructions to configure them via both the Azure Portal and Azure CLI.

### 1. Create Computer Vision API

#### Azure Portal:
1. Go to Azure Portal and search for "Cognitive Services."
2. Click "Create" and select **Computer Vision** as the resource type.
3. Fill in the required fields (resource group, region, etc.) and click "Review + Create."
4. Navigate to the resource to retrieve the **API Key** and **Endpoint**.

#### Azure CLI:
```bash
az cognitiveservices account create --name <name> \
  --resource-group <resource-group> --kind ComputerVision \
  --sku S1 --location <location> --yes
```
Retrieve the API Key and Endpoint:
```bash
az cognitiveservices account keys list --name <name> --resource-group <resource-group>
```

---

### 2. Create Azure OpenAI Service

#### Azure Portal:
1. Search for **Azure OpenAI** in the Azure Portal.
2. Click "Create" and follow the instructions to deploy the resource.
3. Assign access permissions under "Access Control (IAM)."
4. Retrieve the **API Key** and **Endpoint**.

#### Azure CLI:
```bash
az cognitiveservices account create --name <name> \
  --resource-group <resource-group> --kind OpenAI \
  --sku S1 --location <location> --yes
```
Retrieve the API Key:
```bash
az cognitiveservices account keys list --name <name> --resource-group <resource-group>
```

---

### 3. Create Blob Storage

#### Azure Portal:
1. Navigate to the Azure Portal and search for "Storage Accounts."
2. Click "Create" and fill in the required fields.
3. After creation, navigate to the resource, and create a **container** for logs and reports.

#### Azure CLI:
```bash
az storage account create --name <name> \
  --resource-group <resource-group> --location <location> --sku Standard_LRS
```
Create a container:
```bash
az storage container create --account-name <name> --name <container-name>
```

---

### 4. Deploy App Service (Web App)

#### Azure Portal:
1. Search for "App Service" in the Azure Portal.
2. Click "Create" and select **.NET 8 (LTS)** as the runtime stack.
3. Configure the deployment settings and deploy the backend code.

#### Azure CLI:
```bash
az webapp create --name <app-name> --resource-group <resource-group> \
  --runtime "DOTNET|8" --plan <app-service-plan>
```

---

### 5. Set Up Azure Static Web Apps

#### Azure Portal:
1. Search for "Static Web Apps" in the Azure Portal.
2. Click "Create" and follow the instructions to link your GitHub repository for CI/CD.

#### Azure CLI:
```bash
az staticwebapp create --name <static-web-app-name> \
  --resource-group <resource-group> --source <frontend-directory> \
  --location <location>
```

---

### 6. (Optional) Add Azure Functions

#### Azure Portal:
1. Search for "Function App" in the Azure Portal.
2. Create a serverless function and configure the desired triggers.

#### Azure CLI:
```bash
az functionapp create --name <function-app-name> \
  --resource-group <resource-group> --consumption-plan-location <location>
```

---

## Solution Structure

### Backend
- **Path**: `/backend`
- **Projects**:
  - **`Azure.AI.WebAccessibilityTool`**:
    - **`Models`**: Represents data models like `WCAGResult`.
    - **`Services`**: Core logic for analyzing accessibility using Azure AI services.
    - **`Utils`**: Helper classes for HTTP requests and other utilities.
  - **`Azure.AI.WebAccessibilityTool.API`**:
    - **`Controllers`**: Exposes API endpoints like `AccessibilityController`.
    - **`appsettings.json`**: Configuration for Azure services (keys, endpoints, etc.).
  - **`Azure.AI.WebAccessibilityTool.Tests`**:
    - **`ApiTests`**: Tests for API endpoints.
    - **`BusinessTests`**: Tests for backend logic and services.
    - **`GlobalUsings.cs`**: Common usings for the test suite.

### Frontend
- **Path**: `/frontend/web-accessibility-app`
- **Structure**:
  - `/src`:
    - **`api`**: API calls with Axios (`api.ts`).
    - **`components`**: Reusable React components (`InputForm`, `Results`).
    - **`styles`**: Tailwind CSS setup.
    - **`types`**: TypeScript type definitions.
  - `/public`: Static assets like `index.html`.
  - **Configuration Files**:
    - `.env`: Backend API URL.
    - `tailwind.config.js`: Tailwind CSS configuration.
    - `jest.config.js`: Jest testing configuration.

---

## Getting Started

### Backend Setup
1. Navigate to `/backend`:
   ```bash
   cd backend
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Update `appsettings.json` with Azure API keys and endpoints.
4. Run the backend:
   ```bash
   dotnet run
   ```
5. Run tests:
   ```bash
   dotnet test
   ```

### Frontend Setup
1. Navigate to `/frontend/web-accessibility-app`:
   ```bash
   cd frontend/web-accessibility-app
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Update `.env` with the backend API URL.
4. Start the development server:
   ```bash
   npm start
   ```
5. Run tests:
   ```bash
   npm test
   ```

---

## Deployment

### Backend Deployment
1. Publish the backend:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
2. Deploy to Azure App Service:
   ```bash
   az webapp deploy --resource-group <resource-group> --name <app-name> --src-path ./publish
   ```

### Frontend Deployment
1. Build the frontend:
   ```bash
   npm run build
   ```
2. Deploy to Azure Static Web Apps:
   ```bash
   az staticwebapp update --name <static-web-app-name> --resource-group <resource-group> --source ./build
   ```

### GitHub Actions for Automated Deployment

This solution includes GitHub Actions workflows to automate the deployment process for both the backend and frontend. By using these workflows, you can ensure that your application is built and deployed to Azure every time a change is pushed to the `main` branch.

#### Backend Deployment Workflow

The `backend-deploy.yml` workflow is used to build and deploy the backend API to Azure App Service. Below is an overview of how the workflow operates:

- **Trigger**: The workflow runs on every push to the `main` branch.
- **Steps**:
  1. Check out the repository using the `actions/checkout` action.
  2. Set up the .NET environment using the `actions/setup-dotnet` action with .NET 8.
  3. Build the backend project.
  4. Deploy the backend API to Azure App Service using the `az webapp deploy` CLI command.

##### Workflow File (`.github/workflows/backend-deploy.yml`):
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

#### Frontend Deployment Workflow

The `frontend-deploy.yml` workflow automates the build and deployment of the frontend to Azure Static Web Apps. Here's how it works:

- **Trigger**: The workflow runs on every push to the `main` branch.
- **Steps**:
  1. Check out the repository using the `actions/checkout` action.
  2. Set up the Node.js environment using the `actions/setup-node` action with Node.js version 16.
  3. Install frontend dependencies.
  4. Build the frontend using the `npm run build` command.
  5. Deploy the built frontend to Azure Static Web Apps using the `az staticwebapp upload` CLI command.

##### Workflow File (`.github/workflows/frontend-deploy.yml`):
```yaml
name: Deploy Frontend
on:
  push:
    branches:
      - main
jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Install Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '16'
    - name: Install dependencies
      run: npm install
    - name: Build
      run: npm run build
    - name: Deploy to Azure
      run: |
        az staticwebapp upload --name web-accessibility-app \
                              --resource-group website-accessibility \
                              --source ./frontend/build
```

#### How to Set Up GitHub Actions Workflows

1. **Create the Workflow Files**:
   - Place the `backend-deploy.yml` file in the `.github/workflows` directory.
   - Place the `frontend-deploy.yml` file in the same directory.

2. **Configure Azure CLI**:
   - Ensure your GitHub Actions environment has access to your Azure subscription.
   - Use the `az login` command locally and set up a service principal for automated access:
     ```bash
     az ad sp create-for-rbac --name "<github-actions-service-principal>" --role contributor \
         --scopes /subscriptions/<subscription-id> --sdk-auth
     ```
   - Add the service principal credentials to your repository's secrets under the name `AZURE_CREDENTIALS`.

3. **Monitor Deployments**:
   - Navigate to the "Actions" tab in your GitHub repository to monitor the workflow runs.
   - Successful runs will deploy the backend to Azure App Service and the frontend to Azure Static Web Apps.

This automated deployment process ensures smooth CI/CD integration, saving time and reducing the chances of human error during deployment.

---

## Disclaimer

---

> ⚠ This project uses Azure services, which may incur costs. Be sure to review Azure's pricing and monitor your resource usage to avoid unexpected charges.

---

## Authors

- **Fermin Piccolo**
  - [GitHub](https://github.com/frmpiccolo)
  - [LinkedIn](https://www.linkedin.com/in/ferminpiccolo/)

```
