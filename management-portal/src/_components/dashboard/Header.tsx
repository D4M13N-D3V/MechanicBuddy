"use client";

import { LogOut, User } from "lucide-react";
import { logout } from "@/_lib/auth";
import type { AdminUser } from "@/types";
import { getInitials } from "@/_lib/utils";
import { useRouter } from "next/navigation";

interface HeaderProps {
  user: AdminUser;
}

export function Header({ user }: HeaderProps) {
  const router = useRouter();

  const handleLogout = async () => {
    await logout();
    router.push("/login");
    router.refresh();
  };

  return (
    <header className="flex h-16 items-center justify-between border-b border-dark-200 bg-white px-6">
      <div>
        <h1 className="text-2xl font-bold text-dark-900">Management Portal</h1>
      </div>
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-dark-900 text-white font-bold text-sm">
            {getInitials(user.name)}
          </div>
          <div className="text-sm">
            <p className="font-semibold text-dark-900">{user.name}</p>
            <p className="text-dark-500">{user.email}</p>
          </div>
        </div>
        <button
          onClick={handleLogout}
          className="flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold text-dark-600 hover:bg-dark-100 hover:text-primary-600 transition-all duration-200"
        >
          <LogOut className="h-4 w-4" />
          Logout
        </button>
      </div>
    </header>
  );
}
