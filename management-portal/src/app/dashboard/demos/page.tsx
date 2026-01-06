import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/_components/ui/Table";
import { Badge } from "@/_components/ui/Badge";
import { Button } from "@/_components/ui/Button";
import { formatDate, formatRelativeTime } from "@/_lib/utils";

// Mock data - replace with actual API calls
const demoRequests = [
  {
    id: "1",
    email: "john@garageplus.com",
    companyName: "Garage Plus",
    message: "Looking for a solution to manage our 3 mechanics and inventory tracking.",
    status: "pending",
    createdAt: "2026-01-06T08:30:00Z",
  },
  {
    id: "2",
    email: "sarah@speedyauto.com",
    companyName: "Speedy Auto Repair",
    message: "Interested in the premium plan. Need multi-location support.",
    status: "contacted",
    createdAt: "2026-01-05T14:20:00Z",
    contactedAt: "2026-01-05T16:45:00Z",
  },
  {
    id: "3",
    email: "mike@cityservice.com",
    companyName: "City Service Center",
    message: "Want to see how the invoicing works. Currently using spreadsheets.",
    status: "pending",
    createdAt: "2026-01-05T11:00:00Z",
  },
  {
    id: "4",
    email: "lisa@elitemotors.com",
    companyName: "Elite Motors",
    message: "Need enterprise solution for 20+ mechanics across 3 locations.",
    status: "converted",
    createdAt: "2026-01-04T09:15:00Z",
    contactedAt: "2026-01-04T10:30:00Z",
  },
  {
    id: "5",
    email: "tom@quickfix.com",
    companyName: "Quick Fix Auto",
    message: "Just starting out, checking options.",
    status: "declined",
    createdAt: "2026-01-03T16:45:00Z",
    contactedAt: "2026-01-04T08:00:00Z",
  },
];

const statusColors: Record<string, "default" | "success" | "warning" | "danger" | "info"> = {
  pending: "warning",
  contacted: "info",
  converted: "success",
  declined: "default",
};

export default function DemosPage() {
  const pendingCount = demoRequests.filter(d => d.status === "pending").length;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Demo Requests</h1>
          <p className="text-gray-600 mt-1">
            {pendingCount} pending request{pendingCount !== 1 ? "s" : ""} waiting for contact
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>All Demo Requests ({demoRequests.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Company</TableHead>
                <TableHead>Email</TableHead>
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
                    <p className="font-medium text-gray-900">{request.companyName}</p>
                  </TableCell>
                  <TableCell>
                    <a
                      href={`mailto:${request.email}`}
                      className="text-primary-600 hover:text-primary-700 text-sm"
                    >
                      {request.email}
                    </a>
                  </TableCell>
                  <TableCell>
                    <p className="text-sm text-gray-600 max-w-md truncate">
                      {request.message}
                    </p>
                  </TableCell>
                  <TableCell>
                    <Badge variant={statusColors[request.status]}>
                      {request.status}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <span className="text-sm text-gray-600">
                      {formatRelativeTime(request.createdAt)}
                    </span>
                  </TableCell>
                  <TableCell>
                    {request.status === "pending" && (
                      <div className="flex gap-2">
                        <Button variant="outline" size="sm">Contact</Button>
                        <Button variant="ghost" size="sm">Decline</Button>
                      </div>
                    )}
                    {request.status === "contacted" && (
                      <Button variant="outline" size="sm">Convert</Button>
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
