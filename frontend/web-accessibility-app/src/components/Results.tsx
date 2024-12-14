import React from "react";
import { AnalysisResult } from "../types/Accessibility";
import { motion } from "framer-motion";

interface ResultsProps {
  result: AnalysisResult | undefined;
}

const Results: React.FC<ResultsProps> = ({ result }) => {
  if (result && (!result.items || result.items.length === 0)) {
    return (
      <motion.p
        className="no-issues"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration: 0.5 }}
      >
        No accessibility issues detected
      </motion.p>
    );
  }

  if (!result || !result.items || result.items.length === 0) {
    return null;
  }

  return (
    <motion.div
      className="result-container"
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.5 }}
    >
      <h2 className="result-title">Analysis Results</h2>
      <p className="result-explanation">{result.explanation}</p>
      <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {result.items.map((item, index) => (
          <motion.li
            key={index}
            className="issue-card"
            whileHover={{ scale: 1.02 }}
          >
            <h3 className="issue-title">Issue #{index + 1}</h3>
            <p>
              <strong>Element:</strong> {item.element}
            </p>
            <p>
              <strong>Description:</strong> {item.issue}
            </p>
            <p>
              <strong>Severity:</strong> {item.severity}
            </p>
            <p>
              <strong>Recommendation:</strong> {item.recommendation}
            </p>
            <p>
              <strong>Source:</strong> {item.source}
            </p>
            <p>
              <strong>Details:</strong> {item.details}
            </p>
            {item.imageDescriptionRecommendation && (
              <p>
                <strong>Image description recommendation:</strong>{" "}
                {item.imageDescriptionRecommendation}
              </p>
            )}
          </motion.li>
        ))}
      </ul>
    </motion.div>
  );
};

export default Results;
