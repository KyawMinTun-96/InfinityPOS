import apiClient from "./apiClient";

export async function getCustomers(isActive = null) {
  const params = {};

  if (isActive !== null) {
    params.isActive = isActive;
  }

  const response = await apiClient.get("/customers", {
    params,
  });

  return response.data;
}

export async function getCustomer(id) {
  const response = await apiClient.get(`/customers/${id}`);
  return response.data;
}
