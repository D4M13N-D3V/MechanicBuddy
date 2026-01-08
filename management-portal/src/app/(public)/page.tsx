import Link from "next/link";
import { Button } from "@/_components/ui/Button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/_components/ui/Card";
import { Users, FileText, Package, BarChart, ArrowRight, Github } from "lucide-react";

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
    { name: "Solo", price: "$0", period: "forever", description: "1 user, limited work orders" },
    { name: "Team", price: "$20", period: "/month", description: "Unlimited users & work orders", popular: true },
    { name: "Lifetime", price: "$250", period: "once", description: "Unlimited users & work orders forever" },
  ];

  return (
    <div>
      {/* Hero Section */}
      <section className="relative overflow-hidden bg-dark-950 py-24">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-primary-900/20 via-dark-950 to-dark-950" />
        <div className="relative mx-auto max-w-7xl px-6">
          <div className="text-center">
            <h1 className="text-5xl md:text-6xl font-bold text-white mb-6 tracking-tight">
              Workshop Management<br />
              <span className="text-primary-500">Made Simple</span>
            </h1>
            <p className="text-xl text-dark-300 mb-10 max-w-2xl mx-auto leading-relaxed">
              MechanicBuddy is a self-hosted workshop management system for vehicle
              service centers. Handle work orders, inventory, invoicing, and more.
            </p>
            <div className="flex flex-col items-center gap-4">
              <Link href="/signup">
                <Button size="lg" className="gap-2 px-8 py-6 text-lg">
                  Get Started Free
                  <ArrowRight className="h-5 w-5" />
                </Button>
              </Link>
              <div className="flex justify-center gap-4">
                <a href="https://docs.mechanicbuddy.app" target="_blank" rel="noopener noreferrer">
                  <Button variant="outline" size="lg" className="border-dark-600 text-white hover:bg-dark-800 hover:border-dark-500">
                    View Documentation
                  </Button>
                </a>
                <Link href="/pricing">
                  <Button variant="outline" size="lg" className="border-dark-600 text-white hover:bg-dark-800 hover:border-dark-500">
                    View Pricing
                  </Button>
                </Link>
              </div>
              <a
                href="https://github.com/D4m13n-d3v/mechanicbuddy"
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex items-center gap-2 text-dark-400 hover:text-white transition-colors mt-4"
              >
                <Github className="h-5 w-5" />
                <span>Open Source on GitHub</span>
              </a>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-24 bg-white">
        <div className="mx-auto max-w-7xl px-6">
          <div className="text-center mb-16">
            <h2 className="text-4xl font-bold text-dark-900 mb-4">
              Everything You Need to Run Your Workshop
            </h2>
            <p className="text-lg text-dark-500">
              Powerful features designed for vehicle service centers
            </p>
          </div>
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            {features.map((feature) => {
              const Icon = feature.icon;
              return (
                <Card key={feature.title} className="border-0 shadow-lg hover:shadow-xl transition-shadow">
                  <CardHeader>
                    <div className="flex h-14 w-14 items-center justify-center rounded-xl bg-dark-900 mb-4">
                      <Icon className="h-7 w-7 text-white" />
                    </div>
                    <CardTitle className="text-lg">{feature.title}</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <CardDescription className="text-dark-500">{feature.description}</CardDescription>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      </section>

      {/* Pricing Preview Section */}
      <section className="bg-dark-50 py-24">
        <div className="mx-auto max-w-7xl px-6">
          <div className="text-center mb-16">
            <h2 className="text-4xl font-bold text-dark-900 mb-4">
              Simple, Transparent Pricing
            </h2>
            <p className="text-lg text-dark-500">
              Choose the plan that fits your workshop
            </p>
          </div>
          <div className="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
            {pricingPreview.map((plan) => (
              <Card key={plan.name} className={plan.popular ? "border-2 border-primary-600 shadow-xl shadow-primary-600/10 scale-105" : "border-dark-200"}>
                <CardHeader className="text-center pb-2">
                  {plan.popular && (
                    <div className="inline-flex items-center justify-center rounded-full bg-primary-600 text-white text-xs font-bold px-3 py-1 mb-4">
                      MOST POPULAR
                    </div>
                  )}
                  <CardTitle className="text-xl">{plan.name}</CardTitle>
                  <div className="mt-4">
                    <span className="text-5xl font-bold text-dark-900">{plan.price}</span>
                    <span className="text-dark-500">{plan.period}</span>
                  </div>
                </CardHeader>
                <CardContent className="pt-4">
                  <p className="text-sm text-dark-500 mb-6 text-center">{plan.description}</p>
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
      <section className="py-24 bg-dark-950">
        <div className="mx-auto max-w-4xl px-6 text-center">
          <h2 className="text-4xl font-bold text-white mb-4">
            Ready to Get Started?
          </h2>
          <p className="text-lg text-dark-300 mb-10">
            Sign up for free and see how MechanicBuddy can transform your workshop
          </p>
          <div className="flex flex-col sm:flex-row justify-center gap-4">
            <Link href="/signup">
              <Button size="lg" className="gap-2 px-8">
                Get Started Free
                <ArrowRight className="h-4 w-4" />
              </Button>
            </Link>
            <Link href="/demo">
              <Button variant="outline" size="lg" className="border-dark-600 text-white hover:bg-dark-800 hover:border-dark-500">
                Request a Demo
              </Button>
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
