import apiClient from "./apiClient";

export async function getProductStockBatches(productId, warehouseId) {
const response = await apiClient.get(
`/ProductStockBatches/product/${productId}/warehouse/${warehouseId}`
);

return response.data;
}