// Tenant types
export interface Tenant {
  id: string;
  companyName: string;
  subdomain: string;
  plan: 'free' | 'standard' | 'premium' | 'enterprise';
  status: 'active' | 'suspended' | 'cancelled' | 'trial';
  createdAt: string;
  updatedAt: string;
  billingEmail: string;
  mechanicCount: number;
  storageUsedMb: number;
  lastActivityAt: string;
  trialEndsAt?: string;
}

export interface TenantStats {
  totalRevenue: number;
  activeUsers: number;
  totalWorkOrders: number;
  totalInvoices: number;
}

// Demo request types
export interface DemoRequest {
  id: string;
  email: string;
  companyName: string;
  phoneNumber?: string;
  message: string;
  status: 'pending' | 'contacted' | 'converted' | 'declined';
  createdAt: string;
  contactedAt?: string;
  notes?: string;
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

// Admin user types
export interface AdminUser {
  id: string;
  email: string;
  name: string;
  role: 'super_admin' | 'support';
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
