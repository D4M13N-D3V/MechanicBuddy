"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/_components/ui/Button";
import {
  Dialog,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogContent,
  DialogFooter,
} from "@/_components/ui/Dialog";
import { suspendTenant } from "@/_lib/api";

interface SuspendTenantButtonProps {
  tenantId: string;
  companyName: string;
  currentStatus: string;
}

export function SuspendTenantButton({ tenantId, companyName, currentStatus }: SuspendTenantButtonProps) {
  const router = useRouter();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [isSuspending, setIsSuspending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [reason, setReason] = useState("");

  const handleSuspend = async () => {
    setIsSuspending(true);
    setError(null);

    const response = await suspendTenant(tenantId, reason || "Suspended by admin");

    if (response.success) {
      setDialogOpen(false);
      router.refresh();
    } else {
      setError(response.error || "Failed to suspend tenant");
      setIsSuspending(false);
    }
  };

  // Don't show button if already suspended
  if (currentStatus === "suspended") {
    return null;
  }

  return (
    <>
      <Button variant="outline" onClick={() => setDialogOpen(true)}>
        Suspend
      </Button>

      <Dialog open={dialogOpen} onClose={() => !isSuspending && setDialogOpen(false)}>
        <DialogHeader>
          <DialogTitle>Suspend Tenant</DialogTitle>
          <DialogDescription>
            Suspend this tenant&apos;s access to the platform.
          </DialogDescription>
        </DialogHeader>

        <DialogContent>
          <p className="text-sm text-dark-700">
            You are about to suspend <strong>{companyName}</strong> ({tenantId}).
          </p>
          <p className="text-sm text-dark-500 mt-2">
            This will:
          </p>
          <ul className="text-sm text-dark-500 mt-1 list-disc list-inside space-y-1">
            <li>Set the tenant status to &quot;suspended&quot;</li>
            <li>Downgrade the tenant to solo tier</li>
            <li>Disable all non-admin users</li>
            <li>Delete the Kubernetes namespace</li>
            <li>Remove DNS records</li>
          </ul>
          <div className="mt-4">
            <label htmlFor="reason" className="block text-sm font-medium text-dark-700 mb-1">
              Reason (optional)
            </label>
            <input
              id="reason"
              type="text"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Enter suspension reason..."
              className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>
          {error && (
            <p className="text-sm text-red-600 mt-4 p-3 bg-red-50 rounded-lg">
              {error}
            </p>
          )}
        </DialogContent>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => setDialogOpen(false)}
            disabled={isSuspending}
          >
            Cancel
          </Button>
          <Button
            variant="danger"
            onClick={handleSuspend}
            disabled={isSuspending}
          >
            {isSuspending ? "Suspending..." : "Suspend Tenant"}
          </Button>
        </DialogFooter>
      </Dialog>
    </>
  );
}
