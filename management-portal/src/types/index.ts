// Tenant types
export interface Tenant {
  id: number;
  tenantId: string;
  companyName: string;
  tier: string;
  status: 'provisioning' | 'active' | 'suspended' | 'deleted' | 'trial';
  ownerEmail: string;
  ownerName?: string;
  createdAt: string;
  trialEndsAt?: string;
  subscriptionEndsAt?: string;
  lastBilledAt?: string;
  maxMechanics: number;
  maxStorage: number;
  isDemo: boolean;
  apiUrl?: string;
  mechanicCount?: number;
  lastActivityAt?: string;
}

export interface TenantStats {
  totalRevenue: number;
  activeUsers: number;
  totalWorkOrders: number;
  totalInvoices: number;
}

// Demo request types
export type DemoRequestStatus = 'new' | 'pending_trial' | 'complete' | 'cancelled';

export interface DemoRequest {
  id: string;
  email: string;
  companyName: string;
  phoneNumber?: string;
  message?: string;
  status: DemoRequestStatus;
  createdAt: string;
  notes?: string;
  tenantId?: string;
}

// Pricing types
export interface PricingTier {
  id: string;
  name: string;
  price: number;
  interval: 'month' | 'year';
  features: string[];
  maxMechanics?: number;
  storageLimitGb?: number;
  popular?: boolean;
}

// Analytics types
export interface DashboardAnalytics {
  totalTenants: number;
  activeTenants: number;
  trialTenants: number;
  suspendedTenants: number;
  totalRevenue: number;
  monthlyRecurringRevenue: number;
  averageRevenuePerTenant: number;
  totalDemoRequests: number;
  pendingDemoRequests: number;
  conversionRate: number;
  recentTenants: Tenant[];
  revenueByMonth: RevenueDataPoint[];
  tenantsByPlan: PlanDistribution[];
}

export interface RevenueDataPoint {
  month: string;
  revenue: number;
  tenants: number;
}

export interface PlanDistribution {
  plan: string;
  count: number;
  revenue: number;
}

// Billing types
export interface BillingTransaction {
  id: string;
  tenantId: string;
  tenantName: string;
  amount: number;
  status: 'pending' | 'completed' | 'failed' | 'refunded';
  type: 'subscription' | 'one-time';
  createdAt: string;
  processedAt?: string;
  stripePaymentId?: string;
}

export interface SubscriptionStatus {
  tenantId: string;
  tier: string;
  status: string;
  hasSubscription: boolean;
  subscription?: {
    id: string;
    status: string;
    currentPeriodEnd?: string;
    cancelAtPeriodEnd: boolean;
  };
  invoices: Invoice[];
}

export interface Invoice {
  id: string;
  amount: number;
  currency: string;
  status: string;
  date: string;
  pdfUrl?: string;
  hostedUrl?: string;
}

// Admin user types
export interface AdminUser {
  id: string;
  email: string;
  name: string;
  role: 'admin' | 'owner' | 'support' | 'user';
  createdAt: string;
}

// Portal user types (for regular users who own tenants)
export interface PortalUser {
  id: string;
  email: string;
  name: string;
  role: 'admin' | 'owner' | 'support' | 'user';
  createdAt: string;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface AuthSession {
  user: AdminUser;
  token: string;
  expiresAt: string;
}

// Form types
export interface DemoFormData {
  email: string;
  companyName: string;
  phoneNumber?: string;
  message: string;
}

export interface RequestTenantData {
  companyName: string;
  message?: string;
}

// Domain types
export interface DomainVerification {
  id: number;
  domain: string;
  verificationToken: string;
  verificationMethod: 'dns';
  isVerified: boolean;
  createdAt: string;
  verifiedAt?: string;
  expiresAt?: string;
  isExpired?: boolean;
  instructions?: DomainVerificationInstructions;
}

export interface DomainVerificationInstructions {
  type: string;
  host: string;
  value: string;
  alternativeHost?: string;
  description: string;
}

export interface DomainVerificationResult {
  success: boolean;
  status: 'verified' | 'pending' | 'expired' | 'not_found' | 'error';
  domain?: string;
  verifiedAt?: string;
  errorCode?: string;
  errorMessage?: string;
  dnsCheck?: DnsCheckResult;
}

export interface DnsCheckResult {
  recordFound: boolean;
  actualValue?: string;
  expectedValue: string;
  host: string;
  allRecordsFound: string[];
}

// API response types
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
