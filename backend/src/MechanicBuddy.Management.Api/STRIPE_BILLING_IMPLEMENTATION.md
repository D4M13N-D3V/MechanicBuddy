# Stripe Billing Implementation

## Overview

This document describes the complete Stripe billing implementation for the MechanicBuddy SaaS platform. The implementation handles subscription management, mechanic count-based pricing tiers, webhook processing, and customer billing portal access.

## Pricing Model

The pricing is based on the number of mechanics (users) in a tenant's workspace:

| Tier | Mechanics | Price per Mechanic | Monthly Cost (examples) |
|------|-----------|-------------------|------------------------|
| **Free** | 1 | $0 | $0 |
| **Standard** | 2-10 | $20/month | $40-$200 |
| **Growth** | 11-20 | $15/month | $165-$300 |
| **Scale** | 21+ | $10/month | $210+ |

Pricing automatically adjusts as mechanic counts change, with prorated billing when switching tiers.

## Architecture

### Components

1. **StripeClient** (`Infrastructure/StripeClient.cs`)
   - Wraps the Stripe.net SDK
   - Provides strongly-typed methods for all Stripe operations
   - Handles customer, subscription, invoice, and billing portal operations

2. **BillingService** (`Services/BillingService.cs`)
   - Business logic layer for billing operations
   - Manages tenant subscriptions and pricing tiers
   - Processes Stripe webhook events
   - Syncs mechanic counts with Stripe subscriptions

3. **BillingController** (`Controllers/BillingController.cs`)
   - REST API endpoints for billing operations
   - Webhook endpoint for Stripe events
   - Customer portal session creation

## API Endpoints

### Customer and Subscription Management

#### POST `/api/billing/create-customer`
Creates a Stripe customer for a tenant.

**Request:**
```json
{
  "tenantId": "tenant_abc123",
  "email": "admin@company.com",
  "name": "Company Name"
}
```

**Response:**
```json
{
  "customerId": "cus_stripe123"
}
```

#### POST `/api/billing/create-subscription`
Creates a subscription for an existing customer.

**Request:**
```json
{
  "tenantId": "tenant_abc123",
  "priceId": "price_standard_mechanic"
}
```

**Response:**
```json
{
  "subscriptionId": "sub_stripe123"
}
```

#### POST `/api/billing/create-customer-and-subscription`
Creates both customer and subscription in one operation (recommended for new paid tenants).

**Request:**
```json
{
  "tenantId": "tenant_abc123",
  "email": "admin@company.com",
  "name": "Company Name",
  "mechanicCount": 5
}
```

**Response:**
```json
{
  "customerId": "cus_stripe123",
  "subscriptionId": "sub_stripe123"
}
```

**Note:** The price tier is automatically selected based on `mechanicCount`.

### Mechanic Count Sync

#### POST `/api/billing/sync-mechanics`
Updates the subscription quantity when mechanic count changes. Called by the CronJob that monitors mechanic counts.

**Request:**
```json
{
  "tenantId": "tenant_abc123",
  "mechanicCount": 8
}
```

**Response:**
```json
{
  "message": "Mechanic count synced successfully",
  "mechanicCount": 8
}
```

**Behavior:**
- If mechanic count = 1: Moves to free tier (no subscription needed)
- If mechanic count changes tiers (e.g., 10 → 11): Switches price ID with proration
- If within same tier: Updates quantity only

### Billing Portal

#### POST `/api/billing/portal-session`
Creates a Stripe Customer Portal session URL for self-service billing management.

**Request:**
```json
{
  "tenantId": "tenant_abc123",
  "returnUrl": "https://admin.mechanicbuddy.com/billing"
}
```

**Response:**
```json
{
  "url": "https://billing.stripe.com/session/xxx"
}
```

The customer portal allows users to:
- Update payment methods
- View invoices
- Download receipts
- Cancel subscriptions
- Update billing information

### Billing History

#### GET `/api/billing/invoices/{tenantId}`
Retrieves invoice history for a tenant.

**Response:**
```json
[
  {
    "id": "in_stripe123",
    "amountDue": 100.00,
    "amountPaid": 100.00,
    "currency": "USD",
    "status": "paid",
    "periodStart": "2026-01-01T00:00:00Z",
    "periodEnd": "2026-02-01T00:00:00Z",
    "created": "2026-01-01T00:00:00Z",
    "hostedInvoiceUrl": "https://invoice.stripe.com/i/xxx",
    "invoicePdf": "https://invoice.stripe.com/i/xxx/pdf"
  }
]
```

#### GET `/api/billing/history/{tenantId}`
Retrieves billing event history from the local database.

**Query Parameters:**
- `skip`: Number of records to skip (default: 0)
- `take`: Number of records to return (default: 50)

### Subscription Management

#### POST `/api/billing/cancel-subscription`
Cancels a tenant's subscription (at period end by default).

