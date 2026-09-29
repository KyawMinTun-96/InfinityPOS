import apiClient from "./apiClient";

export async function getProductPrices() {
  const response = await apiClient.get("/productprices");
  return response.data;
}

export async function getProductPrice(id) {
  const response = await apiClient.get(
    `/productprices/${id}`
  );

  return response.data;
}

export async function getProductPricesByProduct(productId) {
  const response = await apiClient.get(
    `/productprices/product/${productId}`
  );

  return response.data;
}

export async function createProductPrice(priceData) {
  const response = await apiClient.post(
    "/productprices",
    priceData
  );

  return response.data;
}

export async function updateProductPrice(
  id,
  priceData
) {
  const response = await apiClient.put(
    `/productprices/${id}`,
    priceData
  );

  return response.data;
}

export async function deleteProductPrice(id) {
  const response = await apiClient.delete(
    `/productprices/${id}`
  );

  return response.data;
}