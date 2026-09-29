import apiClient from "./apiClient";

export async function getSalesInvoices() {
  const response = await apiClient.get("/salesinvoices");
  return response.data;
}

export async function getSalesInvoice(id) {
  const response = await apiClient.get(`/salesinvoices/${id}`);
  return response.data;
}


export async function createSalesInvoice(invoiceData) {
  const response = await apiClient.post(
    "/salesinvoices",
    invoiceData
  );
  return response.data;
}

export async function recalculateSalesInvoice(id) {
  const response = await apiClient.post(
    `/salesinvoices/recalculate/${id}`
  );
  return response.data;
}

export async function postSalesInvoice(id) {
  const response = await apiClient.post(
    `/salesinvoices/${id}/post`
  );
  return response.data;
}



