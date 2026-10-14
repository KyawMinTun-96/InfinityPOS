import apiClient from "./apiClient";

// ============================================================
// GET CURRENT OPEN CASH SESSION
// GET /api/cash/current
// ============================================================

export async function getCurrentCashSession() {
  const response = await apiClient.get("/cash/current");
  return response.data;
}

// ============================================================
// GET CASH SESSION BY DATE
// GET /api/cash/session?date=2026-09-30
// ============================================================

export async function getCashSessionByDate(date) {
  const response = await apiClient.get("/cash/session", {
    params: {
      date,
    },
  });

  return response.data;
}

// ============================================================
// GET CASH SESSION BY ID
// GET /api/cash/{id}
// ============================================================

export async function getCashSession(id) {
  const response = await apiClient.get(`/cash/${id}`);
  return response.data;
}

// ============================================================
// OPEN CASH SESSION
// POST /api/cash/open
// ============================================================

export async function openCashSession(data) {
  const response = await apiClient.post(
    "/cash/open",
    data
  );

  return response.data;
}

// ============================================================
// CLOSE CASH SESSION
// POST /api/cash/{id}/close
// ============================================================

export async function closeCashSession(id, data) {
  const response = await apiClient.post(
    `/cash/${id}/close`,
    data
  );

  return response.data;
}

// ============================================================
// CREATE CASH ADJUSTMENT
// POST /api/cash/{id}/adjustment
// ============================================================

export async function createCashAdjustment(id, data) {
  const response = await apiClient.post(
    `/cash/${id}/adjustment`,
    data
  );

  return response.data;
}

// ============================================================
// VOID CASH ADJUSTMENT
// POST /api/cash/adjustment/{adjustmentId}/void
// ============================================================

export async function voidCashAdjustment(
  adjustmentId
) {
  const response = await apiClient.post(
    `/cash/adjustment/${adjustmentId}/void`
  );

  return response.data;
}

// ============================================================
// GET DAILY CASH LEDGER
// GET /api/cash/{id}/ledger
// ============================================================

export async function getCashLedger(id) {
  const response = await apiClient.get(
    `/cash/${id}/ledger`
  );

  return response.data;
}

// ============================================================
// GET CASH SESSION HISTORY
// GET /api/cash/history
// ============================================================

export async function getCashSessionHistory(
  from,
  to
) {
  const response = await apiClient.get(
    "/cash/history",
    {
      params: {
        from,
        to,
      },
    }
  );

  return response.data;
}
