import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/_components/ui/Card";
import { DemoRequestForm } from "@/_components/forms/DemoRequestForm";

export default function DemoPage() {
  return (
    <div className="py-20">
      <div className="mx-auto max-w-2xl px-6">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-gray-900 mb-4">
            Request a Demo
          </h1>
          <p className="text-lg text-gray-600">
            See MechanicBuddy in action. Fill out the form below and we&apos;ll get back to you within 24 hours.
          </p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Tell Us About Your Workshop</CardTitle>
            <CardDescription>
              We&apos;ll set up a personalized demo to show you how MechanicBuddy can help your business
            </CardDescription>
          </CardHeader>
          <CardContent>
            <DemoRequestForm />
          </CardContent>
        </Card>

        <div className="mt-8 text-center text-sm text-gray-600">
          <p>
            Already have an account?{" "}
            <a href="/login" className="text-primary-600 hover:text-primary-700 font-medium">
              Sign in
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}
