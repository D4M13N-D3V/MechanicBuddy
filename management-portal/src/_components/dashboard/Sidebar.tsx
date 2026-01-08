"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  MessageSquare,
  Building2,
  Wrench,
  User,
  CreditCard,
  Receipt,
  LucideIcon
} from "lucide-react";
import { cn } from "@/_lib/utils";

interface NavItem {
  name: string;
  href: string;
  icon: LucideIcon;
  adminOnly?: boolean;
  exact?: boolean; // If true, only exact match counts as active
}

const navigation: NavItem[] = [
  { name: "Overview", href: "/dashboard", icon: LayoutDashboard, adminOnly: true, exact: true },
  { name: "Tenants", href: "/dashboard/tenants", icon: Building2, adminOnly: true },
  { name: "Demo Requests", href: "/dashboard/demos", icon: MessageSquare, adminOnly: true },
  { name: "Subscriptions", href: "/dashboard/admin-billing", icon: Receipt, adminOnly: true },
  { name: "My Tenants", href: "/dashboard/account", icon: User, adminOnly: false },
  { name: "Billing", href: "/dashboard/billing", icon: CreditCard, adminOnly: false },
];

const ADMIN_ROLES = ["super_admin", "admin", "support"];

interface SidebarProps {
  userRole?: string;
}

export function Sidebar({ userRole = "owner" }: SidebarProps) {
  const pathname = usePathname();
  const isAdmin = ADMIN_ROLES.includes(userRole);

  // Filter navigation based on role
  const visibleNavigation = navigation.filter(item => {
    if (item.adminOnly && !isAdmin) return false;
    return true;
  });

  const isItemActive = (item: NavItem) => {
    if (item.exact) {
      return pathname === item.href;
    }
    return pathname === item.href || pathname.startsWith(item.href + "/");
  };

  return (
    <div className="flex h-full w-64 flex-col bg-dark-950 dark-scrollbar">
      <div className="flex h-16 items-center px-6 border-b border-dark-800">
        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-600">
          <Wrench className="h-5 w-5 text-white" />
        </div>
        <span className="ml-3 text-xl font-bold text-white">MechanicBuddy</span>
      </div>
      <nav className="flex-1 space-y-1 px-3 py-4">
        {visibleNavigation.map((item) => {
          const isActive = isItemActive(item);
          const Icon = item.icon;
          return (
            <Link
              key={item.name}
              href={item.href}
              className={cn(
                "flex items-center rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-200",
                isActive
                  ? "bg-primary-600 text-white shadow-lg shadow-primary-600/25"
                  : "text-dark-400 hover:bg-dark-800 hover:text-white"
              )}
            >
              <Icon className={cn("mr-3 h-5 w-5", isActive && "text-white")} />
              {item.name}
            </Link>
          );
        })}
      </nav>
      <div className="border-t border-dark-800 p-4">
        <p className="text-xs text-dark-500">Management Portal v1.0</p>
      </div>
    </div>
  );
}
