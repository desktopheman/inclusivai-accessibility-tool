import React, { useState } from "react";
import { AnalysisResult } from "./types/Accessibility";
import "./styles/tailwind.css";

import InputForm from "./components/InputForm";
import Results from "./components/Results";
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
        <h1 className="text-3xl font-bold text-gray-800">
          Website Accessibility Analyzer
        </h1>
      </header>
      <InputForm handleAnalyze={handleAnalyze} />
      {isLoading ? (
        <p className="loading-text">Analyzing...</p>
      ) : (
        <Results result={result} />
      )}
    </div>
  );
};

export default App;
