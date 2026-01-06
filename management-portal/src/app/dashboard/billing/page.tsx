import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { StatCard } from "@/_components/dashboard/StatCard";
import { DollarSign, TrendingUp, CreditCard, Users, AlertCircle } from "lucide-react";
import { formatCurrency, formatDate } from "@/_lib/utils";
import { getBillingStats, getBillingTransactions } from "@/_lib/api";

const statusColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  completed: "success",
  pending: "warning",
  failed: "danger",
  refunded: "default",
};

export default async function BillingPage() {
  const [statsResponse, transactionsResponse] = await Promise.all([
    getBillingStats(),
    getBillingTransactions(1, 20),
  ]);

  const hasError = !statsResponse.success || !transactionsResponse.success;

  if (hasError) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Billing & Revenue</h1>
          <p className="text-gray-600 mt-1">Monitor subscription revenue and transactions</p>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <p>Unable to load billing data. Please check that the Management API is running.</p>
            </div>
            <p className="text-sm text-gray-500 mt-2">
              Error: {statsResponse.error || transactionsResponse.error || "Connection failed"}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const stats = statsResponse.data!;
  const transactions = transactionsResponse.data?.items || [];

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
          trend={stats.totalRevenue > 0 ? { value: 18, isPositive: true } : undefined}
        />
        <StatCard
          title="MRR"
          value={formatCurrency(stats.monthlyRecurringRevenue)}
          icon={TrendingUp}
          trend={stats.monthlyRecurringRevenue > 0 ? { value: 15, isPositive: true } : undefined}
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

      {/* Recent Transactions */}
      <Card>
        <CardHeader>
          <CardTitle>Recent Transactions</CardTitle>
        </CardHeader>
        <CardContent>
          {transactions.length > 0 ? (
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
                      <Badge variant={statusColors[transaction.status] || "default"}>
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
          ) : (
            <div className="text-center py-8 text-gray-500">
              <p>No transactions yet</p>
              <p className="text-sm mt-2">Billing transactions will appear here once tenants subscribe.</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
