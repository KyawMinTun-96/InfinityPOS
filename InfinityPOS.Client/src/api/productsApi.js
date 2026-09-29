import apiClient from "./apiClient";

export async function getProducts(isActive = null) {
  const params = {};

  if (isActive !== null) {
    params.isActive = isActive;
  }

  const response = await apiClient.get("/products", {
    params,
  });

  return response.data;
}

export async function getProduct(id) {
  const response = await apiClient.get(`/products/${id}`);
  return response.data;
}

export async function createProduct(product) {
  const response = await apiClient.post("/products", product);
  return response.data;
}

export async function updateProduct(id, product) {
  const response = await apiClient.put(`/products/${id}`, product);
  return response.data;
}

export async function deleteProduct(id) {
  const response = await apiClient.delete(`/products/${id}`);
  return response.data;
}