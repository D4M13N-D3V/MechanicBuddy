import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { Button } from "@/_components/ui/Button";
import { formatDate, formatRelativeTime } from "@/_lib/utils";
import Link from "next/link";

// Mock data - replace with actual API calls
const tenants = [
  {
    id: "1",
    companyName: "Auto Express LLC",
    subdomain: "autoexpress",
    plan: "standard",
    status: "active",
    mechanicCount: 5,
    storageUsedMb: 2400,
    lastActivityAt: "2026-01-06T10:30:00Z",
    createdAt: "2025-11-15T00:00:00Z",
  },
  {
    id: "2",
    companyName: "Quick Fix Garage",
    subdomain: "quickfix",
    plan: "premium",
    status: "trial",
    mechanicCount: 3,
    storageUsedMb: 1200,
    lastActivityAt: "2026-01-06T09:15:00Z",
    createdAt: "2026-01-04T00:00:00Z",
  },
  {
    id: "3",
    companyName: "City Motors",
    subdomain: "citymotors",
    plan: "standard",
    status: "active",
    mechanicCount: 8,
    storageUsedMb: 5600,
    lastActivityAt: "2026-01-05T18:45:00Z",
    createdAt: "2025-09-22T00:00:00Z",
  },
  {
    id: "4",
    companyName: "Elite Auto Service",
    subdomain: "eliteauto",
    plan: "enterprise",
    status: "active",
    mechanicCount: 15,
    storageUsedMb: 12800,
    lastActivityAt: "2026-01-06T11:00:00Z",
    createdAt: "2025-06-10T00:00:00Z",
  },
  {
    id: "5",
    companyName: "Speedy Repairs",
    subdomain: "speedyrepairs",
    plan: "free",
    status: "active",
    mechanicCount: 1,
    storageUsedMb: 450,
    lastActivityAt: "2026-01-03T14:20:00Z",
    createdAt: "2025-12-28T00:00:00Z",
  },
  {
    id: "6",
    companyName: "Premium Motors Inc",
    subdomain: "premiummotors",
    plan: "premium",
    status: "suspended",
    mechanicCount: 6,
    storageUsedMb: 3200,
    lastActivityAt: "2025-12-20T10:00:00Z",
    createdAt: "2025-08-15T00:00:00Z",
  },
];

const planColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  free: "default",
  standard: "info",
  premium: "warning",
  enterprise: "success",
};

const statusColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  active: "success",
  trial: "warning",
  suspended: "danger",
  cancelled: "default",
};

export default function TenantsPage() {
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
          <CardTitle>All Tenants ({tenants.length})</CardTitle>
        </CardHeader>
        <CardContent>
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
                    <Badge variant={planColors[tenant.plan]}>
                      {tenant.plan}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Badge variant={statusColors[tenant.status]}>
                      {tenant.status}
                    </Badge>
                  </TableCell>
                  <TableCell>{tenant.mechanicCount}</TableCell>
                  <TableCell>
                    {(tenant.storageUsedMb / 1024).toFixed(1)} GB
                  </TableCell>
                  <TableCell>
                    <span className="text-sm text-gray-600">
                      {formatRelativeTime(tenant.lastActivityAt)}
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
        </CardContent>
      </Card>
    </div>
  );
}
