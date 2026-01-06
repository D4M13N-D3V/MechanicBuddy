import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { StatCard } from "@/_components/dashboard/StatCard";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { Building2, Users, DollarSign, MessageSquare, TrendingUp } from "lucide-react";
import { formatCurrency, formatDate } from "@/_lib/utils";

// Mock data - replace with actual API calls
const stats = {
  totalTenants: 247,
  activeTenants: 189,
  monthlyRecurringRevenue: 4780,
  pendingDemos: 12,
};

const recentTenants = [
  { id: "1", name: "Auto Express LLC", plan: "standard", status: "active", joinedAt: "2026-01-05" },
  { id: "2", name: "Quick Fix Garage", plan: "premium", status: "trial", joinedAt: "2026-01-04" },
  { id: "3", name: "City Motors", plan: "standard", status: "active", joinedAt: "2026-01-03" },
  { id: "4", name: "Elite Auto Service", plan: "enterprise", status: "active", joinedAt: "2026-01-02" },
  { id: "5", name: "Speedy Repairs", plan: "free", status: "active", joinedAt: "2026-01-01" },
];

const revenueData = [
  { month: "Aug", revenue: 3200 },
  { month: "Sep", revenue: 3600 },
  { month: "Oct", revenue: 4100 },
  { month: "Nov", revenue: 4400 },
  { month: "Dec", revenue: 4780 },
];

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Dashboard Overview</h1>
        <p className="text-gray-600 mt-1">Monitor your SaaS platform performance</p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title="Total Tenants"
          value={stats.totalTenants}
          icon={Building2}
          trend={{ value: 12, isPositive: true }}
        />
        <StatCard
          title="Active Tenants"
          value={stats.activeTenants}
          icon={Users}
          trend={{ value: 8, isPositive: true }}
        />
        <StatCard
          title="MRR"
          value={formatCurrency(stats.monthlyRecurringRevenue)}
          icon={DollarSign}
          trend={{ value: 15, isPositive: true }}
        />
        <StatCard
          title="Pending Demos"
          value={stats.pendingDemos}
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
            <div className="space-y-4">
              {revenueData.map((data) => (
                <div key={data.month} className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700">{data.month}</span>
                  <div className="flex items-center gap-4 flex-1 ml-4">
                    <div className="flex-1 bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-primary-600 h-2 rounded-full"
                        style={{ width: `${(data.revenue / 5000) * 100}%` }}
                      />
                    </div>
                    <span className="text-sm font-semibold text-gray-900 w-20 text-right">
                      {formatCurrency(data.revenue)}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Recent Tenants */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Tenants</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Company</TableHead>
                  <TableHead>Plan</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {recentTenants.map((tenant) => (
                  <TableRow key={tenant.id}>
                    <TableCell>
                      <div>
                        <p className="font-medium text-gray-900">{tenant.name}</p>
                        <p className="text-xs text-gray-500">{formatDate(tenant.joinedAt)}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm capitalize">{tenant.plan}</span>
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
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
