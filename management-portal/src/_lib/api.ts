"use server";

import type {
  ApiResponse,
  PaginatedResponse,
  Tenant,
  DemoRequest,
  DashboardAnalytics,
  BillingTransaction,
} from "@/types";
import { getAuthToken } from "./auth";

const API_URL = process.env.MANAGEMENT_API_URL || "http://localhost:15568";

/**
 * Base fetch wrapper with error handling and auth
 */
async function fetchApi<T>(
  endpoint: string,
  options?: RequestInit
): Promise<ApiResponse<T>> {
  try {
    const token = await getAuthToken();
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
      ...(options?.headers as Record<string, string>),
    };

    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    }

    const response = await fetch(`${API_URL}${endpoint}`, {
      ...options,
      headers,
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: "Unknown error" }));
      return {
        success: false,
        error: error.message || `HTTP ${response.status}`,
      };
    }

    const data = await response.json();
    return {
      success: true,
      data,
    };
  } catch (error) {
    console.error("API Error:", error);
    return {
      success: false,
      error: error instanceof Error ? error.message : "Network error",
    };
  }
}

// Dashboard API
export async function getDashboardAnalytics(): Promise<ApiResponse<DashboardAnalytics>> {
  return fetchApi<DashboardAnalytics>("/api/dashboard/analytics");
}

// Tenants API
export async function getTenants(
  page = 1,
  pageSize = 20,
  status?: string
): Promise<ApiResponse<PaginatedResponse<Tenant>>> {
  const skip = (page - 1) * pageSize;
  const response = await fetchApi<Tenant[]>(`/api/tenants?skip=${skip}&take=${pageSize}`);

  if (!response.success || !response.data) {
    return { success: false, error: response.error };
  }

  // Transform array response to paginated format
  return {
    success: true,
    data: {
      items: response.data,
      total: response.data.length,
      page,
      pageSize,
      totalPages: Math.ceil(response.data.length / pageSize) || 1,
    },
  };
}

export async function getTenant(id: string): Promise<ApiResponse<Tenant>> {
  return fetchApi<Tenant>(`/api/tenants/${id}`);
}

export async function updateTenantStatus(
  id: string,
  status: Tenant["status"]
): Promise<ApiResponse<Tenant>> {
  return fetchApi<Tenant>(`/api/tenants/${id}/status`, {
    method: "PATCH",
    body: JSON.stringify({ status }),
  });
}

export async function deleteTenant(tenantId: string): Promise<ApiResponse<void>> {
  return fetchApi<void>(`/api/tenants/${tenantId}`, {
    method: "DELETE",
  });
}

export interface CreateTenantData {
  companyName: string;
  ownerEmail: string;
  ownerName: string;
  isDemo?: boolean;
}

export async function createTenant(data: CreateTenantData): Promise<ApiResponse<Tenant>> {
  return fetchApi<Tenant>("/api/tenants", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

// Demo Requests API
export async function getDemoRequests(
  page = 1,
  pageSize = 20,
  status?: string
): Promise<ApiResponse<PaginatedResponse<DemoRequest>>> {
  const skip = (page - 1) * pageSize;
  const response = await fetchApi<DemoRequest[]>(`/api/demorequests?skip=${skip}&take=${pageSize}`);

  if (!response.success || !response.data) {
    return { success: false, error: response.error };
  }

  return {
    success: true,
    data: {
      items: response.data,
      total: response.data.length,
      page,
      pageSize,
      totalPages: Math.ceil(response.data.length / pageSize) || 1,
    },
  };
}

export async function createDemoRequest(
  data: Pick<DemoRequest, "email" | "companyName" | "phoneNumber" | "message">
): Promise<ApiResponse<DemoRequest>> {
  return fetchApi<DemoRequest>("/api/demorequests", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function approveDemoRequest(
  id: string,
  notes?: string
): Promise<ApiResponse<DemoRequest>> {
  return fetchApi<DemoRequest>(`/api/demorequests/${id}/approve`, {
    method: "POST",
    body: JSON.stringify({ notes }),
  });
}

export async function rejectDemoRequest(
  id: string,
  reason: string
): Promise<ApiResponse<DemoRequest>> {
  return fetchApi<DemoRequest>(`/api/demorequests/${id}/reject`, {
    method: "POST",
    body: JSON.stringify({ reason }),
  });
}

export async function updateDemoRequestStatus(
  id: string,
  status: string
): Promise<ApiResponse<DemoRequest>> {
  return fetchApi<DemoRequest>(`/api/demorequests/${id}/status`, {
    method: "PATCH",
    body: JSON.stringify({ status }),
  });
}

// Billing API
export async function getBillingTransactions(
  page = 1,
  pageSize = 20
): Promise<ApiResponse<PaginatedResponse<BillingTransaction>>> {
  const skip = (page - 1) * pageSize;
  return fetchApi<PaginatedResponse<BillingTransaction>>(`/api/billing/transactions?skip=${skip}&take=${pageSize}`);
}

export async function getBillingStats(): Promise<ApiResponse<{
  totalRevenue: number;
  monthlyRecurringRevenue: number;
  averageRevenuePerTenant: number;
  activeSubscriptions: number;
}>> {
  return fetchApi("/api/billing/stats");
}

// Health check
export async function getHealthStatus(): Promise<ApiResponse<{ status: string; timestamp: string }>> {
  return fetchApi<{ status: string; timestamp: string }>("/api/health");
}
