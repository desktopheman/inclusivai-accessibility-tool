import React, { useState } from "react";
import { AnalysisResult } from "./types/Accessibility";
import "./styles/tailwind.css";

import InputForm from "./components/InputForm";
import Results from "./components/Results";
import Spinner from "./components/Spinner";
import api from "./api/api";

const App: React.FC = () => {
  const [result, setResult] = useState<AnalysisResult>();
  const [isLoading, setIsLoading] = useState(false);

  const handleAnalyze = async (url: string, useAssistant: boolean) => {
    setIsLoading(true);
    try {
      const payload = { Url: url, GetImageDescriptions: true };
      const endpoint = `accessibility/${useAssistant ? "urlWithAssistant" : "urlWithChat"}`;
      const response = await api.post(endpoint, payload);
      setResult(response.data);
    } catch (error) {
      console.error("Failed to analyze URL:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="App">
      <header className="text-center mb-6">
        <h1 className="text-3xl font-bold text-gray-800">InclusivAI</h1>
        <p>&nbsp;</p>
        <h2>
          <strong>InclusivAI</strong> is an advanced AI-powered accessibility
          verification tool for websites. Leveraging Azure OpenAI with GPT-4,
          InclusivAI provides comprehensive HTML analysis via chat and assistant
          interfaces, identifying compliance issues with WCAG (Web Content
          Accessibility Guidelines), ADA (Americans with Disabilities Act), and
          Section 508 (Rehabilitation Act) standards.
        </h2>
        <h3>
          Additionally, InclusivAI integrates Azure Computer Vision to
          intelligently suggest alternative text (alt text) for images,
          enhancing accessibility for visually impaired users. With its robust
          and user-friendly approach, InclusivAI empowers developers and
          organizations to create inclusive, accessible digital experiences.
        </h3>
      </header>
      <InputForm handleAnalyze={handleAnalyze} />
      {isLoading ? (
        <>
          <p className="loading-text">Analyzing...</p>
          <Spinner />
        </>
      ) : (
        <Results result={result} />
      )}
    </div>
  );
};

export default App;
