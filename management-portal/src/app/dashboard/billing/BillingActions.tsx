"use client";

import { Button } from "@/_components/ui/Button";
import { useState } from "react";
import {
  createTeamCheckoutAction,
  createLifetimeCheckoutAction,
  openBillingPortalAction,
} from "./actions";

interface BillingActionsProps {
  tenantId: string;
  tier: string;
  hasSubscription: boolean;
}

export function BillingActions({ tenantId, tier, hasSubscription }: BillingActionsProps) {
  const [loading, setLoading] = useState<string | null>(null);

  const handleTeamUpgrade = async () => {
    setLoading("team");
    try {
      const url = await createTeamCheckoutAction(tenantId);
      window.location.href = url;
    } catch (error) {
      console.error("Failed to create checkout session:", error);
      alert("Failed to start checkout. Please try again.");
      setLoading(null);
    }
  };

  const handleLifetimeUpgrade = async () => {
    setLoading("lifetime");
    try {
      const url = await createLifetimeCheckoutAction(tenantId);
      window.location.href = url;
    } catch (error) {
      console.error("Failed to create checkout session:", error);
      alert("Failed to start checkout. Please try again.");
      setLoading(null);
    }
  };

  const handleManageSubscription = async () => {
    setLoading("portal");
    try {
      const url = await openBillingPortalAction(tenantId);
      window.location.href = url;
    } catch (error) {
      console.error("Failed to open billing portal:", error);
      alert("Failed to open billing portal. Please try again.");
      setLoading(null);
    }
  };

  // Lifetime tier - no actions needed
  if (tier === "lifetime") {
    return (
      <div className="bg-green-50 border border-green-200 rounded-lg p-4">
        <p className="text-sm font-medium text-green-800">
          Lifetime Access Active - No action needed
        </p>
      </div>
    );
  }

  // Team tier - can upgrade to lifetime or manage subscription
  if (tier === "team" && hasSubscription) {
    return (
      <div className="flex items-center gap-2">
        <Button
          onClick={handleLifetimeUpgrade}
          disabled={loading !== null}
          variant="primary"
        >
          {loading === "lifetime" ? "Loading..." : "Upgrade to Lifetime ($250)"}
        </Button>
        <Button
          onClick={handleManageSubscription}
          disabled={loading !== null}
          variant="outline"
        >
          {loading === "portal" ? "Loading..." : "Manage Subscription"}
        </Button>
      </div>
    );
  }

  // Free tier - can upgrade to Team or Lifetime
  return (
    <div className="flex items-center gap-2">
      <Button
        onClick={handleTeamUpgrade}
        disabled={loading !== null}
        variant="primary"
      >
        {loading === "team" ? "Loading..." : "Upgrade to Team ($20/mo)"}
      </Button>
      <Button
        onClick={handleLifetimeUpgrade}
        disabled={loading !== null}
        variant="outline"
      >
        {loading === "lifetime" ? "Loading..." : "Buy Lifetime ($250)"}
      </Button>
    </div>
  );
}
