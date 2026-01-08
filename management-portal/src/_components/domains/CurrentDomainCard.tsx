"use client";

import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/_components/ui/Card";
import { Button } from "@/_components/ui/Button";
import { Badge } from "@/_components/ui/Badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from "@/_components/ui/Dialog";
import { removeDomain } from "@/_lib/api";
import type { DomainVerification } from "@/types";

interface CurrentDomainCardProps {
  domain: DomainVerification;
  tenantId: number;
  onRemoved: () => void;
}

export function CurrentDomainCard({ domain, tenantId, onRemoved }: CurrentDomainCardProps) {
  const [isRemoving, setIsRemoving] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleRemove = async () => {
    setIsRemoving(true);
    setError(null);

    try {
      const result = await removeDomain(tenantId, domain.domain);
      if (result.success) {
        setShowConfirm(false);
        onRemoved();
      } else {
        setError(result.error || "Failed to remove domain");
      }
    } catch {
      setError("An error occurred while removing the domain");
    } finally {
      setIsRemoving(false);
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "Unknown";
    return new Date(dateString).toLocaleDateString(undefined, {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-100">
                <svg
                  className="h-5 w-5 text-primary-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9"
                  />
                </svg>
              </div>
              <div>
                <CardTitle>Current Custom Domain</CardTitle>
                <CardDescription>Your workshop is accessible at this domain</CardDescription>
              </div>
            </div>
            <Badge variant={domain.isVerified ? "success" : "warning"}>
              {domain.isVerified ? "Active" : "Pending Verification"}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-between p-4 bg-dark-50 rounded-lg">
            <div>
              <p className="font-mono text-lg font-semibold text-dark-900">
                {domain.domain}
              </p>
              {domain.isVerified && domain.verifiedAt && (
                <p className="text-sm text-dark-500 mt-1">
                  Verified on {formatDate(domain.verifiedAt)}
                </p>
              )}
            </div>
            <div className="flex gap-2">
              {domain.isVerified && (
                <a
                  href={`https://${domain.domain}`}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <Button variant="outline" size="sm">
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
                        d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
                      />
                    </svg>
                    Visit
                  </Button>
                </a>
              )}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowConfirm(true)}
                className="text-dark-500 hover:text-red-600"
              >
                <svg
                  className="h-4 w-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                  />
                </svg>
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Remove confirmation dialog */}
      <Dialog open={showConfirm} onClose={() => setShowConfirm(false)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Remove Custom Domain</DialogTitle>
            <DialogDescription>
              Are you sure you want to remove <span className="font-mono font-semibold">{domain.domain}</span>?
            </DialogDescription>
          </DialogHeader>
          <div className="py-4">
            <p className="text-sm text-dark-600">
              Your workshop will no longer be accessible at this domain. You can add it again later if needed.
            </p>
            {error && (
              <p className="mt-3 text-sm text-red-600">{error}</p>
            )}
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setShowConfirm(false)}
              disabled={isRemoving}
            >
              Cancel
            </Button>
            <Button
              variant="danger"
              onClick={handleRemove}
              isLoading={isRemoving}
            >
              Remove Domain
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
