import apiClient from "./apiClient";

export async function getStockBalances() {
  const response = await apiClient.get("/stockbalances");
  return response.data;
}

export async function getStockBalance(id) {
  const response = await apiClient.get(`/stockbalances/${id}`);
  return response.data;
}

export async function getStockBalancesByProduct(productId) {
  const response = await apiClient.get(
    `/stockbalances/product/${productId}`
  );
  return response.data;
}

export async function getStockBalancesByWarehouse(warehouseId) {
  const response = await apiClient.get(
    `/stockbalances/warehouse/${warehouseId}`
  );
  return response.data;
}