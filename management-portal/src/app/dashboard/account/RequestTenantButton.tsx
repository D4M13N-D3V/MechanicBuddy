"use client";

import { useState } from "react";
import { Button } from "@/_components/ui/Button";
import { Plus } from "lucide-react";
import { requestNewTenant } from "@/_lib/api";
import { useRouter } from "next/navigation";

export function RequestTenantButton() {
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    companyName: "",
    message: "",
  });
  const router = useRouter();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      const response = await requestNewTenant(formData);

      if (response.success) {
        setIsOpen(false);
        setFormData({ companyName: "", message: "" });
        // Refresh the page to show the new tenant
        router.refresh();
      } else {
        setError(response.error || "Failed to request tenant");
      }
    } catch {
      setError("An unexpected error occurred");
    } finally {
      setIsLoading(false);
    }
  };

  if (!isOpen) {
    return (
      <Button onClick={() => setIsOpen(true)} size="sm">
        <Plus className="h-4 w-4 mr-2" />
        Request New Tenant
      </Button>
    );
  }

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
        <h2 className="text-2xl font-bold text-dark-900 mb-4">Request New Tenant</h2>
        <p className="text-dark-600 mb-6">
          Submit a request for a new workshop tenant. Our team will review and provision it for you.
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="companyName" className="block text-sm font-medium text-dark-700 mb-2">
              Company Name *
            </label>
            <input
              id="companyName"
              type="text"
              required
              value={formData.companyName}
              onChange={(e) => setFormData({ ...formData, companyName: e.target.value })}
              className="w-full px-4 py-2 border border-dark-300 rounded-lg focus:ring-2 focus:ring-primary-600 focus:border-transparent"
              placeholder="Your Workshop Name"
            />
          </div>

          <div>
            <label htmlFor="message" className="block text-sm font-medium text-dark-700 mb-2">
              Message (Optional)
            </label>
            <textarea
              id="message"
              rows={4}
              value={formData.message}
              onChange={(e) => setFormData({ ...formData, message: e.target.value })}
              className="w-full px-4 py-2 border border-dark-300 rounded-lg focus:ring-2 focus:ring-primary-600 focus:border-transparent"
              placeholder="Any additional information or requirements..."
            />
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
              {error}
            </div>
          )}

          <div className="flex gap-3 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                setIsOpen(false);
                setError(null);
                setFormData({ companyName: "", message: "" });
              }}
              disabled={isLoading}
              className="flex-1"
            >
              Cancel
            </Button>
            <Button type="submit" isLoading={isLoading} className="flex-1">
              Submit Request
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
