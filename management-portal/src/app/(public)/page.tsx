import Link from "next/link";
import { Button } from "@/_components/ui/Button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/_components/ui/Card";
import { CheckCircle2, Users, FileText, Package, BarChart } from "lucide-react";

export default function HomePage() {
  const features = [
    {
      icon: Users,
      title: "Client & Vehicle Management",
      description: "Organize client information and vehicle histories in one place",
    },
    {
      icon: FileText,
      title: "Work Orders & Invoicing",
      description: "Create professional work orders and invoices with PDF generation",
    },
    {
      icon: Package,
      title: "Inventory Tracking",
      description: "Manage spare parts inventory with low stock alerts",
    },
    {
      icon: BarChart,
      title: "Analytics & Reports",
      description: "Track revenue, monitor performance, and make data-driven decisions",
    },
  ];

  const pricingPreview = [
    { name: "Free", price: "$0", mechanics: "1 mechanic" },
    { name: "Standard", price: "$20", mechanics: "Per mechanic/month", popular: true },
    { name: "Premium", price: "$50", mechanics: "Per mechanic/month" },
  ];

  return (
    <div>
      {/* Hero Section */}
      <section className="bg-gradient-to-b from-primary-50 to-white py-20">
        <div className="mx-auto max-w-7xl px-6">
          <div className="text-center">
            <h1 className="text-5xl font-bold text-gray-900 mb-6">
              Workshop Management<br />Made Simple
            </h1>
            <p className="text-xl text-gray-600 mb-8 max-w-2xl mx-auto">
              MechanicBuddy is a self-hosted workshop management system for vehicle
              service centers. Handle work orders, inventory, invoicing, and more.
            </p>
            <div className="flex justify-center gap-4">
              <Link href="/demo">
                <Button size="lg">Request a Demo</Button>
              </Link>
              <Link href="/pricing">
                <Button variant="outline" size="lg">View Pricing</Button>
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-20">
        <div className="mx-auto max-w-7xl px-6">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">
              Everything You Need to Run Your Workshop
            </h2>
            <p className="text-lg text-gray-600">
              Powerful features designed for vehicle service centers
            </p>
          </div>
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
            {features.map((feature) => {
              const Icon = feature.icon;
              return (
                <Card key={feature.title}>
                  <CardHeader>
                    <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary-100 mb-4">
                      <Icon className="h-6 w-6 text-primary-600" />
                    </div>
                    <CardTitle className="text-lg">{feature.title}</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <CardDescription>{feature.description}</CardDescription>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      </section>

      {/* Pricing Preview Section */}
      <section className="bg-gray-50 py-20">
        <div className="mx-auto max-w-7xl px-6">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">
              Simple, Transparent Pricing
            </h2>
            <p className="text-lg text-gray-600">
              Choose the plan that fits your workshop
            </p>
          </div>
          <div className="grid md:grid-cols-3 gap-6 max-w-4xl mx-auto">
            {pricingPreview.map((plan) => (
              <Card key={plan.name} className={plan.popular ? "border-primary-600 border-2" : ""}>
                <CardHeader>
                  {plan.popular && (
                    <div className="text-xs font-semibold text-primary-600 mb-2">MOST POPULAR</div>
                  )}
                  <CardTitle>{plan.name}</CardTitle>
                  <div className="mt-4">
                    <span className="text-4xl font-bold">{plan.price}</span>
                    {plan.name !== "Free" && <span className="text-gray-600">/mo</span>}
                  </div>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-gray-600 mb-6">{plan.mechanics}</p>
                  <Link href="/pricing">
                    <Button
                      variant={plan.popular ? "primary" : "outline"}
                      className="w-full"
                    >
                      Learn More
                    </Button>
                  </Link>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20">
        <div className="mx-auto max-w-4xl px-6 text-center">
          <h2 className="text-3xl font-bold text-gray-900 mb-4">
            Ready to Get Started?
          </h2>
          <p className="text-lg text-gray-600 mb-8">
            Request a demo and see how MechanicBuddy can transform your workshop
          </p>
          <Link href="/demo">
            <Button size="lg">Request a Demo</Button>
          </Link>
        </div>
      </section>
    </div>
  );
}
