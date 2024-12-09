import React, { useState } from "react";

import { WCAGResult } from "./types/Accessibility";
import "./styles/tailwind.css";

import InputForm from "./components/InputForm";
import Results from "./components/Results";

import api from "./api/api";

const App: React.FC = () => {
  const [results, setResults] = useState<WCAGResult[]>([]);

  const handleAnalyze = async (url: string) => {
    try {
      const response = await api.post("/image", { url });
      setResults(response.data.results);
    } catch (error) {
      console.error("Error analyzing URL:", error);
      alert("Failed to analyze the URL. Please try again.");
    }
  };

  return (
    <div className="App">
      <h1>Web Accessibility Tool</h1>
      <InputForm onAnalyze={handleAnalyze} />
      <Results results={results} />
    </div>
  );
};

export default App;
