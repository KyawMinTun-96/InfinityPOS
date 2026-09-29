import apiClient from "./apiClient";

export async function login(username, password) {
  const response = await apiClient.post("/auth/login", {
    username,
    password,
  });

  return response.data;
}

export function saveLoginSession(loginData) {
  localStorage.setItem(
    "infinitypos_token",
    loginData.token
  );

  localStorage.setItem(
    "infinitypos_user",
    JSON.stringify({
      userId: loginData.userId,
      username: loginData.username,
      displayName: loginData.displayName,
      roleId: loginData.roleId,
      roleCode: loginData.roleCode,
      roleName: loginData.roleName,
    })
  );
}

export function getCurrentUser() {
  const user = localStorage.getItem("infinitypos_user");

  if (!user) {
    return null;
  }

  try {
    return JSON.parse(user);
  } catch {
    return null;
  }
}

export function logout() {
  localStorage.removeItem("infinitypos_token");
  localStorage.removeItem("infinitypos_user");
}

export function isLoggedIn() {
  return Boolean(
    localStorage.getItem("infinitypos_token")
  );
}