import { redirect } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { formatRelativeTime } from "@/_lib/utils";
import { getDemoRequests } from "@/_lib/api";
import { getCurrentUser } from "@/_lib/auth";
import { AlertCircle } from "lucide-react";
import { DemoActions } from "./DemoActions";
import type { DemoRequestStatus } from "@/types";

const ADMIN_ROLES = ["admin", "support"];

const statusColors: Record<DemoRequestStatus, "default" | "success" | "warning" | "danger" | "info"> = {
  new: "info",
  pending_trial: "warning",
  complete: "success",
  cancelled: "default",
};

const statusLabels: Record<DemoRequestStatus, string> = {
  new: "New",
  pending_trial: "Pending Trial",
  complete: "Complete",
  cancelled: "Cancelled",
};

export default async function DemosPage() {
  // Check if user is admin
  const user = await getCurrentUser();
  if (!user || !ADMIN_ROLES.includes(user.role)) {
    redirect("/dashboard/account");
  }

  const response = await getDemoRequests(1, 50);

  if (!response.success || !response.data) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-dark-900">Demo Requests</h1>
            <p className="text-dark-500 mt-1">Manage demo requests</p>
          </div>
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3 text-amber-600">
              <AlertCircle className="h-5 w-5" />
              <p>Unable to load demo requests. Please check that the Management API is running.</p>
            </div>
            <p className="text-sm text-dark-500 mt-2">
              Error: {response.error || "Connection failed"}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const allRequests = response.data.items;
  // Filter out completed and cancelled requests
  const demoRequests = allRequests.filter(d => d.status !== "complete" && d.status !== "cancelled");
  const newCount = demoRequests.filter(d => d.status === "new").length;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-dark-900">Demo Requests</h1>
          <p className="text-dark-500 mt-1">
            {newCount} new request{newCount !== 1 ? "s" : ""} waiting for review
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Active Demo Requests ({demoRequests.length})</CardTitle>
        </CardHeader>
        <CardContent>
          {demoRequests.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Company</TableHead>
                  <TableHead>Contact</TableHead>
                  <TableHead>Message</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Submitted</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {demoRequests.map((request) => (
                  <TableRow key={request.id}>
                    <TableCell>
                      <p className="font-semibold text-dark-900">{request.companyName || "N/A"}</p>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        <a
                          href={`mailto:${request.email}`}
                          className="text-primary-600 hover:text-primary-700 text-sm font-medium block"
                        >
                          {request.email}
                        </a>
                        {request.phoneNumber && (
                          <a
                            href={`tel:${request.phoneNumber}`}
                            className="text-dark-500 hover:text-dark-700 text-sm block"
                          >
                            {request.phoneNumber}
                          </a>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <p className="text-sm text-dark-500 max-w-md" title={request.message || undefined}>
                        {request.message ? (request.message.length > 100 ? request.message.slice(0, 100) + "..." : request.message) : "No message"}
                      </p>
                    </TableCell>
                    <TableCell>
                      <Badge variant={statusColors[request.status as DemoRequestStatus] || "default"}>
                        {statusLabels[request.status as DemoRequestStatus] || request.status}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm text-dark-500">
                        {formatRelativeTime(request.createdAt)}
                      </span>
                    </TableCell>
                    <TableCell>
                      <DemoActions id={String(request.id)} status={request.status} />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-dark-500">
              <p>No demo requests yet</p>
              <p className="text-sm mt-2">Demo requests will appear here once visitors submit the form.</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
