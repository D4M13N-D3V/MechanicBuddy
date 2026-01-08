'use server'

import { httpGet } from "../query-api";

interface CanManageUsersResponse {
  canManageUsers: boolean;
  tier: string;
  workOrderCount: number;
  workOrderLimit: number;
  hasWorkOrderLimit: boolean;
}

export async function checkCanManageUsers(): Promise<CanManageUsersResponse> {
  try {
    const response = await httpGet("usermanagement/canmanage");
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Failed to check user management permissions:", error);
    return {
      canManageUsers: false,
      tier: "unknown",
      workOrderCount: 0,
      workOrderLimit: 0,
      hasWorkOrderLimit: false
    };
  }
}
