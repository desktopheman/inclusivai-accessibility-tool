import React, { useState } from "react";

interface InputFormProps {
  onAnalyze: (url: string) => void;
}

const InputForm: React.FC<InputFormProps> = ({ onAnalyze }) => {
  const [url, setUrl] = useState<string>(""); // State to store the URL entered by the user

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (url) onAnalyze(url);
  };

  return (
    <form onSubmit={handleSubmit}>
      <label htmlFor="urlInput">Website URL:</label>
      <input
        id="urlInput"
        type="text"
        placeholder="Enter website URL"
        value={url}
        onChange={(e) => setUrl(e.target.value)}
        required
      />
      <button type="submit">Analyze</button>
    </form>
  );
};

export default InputForm;
