import apiClient from "./apiClient";

export async function getWarehouses(isActive = null) {
  const params = {};

  if (isActive !== null) {
    params.isActive = isActive;
  }

  const response = await apiClient.get(
    "/warehouses",
    { params }
  );

  return response.data;
}

export async function getWarehouse(id) {
  const response = await apiClient.get(
    `/warehouses/${id}`
  );

  return response.data;
}
