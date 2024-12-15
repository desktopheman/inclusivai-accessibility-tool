import React, { useState } from "react";
import { AnalysisResult } from "./types/Accessibility";
import "./styles/tailwind.css";

import InputForm from "./components/InputForm";
import Results from "./components/Results";
import Spinner from "./components/Spinner";

import api from "./api/api";

import { motion } from "framer-motion";

const App: React.FC = () => {
  const [activeTab, setActiveTab] = useState("url");
  const [result, setResult] = useState<AnalysisResult>();
  const [isLoading, setIsLoading] = useState(false);

  /**
   * Function to handle analysis of different input types.
   * Supports URL, HTML content, and PDF file uploads.
   */
  const handleAnalyze = async (
    input: string | File,
    type: string,
    useAssistant: boolean
  ) => {
    setIsLoading(true);
    try {
      let payload;
      let endpoint = "";

      if (type === "url") {
        payload = { Url: input, GetImageDescriptions: true };
        endpoint = `accessibility/${useAssistant ? "urlWithAssistant" : "urlWithChat"}`;
      } else if (type === "html") {
        payload = { Html: input, GetImageDescriptions: true };
        endpoint = `accessibility/${useAssistant ? "htmlWithAssistant" : "htmlWithChat"}`;
      } else if (type === "pdf") {
        const formData = new FormData();
        formData.append("file", input as File);
        endpoint = `accessibility/${useAssistant ? "pdfWithAssistant" : "pdfWithChat"}`;
        payload = formData;
      }

      const response = await api.post(endpoint, payload, {
        headers:
          type === "pdf"
            ? { "Content-Type": "multipart/form-data" }
            : undefined,
      });

      setResult(response.data);
    } catch (error) {
      console.error("Failed to analyze content:", error);
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
      {/* Header Section */}
      <header className="text-center mb-6">
        {/* 
          Updated alt text for clarity and accessibility.
          Helps screen readers describe the purpose of the logo image.
        */}
        <motion.img
          src="/images/logo.png"
          alt="InclusivAI - Web Accessibility Analysis Tool Logo"
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
          Advanced AI-powered accessibility verification tool for websites, HTML
          content, and PDF documents
        </p>
      </header>

      {/* Tabs Section */}
      <div
        className="tabs"
        role="tablist"
        aria-label="Input Type Tabs" /* Improves accessibility for screen readers */
      >
        {["url", "html", "pdf"].map((tab) => (
          <button
            key={tab}
            role="tab" /* Adds proper semantic role for accessibility */
            aria-selected={activeTab === tab}
            aria-controls={`${tab}-panel`} /* Links to the panel content */
            className={`tab-button ${activeTab === tab ? "active" : ""}`}
            onClick={() => setActiveTab(tab)}
          >
            {tab.toUpperCase()}
          </button>
        ))}
      </div>

      {/* Dynamic Content Section */}
      <main role="tabpanel" id={`${activeTab}-panel`}>
        {/* Input Form */}
        <InputForm activeTab={activeTab} handleAnalyze={handleAnalyze} />
        {/* Loading Spinner or Results */}
        {isLoading ? <Spinner /> : <Results result={result} />}
      </main>
    </motion.div>
  );
};

export default App;
