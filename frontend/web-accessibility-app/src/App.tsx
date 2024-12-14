import React, { useState } from "react";

import { AnalysisResult } from "./types/Accessibility";
import "./styles/tailwind.css";

import InputForm from "./components/InputForm";
import Results from "./components/Results";

import api from "./api/api";

const App: React.FC = () => {
  const [result, setResult] = useState<AnalysisResult>();
  const [isLoading, setIsLoading] = useState(false);

  const handleAnalyze = async (url: string) => {
    setIsLoading(true);
    try {
      const payload = {
        Url: url,
        GetImageDescriptions: true,
      };

      const response = await api.post("accessibility/urlWithChat", payload);
      setResult(response.data);
    } catch (error) {
      alert("Failed to analyze the URL. Please try again. Error: " + error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="App">
      <h1>Web Accessibility Tool</h1>
      <InputForm onAnalyze={handleAnalyze} />
      {isLoading ? (
        <p className="loading-text">Analyzing...</p>
      ) : (
        <Results result={result} />
      )}
    </div>
  );
};

export default App;
