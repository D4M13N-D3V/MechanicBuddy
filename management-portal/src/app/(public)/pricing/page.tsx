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
        "1 mechanic account",
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
      interval: "per mechanic/month",
      description: "For growing workshops",
      features: [
        "Unlimited mechanics",
        "Everything in Free, plus:",
        "Advanced reporting & analytics",
        "Inventory management",
        "PDF generation & customization",
        "10GB storage per mechanic",
        "Priority email support",
        "API access",
      ],
      popular: true,
      cta: "Request Demo",
      href: "/demo",
    },
    {
      name: "Premium",
      price: "$50",
      interval: "per mechanic/month",
      description: "For established service centers",
      features: [
        "Everything in Standard, plus:",
        "Custom branding",
        "Multi-location support",
        "Advanced integrations",
        "50GB storage per mechanic",
        "Phone & email support",
        "Dedicated account manager",
        "Custom training",
      ],
      cta: "Contact Sales",
      href: "/demo",
    },
    {
      name: "Enterprise",
      price: "Custom",
      interval: "pricing",
      description: "For large organizations",
      features: [
        "Everything in Premium, plus:",
        "Custom deployment options",
        "Unlimited storage",
        "SLA guarantees",
        "24/7 phone support",
        "Custom development",
        "On-premise hosting option",
      ],
      cta: "Contact Sales",
      href: "/demo",
    },
  ];

  return (
    <div className="py-20">
      <div className="mx-auto max-w-7xl px-6">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-gray-900 mb-4">
            Simple, Transparent Pricing
          </h1>
          <p className="text-lg text-gray-600 max-w-2xl mx-auto">
            Choose the plan that fits your workshop. All plans include core features
            with no hidden fees. Cancel anytime.
          </p>
        </div>

        <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {plans.map((plan) => (
            <Card
              key={plan.name}
              className={plan.popular ? "border-primary-600 border-2 relative" : ""}
            >
              {plan.popular && (
                <div className="absolute -top-4 left-1/2 -translate-x-1/2">
                  <div className="bg-primary-600 text-white text-xs font-semibold px-3 py-1 rounded-full">
                    MOST POPULAR
                  </div>
                </div>
              )}
              <CardHeader>
                <CardTitle className="text-2xl">{plan.name}</CardTitle>
                <div className="mt-4 mb-2">
                  <span className="text-4xl font-bold text-gray-900">{plan.price}</span>
                  {plan.name !== "Enterprise" && plan.name !== "Free" && (
                    <span className="text-gray-600 text-sm ml-1">/mo</span>
                  )}
                </div>
                <p className="text-sm text-gray-600">{plan.interval}</p>
                <p className="text-sm text-gray-700 mt-4">{plan.description}</p>
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
                      <span className="text-sm text-gray-700">{feature}</span>
                    </li>
                  ))}
                </ul>
              </CardContent>
            </Card>
          ))}
        </div>

        {/* FAQ Section */}
        <div className="mt-20 max-w-3xl mx-auto">
          <h2 className="text-3xl font-bold text-gray-900 mb-8 text-center">
            Frequently Asked Questions
          </h2>
          <div className="space-y-6">
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Can I switch plans later?
              </h3>
              <p className="text-gray-600">
                Yes! You can upgrade or downgrade your plan at any time. Changes will be
                reflected in your next billing cycle.
              </p>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Is there a free trial?
              </h3>
              <p className="text-gray-600">
                All paid plans come with a 14-day free trial. No credit card required.
              </p>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                What payment methods do you accept?
              </h3>
              <p className="text-gray-600">
                We accept all major credit cards (Visa, MasterCard, American Express) and
                ACH transfers for annual plans.
              </p>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Is my data secure?
              </h3>
              <p className="text-gray-600">
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
