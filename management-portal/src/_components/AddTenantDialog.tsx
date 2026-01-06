"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Dialog, DialogHeader, DialogTitle, DialogDescription, DialogContent, DialogFooter } from "@/_components/ui/Dialog";
import { Button } from "@/_components/ui/Button";
import { Input } from "@/_components/ui/Input";
import { Select } from "@/_components/ui/Select";
import { createTenant, type CreateTenantData } from "@/_lib/api";

interface AddTenantDialogProps {
  open: boolean;
  onClose: () => void;
}

export function AddTenantDialog({ open, onClose }: AddTenantDialogProps) {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState<CreateTenantData>({
    companyName: "",
    ownerEmail: "",
    ownerName: "",
    tier: "free",
    isDemo: false,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const response = await createTenant(formData);

      if (response.success) {
        onClose();
        setFormData({
          companyName: "",
          ownerEmail: "",
          ownerName: "",
          tier: "free",
          isDemo: false,
        });
        router.refresh();
      } else {
        setError(response.error || "Failed to create tenant");
      }
    } catch (err) {
      setError("An unexpected error occurred");
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (field: keyof CreateTenantData, value: string | boolean) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  return (
    <Dialog open={open} onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <DialogHeader>
          <DialogTitle>Add New Tenant</DialogTitle>
          <DialogDescription>
            Create a new workshop tenant. They will receive an email with login credentials.
          </DialogDescription>
        </DialogHeader>

        <DialogContent>
          <div className="space-y-4">
            <Input
              label="Company Name"
              value={formData.companyName}
              onChange={(e) => handleChange("companyName", e.target.value)}
              placeholder="Acme Auto Repair"
              required
            />

            <Input
              label="Owner Name"
              value={formData.ownerName}
              onChange={(e) => handleChange("ownerName", e.target.value)}
              placeholder="John Smith"
              required
            />

            <Input
              label="Owner Email"
              type="email"
              value={formData.ownerEmail}
              onChange={(e) => handleChange("ownerEmail", e.target.value)}
              placeholder="john@acmeauto.com"
              required
            />

            <Select
              label="Plan"
              value={formData.tier}
              onChange={(e) => handleChange("tier", e.target.value)}
            >
              <option value="free">Free</option>
              <option value="starter">Starter</option>
              <option value="professional">Professional</option>
              <option value="enterprise">Enterprise</option>
            </Select>

            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isDemo"
                checked={formData.isDemo}
                onChange={(e) => handleChange("isDemo", e.target.checked)}
                className="h-4 w-4 rounded border-gray-300 text-primary-600 focus:ring-primary-600"
              />
              <label htmlFor="isDemo" className="text-sm text-gray-700">
                This is a demo account (14-day trial)
              </label>
            </div>

            {error && (
              <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
                {error}
              </div>
            )}
          </div>
        </DialogContent>

        <DialogFooter>
          <Button type="button" variant="outline" onClick={onClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isLoading}>
            Create Tenant
          </Button>
        </DialogFooter>
      </form>
    </Dialog>
  );
}
