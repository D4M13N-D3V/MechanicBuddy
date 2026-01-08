"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { DomainWizard } from "@/_components/domains/DomainWizard";
import { CurrentDomainCard } from "@/_components/domains/CurrentDomainCard";
import type { DomainVerification } from "@/types";

interface DomainPageClientProps {
  tenantId: number;
  verifiedDomain?: DomainVerification;
  pendingDomain?: DomainVerification;
}

export function DomainPageClient({
  tenantId,
  verifiedDomain,
  pendingDomain,
}: DomainPageClientProps) {
  const router = useRouter();
  const [currentVerifiedDomain, setCurrentVerifiedDomain] = useState(verifiedDomain);
  const [currentPendingDomain, setCurrentPendingDomain] = useState(pendingDomain);

  const handleDomainRemoved = () => {
    setCurrentVerifiedDomain(undefined);
    setCurrentPendingDomain(undefined);
    router.refresh();
  };

  const handleVerificationComplete = () => {
    router.refresh();
  };

  // Show verified domain card if domain is verified
  if (currentVerifiedDomain) {
    return (
      <CurrentDomainCard
        domain={currentVerifiedDomain}
        tenantId={tenantId}
        onRemoved={handleDomainRemoved}
      />
    );
  }

  // Show wizard - either continue pending verification or start new one
  return (
    <DomainWizard
      tenantId={tenantId}
      existingVerification={currentPendingDomain}
      onComplete={handleVerificationComplete}
    />
  );
}
