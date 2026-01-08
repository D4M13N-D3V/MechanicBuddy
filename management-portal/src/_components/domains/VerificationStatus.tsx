"use client";

import type { DomainVerificationResult } from "@/types";

interface VerificationStatusProps {
  result: DomainVerificationResult | null;
  isPolling: boolean;
  attempts?: number;
}

const ERROR_TITLES: Record<string, string> = {
  DNS_RECORD_NOT_FOUND: "DNS Record Not Found",
  DNS_VALUE_MISMATCH: "Value Mismatch",
  DNS_QUERY_FAILED: "DNS Lookup Failed",
  DNS_CHECK_ERROR: "Check Error",
  VERIFICATION_EXPIRED: "Token Expired",
  DOMAIN_NOT_FOUND: "Domain Not Found",
};

const ERROR_HINTS: Record<string, string> = {
  DNS_RECORD_NOT_FOUND: "DNS changes can take anywhere from a few minutes to 48 hours to propagate globally. If you just added the record, please wait a few minutes and try again.",
  DNS_VALUE_MISMATCH: "Make sure you copied the verification token exactly as shown, without any extra spaces or characters.",
  DNS_QUERY_FAILED: "There was an issue looking up your DNS records. This might be a temporary network issue. Please try again in a few minutes.",
  DNS_CHECK_ERROR: "An unexpected error occurred. Please try again later or contact support if the issue persists.",
  VERIFICATION_EXPIRED: "Your verification token has expired. Please remove this domain and add it again to get a new token.",
};

export function VerificationStatus({ result, isPolling, attempts = 0 }: VerificationStatusProps) {
  if (isPolling) {
    return (
      <div className="flex items-center gap-3 p-4 bg-dark-50 rounded-lg border border-dark-200">
        <svg
          className="h-5 w-5 animate-spin text-primary-600"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            className="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            strokeWidth="4"
          />
          <path
            className="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          />
        </svg>
        <div>
          <span className="text-dark-700">Checking DNS records...</span>
          {attempts > 0 && (
            <span className="text-dark-500 text-sm ml-2">
              (Attempt {attempts})
            </span>
          )}
        </div>
      </div>
    );
  }

  if (!result) return null;

  if (result.success) {
    return (
      <div className="flex items-center gap-3 p-4 bg-emerald-50 rounded-lg border border-emerald-200">
        <svg
          className="h-5 w-5 text-emerald-600 flex-shrink-0"
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
        <span className="text-emerald-800 font-medium">Domain verified successfully!</span>
      </div>
    );
  }

  // Error state
  const errorTitle = result.errorCode ? ERROR_TITLES[result.errorCode] || "Verification Failed" : "Verification Failed";
  const errorHint = result.errorCode ? ERROR_HINTS[result.errorCode] : null;

  return (
    <div className="p-4 bg-amber-50 rounded-lg border border-amber-200 space-y-3">
      <div className="flex items-start gap-3">
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
            d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
          />
        </svg>
        <div className="flex-1">
          <p className="font-medium text-amber-800">{errorTitle}</p>
          {result.errorMessage && (
            <p className="text-sm text-amber-700 mt-1">{result.errorMessage}</p>
          )}
          {errorHint && (
            <p className="text-sm text-amber-600 mt-2">{errorHint}</p>
          )}
        </div>
      </div>

      {/* DNS Debug Info */}
      {result.dnsCheck && (
        <DnsDebugInfo dnsCheck={result.dnsCheck} />
      )}
    </div>
  );
}

interface DnsDebugInfoProps {
  dnsCheck: NonNullable<DomainVerificationResult["dnsCheck"]>;
}

function DnsDebugInfo({ dnsCheck }: DnsDebugInfoProps) {
  if (!dnsCheck.allRecordsFound?.length && !dnsCheck.recordFound) {
    return null;
  }

  return (
    <div className="mt-3 p-3 bg-white rounded border border-amber-200">
      <p className="text-xs font-medium text-dark-700 mb-2">DNS Check Details</p>
      <div className="space-y-1 text-xs font-mono">
        <div className="flex gap-2">
          <span className="text-dark-500">Host:</span>
          <span className="text-dark-800">{dnsCheck.host}</span>
        </div>
        <div className="flex gap-2">
          <span className="text-dark-500">Expected:</span>
          <span className="text-dark-800 break-all">{dnsCheck.expectedValue}</span>
        </div>
        {dnsCheck.actualValue && (
          <div className="flex gap-2">
            <span className="text-dark-500">Found:</span>
            <span className="text-amber-700 break-all">{dnsCheck.actualValue}</span>
          </div>
        )}
        {dnsCheck.allRecordsFound?.length > 0 && (
          <div className="mt-2">
            <span className="text-dark-500">All TXT records found:</span>
            <ul className="mt-1 space-y-1">
              {dnsCheck.allRecordsFound.map((record, i) => (
                <li key={i} className="text-dark-600 break-all pl-2 border-l-2 border-dark-200">
                  {record}
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
}
