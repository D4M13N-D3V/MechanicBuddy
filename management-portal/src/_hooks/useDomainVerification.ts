"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import { verifyDomain } from "@/_lib/api";
import type { DomainVerificationResult } from "@/types";

interface UseDomainVerificationOptions {
  tenantId: number;
  domain: string;
  pollInterval?: number; // default 10 seconds
  maxAttempts?: number; // default 30 (5 minutes total)
  onSuccess?: () => void;
}

interface UseDomainVerificationReturn {
  isPolling: boolean;
  attempts: number;
  result: DomainVerificationResult | null;
  error: string | null;
  startPolling: () => void;
  stopPolling: () => void;
  checkOnce: () => Promise<void>;
}

export function useDomainVerification({
  tenantId,
  domain,
  pollInterval = 10000,
  maxAttempts = 30,
  onSuccess,
}: UseDomainVerificationOptions): UseDomainVerificationReturn {
  const [isPolling, setIsPolling] = useState(false);
  const [attempts, setAttempts] = useState(0);
  const [result, setResult] = useState<DomainVerificationResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const onSuccessRef = useRef(onSuccess);
  onSuccessRef.current = onSuccess;

  const checkVerification = useCallback(async () => {
    try {
      const response = await verifyDomain(tenantId, domain);
      if (response.success && response.data) {
        setResult(response.data);
        if (response.data.success) {
          setIsPolling(false);
          onSuccessRef.current?.();
        }
        return response.data.success;
      } else {
        setError(response.error || "Verification failed");
        return false;
      }
    } catch {
      setError("Network error");
      return false;
    }
  }, [tenantId, domain]);

  const startPolling = useCallback(() => {
    setIsPolling(true);
    setAttempts(0);
    setError(null);
    setResult(null);
  }, []);

  const stopPolling = useCallback(() => {
    setIsPolling(false);
  }, []);

  const checkOnce = useCallback(async () => {
    setError(null);
    setAttempts((prev) => prev + 1);
    await checkVerification();
  }, [checkVerification]);

  useEffect(() => {
    if (!isPolling) return;

    let timeoutId: NodeJS.Timeout;
    let isCancelled = false;

    const poll = async () => {
      if (isCancelled) return;

      setAttempts((prev) => {
        const newAttempts = prev + 1;
        if (newAttempts >= maxAttempts) {
          setIsPolling(false);
          setError("Verification timed out. DNS changes can take up to 48 hours to propagate. Please try again later.");
        }
        return newAttempts;
      });

      const success = await checkVerification();

      if (!isCancelled && !success && isPolling) {
        timeoutId = setTimeout(poll, pollInterval);
      }
    };

    // Start immediately
    poll();

    return () => {
      isCancelled = true;
      if (timeoutId) clearTimeout(timeoutId);
    };
  }, [isPolling, pollInterval, maxAttempts, checkVerification]);

  return {
    isPolling,
    attempts,
    result,
    error,
    startPolling,
    stopPolling,
    checkOnce,
  };
}
