import React, { useState, useRef } from "react";
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

  // Ref for the results section
  const resultsRef = useRef<HTMLDivElement>(null);

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

      // Scroll to the results section after the result is set
      setTimeout(() => {
        resultsRef.current?.scrollIntoView({ behavior: "smooth" });
      }, 100);
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
        <motion.img
          src="/images/inclusivAI_header.jpg"
          alt="InclusivAI - AI-Powered Accessibility Checker for HTML and PDF header image"
          className="mx-auto"
          width={500}
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ duration: 0.5 }}
        />
        <h1 className="text-3xl font-bold text-gray-800 mt-4">InclusivAI</h1>
        <h2 className="text-2xl font-bold text-gray-800 mt-4">
          AI-Powered Accessibility Checker for HTML and PDF
        </h2>
        <h3 className="text-1xl font-bold text-gray-600 mt-3">
          Empowering Accessibility, One Website and Document at a Time
        </h3>
      </header>

      {/* Tabs Section */}
      <div className="tabs" role="tablist" aria-label="Input Type Tabs">
        {["url", "html", "pdf"].map((tab) => (
          <button
            key={tab}
            role="tab"
            aria-selected={activeTab === tab}
            aria-controls={`${tab}-panel`}
            className={`tab-button ${activeTab === tab ? "active" : ""}`}
            onClick={() => setActiveTab(tab)}
          >
            {tab.toUpperCase()}
          </button>
        ))}
      </div>

      {/* Dynamic Content Section */}
      <main role="tabpanel" id={`${activeTab}-panel`}>
        <InputForm activeTab={activeTab} handleAnalyze={handleAnalyze} />
        {isLoading ? (
          <Spinner />
        ) : (
          <div ref={resultsRef}>
            <Results result={result} />
          </div>
        )}
      </main>
    </motion.div>
  );
};

export default App;
