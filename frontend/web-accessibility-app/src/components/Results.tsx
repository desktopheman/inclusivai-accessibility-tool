import React from "react";
import { AnalysisResult } from "../types/Accessibility";
import { motion } from "framer-motion";

interface ResultsProps {
  result: AnalysisResult | undefined;
}

const Results: React.FC<ResultsProps> = ({ result }) => {
  /**
   * Determines the CSS classes for severity levels.
   * Accessibility Note: Ensures appropriate background and text colors for contrast.
   */
  const getSeverityClass = (severity: string) => {
    switch (severity.toLowerCase()) {
      case "low":
        return "bg-yellow-100 border-yellow-400 text-yellow-800";
      case "medium":
        return "bg-orange-100 border-orange-400 text-orange-800";
      case "high":
        return "bg-red-100 border-red-400 text-red-800";
      case "improvement":
        return "bg-blue-100 border-blue-400 text-blue-800";
      default:
        return "bg-gray-100 border-gray-400 text-gray-800";
    }
  };

  /**
   * Handles the case where no accessibility issues are found.
   */
  if (result && (!result.items || result.items.length === 0)) {
    return (
      <motion.p
        className="no-issues"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration: 0.5 }}
        role="alert" /* Announces this message to assistive technologies */
        aria-live="polite" /* Ensures it's read out when updated */
      >
        No accessibility issues detected
      </motion.p>
    );
  }

  /**
   * Handles the case where the result is undefined or empty.
   */
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
      {/* Header for Analysis Results */}
      <h2 id="resultsTitle" className="result-title">
        <p className="text-center font-bold mb-4 text-black">Analysis Result</p>
      </h2>
      {/* Descriptive explanation of results */}
      <p className="result-explanation mb-6" aria-describedby="resultsTitle">
        {result.explanation}
      </p>
      <ul
        className="grid gap-6 sm:grid-cols-1 lg:grid-cols-2"
        aria-labelledby="resultsTitle"
        role="list" /* Semantic list role for screen readers */
      >
        {result.items.map((item, index) => (
          <motion.li
            key={index}
            className={`issue-card border-l-4 ${getSeverityClass(
              item.severity
            )} p-4`}
            whileHover={{ scale: 1.02 }}
            role="listitem" /* Improves semantic structure */
            aria-label={`Issue ${index + 1}: ${item.issue}, Severity: ${item.severity}`} /* Descriptive label */
          >
            {/* Individual Issue Card */}
            <h3 className="issue-title mb-2 font-semibold">
              Issue #{index + 1}
            </h3>
            <p className="mb-2">
              <strong>Element:</strong> {item.element}
            </p>
            <p className="mb-2">
              <strong>Description:</strong> {item.issue}
            </p>
            <p className="mb-2">
              <strong>Severity:</strong> {item.severity}
            </p>
            <p className="mb-2">
              <strong>Recommendation:</strong> {item.recommendation}
            </p>
            <p className="mb-2">
              <strong>Source:</strong> {item.source}
            </p>
            <p className="mb-2">
              <strong>Details:</strong> {item.details}
            </p>
            {/* Optional Image Description Recommendation */}
            {item.imageDescriptionRecommendation && (
              <p className="mb-2">
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
