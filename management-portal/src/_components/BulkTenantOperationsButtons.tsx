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
import { restartAllTenants, migrateAllTenants, type BulkTenantOperationResponse } from "@/_lib/api";
import { RefreshCw, Database, Server } from "lucide-react";

type OperationType = "restart-all" | "migrate-all" | null;

export function BulkTenantOperationsButtons() {
  const router = useRouter();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [operationType, setOperationType] = useState<OperationType>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [result, setResult] = useState<{ success: boolean; data?: BulkTenantOperationResponse; error?: string } | null>(null);

  const operationConfig = {
    "restart-all": {
      title: "Restart All Tenants",
      description: "This will restart ALL active tenant API and frontend deployments. All tenants will experience brief downtime during the restart.",
      action: restartAllTenants,
      icon: Server,
      buttonText: "Restart All",
      loadingText: "Restarting...",
    },
    "migrate-all": {
      title: "Migrate All Tenants",
      description: "This will run database migrations for ALL active tenants. This may take several minutes to complete.",
      action: migrateAllTenants,
      icon: Database,
      buttonText: "Migrate All",
      loadingText: "Starting migrations...",
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
    const response = await config.action();

    if (response.success && response.data) {
      setResult({ success: true, data: response.data });
      setTimeout(() => {
        router.refresh();
      }, 1500);
    } else {
      setResult({ success: false, error: response.error || "Operation failed" });
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
          onClick={() => openDialog("restart-all")}
          title="Restart All Tenant Deployments"
        >
          <Server className="h-4 w-4 mr-1" />
          Restart All
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={() => openDialog("migrate-all")}
          title="Run Migrations for All Tenants"
        >
          <Database className="h-4 w-4 mr-1" />
          Migrate All
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
              <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 text-amber-800 text-sm">
                <strong>Warning:</strong> This action affects all active tenants and cannot be undone.
              </div>

              {result && (
                <div className="mt-4">
                  {result.success && result.data ? (
                    <div className="space-y-3">
                      <div className="text-sm p-3 rounded-lg bg-green-50 text-green-700">
                        {result.data.message}
                      </div>
                      <div className="text-sm text-dark-600">
                        <p>Total tenants: <strong>{result.data.totalTenants}</strong></p>
                        <p>Successful: <strong className="text-green-600">{result.data.successCount}</strong></p>
                        {result.data.errorCount > 0 && (
                          <p>Failed: <strong className="text-red-600">{result.data.errorCount}</strong></p>
                        )}
                      </div>
                      {result.data.errors.length > 0 && (
                        <div className="mt-2 p-2 bg-red-50 rounded text-sm text-red-700">
                          <p className="font-medium mb-1">Errors:</p>
                          <ul className="list-disc list-inside">
                            {result.data.errors.map((err, idx) => (
                              <li key={idx}>{err.tenantId}: {err.error}</li>
                            ))}
                          </ul>
                        </div>
                      )}
                    </div>
                  ) : (
                    <div className="text-sm p-3 rounded-lg bg-red-50 text-red-600">
                      {result.error}
                    </div>
                  )}
                </div>
              )}
            </DialogContent>

            <DialogFooter>
              <Button variant="outline" onClick={closeDialog} disabled={isLoading}>
                {result?.success ? "Close" : "Cancel"}
              </Button>
              {!result?.success && (
                <Button onClick={handleOperation} disabled={isLoading} variant="danger">
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
