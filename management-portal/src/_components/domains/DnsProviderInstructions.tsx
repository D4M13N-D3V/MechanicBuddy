"use client";

import { useState } from "react";
import { cn } from "@/_lib/utils";

const DNS_PROVIDERS = [
  { name: "Cloudflare", id: "cloudflare" },
  { name: "GoDaddy", id: "godaddy" },
  { name: "Namecheap", id: "namecheap" },
  { name: "Google Domains", id: "google" },
  { name: "Other", id: "other" },
] as const;

interface DnsProviderInstructionsProps {
  domain: string;
  host: string;
  value?: string; // Token value - included for potential future use
}

const PROVIDER_INSTRUCTIONS: Record<string, (props: { domain: string; host: string }) => React.ReactNode> = {
  cloudflare: ({ host }) => (
    <ol className="list-decimal list-inside space-y-2 text-sm text-dark-600">
      <li>Log in to your Cloudflare dashboard</li>
      <li>Select your domain from the list</li>
      <li>Click on <span className="font-medium text-dark-900">DNS</span> in the sidebar</li>
      <li>Click <span className="font-medium text-dark-900">Add record</span></li>
      <li>Select <span className="font-medium text-dark-900">TXT</span> as the type</li>
      <li>
        In the <span className="font-medium text-dark-900">Name</span> field, enter:{" "}
        <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs">{host.split(".")[0]}</code>
      </li>
      <li>Paste the verification token in the <span className="font-medium text-dark-900">Content</span> field</li>
      <li>Click <span className="font-medium text-dark-900">Save</span></li>
    </ol>
  ),
  godaddy: ({ host }) => (
    <ol className="list-decimal list-inside space-y-2 text-sm text-dark-600">
      <li>Log in to your GoDaddy account</li>
      <li>Go to <span className="font-medium text-dark-900">My Products</span> and click <span className="font-medium text-dark-900">DNS</span> next to your domain</li>
      <li>Click <span className="font-medium text-dark-900">Add</span> to add a new record</li>
      <li>Select <span className="font-medium text-dark-900">TXT</span> as the type</li>
      <li>
        In the <span className="font-medium text-dark-900">Host</span> field, enter:{" "}
        <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs">{host.split(".")[0]}</code>
      </li>
      <li>Paste the verification token in the <span className="font-medium text-dark-900">TXT Value</span> field</li>
      <li>Click <span className="font-medium text-dark-900">Save</span></li>
    </ol>
  ),
  namecheap: ({ host }) => (
    <ol className="list-decimal list-inside space-y-2 text-sm text-dark-600">
      <li>Log in to your Namecheap account</li>
      <li>Go to <span className="font-medium text-dark-900">Domain List</span> and click <span className="font-medium text-dark-900">Manage</span> next to your domain</li>
      <li>Click on <span className="font-medium text-dark-900">Advanced DNS</span></li>
      <li>Click <span className="font-medium text-dark-900">Add New Record</span></li>
      <li>Select <span className="font-medium text-dark-900">TXT Record</span> as the type</li>
      <li>
        In the <span className="font-medium text-dark-900">Host</span> field, enter:{" "}
        <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs">{host.split(".")[0]}</code>
      </li>
      <li>Paste the verification token in the <span className="font-medium text-dark-900">Value</span> field</li>
      <li>Click the checkmark to save</li>
    </ol>
  ),
  google: ({ host }) => (
    <ol className="list-decimal list-inside space-y-2 text-sm text-dark-600">
      <li>Log in to Google Domains (now Squarespace Domains)</li>
      <li>Select your domain</li>
      <li>Click on <span className="font-medium text-dark-900">DNS</span> in the sidebar</li>
      <li>Scroll to <span className="font-medium text-dark-900">Custom records</span></li>
      <li>Click <span className="font-medium text-dark-900">Manage custom records</span></li>
      <li>Click <span className="font-medium text-dark-900">Create new record</span></li>
      <li>
        In the <span className="font-medium text-dark-900">Host name</span> field, enter:{" "}
        <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs">{host.split(".")[0]}</code>
      </li>
      <li>Select <span className="font-medium text-dark-900">TXT</span> as the type</li>
      <li>Paste the verification token in the <span className="font-medium text-dark-900">Data</span> field</li>
      <li>Click <span className="font-medium text-dark-900">Save</span></li>
    </ol>
  ),
  other: ({ host }) => (
    <div className="space-y-3 text-sm text-dark-600">
      <p>General instructions for adding a TXT record:</p>
      <ol className="list-decimal list-inside space-y-2">
        <li>Log in to your domain registrar or DNS provider</li>
        <li>Navigate to DNS settings or DNS management</li>
        <li>Add a new TXT record</li>
        <li>
          Set the host/name to:{" "}
          <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs">{host.split(".")[0]}</code>
        </li>
        <li>Set the value to the verification token shown above</li>
        <li>Save the record</li>
      </ol>
      <p className="text-dark-500 text-xs mt-3">
        Note: Some providers require the full hostname ({host}), while others only need
        the subdomain part ({host.split(".")[0]}). Try the shorter version first.
      </p>
    </div>
  ),
};

export function DnsProviderInstructions({ domain, host }: DnsProviderInstructionsProps) {
  const [selectedProvider, setSelectedProvider] = useState<string | null>(null);

  return (
    <div className="space-y-4">
      <div>
        <p className="text-sm font-medium text-dark-700 mb-2">
          Select your DNS provider for specific instructions:
        </p>
        <div className="flex flex-wrap gap-2">
          {DNS_PROVIDERS.map((provider) => (
            <button
              key={provider.id}
              onClick={() => setSelectedProvider(
                selectedProvider === provider.id ? null : provider.id
              )}
              className={cn(
                "px-3 py-1.5 rounded-full text-sm border transition-colors",
                selectedProvider === provider.id
                  ? "bg-primary-600 text-white border-primary-600"
                  : "bg-white text-dark-700 border-dark-200 hover:border-primary-300"
              )}
            >
              {provider.name}
            </button>
          ))}
        </div>
      </div>

      {selectedProvider && (
        <div className="p-4 bg-dark-50 rounded-lg border border-dark-200">
          <h4 className="font-medium text-dark-900 mb-3">
            {DNS_PROVIDERS.find(p => p.id === selectedProvider)?.name} Instructions
          </h4>
          {PROVIDER_INSTRUCTIONS[selectedProvider]?.({ domain, host })}
        </div>
      )}
    </div>
  );
}
