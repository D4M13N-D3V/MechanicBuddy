import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { Button } from "@/_components/ui/Button";
import { formatDate, formatRelativeTime } from "@/_lib/utils";
import { getMyTenants } from "@/_lib/api";
import { getCurrentUser as getCurrentUserAuth } from "@/_lib/auth";
import Link from "next/link";
import { AlertCircle, User, Building2 } from "lucide-react";
import { RequestTenantButton } from "./RequestTenantButton";

const statusColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  active: "success",
  trial: "warning",
  suspended: "danger",
  deleted: "default",
  provisioning: "info",
};

const ADMIN_ROLES = ["super_admin", "admin", "support"];

export default async function AccountPage() {
  // Get user from auth session
  const user = await getCurrentUserAuth();
  const isAdmin = user ? ADMIN_ROLES.includes(user.role) : false;

  if (!user) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-dark-900">Account</h1>
            <p className="text-dark-500 mt-1">Manage your profile and tenants</p>
          </div>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <p>Unable to load user information. Please log in again.</p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Get user's tenants
  const tenantsResponse = await getMyTenants();
  const tenants = tenantsResponse.success && tenantsResponse.data ? tenantsResponse.data : [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-dark-900">Account</h1>
          <p className="text-dark-500 mt-1">Manage your profile and tenants</p>
        </div>
      </div>

      {/* User Profile Section */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary-100">
              <User className="h-6 w-6 text-primary-600" />
            </div>
            <div>
              <CardTitle>User Profile</CardTitle>
              <p className="text-sm text-dark-500 mt-1">Your account information</p>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="text-sm font-medium text-dark-700">Name</label>
                <p className="text-base text-dark-900 mt-1">{user.name}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-dark-700">Email</label>
                <p className="text-base text-dark-900 mt-1">{user.email}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-dark-700">Role</label>
                <div className="mt-1">
                  <Badge variant={user.role === 'admin' || user.role === 'owner' ? 'success' : 'default'}>
                    {user.role === 'admin' ? 'Admin' : user.role === 'owner' ? 'Owner' : user.role === 'support' ? 'Support' : 'User'}
                  </Badge>
                </div>
              </div>
              <div>
                <label className="text-sm font-medium text-dark-700">Member Since</label>
                <p className="text-base text-dark-900 mt-1">{formatDate(user.createdAt)}</p>
              </div>
            </div>
            <div className="pt-4 border-t border-dark-200">
              <Button variant="outline" size="sm">
                Edit Profile
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* My Tenants Section */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-100">
                <Building2 className="h-5 w-5 text-primary-600" />
              </div>
              <div>
                <CardTitle>My Tenants ({tenants.length})</CardTitle>
                <p className="text-sm text-dark-500 mt-1">Workshops you own or manage</p>
              </div>
            </div>
            {isAdmin && <RequestTenantButton />}
          </div>
        </CardHeader>
        <CardContent>
          {!tenantsResponse.success ? (
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <div>
                <p>Unable to load your tenants.</p>
                <p className="text-sm text-dark-500 mt-1">
                  Error: {tenantsResponse.error || "Connection failed"}
                </p>
              </div>
            </div>
          ) : tenants.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Company</TableHead>
                  <TableHead>Subdomain</TableHead>
                  <TableHead>Tier</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Last Active</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {tenants.map((tenant) => (
                  <TableRow key={tenant.id}>
                    <TableCell>
                      <div>
                        <p className="font-semibold text-dark-900">{tenant.companyName}</p>
                        <p className="text-xs text-dark-500">Created {formatDate(tenant.createdAt)}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <a
                        href={tenant.apiUrl || `https://${tenant.tenantId}.mechanicbuddy.app`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-sm bg-dark-100 text-dark-700 px-2 py-1 rounded font-mono hover:bg-dark-200 transition-colors"
                      >
                        {tenant.tenantId}.mechanicbuddy.app
                      </a>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm font-medium text-dark-700 capitalize">
                        {tenant.tier}
                      </span>
                    </TableCell>
                    <TableCell>
                      <Badge variant={statusColors[tenant.status] || "default"}>
                        {tenant.status}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm text-dark-500">
                        {tenant.lastActivityAt ? formatRelativeTime(tenant.lastActivityAt) : "Never"}
                      </span>
                    </TableCell>
                    <TableCell>
                      <Link href={`/dashboard/tenants/${tenant.id}`}>
                        <Button variant="ghost" size="sm">View Details</Button>
                      </Link>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-12">
              <div className="flex justify-center mb-4">
                <div className="flex h-16 w-16 items-center justify-center rounded-full bg-dark-100">
                  <Building2 className="h-8 w-8 text-dark-400" />
                </div>
              </div>
              <h3 className="text-lg font-semibold text-dark-900 mb-2">No tenants yet</h3>
              <p className="text-dark-500 mb-6">
                You don&apos;t have any workshop tenants. Request one to get started.
              </p>
              {isAdmin && <RequestTenantButton />}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
