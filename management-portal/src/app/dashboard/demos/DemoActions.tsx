"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { updateDemoRequestStatus } from "@/_lib/api";
import type { DemoRequestStatus } from "@/types";

const statuses: { value: DemoRequestStatus; label: string }[] = [
  { value: "new", label: "New" },
  { value: "pending_trial", label: "Pending Trial" },
  { value: "complete", label: "Complete" },
  { value: "cancelled", label: "Cancelled" },
];

interface DemoActionsProps {
  id: string;
  status: string;
}

export function DemoActions({ id, status }: DemoActionsProps) {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const [currentStatus, setCurrentStatus] = useState(status);

  const handleStatusChange = async (newStatus: string) => {
    if (newStatus === currentStatus) return;

    setIsLoading(true);
    try {
      const result = await updateDemoRequestStatus(id, newStatus);
      if (result.success) {
        setCurrentStatus(newStatus);
        router.refresh();
      } else {
        alert(result.error || "Failed to update status");
      }
    } catch (error) {
      alert("An error occurred");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <select
      value={currentStatus}
      onChange={(e) => handleStatusChange(e.target.value)}
      disabled={isLoading}
      className="block w-full rounded-md border-gray-300 py-1.5 px-2 text-sm shadow-sm focus:border-primary-500 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
    >
      {statuses.map((s) => (
        <option key={s.value} value={s.value}>
          {s.label}
        </option>
      ))}
    </select>
  );
}
