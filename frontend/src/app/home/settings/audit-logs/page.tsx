'use server'

import Main from "../../_components/Main";
import { SearchCardHeader } from "../../_components/SearchCardHeader";
import { httpGet } from "@/_lib/server/query-api";
import AuditLogsTable from "./_components/AuditLogsTable";
import AuditLogsFilters from "./_components/AuditLogsFilters";
import { redirect } from "next/navigation";

interface AuditLog {
  id: string;
  userName: string;
  employeeId: string | null;
  ipAddress: string;
  userAgent: string;
  actionType: string;
  httpMethod: string;
  endpoint: string;
  resourceType: string | null;
  resourceId: string | null;
  actionDescription: string;
  timestamp: string;
  durationMs: number | null;
  statusCode: number;
  wasSuccessful: boolean;
}

interface AuditLogPageResult {
  items: AuditLog[];
  total: number;
  hasMore: boolean;
}

interface AuditLogStats {
  totalRequests: number;
  uniqueUsers: number;
  crudOperations: number;
  authEvents: number;
  failedRequests: number;
}

export default async function Page({
  searchParams
}: {
  searchParams: Promise<Record<string, string>>
}) {
  const params = await searchParams;

  // Check if user can view audit logs
  const canViewResponse = await httpGet('auditlogs/canview');
  const canViewData = await canViewResponse.json();

  if (!canViewData.canView) {
    redirect('/home/settings');
  }

  // Build query string from search params
  const queryParams = new URLSearchParams();
  if (params.searchText) queryParams.set('searchText', params.searchText);
  if (params.actionType) queryParams.set('actionType', params.actionType);
  if (params.fromDate) queryParams.set('fromDate', params.fromDate);
  if (params.toDate) queryParams.set('toDate', params.toDate);
  queryParams.set('limit', params.limit || '50');
  queryParams.set('offset', params.offset || '0');

  const [logsResponse, statsResponse] = await Promise.all([
    httpGet(`auditlogs?${queryParams.toString()}`),
    httpGet('auditlogs/stats?days=7')
  ]);

  const data: AuditLogPageResult = await logsResponse.json();
  const stats: AuditLogStats = await statsResponse.json();

  return (
    <Main header={
      <SearchCardHeader title="Audit Logs" pageName="settings/audit-logs" />
    } narrow={false}>

      {/* Stats Summary */}
      <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-6">
        <StatCard label="Total Requests (7d)" value={stats.totalRequests} />
        <StatCard label="Unique Users" value={stats.uniqueUsers} />
        <StatCard label="CRUD Operations" value={stats.crudOperations} />
        <StatCard label="Auth Events" value={stats.authEvents} />
        <StatCard label="Failed Requests" value={stats.failedRequests} className="text-red-600" />
      </div>

      {/* Filters */}
      <AuditLogsFilters searchParams={params} />

      {/* Table */}
      <AuditLogsTable
        logs={data.items}
        total={data.total}
        hasMore={data.hasMore}
        currentOffset={parseInt(params.offset || '0')}
        limit={parseInt(params.limit || '50')}
      />
    </Main>
  );
}

function StatCard({ label, value, className = '' }: { label: string; value: number; className?: string }) {
  return (
    <div className="bg-white rounded-lg shadow p-4 border border-gray-200">
      <dt className="text-sm font-medium text-gray-500 truncate">{label}</dt>
      <dd className={`mt-1 text-2xl font-semibold ${className || 'text-gray-900'}`}>
        {value.toLocaleString()}
      </dd>
    </div>
  );
}
