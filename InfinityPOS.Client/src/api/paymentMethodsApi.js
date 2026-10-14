import apiClient from "./apiClient";

/* ============================================================
   GET ALL ACTIVE PAYMENT METHODS
   GET: api/paymentmethods
   ============================================================ */

export async function getPaymentMethods() {
  const response = await apiClient.get(
    "/paymentmethods"
  );

  return response.data;
}

/* ============================================================
   GET PAYMENT METHOD BY ID
   GET: api/paymentmethods/1
   ============================================================ */

export async function getPaymentMethod(id) {
  const response = await apiClient.get(
    `/paymentmethods/${id}`
  );

  return response.data;
}
