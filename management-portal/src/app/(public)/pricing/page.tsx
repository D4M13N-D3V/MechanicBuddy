import Link from "next/link";
import { Button } from "@/_components/ui/Button";
import { Card, CardContent, CardHeader, CardTitle } from "@/_components/ui/Card";
import { CheckCircle2 } from "lucide-react";

export default function PricingPage() {
  const plans = [
    {
      name: "Free",
      price: "$0",
      interval: "forever",
      description: "Perfect for solo mechanics getting started",
      features: [
        "1 user account",
        "Unlimited clients & vehicles",
        "Work order management",
        "Basic invoicing",
        "1GB storage",
        "Email support",
      ],
      cta: "Get Started Free",
      href: "/demo",
    },
    {
      name: "Standard",
      price: "$20",
      interval: "per user/month",
      description: "For growing workshops with 2-9 users",
      features: [
        "2-9 user accounts",
        "Everything in Free, plus:",
        "Advanced reporting & analytics",
        "Inventory management",
        "PDF generation & customization",
        "10GB storage per user",
        "Priority email support",
        "API access",
      ],
      popular: true,
      cta: "Request Demo",
      href: "/demo",
    },
    {
      name: "Volume",
      price: "$10",
      interval: "per user/month",
      description: "Best value for teams of 10 or more",
      features: [
        "10+ user accounts",
        "Everything in Standard, plus:",
        "Volume discount pricing",
        "Custom branding",
        "Multi-location support",
        "20GB storage per user",
        "Phone & email support",
        "Dedicated account manager",
      ],
      cta: "Request Demo",
      href: "/demo",
    },
  ];

  return (
    <div className="py-20">
      <div className="mx-auto max-w-7xl px-6">
        <div className="text-center mb-16">
          <h1 className="text-4xl font-bold text-dark-900 mb-4">
            Simple, Transparent Pricing
          </h1>
          <p className="text-lg text-dark-500 max-w-2xl mx-auto">
            Choose the plan that fits your workshop. All plans include core features
            with no hidden fees. Cancel anytime.
          </p>
        </div>

        <div className="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto mb-12">
          {plans.map((plan) => (
            <Card
              key={plan.name}
              className={plan.popular ? "border-primary-600 border-2 relative shadow-xl shadow-primary-600/10 scale-105" : "border-dark-200"}
            >
              {plan.popular && (
                <div className="absolute -top-4 left-1/2 -translate-x-1/2">
                  <div className="bg-primary-600 text-white text-xs font-bold px-4 py-1.5 rounded-full">
                    MOST POPULAR
                  </div>
                </div>
              )}
              <CardHeader className="pt-8">
                <CardTitle className="text-2xl">{plan.name}</CardTitle>
                <div className="mt-4 mb-2">
                  <span className="text-5xl font-bold text-dark-900">{plan.price}</span>
                  {plan.name !== "Free" && (
                    <span className="text-dark-500 text-sm ml-1">/mo</span>
                  )}
                </div>
                <p className="text-sm text-dark-500">{plan.interval}</p>
                <p className="text-sm text-dark-600 mt-4">{plan.description}</p>
              </CardHeader>
              <CardContent>
                <Link href={plan.href}>
                  <Button
                    variant={plan.popular ? "primary" : "outline"}
                    className="w-full mb-6"
                  >
                    {plan.cta}
                  </Button>
                </Link>
                <ul className="space-y-3">
                  {plan.features.map((feature) => (
                    <li key={feature} className="flex items-start gap-2">
                      <CheckCircle2 className="h-5 w-5 text-primary-600 flex-shrink-0 mt-0.5" />
                      <span className="text-sm text-dark-700">{feature}</span>
                    </li>
                  ))}
                </ul>
              </CardContent>
            </Card>
          ))}
        </div>

        {/* FAQ Section */}
        <div className="mt-20 max-w-3xl mx-auto">
          <h2 className="text-3xl font-bold text-dark-900 mb-8 text-center">
            Frequently Asked Questions
          </h2>
          <div className="space-y-6">
            <div>
              <h3 className="text-lg font-semibold text-dark-900 mb-2">
                Can I switch plans later?
              </h3>
              <p className="text-dark-500">
                Yes! You can upgrade or downgrade your plan at any time. Changes will be
                reflected in your next billing cycle.
              </p>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-dark-900 mb-2">
                How does the volume discount work?
              </h3>
              <p className="text-dark-500">
                Once you have 10 or more users, all users are billed at $10/month instead of $20/month.
                The discount applies automatically.
              </p>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-dark-900 mb-2">
                What payment methods do you accept?
              </h3>
              <p className="text-dark-500">
                We accept all major credit cards (Visa, MasterCard, American Express) and
                ACH transfers for annual plans.
              </p>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-dark-900 mb-2">
                Is my data secure?
              </h3>
              <p className="text-dark-500">
                Absolutely. We use industry-standard encryption and security practices.
                Your data is backed up daily and stored securely.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
