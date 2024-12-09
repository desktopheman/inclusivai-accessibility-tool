import React from "react";
import { render, screen } from "@testing-library/react";
import Results from "./Results";

describe("Results Component", () => {
  it("renders accessibility results correctly", () => {
    const mockResults = [
      { issue: "Issue 1", recommendation: "Fix 1", severity: "High" },
      { issue: "Issue 2", recommendation: "Fix 2", severity: "Low" },
    ];
    render(<Results results={mockResults} />);

    mockResults.forEach((result) => {
      expect(screen.getByText(result.issue)).toBeInTheDocument();
      expect(screen.getByText(result.recommendation)).toBeInTheDocument();
      expect(screen.getByText(result.severity)).toBeInTheDocument();
    });
  });

  it("shows a message when no results are provided", () => {
    render(<Results results={[]} />);
    expect(screen.getByText(/No accessibility issues found!/i)).toBeInTheDocument();
  });
});
