# Frontend for Website Accessibility Solution

This is the frontend application for the **Website Accessibility Solution**, built with React. It provides an interactive user interface to analyze website accessibility, visualize WCAG issues, and receive actionable insights.

---

## Features

- **Interactive Analysis**:
  - Upload HTML content for accessibility analysis.
  - View WCAG issues and recommendations in real-time.
- **Image Analysis**:
  - Analyze image URLs for accessibility metadata like `alt` text suggestions.
- **Responsive Design**:
  - Optimized for desktop and mobile using Tailwind CSS.
- **API Integration**:
  - Connects to the backend API for analysis using Axios.
- **Hosting**:
  - Deployed using Azure Static Web Apps for seamless scalability.

---

## Prerequisites

1. **Node.js**: Ensure Node.js (v16 or above) is installed.
2. **Backend API**: The backend API must be deployed and accessible.

---

## Configuration

1. Create a `.env` file in the root directory with the following content:
   ```env
   REACT_APP_BACKEND_API_URL=https://<your-backend-api-url>
   ```

2. Ensure the backend API is running and accessible.

---

## Development Setup

### 1. Clone the Repository
```bash
git clone https://github.com/your-repo-name.git
cd frontend/web-accessibility-app
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
frontend/web-accessibility-app/
├── public/                          # Static assets
│   ├── index.html                   # Main HTML file
│   └── favicon.ico                  # Favicon
├── src/                             # Source files
│   ├── api/                         # API integration using Axios
│   │   └── api.ts                   # Functions to call backend endpoints
│   ├── components/                  # Reusable React components
│   │   ├── InputForm.tsx            # Component for HTML input
│   │   ├── Results.tsx              # Component to display analysis results
│   │   └── Header.tsx               # Application header
│   ├── styles/                      # Tailwind CSS styles
│   │   └── global.css               # Global CSS overrides
│   ├── types/                       # TypeScript type definitions
│   │   └── wcag.d.ts                # Types for WCAG issues and results
│   ├── App.tsx                      # Main application component
│   ├── index.tsx                    # Application entry point
│   └── reportWebVitals.ts           # Performance measurement
├── .env                             # Environment variables
├── package.json                     # Node.js project configuration
├── tailwind.config.js               # Tailwind CSS configuration
└── tsconfig.json                    # TypeScript configuration
```

---

## API Integration

### Backend Endpoints

- **HTML Analysis**
  - **Endpoint**: `/api/accessibility/analyze`
  - **Method**: `POST`
  - **Example Usage**:
    ```typescript
    import axios from 'axios';

    const analyzeHtml = async (htmlContent: string) => {
      const response = await axios.post(`${process.env.REACT_APP_BACKEND_API_URL}/api/accessibility/analyze`, {
        htmlContent,
      });
      return response.data;
    };
    ```

- **Image Analysis**
  - **Endpoint**: `/api/accessibility/analyze-image`
  - **Method**: `POST`
  - **Example Usage**:
    ```typescript
    const analyzeImage = async (imageUrl: string) => {
      const response = await axios.post(`${process.env.REACT_APP_BACKEND_API_URL}/api/accessibility/analyze-image`, {
        imageUrl,
      });
      return response.data;
    };
    ```

---

## CI/CD with GitHub Actions

Automate the deployment to Azure Static Web Apps using GitHub Actions.

1. **Create a Workflow File**: `.github/workflows/frontend-deploy.yml`

2. **Sample Workflow**:
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
        node-version: '16'
    - name: Install Dependencies
      run: npm install
    - name: Build Frontend
      run: npm run build
    - name: Deploy to Azure
      run: |
        az staticwebapp upload --name web-accessibility-app \
                              --resource-group website-accessibility \
                              --source ./frontend/web-accessibility-app/build
```

3. **Secrets Configuration**:
   - Add Azure credentials to your repository's secrets under the name `AZURE_CREDENTIALS`.

---

## Testing

### Run Tests
```bash
npm test
```

### Testing Tools
- **Jest**:
  - Unit testing framework for JavaScript and TypeScript.
- **React Testing Library**:
  - Used for component testing.

---

## Authors

- **Fermin Piccolo**
  - [GitHub](https://github.com/frmpiccolo)
  - [LinkedIn](https://www.linkedin.com/in/ferminpiccolo/)
- **Phillie Guimarães**
  - [GitHub](https://github.com/phiguimaraes)
  - [LinkedIn](https://www.linkedin.com/in/phillieguimar%C3%A3es/)
- **Tiago Santiago**
  - [GitHub](https://github.com/tsdes-santiago)

---

> ⚠ **Disclaimer**: This project uses Azure services, which may incur costs. Be sure to review Azure's pricing and monitor your resource usage.
