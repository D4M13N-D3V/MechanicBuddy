'use server'

import { httpGet } from "../query-api";

interface CanViewAuditLogsResponse {
  canView: boolean;
}

export async function checkCanViewAuditLogs(): Promise<CanViewAuditLogsResponse> {
  try {
    const response = await httpGet("auditlogs/canview");
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Failed to check audit log permissions:", error);
    return {
      canView: false
    };
  }
}
