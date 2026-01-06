import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { StatCard } from "@/_components/dashboard/StatCard";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { Building2, Users, DollarSign, MessageSquare, TrendingUp, AlertCircle } from "lucide-react";
import { formatCurrency, formatDate } from "@/_lib/utils";
import { getDashboardAnalytics } from "@/_lib/api";

export default async function DashboardPage() {
  const response = await getDashboardAnalytics();

  if (!response.success || !response.data) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-dark-900">Dashboard Overview</h1>
          <p className="text-dark-500 mt-1">Monitor your SaaS platform performance</p>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <p>Unable to load dashboard data. Please check that the Management API is running.</p>
            </div>
            <p className="text-sm text-dark-500 mt-2">
              Error: {response.error || "Connection failed"}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const analytics = response.data;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-dark-900">Dashboard Overview</h1>
        <p className="text-dark-500 mt-1">Monitor your SaaS platform performance</p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title="Total Tenants"
          value={analytics.totalTenants}
          icon={Building2}
          trend={analytics.totalTenants > 0 ? { value: 12, isPositive: true } : undefined}
        />
        <StatCard
          title="Active Tenants"
          value={analytics.activeTenants}
          icon={Users}
          trend={analytics.activeTenants > 0 ? { value: 8, isPositive: true } : undefined}
        />
        <StatCard
          title="MRR"
          value={formatCurrency(analytics.monthlyRecurringRevenue)}
          icon={DollarSign}
          trend={analytics.monthlyRecurringRevenue > 0 ? { value: 15, isPositive: true } : undefined}
        />
        <StatCard
          title="Pending Demos"
          value={analytics.pendingDemoRequests}
          icon={MessageSquare}
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Revenue Chart */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-primary-600" />
              Monthly Recurring Revenue
            </CardTitle>
          </CardHeader>
          <CardContent>
            {analytics.revenueByMonth.length > 0 ? (
              <div className="space-y-4">
                {analytics.revenueByMonth.map((data) => (
                  <div key={data.month} className="flex items-center justify-between">
                    <span className="text-sm font-medium text-dark-600">{data.month}</span>
                    <div className="flex items-center gap-4 flex-1 ml-4">
                      <div className="flex-1 bg-dark-100 rounded-full h-2.5">
                        <div
                          className="bg-primary-600 h-2.5 rounded-full"
                          style={{ width: `${Math.min((data.revenue / (Math.max(...analytics.revenueByMonth.map(d => d.revenue)) || 1)) * 100, 100)}%` }}
                        />
                      </div>
                      <span className="text-sm font-bold text-dark-900 w-20 text-right">
                        {formatCurrency(data.revenue)}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-dark-500 text-sm">No revenue data yet</p>
            )}
          </CardContent>
        </Card>

        {/* Recent Tenants */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Tenants</CardTitle>
          </CardHeader>
          <CardContent>
            {analytics.recentTenants.length > 0 ? (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Company</TableHead>
                    <TableHead>Plan</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {analytics.recentTenants.map((tenant) => (
                    <TableRow key={tenant.id}>
                      <TableCell>
                        <div>
                          <p className="font-semibold text-dark-900">{tenant.companyName}</p>
                          <p className="text-xs text-dark-500">{formatDate(tenant.createdAt)}</p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className="text-sm capitalize text-dark-700">{tenant.plan}</span>
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={
                            tenant.status === "active"
                              ? "success"
                              : tenant.status === "trial"
                              ? "warning"
                              : "default"
                          }
                        >
                          {tenant.status}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : (
              <p className="text-dark-500 text-sm">No tenants yet</p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Conversion & Distribution */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Demo Conversion Stats */}
        <Card>
          <CardHeader>
            <CardTitle>Demo Conversion</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">Total Demo Requests</span>
                <span className="font-semibold">{analytics.totalDemoRequests}</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">Pending</span>
                <span className="font-semibold text-amber-600">{analytics.pendingDemoRequests}</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">Conversion Rate</span>
                <span className="font-semibold text-green-600">{analytics.conversionRate}%</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">Avg Revenue per Tenant</span>
                <span className="font-semibold">{formatCurrency(analytics.averageRevenuePerTenant)}</span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Plan Distribution */}
        <Card>
          <CardHeader>
            <CardTitle>Tenants by Plan</CardTitle>
          </CardHeader>
          <CardContent>
            {analytics.tenantsByPlan.length > 0 ? (
              <div className="space-y-4">
                {analytics.tenantsByPlan.map((plan) => (
                  <div key={plan.plan} className="flex items-center justify-between">
                    <span className="text-sm font-medium text-gray-700 capitalize">{plan.plan}</span>
                    <div className="flex items-center gap-4">
                      <span className="text-sm text-gray-500">{plan.count} tenants</span>
                      <span className="text-sm font-semibold text-gray-900 w-20 text-right">
                        {formatCurrency(plan.revenue)}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-gray-500 text-sm">No plan data yet</p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
