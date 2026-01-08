"use server";

import {
  createTeamCheckout,
  createLifetimeCheckout,
  createBillingPortalSession,
} from "@/_lib/api";

export async function createTeamCheckoutAction(tenantId: string) {
  const returnUrl = `${process.env.NEXT_PUBLIC_APP_URL || "http://localhost:3000"}/dashboard/billing`;
  const result = await createTeamCheckout(tenantId, returnUrl);

  if (!result.success || !result.data) {
    throw new Error(result.error || "Failed to create checkout session");
  }

  return result.data.url;
}

export async function createLifetimeCheckoutAction(tenantId: string) {
  const returnUrl = `${process.env.NEXT_PUBLIC_APP_URL || "http://localhost:3000"}/dashboard/billing`;
  const result = await createLifetimeCheckout(tenantId, returnUrl);

  if (!result.success || !result.data) {
    throw new Error(result.error || "Failed to create checkout session");
  }

  return result.data.url;
}

export async function openBillingPortalAction(tenantId: string) {
  const returnUrl = `${process.env.NEXT_PUBLIC_APP_URL || "http://localhost:3000"}/dashboard/billing`;
  const result = await createBillingPortalSession(tenantId, returnUrl);

  if (!result.success || !result.data) {
    throw new Error(result.error || "Failed to create billing portal session");
  }

  return result.data.url;
}
