"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  Users,
  MessageSquare,
  CreditCard,
  Settings,
  Building2,
  Wrench
} from "lucide-react";
import { cn } from "@/_lib/utils";

const navigation = [
  { name: "Overview", href: "/dashboard", icon: LayoutDashboard },
  { name: "Tenants", href: "/dashboard/tenants", icon: Building2 },
  { name: "Demo Requests", href: "/dashboard/demos", icon: MessageSquare },
  { name: "Billing", href: "/dashboard/billing", icon: CreditCard },
];

export function Sidebar() {
  const pathname = usePathname();

  return (
    <div className="flex h-full w-64 flex-col bg-dark-950 dark-scrollbar">
      <div className="flex h-16 items-center px-6 border-b border-dark-800">
        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-600">
          <Wrench className="h-5 w-5 text-white" />
        </div>
        <span className="ml-3 text-xl font-bold text-white">MechanicBuddy</span>
      </div>
      <nav className="flex-1 space-y-1 px-3 py-4">
        {navigation.map((item) => {
          const isActive = pathname === item.href;
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
