import { redirect } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { getAuditLogs, getAuditLogStats } from "@/_lib/api";
import { getCurrentUser } from "@/_lib/auth";
import { AlertCircle } from "lucide-react";
import AuditLogsTable from "./_components/AuditLogsTable";
import AuditLogsFilters from "./_components/AuditLogsFilters";

const ADMIN_ROLES = ["admin", "support"];

export default async function AuditLogsPage({
  searchParams
}: {
  searchParams: Promise<Record<string, string>>
}) {
  // Check if user is admin
  const user = await getCurrentUser();
  if (!user || !ADMIN_ROLES.includes(user.role)) {
    redirect("/dashboard/account");
  }

  const params = await searchParams;

  const [logsResponse, statsResponse] = await Promise.all([
    getAuditLogs({
      searchText: params.searchText,
      actionType: params.actionType,
      tenantId: params.tenantId,
      limit: parseInt(params.limit || "50"),
      offset: parseInt(params.offset || "0"),
    }),
    getAuditLogStats(7),
  ]);

  if (!logsResponse.success || !statsResponse.success) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-dark-900">Audit Logs</h1>
          <p className="text-dark-500 mt-1">View all administrative actions and API requests</p>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <p>Unable to load audit logs. Please check that the Management API is running.</p>
            </div>
            <p className="text-sm text-dark-500 mt-2">
              Error: {logsResponse.error || statsResponse.error || "Connection failed"}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const logs = logsResponse.data!;
  const stats = statsResponse.data!;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-dark-900">Audit Logs</h1>
        <p className="text-dark-500 mt-1">View all administrative actions and API requests</p>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
        <Card>
          <CardContent className="pt-4">
            <p className="text-sm text-dark-500">Total Requests (7d)</p>
            <p className="text-2xl font-bold text-dark-900">{stats.totalRequests.toLocaleString()}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <p className="text-sm text-dark-500">Unique Admins</p>
            <p className="text-2xl font-bold text-dark-900">{stats.uniqueAdmins.toLocaleString()}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <p className="text-sm text-dark-500">Tenant Operations</p>
            <p className="text-2xl font-bold text-dark-900">{stats.tenantOperations.toLocaleString()}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <p className="text-sm text-dark-500">Auth Events</p>
            <p className="text-2xl font-bold text-dark-900">{stats.authEvents.toLocaleString()}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <p className="text-sm text-dark-500">Failed Requests</p>
            <p className="text-2xl font-bold text-red-600">{stats.failedRequests.toLocaleString()}</p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Activity Log</CardTitle>
        </CardHeader>
        <CardContent>
          <AuditLogsFilters searchParams={params} />
          <AuditLogsTable
            logs={logs.items}
            total={logs.total}
            hasMore={logs.hasMore}
            currentOffset={parseInt(params.offset || "0")}
            limit={parseInt(params.limit || "50")}
          />
        </CardContent>
      </Card>
    </div>
  );
}
