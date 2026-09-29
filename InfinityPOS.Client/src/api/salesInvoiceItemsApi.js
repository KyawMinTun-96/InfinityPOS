import apiClient from "./apiClient";

export async function getSalesInvoiceItems() {
  const response = await apiClient.get("/salesinvoicesitems");
  return response.data;
}

export async function getSalesInvoiceItem(id) {
  const response = await apiClient.get(
    `/salesinvoicesitems/${id}`
  );
  return response.data;
}

export async function getSalesInvoiceItemsByInvoice(invoiceId) {
  const response = await apiClient.get(
    `/salesinvoicesitems/invoice/${invoiceId}`
  );
  return response.data;
}

export async function createSalesInvoiceItem(itemData) {
  const response = await apiClient.post(
    "/salesinvoicesitems",
    itemData
  );
  return response.data;
}

export async function updateSalesInvoiceItem(id, itemData) {
  const response = await apiClient.put(
    `/salesinvoicesitems/${id}`,
    itemData
  );
  return response.data;
}

export async function deleteSalesInvoiceItem(id) {
  const response = await apiClient.delete(
    `/salesinvoicesitems/${id}`
  );
  return response.data;
}

export async function recalculateSalesInvoiceItems(invoiceId) {
  const response = await apiClient.post(
    `/salesinvoicesitems/recalculate/${invoiceId}`
  );
  return response.data;
}
