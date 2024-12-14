import React, { useState } from "react";
import { motion } from "framer-motion";

interface InputFormProps {
  handleAnalyze: (url: string, useAssistant: boolean) => void;
}

const InputForm: React.FC<InputFormProps> = ({ handleAnalyze }) => {
  const [url, setUrl] = useState<string>("");

  const handleSubmit = (useAssistant: boolean) => {
    if (url.trim()) handleAnalyze(url, useAssistant);
  };

  return (
    <motion.div
      className="max-w-lg mx-auto bg-white p-8 rounded-lg shadow-lg"
      initial={{ y: 20, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.3 }}
    >
      <form className="flex flex-col gap-6">
        <label
          htmlFor="urlInput"
          className="text-lg font-semibold text-gray-700"
        >
          Enter the website URL:
        </label>
        <input
          id="urlInput"
          type="text"
          placeholder="https://example.com"
          value={url}
          onChange={(e) => setUrl(e.target.value)}
          className="input-field"
        />
        <div className="flex gap-4">
          <motion.button
            type="button"
            className="btn-primary"
            onClick={() => handleSubmit(false)}
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
          >
            Analyze with OpenAI Chat
          </motion.button>
          <motion.button
            type="button"
            className="btn-secondary"
            onClick={() => handleSubmit(true)}
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
          >
            Analyze with OpenAI Assistant
          </motion.button>
        </div>
      </form>
    </motion.div>
  );
};

export default InputForm;
