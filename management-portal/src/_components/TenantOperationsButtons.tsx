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
import { restartTenantApi, restartTenantFrontend, runTenantMigration } from "@/_lib/api";
import { RefreshCw, Database, Server, Globe } from "lucide-react";

interface TenantOperationsButtonsProps {
  tenantId: string;
}

type OperationType = "restart-api" | "restart-frontend" | "run-migration" | null;

export function TenantOperationsButtons({ tenantId }: TenantOperationsButtonsProps) {
  const router = useRouter();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [operationType, setOperationType] = useState<OperationType>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [result, setResult] = useState<{ success: boolean; message: string } | null>(null);

  const operationConfig = {
    "restart-api": {
      title: "Restart API",
      description: "This will restart the API deployment for this tenant. The API will be briefly unavailable during the restart.",
      action: restartTenantApi,
      icon: Server,
      buttonText: "Restart API",
      loadingText: "Restarting...",
    },
    "restart-frontend": {
      title: "Restart Frontend",
      description: "This will restart the frontend deployment for this tenant. The web app will be briefly unavailable during the restart.",
      action: restartTenantFrontend,
      icon: Globe,
      buttonText: "Restart Frontend",
      loadingText: "Restarting...",
    },
    "run-migration": {
      title: "Run Migration",
      description: "This will run database migrations for this tenant. This may take a few minutes to complete.",
      action: runTenantMigration,
      icon: Database,
      buttonText: "Run Migration",
      loadingText: "Starting migration...",
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
          variant="outline"
          size="sm"
          onClick={() => openDialog("restart-api")}
          title="Restart API Deployment"
        >
          <Server className="h-4 w-4 mr-1" />
          Restart API
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={() => openDialog("restart-frontend")}
          title="Restart Frontend Deployment"
        >
          <Globe className="h-4 w-4 mr-1" />
          Restart Frontend
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={() => openDialog("run-migration")}
          title="Run Database Migration"
        >
          <Database className="h-4 w-4 mr-1" />
          Run Migration
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
                <Button onClick={handleOperation} disabled={isLoading}>
                  {isLoading ? (
                    <>
                      <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                      {currentConfig.loadingText}
                    </>
                  ) : (
                    currentConfig.buttonText
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
