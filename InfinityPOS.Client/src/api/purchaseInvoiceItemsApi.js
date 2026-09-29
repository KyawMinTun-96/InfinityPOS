import apiClient from "./apiClient";

export async function getPurchaseInvoiceItems() {
  const response = await apiClient.get("/purchaseinvoiceitems");
  return response.data;
}

export async function getPurchaseInvoiceItem(id) {
  const response = await apiClient.get(
    `/purchaseinvoiceitems/${id}`
  );
  return response.data;
}

export async function getPurchaseInvoiceItemsByInvoice(invoiceId) {
  const response = await apiClient.get(
    `/purchaseinvoiceitems/invoice/${invoiceId}`
  );
  return response.data;
}

export async function createPurchaseInvoiceItem(itemData) {
  const response = await apiClient.post(
    "/purchaseinvoiceitems",
    itemData
  );
  return response.data;
}

export async function updatePurchaseInvoiceItem(id, itemData) {
  const response = await apiClient.put(
    `/purchaseinvoiceitems/${id}`,
    itemData
  );
  return response.data;
}

export async function deletePurchaseInvoiceItem(id) {
  const response = await apiClient.delete(
    `/purchaseinvoiceitems/${id}`
  );
  return response.data;
}