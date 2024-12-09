import React from "react";
import { WCAGResult } from "../types/Accessibility";

interface ResultsProps {
  results: WCAGResult[];
}

const Results: React.FC<ResultsProps> = ({ results }) => {
  if (!results || results.length === 0)
    return <p>No accessibility issues found!</p>;

  return (
    <div>
      <h2>Analysis Results</h2>
      <ul>
        {results.map((result, index) => (
          <li key={index}>
            <strong>Issue:</strong> {result.issue} <br />
            <strong>Recommendation:</strong> {result.recommendation} <br />
            <strong>Severity:</strong> {result.severity}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default Results;
