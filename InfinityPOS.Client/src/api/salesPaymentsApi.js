import apiClient from "./apiClient";

export async function getSalesPayments() {
  const response = await apiClient.get("/salespayments");
  return response.data;
}

export async function getSalesPayment(id) {
  const response = await apiClient.get(
    `/salespayments/${id}`
  );
  return response.data;
}

export async function getSalesPaymentsByInvoice(invoiceId) {
  const response = await apiClient.get(
    `/salespayments/invoice/${invoiceId}`
  );
  return response.data;
}

export async function createSalesPayment(paymentData) {
  const response = await apiClient.post(
    "/salespayments",
    paymentData
  );
  return response.data;
}

export async function updateSalesPayment(id, paymentData) {
  const response = await apiClient.put(
    `/salespayments/${id}`,
    paymentData
  );
  return response.data;
}

export async function deleteSalesPayment(id) {
  const response = await apiClient.delete(
    `/salespayments/${id}`
  );
  return response.data;
}
