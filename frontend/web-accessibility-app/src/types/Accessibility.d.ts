export interface AnalysisResult {
  items: IssuesResult[];
  explanation: string;
}

export interface AttributeResult {
  name: string;
  value: string;
}

export interface IssuesResult {
  element: string;
  attributes: AttributeResult[];
  issue: string;
  severity: "Low" | "Medium" | "High";
  recommendation: string;
  imageDescriptionRecommendation: string;
  source: string;
  details: string;
}

export interface ApiResponse {
  result: AnalysisResult[];
}
