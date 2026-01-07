import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Badge } from "@/_components/ui/Badge";
import { Button } from "@/_components/ui/Button";
import { formatDate } from "@/_lib/utils";
import Link from "next/link";
import { ArrowLeft, Building2, Users, Database, Activity, AlertCircle } from "lucide-react";
import { getTenant } from "@/_lib/api";
import { DeleteTenantButton } from "@/_components/DeleteTenantButton";
import { TenantOperationsButtons } from "@/_components/TenantOperationsButtons";

const statusColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  active: "success",
  trial: "warning",
  suspended: "danger",
  deleted: "default",
  provisioning: "info",
};

export default async function TenantDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const response = await getTenant(id);

  if (!response.success || !response.data) {
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
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <p>Unable to load tenant details.</p>
            </div>
            <p className="text-sm text-dark-500 mt-2">
              Error: {response.error || "Tenant not found"}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const tenant = response.data;

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
          <p className="text-gray-600 mt-1">Tenant ID: {tenant.tenantId}</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">Suspend</Button>
          <DeleteTenantButton tenantId={tenant.tenantId} companyName={tenant.companyName} />
        </div>
      </div>

      {/* Operations Card */}
      <Card>
        <CardHeader>
          <CardTitle>Deployment Operations</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-gray-600 mb-4">
            Manage the Kubernetes deployments and database for this tenant.
          </p>
          <TenantOperationsButtons tenantId={tenant.tenantId} />
        </CardContent>
      </Card>

      {/* Overview Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Tier</p>
                <p className="text-2xl font-bold text-gray-900 mt-1 capitalize">
                  {tenant.tier}
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
                <p className="text-sm text-gray-600">Mechanics</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">
                  {tenant.mechanicCount ?? 0} / {tenant.maxMechanics}
                </p>
              </div>
              <Users className="h-8 w-8 text-primary-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Storage Limit</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">
                  {tenant.maxStorage} MB
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
                <p className="text-sm text-gray-600">Status</p>
                <div className="mt-1">
                  <Badge variant={statusColors[tenant.status]}>
                    {tenant.status}
                  </Badge>
                </div>
              </div>
              <Building2 className="h-8 w-8 text-primary-600" />
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
                <a
                  href={tenant.apiUrl || `https://${tenant.tenantId}.mechanicbuddy.app`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="bg-gray-100 px-2 py-1 rounded font-mono hover:bg-gray-200 transition-colors"
                >
                  {tenant.tenantId}.mechanicbuddy.app
                </a>
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
              <label className="text-sm font-medium text-gray-600">Owner Email</label>
              <p className="text-gray-900 mt-1">{tenant.ownerEmail}</p>
            </div>
            {tenant.ownerName && (
              <div>
                <label className="text-sm font-medium text-gray-600">Owner Name</label>
                <p className="text-gray-900 mt-1">{tenant.ownerName}</p>
              </div>
            )}
            <div>
              <label className="text-sm font-medium text-gray-600">Demo Account</label>
              <p className="text-gray-900 mt-1">{tenant.isDemo ? "Yes" : "No"}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-600">Created</label>
              <p className="text-gray-900 mt-1">{formatDate(tenant.createdAt)}</p>
            </div>
            {tenant.trialEndsAt && (
              <div>
                <label className="text-sm font-medium text-gray-600">Trial Ends</label>
                <p className="text-gray-900 mt-1">{formatDate(tenant.trialEndsAt)}</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Activity & Billing */}
        <Card>
          <CardHeader>
            <CardTitle>Activity & Billing</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-gray-600">Last Activity</label>
              <p className="text-gray-900 mt-1">
                {tenant.lastActivityAt ? formatDate(tenant.lastActivityAt) : "Never"}
              </p>
            </div>
            {tenant.lastBilledAt && (
              <div>
                <label className="text-sm font-medium text-gray-600">Last Billed</label>
                <p className="text-gray-900 mt-1">{formatDate(tenant.lastBilledAt)}</p>
              </div>
            )}
            {tenant.subscriptionEndsAt && (
              <div>
                <label className="text-sm font-medium text-gray-600">Subscription Ends</label>
                <p className="text-gray-900 mt-1">{formatDate(tenant.subscriptionEndsAt)}</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
