import apiClient from "./apiClient";

export async function getStockMovements() {
  const response = await apiClient.get("/stockmovements");
  return response.data;
}

export async function getStockMovement(id) {
  const response = await apiClient.get(`/stockmovements/${id}`);
  return response.data;
}

export async function getStockMovementsByProduct(productId) {
  const response = await apiClient.get(
    `/stockmovements/product/${productId}`
  );
  return response.data;
}

export async function getStockMovementsByWarehouse(warehouseId) {
  const response = await apiClient.get(
    `/stockmovements/warehouse/${warehouseId}`
  );
  return response.data;
}