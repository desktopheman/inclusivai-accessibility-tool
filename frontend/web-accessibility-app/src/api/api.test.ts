import axios from "axios";
import api from "./api";

jest.mock("axios");

describe("API Tests", () => {
  it("fetches data successfully", async () => {
    const mockData = { data: { success: true } };
    (axios.get as jest.Mock).mockResolvedValueOnce(mockData);

    const result = await api.get("/test");
    expect(result).toEqual(mockData);
    expect(axios.get).toHaveBeenCalledWith("/test");
  });

  it("handles fetch error", async () => {
    (axios.get as jest.Mock).mockRejectedValueOnce(new Error("Network Error"));

    await expect(api.get("/test")).rejects.toThrow("Network Error");
  });
});
