"use server";

import type {
  ApiResponse,
  PaginatedResponse,
  Tenant,
  DemoRequest,
  DashboardAnalytics,
  BillingTransaction,
  PortalUser,
  RequestTenantData,
  DomainVerification,
  DomainVerificationResult,
} from "@/types";
import { getAuthToken, logout } from "./auth";

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
      // Handle 401 Unauthorized - clear session so middleware redirects to login
      if (response.status === 401) {
        await logout();
        return {
          success: false,
          error: "Session expired. Please log in again.",
        };
      }

      const text = await response.text();
      let errorMessage = `HTTP ${response.status}`;

      if (text) {
        try {
          const error = JSON.parse(text);
          errorMessage = error.message || errorMessage;
        } catch {
          // Response is not JSON, use the raw text if it's short enough
          errorMessage = text.length < 200 ? text : errorMessage;
        }
      }

      return {
        success: false,
        error: errorMessage,
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

export interface DeleteTenantResponse {
  message: string;
  kubernetesDeleted: boolean;
  databaseDeleted: boolean;
  tenantNotInDatabase: boolean;
  warnings: string[];
}

export async function deleteTenant(tenantId: string): Promise<ApiResponse<DeleteTenantResponse>> {
  return fetchApi<DeleteTenantResponse>(`/api/tenants/${tenantId}`, {
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

export async function createTeamCheckout(tenantId: string, returnUrl: string): Promise<ApiResponse<{url: string}>> {
  return fetchApi<{url: string}>("/api/billing/checkout/team", {
    method: "POST",
    body: JSON.stringify({ tenantId, returnUrl }),
  });
}

export async function createLifetimeCheckout(tenantId: string, returnUrl: string): Promise<ApiResponse<{url: string}>> {
  return fetchApi<{url: string}>("/api/billing/checkout/lifetime", {
    method: "POST",
    body: JSON.stringify({ tenantId, returnUrl }),
  });
}

export async function getSubscriptionStatus(tenantId: string): Promise<ApiResponse<import("@/types").SubscriptionStatus>> {
  return fetchApi<import("@/types").SubscriptionStatus>(`/api/billing/subscription/${tenantId}`);
}

export async function createBillingPortalSession(tenantId: string, returnUrl: string): Promise<ApiResponse<{url: string}>> {
  return fetchApi<{url: string}>("/api/billing/portal-session", {
    method: "POST",
    body: JSON.stringify({ tenantId, returnUrl }),
  });
}

// Health check
export async function getHealthStatus(): Promise<ApiResponse<{ status: string; timestamp: string }>> {
  return fetchApi<{ status: string; timestamp: string }>("/api/health");
}

// Tenant Operations
export interface TenantOperationResponse {
  message: string;
  jobName?: string;
}

export async function restartTenantApi(tenantId: string): Promise<ApiResponse<TenantOperationResponse>> {
  return fetchApi<TenantOperationResponse>(`/api/tenants/${tenantId}/restart-api`, {
    method: "POST",
  });
}

export async function restartTenantFrontend(tenantId: string): Promise<ApiResponse<TenantOperationResponse>> {
  return fetchApi<TenantOperationResponse>(`/api/tenants/${tenantId}/restart-frontend`, {
    method: "POST",
  });
}

export async function runTenantMigration(tenantId: string): Promise<ApiResponse<TenantOperationResponse>> {
  return fetchApi<TenantOperationResponse>(`/api/tenants/${tenantId}/run-migration`, {
    method: "POST",
  });
}

// Bulk Tenant Operations
export interface BulkOperationResult {
  tenantId: string;
  success?: boolean;
  jobName?: string;
  error?: string;
}

export interface BulkTenantOperationResponse {
  message: string;
  totalTenants: number;
  successCount: number;
  errorCount: number;
  results: BulkOperationResult[];
  errors: BulkOperationResult[];
}

export async function restartAllTenants(): Promise<ApiResponse<BulkTenantOperationResponse>> {
  return fetchApi<BulkTenantOperationResponse>("/api/tenants/restart-all", {
    method: "POST",
  });
}

export async function migrateAllTenants(): Promise<ApiResponse<BulkTenantOperationResponse>> {
  return fetchApi<BulkTenantOperationResponse>("/api/tenants/migrate-all", {
    method: "POST",
  });
}

// User API
export async function getCurrentUser(): Promise<ApiResponse<PortalUser>> {
  return fetchApi<PortalUser>("/api/user/me");
}

export async function getMyTenants(): Promise<ApiResponse<Tenant[]>> {
  return fetchApi<Tenant[]>("/api/user/tenants");
}

export async function requestNewTenant(data: RequestTenantData): Promise<ApiResponse<Tenant>> {
  return fetchApi<Tenant>("/api/user/request-tenant", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

// Suspend tenant
export interface SuspendTenantResponse {
  message: string;
  tenantId: string;
}

export async function suspendTenant(tenantId: string, reason: string): Promise<ApiResponse<SuspendTenantResponse>> {
  return fetchApi<SuspendTenantResponse>(`/api/tenants/${tenantId}/suspend`, {
    method: "POST",
    body: JSON.stringify({ reason }),
  });
}

// Domain Management API (for tenant owners)
export async function initiateDomainVerification(
  tenantId: number,
  domain: string
): Promise<ApiResponse<DomainVerification>> {
  return fetchApi<DomainVerification>(`/api/user/tenants/${tenantId}/domains`, {
    method: "POST",
    body: JSON.stringify({ domain }),
  });
}

export async function getTenantDomains(
  tenantId: number
): Promise<ApiResponse<{ domains: DomainVerification[] }>> {
  return fetchApi<{ domains: DomainVerification[] }>(
    `/api/user/tenants/${tenantId}/domains`
  );
}

export async function verifyDomain(
  tenantId: number,
  domain: string
): Promise<ApiResponse<DomainVerificationResult>> {
  return fetchApi<DomainVerificationResult>(
    `/api/user/tenants/${tenantId}/domains/${encodeURIComponent(domain)}/verify`,
    { method: "POST" }
  );
}

export async function getDomainStatus(
  tenantId: number,
  domain: string
): Promise<ApiResponse<DomainVerification>> {
  return fetchApi<DomainVerification>(
    `/api/user/tenants/${tenantId}/domains/${encodeURIComponent(domain)}/status`
  );
}

export async function removeDomain(
  tenantId: number,
  domain: string
): Promise<ApiResponse<void>> {
  return fetchApi<void>(
    `/api/user/tenants/${tenantId}/domains/${encodeURIComponent(domain)}`,
    { method: "DELETE" }
  );
}

// Audit Logs API
export interface AuditLog {
  id: number;
  adminId: number | null;
  adminEmail: string;
  adminRole: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  actionType: string;
  httpMethod: string;
  endpoint: string;
  resourceType: string | null;
  resourceId: string | null;
  tenantId: string | null;
  actionDescription: string | null;
  timestamp: string;
  durationMs: number | null;
  statusCode: number;
  wasSuccessful: boolean;
}

export interface AuditLogStats {
  totalRequests: number;
  uniqueAdmins: number;
  tenantOperations: number;
  authEvents: number;
  failedRequests: number;
}

export interface AuditLogPageResult {
  items: AuditLog[];
  total: number;
  hasMore: boolean;
}

export async function getAuditLogs(
  params: {
    searchText?: string;
    actionType?: string;
    tenantId?: string;
    fromDate?: string;
    toDate?: string;
    limit?: number;
    offset?: number;
  } = {}
): Promise<ApiResponse<AuditLogPageResult>> {
  const queryParams = new URLSearchParams();
  if (params.searchText) queryParams.set("searchText", params.searchText);
  if (params.actionType) queryParams.set("actionType", params.actionType);
  if (params.tenantId) queryParams.set("tenantId", params.tenantId);
  if (params.fromDate) queryParams.set("fromDate", params.fromDate);
  if (params.toDate) queryParams.set("toDate", params.toDate);
  if (params.limit) queryParams.set("limit", params.limit.toString());
  if (params.offset) queryParams.set("offset", params.offset.toString());

  return fetchApi<AuditLogPageResult>(`/api/auditlogs?${queryParams.toString()}`);
}

export async function getAuditLogStats(days = 7): Promise<ApiResponse<AuditLogStats>> {
  return fetchApi<AuditLogStats>(`/api/auditlogs/stats?days=${days}`);
}

// Subscription Management API
export interface GrantSubscriptionResponse {
  message: string;
  tenantId: string;
  tier: string;
  subscriptionEndsAt: string | null;
}

export async function grantLifetimeAccess(tenantId: string): Promise<ApiResponse<GrantSubscriptionResponse>> {
  return fetchApi<GrantSubscriptionResponse>(`/api/tenants/${tenantId}/grant-lifetime`, {
    method: "POST",
  });
}

export async function grant30DaysAccess(tenantId: string): Promise<ApiResponse<GrantSubscriptionResponse>> {
  return fetchApi<GrantSubscriptionResponse>(`/api/tenants/${tenantId}/grant-30-days`, {
    method: "POST",
  });
}
