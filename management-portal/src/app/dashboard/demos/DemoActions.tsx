"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/_components/ui/Button";
import { approveDemoRequest, rejectDemoRequest } from "@/_lib/api";

interface DemoActionsProps {
  id: string;
  status: string;
}

export function DemoActions({ id, status }: DemoActionsProps) {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState<string | null>(null);

  const handleApprove = async () => {
    setIsLoading("approve");
    try {
      const result = await approveDemoRequest(id);
      if (result.success) {
        router.refresh();
      } else {
        alert(result.error || "Failed to approve demo request");
      }
    } catch (error) {
      alert("An error occurred");
    } finally {
      setIsLoading(null);
    }
  };

  const handleReject = async () => {
    const reason = prompt("Enter rejection reason:");
    if (!reason) return;

    setIsLoading("reject");
    try {
      const result = await rejectDemoRequest(id, reason);
      if (result.success) {
        router.refresh();
      } else {
        alert(result.error || "Failed to reject demo request");
      }
    } catch (error) {
      alert("An error occurred");
    } finally {
      setIsLoading(null);
    }
  };

  if (status === "pending") {
    return (
      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          onClick={handleApprove}
          disabled={isLoading !== null}
        >
          {isLoading === "approve" ? "Approving..." : "Approve"}
        </Button>
        <Button
          variant="ghost"
          size="sm"
          onClick={handleReject}
          disabled={isLoading !== null}
        >
          {isLoading === "reject" ? "Rejecting..." : "Reject"}
        </Button>
      </div>
    );
  }

  if (status === "approved") {
    return (
      <Button variant="outline" size="sm" disabled>
        Active Demo
      </Button>
    );
  }

  return null;
}
