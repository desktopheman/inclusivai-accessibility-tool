import React, { useState, useRef } from "react";
import { AnalysisResult } from "./types/Accessibility";
import "./styles/tailwind.css";

import InputForm from "./components/InputForm";
import Results from "./components/Results";
import Spinner from "./components/Spinner";
import api from "./api/api";

const App: React.FC = () => {
  const [activeInput, setActiveInput] = useState<"url" | "html" | "pdf">("url");
  const [result, setResult] = useState<AnalysisResult>();
  const [isLoading, setIsLoading] = useState(false);
  const resultsRef = useRef<HTMLDivElement>(null);

  const handleAnalyze = async (
    input: string | File,
    type: string,
    useAssistant: boolean
  ) => {
    setIsLoading(true);
    try {
      const endpoint = `accessibility/${useAssistant ? `${type}WithAssistant` : `${type}WithChat`}`;
      let payload;

      if (type === "pdf") {
        const formData = new FormData();
        formData.append("file", input as File);
        payload = formData;
      } else {
        payload = type === "url" ? { Url: input } : { Html: input };
      }

      const response = await api.post(endpoint, payload, {
        headers:
          type === "pdf" ? { "Content-Type": "multipart/form-data" } : {},
      });

      setResult(response.data);

      setTimeout(
        () => resultsRef.current?.scrollIntoView({ behavior: "smooth" }),
        100
      );
    } catch (error) {
      console.error("Failed to analyze content:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="app-wrapper">
      <div className="app-container">
        <header className="header">
          <img
            src="/images/logo.svg"
            alt="InclusivAI Logo"
            className="logo"
            width="200px"
          />
          <p className="header-description">
            Compliance Checker <br />
          </p>
          <p className="header-details">
            Identifies WCAG (Web Content Accessibility Guidelines), ADA
            (Americans with Disabilities Act), Section 508 (Rehabilitation Act)
            and PDF/UA (ISO 14289-1) compliance issues and gives instructions
            for fixing them
          </p>
        </header>

        <div className="content-container">
          <InputForm
            activeInput={activeInput}
            setActiveInput={setActiveInput}
            handleAnalyze={handleAnalyze}
          />

          {isLoading && <Spinner />}

          <div ref={resultsRef} className="results-container">
            {result && <Results result={result} />}
          </div>
        </div>
      </div>
    </div>
  );
};

export default App;
