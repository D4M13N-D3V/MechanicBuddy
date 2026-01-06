import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { StatCard } from "@/_components/dashboard/StatCard";
import { DollarSign, TrendingUp, CreditCard, Users } from "lucide-react";
import { formatCurrency, formatDate } from "@/_lib/utils";

// Mock data - replace with actual API calls
const stats = {
  totalRevenue: 24560,
  monthlyRecurringRevenue: 4780,
  averageRevenuePerTenant: 25.32,
  activeSubscriptions: 189,
};

const transactions = [
  {
    id: "1",
    tenantName: "Auto Express LLC",
    amount: 100,
    status: "completed",
    type: "subscription",
    createdAt: "2026-01-06T00:00:00Z",
    stripePaymentId: "pi_1234567890",
  },
  {
    id: "2",
    tenantName: "Quick Fix Garage",
    amount: 150,
    status: "completed",
    type: "subscription",
    createdAt: "2026-01-06T00:00:00Z",
    stripePaymentId: "pi_0987654321",
  },
  {
    id: "3",
    tenantName: "City Motors",
    amount: 160,
    status: "completed",
    type: "subscription",
    createdAt: "2026-01-05T00:00:00Z",
    stripePaymentId: "pi_1122334455",
  },
  {
    id: "4",
    tenantName: "Elite Auto Service",
    amount: 750,
    status: "completed",
    type: "subscription",
    createdAt: "2026-01-05T00:00:00Z",
    stripePaymentId: "pi_5544332211",
  },
  {
    id: "5",
    tenantName: "Premium Motors Inc",
    amount: 300,
    status: "failed",
    type: "subscription",
    createdAt: "2026-01-04T00:00:00Z",
    stripePaymentId: "pi_9988776655",
  },
  {
    id: "6",
    tenantName: "Speedy Repairs",
    amount: 0,
    status: "completed",
    type: "subscription",
    createdAt: "2026-01-04T00:00:00Z",
    stripePaymentId: null,
  },
];

const revenueByPlan = [
  { plan: "Free", tenants: 58, revenue: 0 },
  { plan: "Standard", tenants: 112, revenue: 2240 },
  { plan: "Premium", tenants: 65, revenue: 1950 },
  { plan: "Enterprise", tenants: 12, revenue: 600 },
];

const statusColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  completed: "success",
  pending: "warning",
  failed: "danger",
  refunded: "default",
};

export default function BillingPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Billing & Revenue</h1>
        <p className="text-gray-600 mt-1">Monitor subscription revenue and transactions</p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title="Total Revenue"
          value={formatCurrency(stats.totalRevenue)}
          icon={DollarSign}
          trend={{ value: 18, isPositive: true }}
        />
        <StatCard
          title="MRR"
          value={formatCurrency(stats.monthlyRecurringRevenue)}
          icon={TrendingUp}
          trend={{ value: 15, isPositive: true }}
        />
        <StatCard
          title="Avg Revenue/Tenant"
          value={formatCurrency(stats.averageRevenuePerTenant)}
          icon={CreditCard}
        />
        <StatCard
          title="Active Subscriptions"
          value={stats.activeSubscriptions}
          icon={Users}
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Revenue by Plan */}
        <Card>
          <CardHeader>
            <CardTitle>Revenue by Plan</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {revenueByPlan.map((plan) => (
                <div key={plan.plan} className="space-y-2">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium text-gray-900">{plan.plan}</p>
                      <p className="text-sm text-gray-600">{plan.tenants} tenants</p>
                    </div>
                    <p className="font-semibold text-gray-900">
                      {formatCurrency(plan.revenue)}
                    </p>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div
                      className="bg-primary-600 h-2 rounded-full"
                      style={{ width: `${(plan.revenue / 5000) * 100}%` }}
                    />
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Summary Stats */}
        <Card>
          <CardHeader>
            <CardTitle>Summary</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-gray-600">Total Tenants</span>
              <span className="font-semibold text-gray-900">
                {revenueByPlan.reduce((sum, p) => sum + p.tenants, 0)}
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-gray-600">Paying Tenants</span>
              <span className="font-semibold text-gray-900">
                {revenueByPlan.filter(p => p.plan !== "Free").reduce((sum, p) => sum + p.tenants, 0)}
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-gray-600">Free Tier Users</span>
              <span className="font-semibold text-gray-900">
                {revenueByPlan.find(p => p.plan === "Free")?.tenants || 0}
              </span>
            </div>
            <div className="flex items-center justify-between pt-4 border-t">
              <span className="text-gray-600">Total MRR</span>
              <span className="text-xl font-bold text-primary-600">
                {formatCurrency(revenueByPlan.reduce((sum, p) => sum + p.revenue, 0))}
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-gray-600">Conversion Rate</span>
              <span className="font-semibold text-green-600">
                {(
                  (revenueByPlan.filter(p => p.plan !== "Free").reduce((sum, p) => sum + p.tenants, 0) /
                    revenueByPlan.reduce((sum, p) => sum + p.tenants, 0)) *
                  100
                ).toFixed(1)}%
              </span>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Recent Transactions */}
      <Card>
        <CardHeader>
          <CardTitle>Recent Transactions</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Tenant</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Date</TableHead>
                <TableHead>Payment ID</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {transactions.map((transaction) => (
                <TableRow key={transaction.id}>
                  <TableCell>
                    <p className="font-medium text-gray-900">{transaction.tenantName}</p>
                  </TableCell>
                  <TableCell>
                    <p className="font-semibold">{formatCurrency(transaction.amount)}</p>
                  </TableCell>
                  <TableCell>
                    <span className="text-sm capitalize">{transaction.type}</span>
                  </TableCell>
                  <TableCell>
                    <Badge variant={statusColors[transaction.status]}>
                      {transaction.status}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <span className="text-sm text-gray-600">
                      {formatDate(transaction.createdAt)}
                    </span>
                  </TableCell>
                  <TableCell>
                    {transaction.stripePaymentId ? (
                      <code className="text-xs bg-gray-100 px-2 py-1 rounded">
                        {transaction.stripePaymentId}
                      </code>
                    ) : (
                      <span className="text-xs text-gray-400">N/A</span>
                    )}
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
