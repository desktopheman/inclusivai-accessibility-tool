import React from "react";
import { AnalysisResult } from "../types/Accessibility";

interface ResultsProps {
  result: AnalysisResult | undefined;
}

const Results: React.FC<ResultsProps> = ({ result }) => {
  if (result && (!result.items || result.items.length === 0)) {
    // Display this message only when there are no issues
    return <p className="no-issues">No accessibility issues detected</p>;
  }

  if (!result || !result.items || result.items.length === 0) {
    // Do not display anything if the result is undefined or there are no items
    return null;
  }

  return (
    <div className="result-container">
      <h2 className="result-title">Analysis Results</h2>
      <p className="result-explanation">{result.explanation}</p>
      <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {result.items.map((item, index) => (
          <li key={index} className="issue-card">
            <h3 className="issue-title">Issue #{index + 1}</h3>
            <p>
              <strong>Description:</strong> {item.issue}
            </p>
            <p>
              <strong>Severity:</strong> {item.severity}
            </p>
            <p>
              <strong>Recommendation:</strong> {item.recommendation}
            </p>
            {item.imageDescriptionRecommendation && (
              <p>
                <strong>Image description recommendation:</strong>{" "}
                {item.imageDescriptionRecommendation}
              </p>
            )}
            <p>
              <strong>Source:</strong> {item.source}
            </p>
            <p>
              <strong>Details:</strong> {item.details}
            </p>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default Results;
