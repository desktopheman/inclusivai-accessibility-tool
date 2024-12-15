import React, { useState } from "react";
import { motion } from "framer-motion";

interface InputFormProps {
  activeTab: string;
  handleAnalyze: (
    input: string | File,
    type: string,
    useAssistant: boolean
  ) => void;
}

const InputForm: React.FC<InputFormProps> = ({ activeTab, handleAnalyze }) => {
  const [url, setUrl] = useState<string>("");
  const [htmlContent, setHtmlContent] = useState<string>("");
  const [pdfFile, setPdfFile] = useState<File | null>(null);

  /**
   * Submits input to the analysis handler based on the active tab.
   * Ensures inputs are valid before submission.
   */
  const handleSubmit = (useAssistant: boolean) => {
    if (activeTab === "url" && url.trim()) {
      handleAnalyze(url, "url", useAssistant);
    } else if (activeTab === "html" && htmlContent.trim()) {
      handleAnalyze(htmlContent, "html", useAssistant);
    } else if (activeTab === "pdf" && pdfFile) {
      handleAnalyze(pdfFile, "pdf", useAssistant);
    }
  };

  return (
    <motion.div
      className="max-w-lg mx-auto bg-white p-8 rounded-lg shadow-lg"
      initial={{ y: 20, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.3 }}
    >
      {/* URL Input Section */}
      {activeTab === "url" && (
        <div>
          <label
            htmlFor="urlInput"
            className="text-lg font-semibold text-gray-700"
          >
            Enter the website URL:
          </label>
          <input
            id="urlInput"
            type="url" /* Enforces URL input type for better validation */
            placeholder="https://example.com"
            value={url}
            onChange={(e) => setUrl(e.target.value)}
            className="input-field"
            aria-label="Input field for entering a website URL" /* Accessibility improvement */
          />
        </div>
      )}

      {/* HTML Input Section */}
      {activeTab === "html" && (
        <div>
          <label
            htmlFor="htmlInput"
            className="text-lg font-semibold text-gray-700"
          >
            Paste the HTML content:
          </label>
          <textarea
            id="htmlInput"
            placeholder="<html>...</html>"
            value={htmlContent}
            onChange={(e) => setHtmlContent(e.target.value)}
            className="input-field"
            rows={6} /* Improves user experience for entering HTML */
            aria-label="Textarea for pasting HTML content for analysis"
          ></textarea>
        </div>
      )}

      {/* PDF Upload Section */}
      {activeTab === "pdf" && (
        <div>
          <label
            htmlFor="pdfInput"
            className="text-lg font-semibold text-gray-700"
          >
            Upload a PDF file:
          </label>
          <input
            id="pdfInput"
            type="file"
            accept="application/pdf"
            onChange={(e) =>
              setPdfFile(e.target.files ? e.target.files[0] : null)
            }
            className="input-field"
            aria-label="File input for uploading a PDF document"
          />
          {pdfFile && (
            <p className="text-sm text-gray-600 mt-2">
              Selected file: {pdfFile.name}
            </p>
          )}
        </div>
      )}

      {/* Buttons Section */}
      <div className="flex gap-4 mt-4">
        <motion.button
          type="button"
          className="btn-primary"
          onClick={() => handleSubmit(false)}
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
          aria-label="Submit content for analysis using OpenAI Chat"
        >
          Analyze with
          <br />
          OpenAI Chat
        </motion.button>
        <motion.button
          type="button"
          className="btn-secondary"
          onClick={() => handleSubmit(true)}
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
          aria-label="Submit content for analysis using OpenAI Assistant"
        >
          Analyze with
          <br />
          OpenAI Assistant
        </motion.button>
      </div>
    </motion.div>
  );
};

export default InputForm;
