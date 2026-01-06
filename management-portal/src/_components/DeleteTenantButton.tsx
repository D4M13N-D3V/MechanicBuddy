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
import { deleteTenant } from "@/_lib/api";

interface DeleteTenantButtonProps {
  tenantId: string;
  companyName: string;
}

export function DeleteTenantButton({ tenantId, companyName }: DeleteTenantButtonProps) {
  const router = useRouter();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [warnings, setWarnings] = useState<string[]>([]);

  const handleDelete = async () => {
    setIsDeleting(true);
    setError(null);
    setWarnings([]);

    const response = await deleteTenant(tenantId);

    if (response.success && response.data) {
      // Show warnings if any before redirecting
      if (response.data.warnings && response.data.warnings.length > 0) {
        setWarnings(response.data.warnings);
        // Wait a moment to show warnings then redirect
        setTimeout(() => {
          setDialogOpen(false);
          router.push("/dashboard/tenants");
          router.refresh();
        }, 2000);
      } else {
        setDialogOpen(false);
        router.push("/dashboard/tenants");
        router.refresh();
      }
    } else {
      setError(response.error || "Failed to delete tenant");
      setIsDeleting(false);
    }
  };

  return (
    <>
      <Button variant="danger" onClick={() => setDialogOpen(true)}>
        Delete
      </Button>

      <Dialog open={dialogOpen} onClose={() => !isDeleting && setDialogOpen(false)}>
        <DialogHeader>
          <DialogTitle>Delete Tenant</DialogTitle>
          <DialogDescription>
            Are you sure you want to delete this tenant? This action cannot be undone.
          </DialogDescription>
        </DialogHeader>

        <DialogContent>
          <p className="text-sm text-dark-700">
            You are about to permanently delete <strong>{companyName}</strong> ({tenantId}).
          </p>
          <p className="text-sm text-dark-500 mt-2">
            This will:
          </p>
          <ul className="text-sm text-dark-500 mt-1 list-disc list-inside space-y-1">
            <li>Delete all tenant data from the database</li>
            <li>Remove the Kubernetes namespace and all resources</li>
            <li>Delete DNS records from Cloudflare</li>
          </ul>
          {error && (
            <p className="text-sm text-red-600 mt-4 p-3 bg-red-50 rounded-lg">
              {error}
            </p>
          )}
          {warnings.length > 0 && (
            <div className="text-sm text-amber-600 mt-4 p-3 bg-amber-50 rounded-lg">
              <p className="font-medium">Deleted with warnings:</p>
              <ul className="list-disc list-inside mt-1">
                {warnings.map((warning, index) => (
                  <li key={index}>{warning}</li>
                ))}
              </ul>
              <p className="mt-2 text-xs">Redirecting...</p>
            </div>
          )}
        </DialogContent>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => setDialogOpen(false)}
            disabled={isDeleting}
          >
            Cancel
          </Button>
          <Button
            variant="danger"
            onClick={handleDelete}
            disabled={isDeleting}
          >
            {isDeleting ? "Deleting..." : "Delete Tenant"}
          </Button>
        </DialogFooter>
      </Dialog>
    </>
  );
}
