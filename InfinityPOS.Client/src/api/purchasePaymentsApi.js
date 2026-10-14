import apiClient from "./apiClient";

/* ============================================================
   GET ALL PURCHASE PAYMENTS
   GET: api/purchasepayments
   ============================================================ */

export async function getPurchasePayments() {
  const response = await apiClient.get(
    "/purchasepayments"
  );

  return response.data;
}

/* ============================================================
   GET PURCHASE PAYMENT BY ID
   GET: api/purchasepayments/1
   ============================================================ */

export async function getPurchasePayment(id) {
  const response = await apiClient.get(
    `/purchasepayments/${id}`
  );

  return response.data;
}

/* ============================================================
   GET PAYMENTS BY PURCHASE INVOICE
   GET: api/purchasepayments/invoice/1
   ============================================================ */

export async function getPurchasePaymentsByInvoice(
  purchaseInvoiceId
) {
  const response = await apiClient.get(
    `/purchasepayments/invoice/${purchaseInvoiceId}`
  );

  return response.data;
}

/* ============================================================
   CREATE PURCHASE PAYMENT
   POST: api/purchasepayments
   ============================================================ */

export async function createPurchasePayment(
  data
) {
  const response = await apiClient.post(
    "/purchasepayments",
    data
  );

  return response.data;
}

/* ============================================================
   UPDATE PURCHASE PAYMENT
   PUT: api/purchasepayments/1
   ============================================================ */

export async function updatePurchasePayment(
  id,
  data
) {
  const response = await apiClient.put(
    `/purchasepayments/${id}`,
    data
  );

  return response.data;
}

/* ============================================================
   DELETE PURCHASE PAYMENT
   DELETE: api/purchasepayments/1
   ============================================================ */

export async function deletePurchasePayment(
  id
) {
  const response = await apiClient.delete(
    `/purchasepayments/${id}`
  );

  return response.data;
}
