import apiClient from "./apiClient";

export async function getCurrencies(isActive = null) {
  const params = {};

  if (isActive !== null) {
    params.isActive = isActive;
  }

  const response = await apiClient.get("/currencies", { params });
  return response.data;
}

export async function getCurrency(id) {
  const response = await apiClient.get(`/currencies/${id}`);
  return response.data;
}