import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "MechanicBuddy - Workshop Management Software for Auto Repair Shops",
  description: "All-in-one workshop management system for vehicle service centers. Manage work orders, clients, vehicles, inventory, and invoicing. Start free today.",
  openGraph: {
    title: "MechanicBuddy - Workshop Management Made Simple",
    description: "All-in-one workshop management system for vehicle service centers. Manage work orders, clients, vehicles, inventory, and invoicing.",
    type: "website",
  },
  twitter: {
    card: "summary_large_image",
    title: "MechanicBuddy - Workshop Management Made Simple",
    description: "All-in-one workshop management system for vehicle service centers. Manage work orders, clients, vehicles, inventory, and invoicing.",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className="antialiased">
        {children}
      </body>
    </html>
  );
}
