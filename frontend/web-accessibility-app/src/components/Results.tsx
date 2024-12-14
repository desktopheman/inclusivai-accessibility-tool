import React from "react";
import { AnalysisResult } from "../types/Accessibility";

interface ResultsProps {
  result: AnalysisResult | undefined;
}

const Results: React.FC<ResultsProps> = ({ result }) => {
  if (!result) return <p className="no-issues">No accessibility issues</p>;

  return (
    <div className="result-container">
      <h2 className="result-title">Resultados da Análise</h2>
      <p className="result-explanation">{result.explanation}</p>
      <ul className="list-disc list-inside">
        {result.items.map((item, index) => (
          <div key={index} className="issue-container">
            <h3 className="font-bold text-lg">Issue #{index + 1}</h3>
            <p>
              <strong>Description:</strong> {item.issue}
            </p>
            <p>
              <strong>Recommendation:</strong> {item.recommendation}
            </p>
            <p>
              <strong>Severity:</strong> {item.severity}
            </p>
            <p>&nbsp;</p>
          </div>
        ))}
      </ul>
    </div>
  );
};

export default Results;
