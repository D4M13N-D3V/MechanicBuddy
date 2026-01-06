import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Badge } from "@/_components/ui/Badge";
import { Button } from "@/_components/ui/Button";
import { formatDate, formatCurrency } from "@/_lib/utils";
import Link from "next/link";
import { ArrowLeft, Building2, Users, Database, Activity } from "lucide-react";

// Mock data - replace with actual API calls
const tenant = {
  id: "1",
  companyName: "Auto Express LLC",
  subdomain: "autoexpress",
  plan: "standard",
  status: "active",
  billingEmail: "billing@autoexpress.com",
  mechanicCount: 5,
  storageUsedMb: 2400,
  lastActivityAt: "2026-01-06T10:30:00Z",
  createdAt: "2025-11-15T00:00:00Z",
  updatedAt: "2026-01-06T10:30:00Z",
};

const stats = {
  totalRevenue: 600,
  totalWorkOrders: 234,
  totalInvoices: 189,
  activeUsers: 5,
};

const recentActivity = [
  { action: "Invoice created", timestamp: "2026-01-06T10:30:00Z" },
  { action: "Work order completed", timestamp: "2026-01-06T09:15:00Z" },
  { action: "New client added", timestamp: "2026-01-05T16:20:00Z" },
  { action: "User login", timestamp: "2026-01-05T14:45:00Z" },
];

export default async function TenantDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

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

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/dashboard/tenants">
          <Button variant="ghost" size="sm">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Tenants
          </Button>
        </Link>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{tenant.companyName}</h1>
          <p className="text-gray-600 mt-1">Tenant ID: {id}</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">Suspend</Button>
          <Button variant="danger">Delete</Button>
        </div>
      </div>

      {/* Overview Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Total Revenue</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">
                  {formatCurrency(stats.totalRevenue)}
                </p>
              </div>
              <Activity className="h-8 w-8 text-primary-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Work Orders</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">
                  {stats.totalWorkOrders}
                </p>
              </div>
              <Building2 className="h-8 w-8 text-primary-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Invoices</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">
                  {stats.totalInvoices}
                </p>
              </div>
              <Database className="h-8 w-8 text-primary-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Active Users</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">
                  {stats.activeUsers}
                </p>
              </div>
              <Users className="h-8 w-8 text-primary-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Tenant Information */}
        <Card>
          <CardHeader>
            <CardTitle>Tenant Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-gray-600">Subdomain</label>
              <p className="text-gray-900 mt-1">
                <code className="bg-gray-100 px-2 py-1 rounded">{tenant.subdomain}</code>
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-600">Status</label>
              <div className="mt-1">
                <Badge variant={statusColors[tenant.status]}>
                  {tenant.status}
                </Badge>
              </div>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-600">Billing Email</label>
              <p className="text-gray-900 mt-1">{tenant.billingEmail}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-600">Mechanics</label>
              <p className="text-gray-900 mt-1">{tenant.mechanicCount}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-600">Created</label>
              <p className="text-gray-900 mt-1">{formatDate(tenant.createdAt)}</p>
            </div>
          </CardContent>
        </Card>

        {/* Recent Activity */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Activity</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {recentActivity.map((activity, index) => (
                <div key={index} className="flex items-start gap-3">
                  <div className="h-2 w-2 bg-primary-600 rounded-full mt-2" />
                  <div className="flex-1">
                    <p className="text-sm font-medium text-gray-900">{activity.action}</p>
                    <p className="text-xs text-gray-500">{formatDate(activity.timestamp)}</p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
