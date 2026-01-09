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
import { grantLifetimeAccess, grant30DaysAccess } from "@/_lib/api";
import { Crown, Calendar, RefreshCw } from "lucide-react";

interface SubscriptionButtonsProps {
  tenantId: string;
  currentTier: string;
}

type OperationType = "lifetime" | "30-days" | null;

export function SubscriptionButtons({ tenantId, currentTier }: SubscriptionButtonsProps) {
  const router = useRouter();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [operationType, setOperationType] = useState<OperationType>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [result, setResult] = useState<{ success: boolean; message: string } | null>(null);

  const operationConfig = {
    "lifetime": {
      title: "Grant Lifetime Access",
      description: "This will give the tenant permanent lifetime access with unlimited mechanics and 100GB storage. This action cannot be undone.",
      action: grantLifetimeAccess,
      icon: Crown,
      buttonText: "Grant Lifetime",
      loadingText: "Granting...",
      variant: "primary" as const,
    },
    "30-days": {
      title: "Grant 30 Days Access",
      description: "This will add 30 days of team membership to the tenant. If they already have time remaining, the 30 days will be added to their existing subscription.",
      action: grant30DaysAccess,
      icon: Calendar,
      buttonText: "Add 30 Days",
      loadingText: "Granting...",
      variant: "outline" as const,
    },
  };

  const openDialog = (type: OperationType) => {
    setOperationType(type);
    setResult(null);
    setDialogOpen(true);
  };

  const handleOperation = async () => {
    if (!operationType) return;

    setIsLoading(true);
    setResult(null);

    const config = operationConfig[operationType];
    const response = await config.action(tenantId);

    if (response.success && response.data) {
      setResult({ success: true, message: response.data.message });
      // Refresh the page after a short delay
      setTimeout(() => {
        router.refresh();
      }, 1500);
    } else {
      setResult({ success: false, message: response.error || "Operation failed" });
    }

    setIsLoading(false);
  };

  const closeDialog = () => {
    if (!isLoading) {
      setDialogOpen(false);
      setOperationType(null);
      setResult(null);
    }
  };

  const currentConfig = operationType ? operationConfig[operationType] : null;

  return (
    <>
      <div className="flex gap-2">
        <Button
          variant="primary"
          size="sm"
          onClick={() => openDialog("lifetime")}
          title="Grant Lifetime Access"
          disabled={currentTier === "lifetime"}
        >
          <Crown className="h-4 w-4 mr-1" />
          Grant Lifetime
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={() => openDialog("30-days")}
          title="Add 30 Days of Membership"
        >
          <Calendar className="h-4 w-4 mr-1" />
          Add 30 Days
        </Button>
      </div>

      <Dialog open={dialogOpen} onClose={closeDialog}>
        {currentConfig && (
          <>
            <DialogHeader>
              <DialogTitle>{currentConfig.title}</DialogTitle>
              <DialogDescription>{currentConfig.description}</DialogDescription>
            </DialogHeader>

            <DialogContent>
              <p className="text-sm text-dark-700">
                Tenant ID: <strong>{tenantId}</strong>
              </p>
              <p className="text-sm text-dark-700 mt-1">
                Current Tier: <strong className="capitalize">{currentTier}</strong>
              </p>

              {result && (
                <div
                  className={`text-sm mt-4 p-3 rounded-lg ${
                    result.success
                      ? "text-green-600 bg-green-50"
                      : "text-red-600 bg-red-50"
                  }`}
                >
                  {result.message}
                </div>
              )}
            </DialogContent>

            <DialogFooter>
              <Button variant="outline" onClick={closeDialog} disabled={isLoading}>
                {result?.success ? "Close" : "Cancel"}
              </Button>
              {!result?.success && (
                <Button variant={currentConfig.variant} onClick={handleOperation} disabled={isLoading}>
                  {isLoading ? (
                    <>
                      <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                      {currentConfig.loadingText}
                    </>
                  ) : (
                    <>
                      <currentConfig.icon className="h-4 w-4 mr-2" />
                      {currentConfig.buttonText}
                    </>
                  )}
                </Button>
              )}
            </DialogFooter>
          </>
        )}
      </Dialog>
    </>
  );
}
