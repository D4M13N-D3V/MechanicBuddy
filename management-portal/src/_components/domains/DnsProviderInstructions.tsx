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

/**
 * Helper to extract the TXT record name for a given DNS provider
 * For a domain like "workshop.example.com", the full verification host is "_mechanicbuddy-verify.workshop.example.com"
 * In most DNS UIs, you enter the part relative to your zone root.
 *
 * Example for domain "workshop.example.com" where zone is "example.com":
 * - Full host: _mechanicbuddy-verify.workshop.example.com
 * - Enter in DNS UI: _mechanicbuddy-verify.workshop
 */
function getRecordNameForProvider(host: string, domain: string): { name: string; explanation: string } {
  // host is like "_mechanicbuddy-verify.workshop.example.com"
  // domain is like "workshop.example.com"
  // We need to extract "_mechanicbuddy-verify.workshop" (everything before the zone root)

  const parts = domain.split(".");
  // Assume zone is the last 2 parts (e.g., "example.com")
  // So for "workshop.example.com", zone is "example.com"
  const zoneParts = parts.slice(-2);
  const zone = zoneParts.join(".");
  const subdomain = parts.slice(0, -2).join(".");

  // The record name to enter is "_mechanicbuddy-verify" + subdomain if any
  const recordName = subdomain
    ? `_mechanicbuddy-verify.${subdomain}`
    : "_mechanicbuddy-verify";

  return {
    name: recordName,
    explanation: `This creates the full record: ${host}`
  };
}

const PROVIDER_INSTRUCTIONS: Record<string, (props: { domain: string; host: string }) => React.ReactNode> = {
  cloudflare: ({ domain, host }) => {
    const { name } = getRecordNameForProvider(host, domain);
    return (
      <ol className="list-decimal list-inside space-y-2 text-sm text-dark-600">
        <li>Log in to your Cloudflare dashboard</li>
        <li>Select your domain from the list</li>
        <li>Click on <span className="font-medium text-dark-900">DNS</span> in the sidebar</li>
        <li>Click <span className="font-medium text-dark-900">Add record</span></li>
        <li>Select <span className="font-medium text-dark-900">TXT</span> as the type</li>
        <li>
          In the <span className="font-medium text-dark-900">Name</span> field, enter:{" "}
          <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs font-bold">{name}</code>
        </li>
        <li>Paste the verification token in the <span className="font-medium text-dark-900">Content</span> field</li>
        <li>Set Proxy status to <span className="font-medium text-dark-900">DNS only</span> (gray cloud)</li>
        <li>Click <span className="font-medium text-dark-900">Save</span></li>
      </ol>
    );
  },
  godaddy: ({ domain, host }) => {
    const { name } = getRecordNameForProvider(host, domain);
    return (
      <ol className="list-decimal list-inside space-y-2 text-sm text-dark-600">
        <li>Log in to your GoDaddy account</li>
        <li>Go to <span className="font-medium text-dark-900">My Products</span> and click <span className="font-medium text-dark-900">DNS</span> next to your domain</li>
        <li>Click <span className="font-medium text-dark-900">Add</span> to add a new record</li>
        <li>Select <span className="font-medium text-dark-900">TXT</span> as the type</li>
        <li>
          In the <span className="font-medium text-dark-900">Host</span> field, enter:{" "}
          <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs font-bold">{name}</code>
        </li>
        <li>Paste the verification token in the <span className="font-medium text-dark-900">TXT Value</span> field</li>
        <li>Click <span className="font-medium text-dark-900">Save</span></li>
      </ol>
    );
  },
  namecheap: ({ domain, host }) => {
    const { name } = getRecordNameForProvider(host, domain);
    return (
      <ol className="list-decimal list-inside space-y-2 text-sm text-dark-600">
        <li>Log in to your Namecheap account</li>
        <li>Go to <span className="font-medium text-dark-900">Domain List</span> and click <span className="font-medium text-dark-900">Manage</span> next to your domain</li>
        <li>Click on <span className="font-medium text-dark-900">Advanced DNS</span></li>
        <li>Click <span className="font-medium text-dark-900">Add New Record</span></li>
        <li>Select <span className="font-medium text-dark-900">TXT Record</span> as the type</li>
        <li>
          In the <span className="font-medium text-dark-900">Host</span> field, enter:{" "}
          <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs font-bold">{name}</code>
        </li>
        <li>Paste the verification token in the <span className="font-medium text-dark-900">Value</span> field</li>
        <li>Click the checkmark to save</li>
      </ol>
    );
  },
  google: ({ domain, host }) => {
    const { name } = getRecordNameForProvider(host, domain);
    return (
      <ol className="list-decimal list-inside space-y-2 text-sm text-dark-600">
        <li>Log in to Google Domains (now Squarespace Domains)</li>
        <li>Select your domain</li>
        <li>Click on <span className="font-medium text-dark-900">DNS</span> in the sidebar</li>
        <li>Scroll to <span className="font-medium text-dark-900">Custom records</span></li>
        <li>Click <span className="font-medium text-dark-900">Manage custom records</span></li>
        <li>Click <span className="font-medium text-dark-900">Create new record</span></li>
        <li>
          In the <span className="font-medium text-dark-900">Host name</span> field, enter:{" "}
          <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs font-bold">{name}</code>
        </li>
        <li>Select <span className="font-medium text-dark-900">TXT</span> as the type</li>
        <li>Paste the verification token in the <span className="font-medium text-dark-900">Data</span> field</li>
        <li>Click <span className="font-medium text-dark-900">Save</span></li>
      </ol>
    );
  },
  other: ({ domain, host }) => {
    const { name } = getRecordNameForProvider(host, domain);
    return (
      <div className="space-y-3 text-sm text-dark-600">
        <p>General instructions for adding a TXT record:</p>
        <ol className="list-decimal list-inside space-y-2">
          <li>Log in to your domain registrar or DNS provider</li>
          <li>Navigate to DNS settings or DNS management</li>
          <li>Add a new TXT record</li>
          <li>
            Set the host/name to:{" "}
            <code className="bg-dark-100 px-1.5 py-0.5 rounded text-xs font-bold">{name}</code>
          </li>
          <li>Set the value to the verification token shown above</li>
          <li>Save the record</li>
        </ol>
        <p className="text-dark-500 text-xs mt-3">
          Note: This will create the DNS record at <code className="bg-dark-100 px-1 rounded">{host}</code>
        </p>
      </div>
    );
  },
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
