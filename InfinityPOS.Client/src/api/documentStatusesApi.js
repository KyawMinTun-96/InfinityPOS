import apiClient from "./apiClient";

export async function getDocumentStatuses(documentType = null) {
  const params = {};

  if (documentType) {
    params.documentType = documentType;
  }

  const response = await apiClient.get(
    "/documentstatuses",
    { params }
  );

  return response.data;
}

export async function getDocumentStatus(id) {
  const response = await apiClient.get(
    `/documentstatuses/${id}`
  );

  return response.data;
}