**Request:**
```json
{
  "tenantId": "tenant_abc123"
}
```

### Revenue Analytics

#### GET `/api/billing/revenue`
Gets total revenue for a date range.

**Query Parameters:**
- `startDate`: ISO 8601 date (optional)
- `endDate`: ISO 8601 date (optional)

**Response:**
```json
{
  "revenue": 12450.00,
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-01-31T23:59:59Z"
}
```

## Webhook Integration

#### POST `/api/billing/webhook`
Receives and processes Stripe webhook events.

**Required Header:**
- `Stripe-Signature`: Webhook signature for verification

**Supported Events:**

1. **customer.subscription.created**
   - Updates tenant subscription ID
   - Sets tenant status to "active"
   - Records subscription period

2. **customer.subscription.updated**
   - Updates subscription status
   - Syncs mechanic count from quantity
   - Updates tier based on quantity
   - Handles status changes (active, past_due, unpaid, canceled)

3. **customer.subscription.deleted**
   - Sets tenant status to "cancelled"
   - Clears subscription ID
   - Records cancellation event

4. **invoice.paid**
   - Records successful payment
   - Logs payment amount and invoice ID

5. **invoice.payment_failed**
   - Sets tenant status to "suspended"
   - Records failed payment
   - Logs retry information

## Configuration

### appsettings.json

```json
{
  "Stripe": {
    "PriceIds": {
      "Standard": "price_standard_mechanic",
      "Growth": "price_growth_mechanic",
      "Scale": "price_scale_mechanic"
    }
  }
}
```

### appsettings.Secrets.json

```json
{
  "Stripe": {
    "SecretKey": "sk_test_YOUR_STRIPE_SECRET_KEY",
    "PublishableKey": "pk_test_YOUR_STRIPE_PUBLISHABLE_KEY",
    "WebhookSecret": "whsec_YOUR_STRIPE_WEBHOOK_SECRET",
    "PriceIds": {
      "Standard": "price_YOUR_STANDARD_PRICE_ID",
      "Growth": "price_YOUR_GROWTH_PRICE_ID",
      "Scale": "price_YOUR_SCALE_PRICE_ID"
    }
  }
}
```

## Stripe Setup Instructions

### 1. Create Products and Prices

In the Stripe Dashboard:

1. Create three products:
   - "MechanicBuddy Standard"
   - "MechanicBuddy Growth"
   - "MechanicBuddy Scale"

2. For each product, create a recurring price:
   - Standard: $20/month per unit
   - Growth: $15/month per unit
   - Scale: $10/month per unit

3. Copy the price IDs and update `appsettings.Secrets.json`

### 2. Configure Webhook Endpoint

