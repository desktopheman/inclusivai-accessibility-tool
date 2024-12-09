export interface WCAGResult {
  issue: string;
  recommendation: string;
  severity: "Low" | "Medium" | "High";
}

export interface ApiResponse {
  results: WCAGResult[];
}
