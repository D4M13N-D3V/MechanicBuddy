import { redirect } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { formatDate, formatCurrency } from "@/_lib/utils";
import { getTenants, getSubscriptionStatus } from "@/_lib/api";
import { getCurrentUser } from "@/_lib/auth";
import { AlertCircle, CreditCard, DollarSign, FileText, TrendingUp } from "lucide-react";

const ADMIN_ROLES = ["super_admin", "admin", "support"];

const tierColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  free: "default",
  team: "info",
  lifetime: "success",
  standard: "info",
  growth: "warning",
  scale: "success",
};

export default async function AdminBillingPage() {
  // Check if user is admin
  const user = await getCurrentUser();
  if (!user || !ADMIN_ROLES.includes(user.role)) {
    redirect("/dashboard/billing");
  }

  const tenantsResponse = await getTenants(1, 100);

  if (!tenantsResponse.success || !tenantsResponse.data) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-dark-900">Billing Overview</h1>
          <p className="text-dark-500 mt-1">View all subscriptions and revenue</p>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <p>Unable to load billing information. Please check that the Management API is running.</p>
            </div>
            <p className="text-sm text-dark-500 mt-2">
              Error: {tenantsResponse.error || "Connection failed"}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const { items: tenants } = tenantsResponse.data;

  // Fetch subscription status for each tenant
  const tenantsWithBilling = await Promise.all(
    tenants.map(async (tenant) => {
      const statusResponse = await getSubscriptionStatus(tenant.tenantId);
      return {
        tenant,
        subscription: statusResponse.success ? statusResponse.data : null,
      };
    })
  );

  // Calculate stats
  const activeSubscriptions = tenantsWithBilling.filter(t => t.subscription?.hasSubscription).length;
  const lifetimeAccess = tenantsWithBilling.filter(t => t.subscription?.tier === 'lifetime').length;
  const teamSubscriptions = tenantsWithBilling.filter(t => t.subscription?.tier === 'team').length;
  const freeTier = tenantsWithBilling.filter(t => !t.subscription?.tier || t.subscription?.tier === 'free').length;

  // Calculate MRR (Team subscriptions * $20)
  const mrr = teamSubscriptions * 20;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-dark-900">Billing Overview</h1>
        <p className="text-dark-500 mt-1">View all subscriptions and revenue across all tenants</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-100 rounded-lg">
                <TrendingUp className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-dark-500">MRR</p>
                <p className="text-2xl font-bold text-dark-900">{formatCurrency(mrr)}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-100 rounded-lg">
                <CreditCard className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-dark-500">Active Subscriptions</p>
                <p className="text-2xl font-bold text-dark-900">{activeSubscriptions}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-purple-100 rounded-lg">
                <CreditCard className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-sm text-dark-500">Team Plans</p>
                <p className="text-2xl font-bold text-dark-900">{teamSubscriptions}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-emerald-100 rounded-lg">
                <DollarSign className="h-5 w-5 text-emerald-600" />
              </div>
              <div>
                <p className="text-sm text-dark-500">Lifetime Access</p>
                <p className="text-2xl font-bold text-dark-900">{lifetimeAccess}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-gray-100 rounded-lg">
                <FileText className="h-5 w-5 text-gray-600" />
              </div>
              <div>
                <p className="text-sm text-dark-500">Free Tier</p>
                <p className="text-2xl font-bold text-dark-900">{freeTier}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* All Tenants Table */}
      <Card>
        <CardHeader>
          <CardTitle>All Tenant Subscriptions</CardTitle>
        </CardHeader>
        <CardContent>
          {tenantsWithBilling.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Tenant</TableHead>
                  <TableHead>Owner</TableHead>
                  <TableHead>Plan</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Next Billing</TableHead>
                  <TableHead>Created</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {tenantsWithBilling.map(({ tenant, subscription }) => (
                  <TableRow key={tenant.id}>
                    <TableCell>
                      <div>
                        <p className="font-semibold text-dark-900">{tenant.companyName}</p>
                        <p className="text-xs text-dark-500">{tenant.tenantId}.mechanicbuddy.app</p>
                      </div>
                    </TableCell>
                    <TableCell className="text-sm text-dark-600">
                      {tenant.ownerEmail || '-'}
                    </TableCell>
                    <TableCell>
                      <Badge variant={tierColors[subscription?.tier || 'free'] || "default"}>
                        {subscription?.tier?.toUpperCase() || 'FREE'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant={tenant.status === 'active' ? 'success' : 'default'}>
                        {tenant.status}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-sm text-dark-600">
                      {subscription?.subscription?.currentPeriodEnd
                        ? formatDate(subscription.subscription.currentPeriodEnd)
                        : '-'}
                    </TableCell>
                    <TableCell className="text-sm text-dark-500">
                      {formatDate(tenant.createdAt)}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-dark-500">
              <p>No tenants yet</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Revenue Summary */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Revenue Breakdown</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <span className="text-sm text-dark-500">Monthly Recurring (Team Plans)</span>
                <span className="font-bold text-dark-900">{formatCurrency(mrr)}/mo</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-dark-500">Lifetime Purchases (One-time)</span>
                <span className="font-bold text-dark-900">{formatCurrency(lifetimeAccess * 250)}</span>
              </div>
              <div className="flex justify-between items-center pt-4 border-t border-dark-200">
                <span className="text-sm font-semibold text-dark-700">Annual Recurring Revenue (ARR)</span>
                <span className="font-bold text-emerald-600">{formatCurrency(mrr * 12)}</span>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Plan Distribution</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 bg-gray-400 rounded-full"></div>
                  <span className="text-sm text-dark-600">Free</span>
                </div>
                <span className="font-bold text-dark-900">{freeTier}</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 bg-blue-500 rounded-full"></div>
                  <span className="text-sm text-dark-600">Team ($20/mo)</span>
                </div>
                <span className="font-bold text-dark-900">{teamSubscriptions}</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 bg-emerald-500 rounded-full"></div>
                  <span className="text-sm text-dark-600">Lifetime ($250)</span>
                </div>
                <span className="font-bold text-dark-900">{lifetimeAccess}</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
