import Link from "next/link";
import { Card, CardContent } from "@/_components/ui/Card";
import { Button } from "@/_components/ui/Button";
import { getTenant, getTenantDomains } from "@/_lib/api";
import { getCurrentUser } from "@/_lib/auth";
import { DomainPageClient } from "./DomainPageClient";

export default async function DomainConfigPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const tenantId = parseInt(id, 10);

  const [tenantResponse, domainsResponse, user] = await Promise.all([
    getTenant(id),
    getTenantDomains(tenantId),
    getCurrentUser(),
  ]);

  if (!tenantResponse.success || !tenantResponse.data) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Link href={`/dashboard/tenants/${id}`}>
            <Button variant="ghost" size="sm">
              <svg
                className="h-4 w-4 mr-2"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M10 19l-7-7m0 0l7-7m-7 7h18"
                />
              </svg>
              Back to Tenant
            </Button>
          </Link>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <svg
                className="h-5 w-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
              <p>Unable to load tenant details.</p>
            </div>
            <p className="text-sm text-dark-500 mt-2">
              Error: {tenantResponse.error || "Tenant not found"}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const tenant = tenantResponse.data;

  // Check if user owns this tenant
  const isOwner = user?.email?.toLowerCase() === tenant.ownerEmail?.toLowerCase();
  const isAdmin = user?.role === "admin" || user?.role === "owner";

  if (!isOwner && !isAdmin) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Link href={`/dashboard/tenants/${id}`}>
            <Button variant="ghost" size="sm">
              <svg
                className="h-4 w-4 mr-2"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M10 19l-7-7m0 0l7-7m-7 7h18"
                />
              </svg>
              Back to Tenant
            </Button>
          </Link>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-red-600">
              <svg
                className="h-5 w-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 15v2m0 0v2m0-2h2m-2 0H9m3-10V7a3 3 0 016 0v4M5 12h14a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2z"
                />
              </svg>
              <p>Access Denied</p>
            </div>
            <p className="text-sm text-dark-500 mt-2">
              You don&apos;t have permission to manage custom domains for this tenant.
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Get existing domain verification if any
  const domains = domainsResponse.success && domainsResponse.data ? domainsResponse.data.domains : [];
  const verifiedDomain = domains.find((d) => d.isVerified);
  const pendingDomain = domains.find((d) => !d.isVerified);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href={`/dashboard/tenants/${id}`}>
          <Button variant="ghost" size="sm">
            <svg
              className="h-4 w-4 mr-2"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M10 19l-7-7m0 0l7-7m-7 7h18"
              />
            </svg>
            Back to Tenant
          </Button>
        </Link>
      </div>

      <div>
        <h1 className="text-3xl font-bold text-dark-900">Custom Domain</h1>
        <p className="text-dark-500 mt-1">
          Configure a custom domain for {tenant.companyName}
        </p>
      </div>

      {/* Info banner about default subdomain */}
      <div className="p-4 bg-dark-50 rounded-lg border border-dark-200">
        <div className="flex gap-3">
          <svg
            className="h-5 w-5 text-dark-500 flex-shrink-0 mt-0.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <div>
            <p className="text-sm text-dark-700">
              Your workshop is always accessible at the default subdomain:{" "}
              <a
                href={`https://${tenant.tenantId}.mechanicbuddy.app`}
                target="_blank"
                rel="noopener noreferrer"
                className="font-mono font-medium text-primary-600 hover:text-primary-700"
              >
                {tenant.tenantId}.mechanicbuddy.app
              </a>
            </p>
            <p className="text-sm text-dark-500 mt-1">
              Adding a custom domain gives you a more professional URL for your workshop.
            </p>
          </div>
        </div>
      </div>

      {/* Client component for interactive domain management */}
      <DomainPageClient
        tenantId={tenantId}
        verifiedDomain={verifiedDomain}
        pendingDomain={pendingDomain}
      />
    </div>
  );
}