1. Go to Stripe Dashboard → Developers → Webhooks
2. Add endpoint: `https://api.mechanicbuddy.com/api/billing/webhook`
3. Select events to listen for:
   - `customer.subscription.created`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.paid`
   - `invoice.payment_failed`
4. Copy the webhook signing secret to `appsettings.Secrets.json`

### 3. Configure Customer Portal

1. Go to Stripe Dashboard → Settings → Billing → Customer Portal
2. Enable the features you want customers to access:
   - Update payment methods
   - View invoices
   - Cancel subscriptions
3. Configure cancellation behavior (immediate vs. end of period)

## Database Schema

### Tenant Table Updates

The `Tenant` entity includes Stripe-related fields:

- `StripeCustomerId`: Stripe customer ID (e.g., "cus_xxx")
- `StripeSubscriptionId`: Stripe subscription ID (e.g., "sub_xxx")
- `Tier`: Pricing tier ("free", "standard", "growth", "scale")
- `Status`: Tenant status ("active", "suspended", "cancelled", "demo")
- `MaxMechanics`: Maximum allowed mechanics based on subscription
- `SubscriptionEndsAt`: End date of current billing period
- `LastBilledAt`: Last successful billing date

### BillingEvent Table

Records all billing-related events:

- `TenantId`: Associated tenant
- `EventType`: Type of event (e.g., "payment_succeeded", "subscription_updated")
- `Amount`: Transaction amount (for payments)
- `Currency`: Currency code (default: "USD")
- `StripeEventId`: Stripe event ID (for webhooks)
- `InvoiceId`: Stripe invoice ID (if applicable)
- `Metadata`: Additional event data (JSON)
- `CreatedAt`: Event timestamp

## Business Logic

### Tier Selection Algorithm

```csharp
public string? CalculatePriceForMechanicCount(int mechanicCount)
{
    if (mechanicCount <= 1)
        return null; // Free tier
    else if (mechanicCount <= 10)
        return StandardPriceId; // $20/mechanic
    else if (mechanicCount <= 20)
        return GrowthPriceId; // $15/mechanic
    else
        return ScalePriceId; // $10/mechanic
}
```

### Subscription Sync Flow

When mechanic count changes:

1. Fetch current subscription from Stripe
2. Calculate new price ID based on mechanic count
3. If price tier changed:
   - Update subscription with new price ID and quantity
   - Prorate the difference
4. If same tier:
   - Update quantity only
5. Update tenant record with new tier and max mechanics
6. Record event in BillingEvent table

### Payment Failure Handling

When `invoice.payment_failed` webhook received:

1. Extract tenant ID from subscription metadata
2. Set tenant status to "suspended"
3. Record failed payment event with attempt count
4. Log error for monitoring

The tenant remains "suspended" until:
- Payment succeeds (via `invoice.paid` webhook), or
- Subscription is cancelled

### Subscription Metadata

All subscriptions include metadata:

```json
{
  "tenant_id": "tenant_abc123"
}
```

This enables webhook event routing back to the correct tenant.

## Error Handling

### Common Errors

1. **Tenant not found**
   - HTTP 400: "Tenant not found"
   - Check tenant ID is correct

2. **No Stripe customer**
   - HTTP 400: "Tenant does not have a Stripe customer"
   - Call `create-customer` first

3. **Price ID not configured**
   - HTTP 500: "Standard/Growth/Scale price ID not configured"
   - Update `appsettings.Secrets.json`

4. **Webhook signature verification failed**
   - HTTP 400
   - Check webhook secret configuration
   - Ensure raw request body is used (no JSON parsing before signature check)

## Security Considerations

1. **Webhook Verification**
   - Always verify Stripe webhook signatures
   - Use webhook secret from Stripe Dashboard
   - Reject unverified requests

2. **Authorization**
   - All endpoints except `/webhook` require JWT authentication
   - Validate tenant ownership before operations
   - Use `[Authorize]` attribute on controller

3. **Secrets Management**
   - Never commit `appsettings.Secrets.json`
   - Use environment variables in production
   - Rotate webhook secrets periodically

## Testing

### Test Mode

1. Use Stripe test mode API keys (start with `sk_test_`)
2. Use test webhook endpoint during development
3. Use Stripe CLI for local webhook testing:

```bash
stripe listen --forward-to http://localhost:5000/api/billing/webhook
stripe trigger invoice.payment_failed
```

### Test Credit Cards

In test mode, use:
- Success: `4242 4242 4242 4242`
- Declined: `4000 0000 0000 0002`
- Requires 3DS: `4000 0025 0000 3155`

## Monitoring

### Key Metrics to Monitor

1. **Subscription Events**
   - Creation/update/cancellation rates
   - Active vs. cancelled subscriptions

2. **Payment Success Rate**
   - Successful payments / total attempts
   - Failed payment reasons

3. **Revenue Metrics**
   - Monthly Recurring Revenue (MRR)
   - Average Revenue Per User (ARPU)
   - Churn rate

4. **Webhook Reliability**
   - Webhook processing time
   - Failed webhook events
   - Retry counts

### Logging

All operations log:
- Tenant ID
- Operation type
- Stripe resource IDs
- Success/failure status
- Error details (if applicable)

Example log entries:
```
[INFO] Created customer cus_xxx for tenant tenant_abc123
[INFO] Updated subscription sub_xxx quantity to 8
[ERROR] Payment failed for tenant tenant_abc123 - Invoice in_xxx, Amount: 160.00
```

## Integration with CronJob

A separate CronJob should periodically:

1. Query each tenant's mechanic count from the main API
2. Call `/api/billing/sync-mechanics` with current count
3. Handle any errors (e.g., suspended tenants)

Example CronJob implementation:

```csharp
// Runs daily at 2 AM
foreach (var tenant in activeTenants)
{
    var mechanicCount = await GetMechanicCount(tenant.Id);

    await httpClient.PostAsJsonAsync("/api/billing/sync-mechanics", new
    {
        TenantId = tenant.Id,
        MechanicCount = mechanicCount
    });
}
```

## Troubleshooting

### Subscription not updating

1. Check webhook events in Stripe Dashboard
2. Verify webhook endpoint is accessible
3. Check webhook secret configuration
4. Review application logs for errors

### Payment failures

1. Check customer payment method is valid
2. Review Stripe Dashboard for decline reasons
3. Verify customer has sufficient funds
4. Check for card verification requirements

### Incorrect pricing

1. Verify price IDs in configuration
2. Check mechanic count calculation
3. Review subscription quantity in Stripe
4. Verify tier selection logic

## Future Enhancements

Potential improvements:

1. **Annual Billing**
   - Add annual price IDs with discount
   - Support switching between monthly/annual

2. **Usage-Based Billing**
   - Meter storage usage
   - Charge for overages

3. **Custom Pricing**
   - Enterprise tier with custom pricing
   - Volume discounts for large deployments

4. **Invoicing Features**
   - Custom invoice line items
   - Tax calculation integration
   - Multi-currency support

5. **Dunning Management**
   - Automated retry schedules
   - Customer notification emails
   - Grace periods before suspension
