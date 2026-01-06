import Link from "next/link";
import { Wrench } from "lucide-react";
import { Button } from "@/_components/ui/Button";

export default function PublicLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen bg-white">
      <header className="border-b border-dark-100 bg-white/80 backdrop-blur-md sticky top-0 z-50">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4">
          <Link href="/" className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-600">
              <Wrench className="h-5 w-5 text-white" />
            </div>
            <span className="text-xl font-bold text-dark-900">MechanicBuddy</span>
          </Link>
          <div className="flex items-center gap-8">
            <Link href="/pricing" className="text-sm font-semibold text-dark-600 hover:text-dark-900 transition-colors">
              Pricing
            </Link>
            <Link href="/demo" className="text-sm font-semibold text-dark-600 hover:text-dark-900 transition-colors">
              Request Demo
            </Link>
            <Link href="/login">
              <Button variant="outline" size="sm">Admin Login</Button>
            </Link>
          </div>
        </nav>
      </header>
      <main>{children}</main>
      <footer className="border-t border-dark-200 bg-dark-950 mt-16">
        <div className="mx-auto max-w-7xl px-6 py-12">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-600">
                <Wrench className="h-4 w-4 text-white" />
              </div>
              <span className="font-bold text-white">MechanicBuddy</span>
            </div>
            <p className="text-sm text-dark-400">
              &copy; {new Date().getFullYear()} MechanicBuddy. All rights reserved.
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
