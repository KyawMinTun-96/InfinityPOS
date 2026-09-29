import apiClient from "./apiClient";

export async function getPurchaseInvoices() {
  const response = await apiClient.get("/purchaseinvoices");
  return response.data;
}

export async function getPurchaseInvoice(id) {
  const response = await apiClient.get(`/purchaseinvoices/${id}`);
  return response.data;
}

export async function createPurchaseInvoice(invoiceData) {
  const response = await apiClient.post(
    "/purchaseinvoices",
    invoiceData
  );
  return response.data;
}

export async function updatePurchaseInvoice(id, invoiceData) {
  const response = await apiClient.put(
    `/purchaseinvoices/${id}`,
    invoiceData
  );
  return response.data;
}

export async function deletePurchaseInvoice(id) {
  const response = await apiClient.delete(
    `/purchaseinvoices/${id}`
  );
  return response.data;
}


export async function postPurchaseInvoice(id) {
const response = await apiClient.post(
`/purchaseinvoices/${id}/post`
);

return response.data;
}
