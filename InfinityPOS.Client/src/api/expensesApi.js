import apiClient from "./apiClient";

export async function getExpenses() {
  const response = await apiClient.get("/expenses");
  return response.data;
}

export async function getExpense(id) {
  const response = await apiClient.get(`/expenses/${id}`);
  return response.data;
}

export async function getExpenseDetails(id) {
  const response = await apiClient.get(`/expenses/${id}/details`);
  return response.data;
}

export async function createExpense(data) {
  const response = await apiClient.post("/expenses", data);
  return response.data;
}

export async function updateExpense(id, data) {
  const response = await apiClient.put(`/expenses/${id}`, data);
  return response.data;
}

export async function recalculateExpense(id) {
  const response = await apiClient.post(
    `/expenses/${id}/recalculate`
  );
  return response.data;
}

export async function postExpense(id) {
  const response = await apiClient.post(
    `/expenses/${id}/post`
  );
  return response.data;
}

export async function cancelExpense(id) {
  const response = await apiClient.delete(
    `/expenses/${id}`
  );
  return response.data;
}

