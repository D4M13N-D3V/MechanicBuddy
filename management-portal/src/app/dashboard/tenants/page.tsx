import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { Button } from "@/_components/ui/Button";
import { formatDate, formatRelativeTime } from "@/_lib/utils";
import { getTenants } from "@/_lib/api";
import Link from "next/link";
import { AlertCircle } from "lucide-react";

const planColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  free: "default",
  starter: "info",
  professional: "warning",
  enterprise: "success",
};

const statusColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  active: "success",
  trial: "warning",
  suspended: "danger",
  cancelled: "default",
};

export default async function TenantsPage() {
  const response = await getTenants(1, 50);

  if (!response.success || !response.data) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Tenants</h1>
            <p className="text-gray-600 mt-1">Manage all workshop tenants</p>
          </div>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <p>Unable to load tenants. Please check that the Management API is running.</p>
            </div>
            <p className="text-sm text-gray-500 mt-2">
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
          <h1 className="text-3xl font-bold text-gray-900">Tenants</h1>
          <p className="text-gray-600 mt-1">Manage all workshop tenants</p>
        </div>
        <Button>Add Tenant</Button>
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
                  <TableHead>Plan</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Mechanics</TableHead>
                  <TableHead>Storage</TableHead>
                  <TableHead>Last Active</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {tenants.map((tenant) => (
                  <TableRow key={tenant.id}>
                    <TableCell>
                      <div>
                        <p className="font-medium text-gray-900">{tenant.companyName}</p>
                        <p className="text-xs text-gray-500">Since {formatDate(tenant.createdAt)}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <code className="text-sm bg-gray-100 px-2 py-1 rounded">
                        {tenant.subdomain}
                      </code>
                    </TableCell>
                    <TableCell>
                      <Badge variant={planColors[tenant.plan] || "default"}>
                        {tenant.plan}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant={statusColors[tenant.status] || "default"}>
                        {tenant.status}
                      </Badge>
                    </TableCell>
                    <TableCell>{tenant.mechanicCount}</TableCell>
                    <TableCell>
                      {(tenant.storageUsedMb / 1024).toFixed(1)} GB
                    </TableCell>
                    <TableCell>
                      <span className="text-sm text-gray-600">
                        {tenant.lastActivityAt ? formatRelativeTime(tenant.lastActivityAt) : "Never"}
                      </span>
                    </TableCell>
                    <TableCell>
                      <Link href={`/dashboard/tenants/${tenant.id}`}>
                        <Button variant="ghost" size="sm">View</Button>
                      </Link>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <p>No tenants yet</p>
              <p className="text-sm mt-2">Tenants will appear here once they sign up or are created.</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
