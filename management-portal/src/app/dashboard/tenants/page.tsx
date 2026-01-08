import { redirect } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { Button } from "@/_components/ui/Button";
import { formatDate, formatRelativeTime } from "@/_lib/utils";
import { getTenants } from "@/_lib/api";
import { getCurrentUser } from "@/_lib/auth";
import Link from "next/link";
import { AlertCircle } from "lucide-react";
import { AddTenantButton } from "@/_components/TenantsPageClient";
import { BulkTenantOperationsButtons } from "@/_components/BulkTenantOperationsButtons";
import { DeleteTenantButton } from "@/_components/DeleteTenantButton";

const ADMIN_ROLES = ["super_admin", "admin", "support"];

const statusColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  active: "success",
  trial: "warning",
  suspended: "danger",
  deleted: "default",
  provisioning: "info",
};

export default async function TenantsPage() {
  // Check if user is admin
  const user = await getCurrentUser();
  if (!user || !ADMIN_ROLES.includes(user.role)) {
    redirect("/dashboard/account");
  }
  const isSuperAdmin = user.role === "admin" || user.role === "owner";

  const response = await getTenants(1, 50);

  if (!response.success || !response.data) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-dark-900">Tenants</h1>
            <p className="text-dark-500 mt-1">Manage all workshop tenants</p>
          </div>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <p>Unable to load tenants. Please check that the Management API is running.</p>
            </div>
            <p className="text-sm text-dark-500 mt-2">
              Error: {response.error || "Connection failed"}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const { items: tenants, total } = response.data;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-dark-900">Tenants</h1>
          <p className="text-dark-500 mt-1">Manage all workshop tenants</p>
        </div>
        <div className="flex items-center gap-4">
          <BulkTenantOperationsButtons />
          <AddTenantButton />
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>All Tenants ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {tenants.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Company</TableHead>
                  <TableHead>Subdomain</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Mechanics</TableHead>
                  <TableHead>Last Active</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {tenants.map((tenant) => (
                  <TableRow key={tenant.id}>
                    <TableCell>
                      <div>
                        <p className="font-semibold text-dark-900">{tenant.companyName}</p>
                        <p className="text-xs text-dark-500">Since {formatDate(tenant.createdAt)}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <a
                        href={tenant.apiUrl || `https://${tenant.tenantId}.mechanicbuddy.app`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-sm bg-dark-100 text-dark-700 px-2 py-1 rounded font-mono hover:bg-dark-200 transition-colors"
                      >
                        {tenant.tenantId}.mechanicbuddy.app
                      </a>
                    </TableCell>
                    <TableCell>
                      <Badge variant={statusColors[tenant.status] || "default"}>
                        {tenant.status}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-dark-700 font-medium">{tenant.mechanicCount ?? "-"}</TableCell>
                    <TableCell>
                      <span className="text-sm text-dark-500">
                        {tenant.lastActivityAt ? formatRelativeTime(tenant.lastActivityAt) : "Never"}
                      </span>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Link href={`/dashboard/tenants/${tenant.id}`}>
                          <Button variant="ghost" size="sm">View</Button>
                        </Link>
                        {isSuperAdmin && (
                          <DeleteTenantButton tenantId={tenant.tenantId} companyName={tenant.companyName} />
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-dark-500">
              <p>No tenants yet</p>
              <p className="text-sm mt-2">Tenants will appear here once they sign up or are created.</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
