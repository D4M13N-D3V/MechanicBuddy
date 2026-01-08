import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { formatDate } from "@/_lib/utils";
import { getMyTenants, getSubscriptionStatus } from "@/_lib/api";
import { AlertCircle, CreditCard, DollarSign, FileText } from "lucide-react";
import { BillingActions } from "./BillingActions";

const tierColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  free: "default",
  team: "info",
  lifetime: "success",
  standard: "info",
  growth: "warning",
  scale: "success",
};

export default async function BillingPage() {
  const tenantsResponse = await getMyTenants();

  if (!tenantsResponse.success || !tenantsResponse.data) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-dark-900">Billing</h1>
          <p className="text-dark-500 mt-1">Manage subscriptions and billing for your tenants</p>
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

  const tenants = tenantsResponse.data;

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

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-dark-900">Billing</h1>
        <p className="text-dark-500 mt-1">Manage subscriptions and billing for your tenants</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-100 rounded-lg">
                <CreditCard className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-dark-500">Active Subscriptions</p>
                <p className="text-2xl font-bold text-dark-900">
                  {tenantsWithBilling.filter(t => t.subscription?.hasSubscription).length}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-100 rounded-lg">
                <DollarSign className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-dark-500">Lifetime Access</p>
                <p className="text-2xl font-bold text-dark-900">
                  {tenantsWithBilling.filter(t => t.subscription?.tier === 'lifetime').length}
                </p>
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
                <p className="text-2xl font-bold text-dark-900">
                  {tenantsWithBilling.filter(t => t.subscription?.tier === 'free').length}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Your Tenants</CardTitle>
        </CardHeader>
        <CardContent>
          {tenantsWithBilling.length > 0 ? (
            <div className="space-y-6">
              {tenantsWithBilling.map(({ tenant, subscription }) => (
                <div key={tenant.id} className="border border-dark-200 rounded-lg p-6">
                  <div className="flex items-start justify-between mb-4">
                    <div>
                      <h3 className="text-lg font-semibold text-dark-900">{tenant.companyName}</h3>
                      <p className="text-sm text-dark-500">{tenant.tenantId}.mechanicbuddy.app</p>
                    </div>
                    <Badge variant={tierColors[subscription?.tier || 'free'] || "default"}>
                      {subscription?.tier?.toUpperCase() || 'FREE'}
                    </Badge>
                  </div>

                  {subscription && (
                    <>
                      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                        <div>
                          <p className="text-xs text-dark-500 mb-1">Status</p>
                          <p className="text-sm font-medium text-dark-900">{subscription.status}</p>
                        </div>
                        {subscription.subscription && (
                          <>
                            <div>
                              <p className="text-xs text-dark-500 mb-1">Subscription Status</p>
                              <p className="text-sm font-medium text-dark-900">
                                {subscription.subscription.status}
                              </p>
                            </div>
                            {subscription.subscription.currentPeriodEnd && (
                              <div>
                                <p className="text-xs text-dark-500 mb-1">Next Billing Date</p>
                                <p className="text-sm font-medium text-dark-900">
                                  {formatDate(subscription.subscription.currentPeriodEnd)}
                                </p>
                              </div>
                            )}
                          </>
                        )}
                      </div>

                      <div className="flex items-center gap-2">
                        <BillingActions
                          tenantId={tenant.tenantId}
                          tier={subscription.tier}
                          hasSubscription={subscription.hasSubscription}
                        />
                      </div>

                      {subscription.invoices && subscription.invoices.length > 0 && (
                        <div className="mt-4 pt-4 border-t border-dark-200">
                          <h4 className="text-sm font-semibold text-dark-900 mb-3">Recent Invoices</h4>
                          <Table>
                            <TableHeader>
                              <TableRow>
                                <TableHead>Date</TableHead>
                                <TableHead>Amount</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead>Actions</TableHead>
                              </TableRow>
                            </TableHeader>
                            <TableBody>
                              {subscription.invoices.slice(0, 5).map((invoice) => (
                                <TableRow key={invoice.id}>
                                  <TableCell className="text-sm">
                                    {formatDate(invoice.date)}
                                  </TableCell>
                                  <TableCell className="text-sm font-medium">
                                    ${invoice.amount.toFixed(2)} {invoice.currency.toUpperCase()}
                                  </TableCell>
                                  <TableCell>
                                    <Badge variant={invoice.status === 'paid' ? 'success' : 'default'}>
                                      {invoice.status}
                                    </Badge>
                                  </TableCell>
                                  <TableCell>
                                    {invoice.hostedUrl && (
                                      <a
                                        href={invoice.hostedUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="text-sm text-blue-600 hover:text-blue-700"
                                      >
                                        View
                                      </a>
                                    )}
                                    {invoice.pdfUrl && (
                                      <a
                                        href={invoice.pdfUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="text-sm text-blue-600 hover:text-blue-700 ml-3"
                                      >
                                        PDF
                                      </a>
                                    )}
                                  </TableCell>
                                </TableRow>
                              ))}
                            </TableBody>
                          </Table>
                        </div>
                      )}
                    </>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8 text-dark-500">
              <p>No tenants yet</p>
              <p className="text-sm mt-2">Create a tenant to get started.</p>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Pricing Plans</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="border border-dark-200 rounded-lg p-6">
              <div className="mb-4">
                <h3 className="text-xl font-bold text-dark-900">Team</h3>
                <p className="text-3xl font-bold text-blue-600 mt-2">$20<span className="text-lg text-dark-500">/month</span></p>
              </div>
              <ul className="space-y-2 text-sm text-dark-700">
                <li>Unlimited mechanics</li>
                <li>Advanced features</li>
                <li>Priority support</li>
                <li>Cancel anytime</li>
              </ul>
            </div>

            <div className="border border-green-500 border-2 rounded-lg p-6 relative">
              <div className="absolute -top-3 right-4 bg-green-500 text-white px-3 py-1 rounded-full text-xs font-semibold">
                BEST VALUE
              </div>
              <div className="mb-4">
                <h3 className="text-xl font-bold text-dark-900">Lifetime</h3>
                <p className="text-3xl font-bold text-green-600 mt-2">$250<span className="text-lg text-dark-500"> once</span></p>
              </div>
              <ul className="space-y-2 text-sm text-dark-700">
                <li>Everything in Team</li>
                <li>Pay once, use forever</li>
                <li>No recurring fees</li>
                <li>Lifetime updates</li>
              </ul>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
