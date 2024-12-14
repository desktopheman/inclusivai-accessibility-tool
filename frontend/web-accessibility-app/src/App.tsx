import React, { useState } from "react";
import { AnalysisResult } from "./types/Accessibility";
import "./styles/tailwind.css";

import InputForm from "./components/InputForm";
import Results from "./components/Results";
import Spinner from "./components/Spinner";
import api from "./api/api";
import { motion } from "framer-motion";

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
    <motion.div
      className="App"
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.5 }}
    >
      <header className="text-center mb-6">
        <motion.img
          src="/images/logo.png"
          alt="InclusivAI logo"
          className="mx-auto"
          width={200}
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ duration: 0.5 }}
        />
        <h1 className="text-3xl font-bold text-gray-800 mt-4">
          Accessibility Analyzer
        </h1>
        <p className="mt-2 text-gray-600">
          Advanced AI-powered accessibility verification tool for websites and
          PDF documents
        </p>
      </header>
      <InputForm handleAnalyze={handleAnalyze} />
      {isLoading ? <Spinner /> : <Results result={result} />}
    </motion.div>
  );
};

export default App;
