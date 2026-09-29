import apiClient from "./apiClient";

export async function getSuppliers(isActive = null) {
  const params = {};

  if (isActive !== null) {
    params.isActive = isActive;
  }

  const response = await apiClient.get("/suppliers", { params });
  return response.data;
}

export async function getSupplier(id) {
  const response = await apiClient.get(`/suppliers/${id}`);
  return response.data;
}