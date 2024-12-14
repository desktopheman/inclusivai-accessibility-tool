import React, { useState } from "react";

interface InputFormProps {
  onAnalyze: (url: string) => void;
}

const InputForm: React.FC<InputFormProps> = ({ onAnalyze }) => {
  const [url, setUrl] = useState<string>("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (url) onAnalyze(url);
  };

  return (
    <form onSubmit={handleSubmit}>
      <label htmlFor="urlInput" className="font-medium text-lg">
        Enter the website URL
      </label>
      <input
        id="urlInput"
        type="text"
        placeholder="Enter website URL"
        value={url}
        onChange={(e) => setUrl(e.target.value)}
        required
        className="input-field"
      />
      <button type="submit" className="submit-button">
        Analyze
      </button>
    </form>
  );
};

export default InputForm;
