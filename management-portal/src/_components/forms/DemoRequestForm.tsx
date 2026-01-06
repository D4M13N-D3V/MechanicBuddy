"use client";

import { useState, FormEvent } from "react";
import { Input } from "@/_components/ui/Input";
import { Textarea } from "@/_components/ui/Textarea";
import { Button } from "@/_components/ui/Button";
import { createDemoRequest } from "@/_lib/api";
import { isValidEmail } from "@/_lib/utils";

export function DemoRequestForm() {
  const [formData, setFormData] = useState({
    email: "",
    companyName: "",
    phoneNumber: "",
    message: "",
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  const validate = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.email) {
      newErrors.email = "Email is required";
    } else if (!isValidEmail(formData.email)) {
      newErrors.email = "Invalid email format";
    }

    if (!formData.companyName) {
      newErrors.companyName = "Company name is required";
    }

    if (!formData.message) {
      newErrors.message = "Message is required";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    setIsSubmitting(true);

    try {
      const result = await createDemoRequest(formData);

      if (result.success) {
        setIsSuccess(true);
        setFormData({ email: "", companyName: "", phoneNumber: "", message: "" });
        setTimeout(() => setIsSuccess(false), 5000);
      } else {
        setErrors({ submit: result.error || "Failed to submit request" });
      }
    } catch (error) {
      setErrors({ submit: "An unexpected error occurred" });
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isSuccess) {
    return (
      <div className="rounded-lg bg-green-50 p-6 text-center">
        <div className="text-green-600 mb-2">
          <svg className="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h3 className="text-lg font-semibold text-green-900">Request Submitted!</h3>
        <p className="mt-2 text-green-700">
          Thank you for your interest. We&apos;ll get back to you within 24 hours.
        </p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <Input
        label="Email Address"
        type="email"
        placeholder="you@company.com"
        value={formData.email}
        onChange={(e) => setFormData({ ...formData, email: e.target.value })}
        error={errors.email}
        required
      />

      <Input
        label="Company Name"
        type="text"
        placeholder="Your Company"
        value={formData.companyName}
        onChange={(e) => setFormData({ ...formData, companyName: e.target.value })}
        error={errors.companyName}
        required
      />

      <Input
        label="Phone Number (Optional)"
        type="tel"
        placeholder="+1 (555) 123-4567"
        value={formData.phoneNumber}
        onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
        error={errors.phoneNumber}
      />

      <Textarea
        label="Message"
        placeholder="Tell us about your workshop and what you're looking for..."
        rows={4}
        value={formData.message}
        onChange={(e) => setFormData({ ...formData, message: e.target.value })}
        error={errors.message}
        required
      />

      {errors.submit && (
        <div className="rounded-lg bg-red-50 p-4 text-sm text-red-600">
          {errors.submit}
        </div>
      )}

      <Button type="submit" className="w-full" isLoading={isSubmitting}>
        Request Demo
      </Button>
    </form>
  );
}
