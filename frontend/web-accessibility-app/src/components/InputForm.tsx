import React, { useState } from "react";

interface InputFormProps {
  activeInput: "url" | "html" | "pdf";
  setActiveInput: React.Dispatch<React.SetStateAction<"url" | "html" | "pdf">>;
  handleAnalyze: (
    input: string | File,
    type: string,
    useAssistant: boolean
  ) => void;
}

const InputForm: React.FC<InputFormProps> = ({
  activeInput,
  setActiveInput,
  handleAnalyze,
}) => {
  const [url, setUrl] = useState("");
  const [htmlContent, setHtmlContent] = useState("");
  const [pdfFile, setPdfFile] = useState<File | null>(null);

  const renderInputField = () => {
    if (activeInput === "url") {
      return (
        <input
          type="url"
          placeholder="https://example.com"
          value={url}
          onChange={(e) => setUrl(e.target.value)}
          className="input-field"
        />
      );
    } else if (activeInput === "html") {
      return (
        <textarea
          rows={6}
          placeholder="Paste your HTML here"
          value={htmlContent}
          onChange={(e) => setHtmlContent(e.target.value)}
          className="textarea-field"
        ></textarea>
      );
    } else {
      return (
        <input
          type="file"
          accept="application/pdf"
          onChange={(e) => setPdfFile(e.target.files?.[0] || null)}
          className="file-input"
        />
      );
    }
  };

  const nonActiveButtons = ["url", "html", "pdf"].filter(
    (item) => item !== activeInput
  );

  return (
    <div className="form-container">
      <div className="flex items-start gap-2 mb-4">
        <button
          className="selected-button"
          onClick={() => setActiveInput(activeInput)}
        >
          {activeInput.toUpperCase()}
        </button>
        <div className="flex-1">{renderInputField()}</div>
      </div>

      <div className="flex justify-center gap-2 mb-4">
        {nonActiveButtons.map((item) => (
          <button
            key={item}
            onClick={() => setActiveInput(item as "url" | "html" | "pdf")}
            className="non-selected-button"
          >
            {item.toUpperCase()}
          </button>
        ))}
      </div>

      <p className="text-center font-bold mb-2 text-black pt-8 pb-4">
        Analyze with OpenAI:
      </p>

      <div className="flex justify-center gap-4">
        <button
          className="btn btn-red"
          onClick={() =>
            handleAnalyze(url || htmlContent || pdfFile!, activeInput, false)
          }
        >
          CHAT
        </button>
        <button
          className="btn btn-gray"
          onClick={() =>
            handleAnalyze(url || htmlContent || pdfFile!, activeInput, true)
          }
        >
          ASSISTANT
        </button>
      </div>
    </div>
  );
};

export default InputForm;
