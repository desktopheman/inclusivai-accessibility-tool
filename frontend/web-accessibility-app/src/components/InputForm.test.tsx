import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import InputForm from "./InputForm";

describe("InputForm Component", () => {
  it("renders the form elements correctly", () => {
    render(<InputForm onAnalyze={() => {}} />);

    expect(screen.getByLabelText(/Website URL:/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Analyze/i })).toBeInTheDocument();
  });

  it("calls onAnalyze when a valid URL is submitted", () => {
    const mockOnAnalyze = jest.fn();
    render(<InputForm onAnalyze={mockOnAnalyze} />);

    const input = screen.getByLabelText(/Website URL:/i);
    const button = screen.getByRole("button", { name: /Analyze/i });

    fireEvent.change(input, { target: { value: "https://example.com" } });
    fireEvent.click(button);

    expect(mockOnAnalyze).toHaveBeenCalledWith("https://example.com");
    expect(mockOnAnalyze).toHaveBeenCalledTimes(1);
  });

  it("does not call onAnalyze when the input is empty", () => {
    const mockOnAnalyze = jest.fn();
    render(<InputForm onAnalyze={mockOnAnalyze} />);

    const button = screen.getByRole("button", { name: /Analyze/i });

    fireEvent.click(button);

    expect(mockOnAnalyze).not.toHaveBeenCalled();
  });
});
