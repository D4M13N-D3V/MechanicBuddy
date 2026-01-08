"use client";

import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from "@/_components/ui/Card";
import { Button } from "@/_components/ui/Button";
import { Input } from "@/_components/ui/Input";
import { CopyButton } from "@/_components/ui/CopyButton";
import { DnsProviderInstructions } from "./DnsProviderInstructions";
import { VerificationStatus } from "./VerificationStatus";
import { useDomainVerification } from "@/_hooks/useDomainVerification";
import { initiateDomainVerification } from "@/_lib/api";
import type { DomainVerification } from "@/types";

interface DomainWizardProps {
  tenantId: number;
  existingVerification?: DomainVerification;
  onComplete?: () => void;
}

type WizardStep = "enter" | "verify" | "success";

export function DomainWizard({ tenantId, existingVerification, onComplete }: DomainWizardProps) {
  const [step, setStep] = useState<WizardStep>(
    existingVerification && !existingVerification.isVerified ? "verify" : "enter"
  );
  const [domain, setDomain] = useState(existingVerification?.domain || "");
  const [verification, setVerification] = useState<DomainVerification | null>(
    existingVerification || null
  );
  const [isInitiating, setIsInitiating] = useState(false);
  const [initiateError, setInitiateError] = useState<string | null>(null);

  const {
    isPolling,
    attempts,
    result,
    error: verifyError,
    startPolling,
    stopPolling,
    checkOnce,
  } = useDomainVerification({
    tenantId,
    domain: verification?.domain || "",
    onSuccess: () => {
      setStep("success");
      onComplete?.();
    },
  });

  const handleInitiate = async () => {
    setIsInitiating(true);
    setInitiateError(null);

    try {
      const response = await initiateDomainVerification(tenantId, domain);
      if (response.success && response.data) {
        setVerification(response.data);
        setStep("verify");
      } else {
        setInitiateError(response.error || "Failed to initiate domain verification");
      }
    } catch {
      setInitiateError("An error occurred. Please try again.");
    } finally {
      setIsInitiating(false);
    }
  };

  const validateDomain = (value: string): string | null => {
    const cleaned = value.trim().toLowerCase()
      .replace(/^https?:\/\//, "")
      .replace(/\/$/, "");

    if (!cleaned) return "Domain is required";

    const domainRegex = /^(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,}$/;
    if (!domainRegex.test(cleaned)) {
      return "Invalid domain format. Example: workshop.example.com";
    }

    if (cleaned.endsWith(".mechanicbuddy.app")) {
      return "Cannot use mechanicbuddy.app subdomains";
    }

    return null;
  };

  const domainError = domain ? validateDomain(domain) : null;
  const isValidDomain = domain && !domainError;

  // Step 1: Enter Domain
  if (step === "enter") {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Step 1: Enter Your Domain</CardTitle>
          <CardDescription>
            Enter the custom domain you want to use for your workshop.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Input
            label="Custom Domain"
            placeholder="workshop.yourdomain.com"
            value={domain}
            onChange={(e) => setDomain(e.target.value)}
            error={domainError || undefined}
            helperText="Example: shop.example.com or myworkshop.com"
          />
          <div className="p-4 bg-amber-50 rounded-lg border border-amber-200">
            <div className="flex gap-3">
              <svg
                className="h-5 w-5 text-amber-600 flex-shrink-0 mt-0.5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <p className="text-sm text-amber-800">
                Make sure you own this domain and have access to its DNS settings.
                You&apos;ll need to add a TXT record to verify ownership.
              </p>
            </div>
          </div>
          {initiateError && (
            <div className="p-4 bg-red-50 rounded-lg border border-red-200">
              <p className="text-sm text-red-800">{initiateError}</p>
            </div>
          )}
        </CardContent>
        <CardFooter>
          <Button
            onClick={handleInitiate}
            isLoading={isInitiating}
            disabled={!isValidDomain}
          >
            Continue
          </Button>
        </CardFooter>
      </Card>
    );
  }

  // Step 2: DNS Verification
  if (step === "verify" && verification) {
    const host = verification.instructions?.host || `_mechanicbuddy-verify.${verification.domain}`;
    const token = verification.verificationToken;

    return (
      <Card>
        <CardHeader>
          <CardTitle>Step 2: Add DNS Record</CardTitle>
          <CardDescription>
            Add the following TXT record to your domain&apos;s DNS settings.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* DNS Record Table */}
          <div className="border border-dark-200 rounded-lg overflow-hidden">
            <table className="w-full text-sm">
              <tbody>
                <tr className="border-b border-dark-200">
                  <td className="px-4 py-3 bg-dark-50 font-medium text-dark-700 w-32">Type</td>
                  <td className="px-4 py-3 text-dark-900">TXT</td>
                </tr>
                <tr className="border-b border-dark-200">
                  <td className="px-4 py-3 bg-dark-50 font-medium text-dark-700">Host / Name</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <code className="font-mono text-dark-900 break-all">{host}</code>
                      <CopyButton value={host} />
                    </div>
                  </td>
                </tr>
                <tr>
                  <td className="px-4 py-3 bg-dark-50 font-medium text-dark-700">Value</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <code className="font-mono text-dark-900 break-all">{token}</code>
                      <CopyButton value={token} />
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* Provider Instructions */}
          <DnsProviderInstructions
            domain={verification.domain}
            host={host}
            value={token}
          />

          {/* Verification Status */}
          <VerificationStatus
            result={result}
            isPolling={isPolling}
            attempts={attempts}
          />

          {verifyError && !result && (
            <div className="p-4 bg-red-50 rounded-lg border border-red-200">
              <p className="text-sm text-red-800">{verifyError}</p>
            </div>
          )}
        </CardContent>
        <CardFooter className="flex justify-between">
          <Button
            variant="outline"
            onClick={() => {
              stopPolling();
              setStep("enter");
              setVerification(null);
            }}
          >
            Back
          </Button>
          <div className="flex gap-2">
            {isPolling ? (
              <Button variant="outline" onClick={stopPolling}>
                Stop Checking
              </Button>
            ) : (
              <>
                <Button variant="outline" onClick={startPolling}>
                  Auto-Check
                </Button>
                <Button onClick={checkOnce}>
                  Check Now
                </Button>
              </>
            )}
          </div>
        </CardFooter>
      </Card>
    );
  }

  // Step 3: Success
  if (step === "success") {
    return (
      <Card>
        <CardContent className="pt-6">
          <div className="text-center space-y-4">
            <div className="flex justify-center">
              <div className="h-16 w-16 rounded-full bg-emerald-100 flex items-center justify-center">
                <svg
                  className="h-8 w-8 text-emerald-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              </div>
            </div>
            <h3 className="text-xl font-semibold text-dark-900">
              Domain Verified Successfully!
            </h3>
            <p className="text-dark-500">
              Your custom domain{" "}
              <span className="font-mono font-semibold">{verification?.domain}</span>{" "}
              is now active. SSL certificate is being provisioned automatically.
            </p>
            <div className="pt-4 space-y-3">
              <a
                href={`https://${verification?.domain}`}
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex items-center gap-2 text-primary-600 hover:text-primary-700 font-medium"
              >
                Visit your site at https://{verification?.domain}
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
                    d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
                  />
                </svg>
              </a>
              <p className="text-sm text-dark-400">
                Note: It may take a few minutes for the SSL certificate to be provisioned.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  return null;
}
