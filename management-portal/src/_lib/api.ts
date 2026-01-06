"use server";

import type {
  ApiResponse,
  PaginatedResponse,
  Tenant,
  DemoRequest,
  DashboardAnalytics,
  BillingTransaction,
} from "@/types";

const API_URL = process.env.MANAGEMENT_API_URL || "http://localhost:15568";

/**
 * Base fetch wrapper with error handling
 */
async function fetchApi<T>(
  endpoint: string,
  options?: RequestInit
): Promise<ApiResponse<T>> {
  try {
    const response = await fetch(`${API_URL}${endpoint}`, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...options?.headers,
      },
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
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    ...(status && { status }),
  });
  return fetchApi<PaginatedResponse<Tenant>>(`/api/tenants?${params}`);
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

export async function deleteTenant(id: string): Promise<ApiResponse<void>> {
  return fetchApi<void>(`/api/tenants/${id}`, {
    method: "DELETE",
  });
}

// Demo Requests API
export async function getDemoRequests(
  page = 1,
  pageSize = 20,
  status?: string
): Promise<ApiResponse<PaginatedResponse<DemoRequest>>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    ...(status && { status }),
  });
  return fetchApi<PaginatedResponse<DemoRequest>>(`/api/demos?${params}`);
}

export async function createDemoRequest(
  data: Pick<DemoRequest, "email" | "companyName" | "message">
): Promise<ApiResponse<DemoRequest>> {
  return fetchApi<DemoRequest>("/api/demos", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function updateDemoRequestStatus(
  id: string,
  status: DemoRequest["status"],
  notes?: string
): Promise<ApiResponse<DemoRequest>> {
  return fetchApi<DemoRequest>(`/api/demos/${id}`, {
    method: "PATCH",
    body: JSON.stringify({ status, notes }),
  });
}

// Billing API
export async function getBillingTransactions(
  page = 1,
  pageSize = 20
): Promise<ApiResponse<PaginatedResponse<BillingTransaction>>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });
  return fetchApi<PaginatedResponse<BillingTransaction>>(`/api/billing/transactions?${params}`);
}

// Health check
export async function getHealthStatus(): Promise<ApiResponse<{ status: string; timestamp: string }>> {
  return fetchApi<{ status: string; timestamp: string }>("/api/health");
}
