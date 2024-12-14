import React, { useState } from "react";

interface InputFormProps {
  handleAnalyze: (url: string, useAssistant: boolean) => void;
}

const InputForm: React.FC<InputFormProps> = ({ handleAnalyze }) => {
  const [url, setUrl] = useState<string>("");

  const handleSubmit = (useAssistant: boolean) => {
    if (url.trim()) handleAnalyze(url, useAssistant);
  };

  return (
    <div className="max-w-lg mx-auto bg-white p-8 rounded-lg shadow-lg">
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
          <button
            type="button"
            className="btn-primary"
            onClick={() => handleSubmit(false)}
          >
            Analyze with OpenAI Chat
          </button>
          <button
            type="button"
            className="btn-secondary"
            onClick={() => handleSubmit(true)}
          >
            Analyze with OpenAI Assistant
          </button>
        </div>
      </form>
    </div>
  );
};

export default InputForm;
