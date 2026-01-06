import Link from "next/link";
import { Building2 } from "lucide-react";
import { Button } from "@/_components/ui/Button";

export default function PublicLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen bg-gray-50">
      <header className="border-b bg-white">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4">
          <Link href="/" className="flex items-center gap-2">
            <Building2 className="h-8 w-8 text-primary-600" />
            <span className="text-xl font-bold text-gray-900">MechanicBuddy</span>
          </Link>
          <div className="flex items-center gap-6">
            <Link href="/pricing" className="text-sm font-medium text-gray-700 hover:text-gray-900">
              Pricing
            </Link>
            <Link href="/demo" className="text-sm font-medium text-gray-700 hover:text-gray-900">
              Request Demo
            </Link>
            <Link href="/login">
              <Button variant="outline" size="sm">Admin Login</Button>
            </Link>
          </div>
        </nav>
      </header>
      <main>{children}</main>
      <footer className="border-t bg-white mt-16">
        <div className="mx-auto max-w-7xl px-6 py-8">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Building2 className="h-6 w-6 text-primary-600" />
              <span className="font-semibold text-gray-900">MechanicBuddy</span>
            </div>
            <p className="text-sm text-gray-500">
              &copy; {new Date().getFullYear()} MechanicBuddy. All rights reserved.
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
