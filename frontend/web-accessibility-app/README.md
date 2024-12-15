# InclusivAI Frontend

This repository contains the frontend application for InclusivAI, a tool designed to evaluate and improve web accessibility. Built with React, the frontend provides an intuitive interface to analyze accessibility issues and visualize actionable insights for both HTML and PDF content.

---

## Table of Contents

1. [Features](#features)
2. [Prerequisites](#prerequisites)
3. [Configuration](#configuration)
4. [Local Development](#local-development)
5. [Deployment](#deployment)
6. [Project Structure](#project-structure)
7. [API Integration](#api-integration)
8. [CI/CD with GitHub Actions](#cicd-with-github-actions)
9. [Authors](#authors)

---

## Features

- **Accessibility Analysis**:
  - Upload and analyze HTML and PDF files.
  - Visualize WCAG and PDF/UA compliance issues.
- **AI-Driven Suggestions**:
  - Leverages Azure Computer Vision for alt text suggestions.
  - Provides recommendations powered by Azure OpenAI.
- **Responsive Design**:
  - Optimized for desktop and mobile using Tailwind CSS.
- **API Integration**:
  - Connects to the backend API for accessibility analysis.
- **Hosting**:
  - Deployed using Azure Static Web Apps for seamless scalability.

---

## Prerequisites

1. **Node.js**: Ensure Node.js (v16 or above) is installed.
2. **Backend API**: The InclusivAI backend must be running and accessible.

---

## Configuration

1. Create a `.env` file in the root directory with the following content:

   ```env
   REACT_APP_BACKEND_API_URL=https://<your-backend-api-url>
   ```

2. Ensure the backend API is running and accessible.

---

## Local Development

### 1. Clone the Repository

```bash
git clone https://github.com/your-repo-name.git
cd frontend
```

### 2. Install Dependencies

```bash
npm install
```

### 3. Start the Development Server

```bash
npm start
```

The application will be available at `http://localhost:3000`.

### 4. Run Unit Tests

```bash
npm test
```

---

## Deployment

### 1. Build the Frontend

Generate a production-ready build:

```bash
npm run build
```

### 2. Deploy to Azure Static Web Apps

Using the Azure CLI:

```bash
az staticwebapp create --name <static-web-app-name> \
  --resource-group <resource-group> --source ./build \
  --location <location>
```

---

## Project Structure

```plaintext
frontend/
├── public/                          # Static assets
│   ├── images/                      # Image assets
│   ├── index.html                   # Main HTML file
│   ├── manifest.json                # Web manifest
│   └── robots.txt                   # Robots configuration
├── src/                             # Source files
│   ├── api/                         # API integration using Axios
│   │   └── api.ts                   # Functions to call backend endpoints
│   ├── components/                  # Reusable React components
│   │   ├── InputForm.tsx            # Component for input submission
│   │   ├── Results.tsx              # Component to display analysis results
│   │   └── Spinner.tsx              # Loading spinner component
│   ├── styles/                      # Tailwind CSS styles
│   │   └── tailwind.css             # Tailwind CSS configuration
│   ├── types/                       # TypeScript type definitions
│   │   ├── App.tsx                  # Main App types
│   │   ├── index.tsx                # Entry point types
│   │   └── react-app-env.d.ts       # React environment types
│   ├── App.tsx                      # Main application component
│   ├── index.tsx                    # Application entry point
│   ├── tailwind.config.js           # Tailwind configuration
│   └── setupTests.ts                # Testing setup file
├── .env                             # Environment variables
├── .eslintrc.js                     # ESLint configuration
├── .gitignore                       # Git ignore file
├── LICENSE                          # License file
├── package.json                     # Node.js project configuration
├── package-lock.json                # Lock file for npm
├── postcss.config.js                # PostCSS configuration
├── README.md                        # Documentation file
├── tsconfig.json                    # TypeScript configuration
└── tailwind.config.js               # Tailwind CSS config
```

---

## API Integration

### Backend Endpoints

#### Analyze Image
**POST** `/api/accessibility/imageUrl`

**Request Body**:
```json
{
  "url": "https://example.com/image.png"
}
```

**Response**:
```json
{
  "suggestedAltText": "A modern office setup with a laptop and desk accessories."
}
```

---

#### Analyze HTML from URL (Chat)
**POST** `/api/accessibility/urlWithChat`

**Request Body**:
```json
{
  "url": "https://example.com",
  "getImageDescriptions": true
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
  "summary": "Found accessibility issues on the provided URL."
}
```

---

#### Analyze HTML from URL (Assistant)
**POST** `/api/accessibility/urlWithAssistant`

**Request Body**:
```json
{
  "url": "https://example.com",
  "getImageDescriptions": true
}
```

**Response**:
```json
{
  "issues": [
    {
      "element": "button",
      "issue": "No accessible label",
      "recommendation": "Add an aria-label or text content."
    }
  ],
  "summary": "Assistant analyzed accessibility successfully."
}
```

---

#### Analyze HTML Content (Chat)
**POST** `/api/accessibility/htmlWithChat`

**Request Body**:
```json
{
  "html": "<html>...</html>",
  "getImageDescriptions": true
}
```

#**Response**:
```json
{
  "issues": [
    {
      "element": "img",
      "issue": "Missing alt attribute",
      "recommendation": "Provide an alt attribute for better accessibility."
    }
  ]
}
```

---

#### Analyze PDF (Chat)
**POST** `/api/accessibility/pdfWithChat`

**Request Body**: FormData

- **file**: PDF file to upload.

**Response**:
```json
{
  "issues": [
    {
      "page": 1,
      "issue": "No document title",
      "recommendation": "Add a title to improve PDF accessibility."
    }
  ]
}
```

---

## CI/CD with GitHub Actions

Automate the deployment to Azure Static Web Apps using GitHub Actions.

### 1. Create a Workflow File

Place the following file in `.github/workflows/frontend-deploy.yml`:

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
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: "16"
      - name: Install Dependencies
        run: npm install
      - name: Build Frontend
        run: npm run build
      - name: Deploy to Azure
        run: |
          az staticwebapp upload --name inclusivai-frontend \
                                --resource-group inclusivai-resources \
                                --source ./build
```

### 2. Secrets Configuration

Add Azure credentials to your repository's secrets under the name `AZURE_CREDENTIALS`.

---

## Authors

- **Fermin Piccolo**
  - [GitHub](https://github.com/frmpiccolo)
  - [LinkedIn](https://www.linkedin.com/in/ferminpiccolo/)
