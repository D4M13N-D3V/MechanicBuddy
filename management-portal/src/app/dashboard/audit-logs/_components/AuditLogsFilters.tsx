'use client'

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { Search } from 'lucide-react';

interface Props {
  searchParams: Record<string, string>;
}

export default function AuditLogsFilters({ searchParams }: Props) {
  const router = useRouter();
  const [searchText, setSearchText] = useState(searchParams.searchText || '');
  const [actionType, setActionType] = useState(searchParams.actionType || '');
  const [tenantId, setTenantId] = useState(searchParams.tenantId || '');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const params = new URLSearchParams();
    if (searchText) params.set('searchText', searchText);
    if (actionType) params.set('actionType', actionType);
    if (tenantId) params.set('tenantId', tenantId);
    params.set('offset', '0');
    router.push(`/dashboard/audit-logs?${params.toString()}`);
  };

  const handleClear = () => {
    setSearchText('');
    setActionType('');
    setTenantId('');
    router.push('/dashboard/audit-logs');
  };

  const hasFilters = searchText || actionType || tenantId;

  return (
    <form onSubmit={handleSubmit} className="mb-6">
      <div className="flex flex-wrap gap-4 items-end">
        <div className="flex-1 min-w-[200px]">
          <label htmlFor="searchText" className="block text-sm font-medium text-dark-600 mb-1">
            Search
          </label>
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-dark-400" />
            <input
              type="text"
              id="searchText"
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              placeholder="Search by admin, endpoint, or description..."
              className="block w-full pl-10 pr-3 py-2 bg-dark-50 border border-dark-200 rounded-lg text-dark-900 placeholder-dark-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
            />
          </div>
        </div>
        <div className="min-w-[150px]">
          <label htmlFor="actionType" className="block text-sm font-medium text-dark-600 mb-1">
            Action Type
          </label>
          <select
            id="actionType"
            value={actionType}
            onChange={(e) => setActionType(e.target.value)}
            className="block w-full px-3 py-2 bg-dark-50 border border-dark-200 rounded-lg text-dark-900 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
          >
            <option value="">All Types</option>
            <option value="api_request">API Request</option>
            <option value="crud">CRUD</option>
            <option value="auth">Auth</option>
            <option value="admin">Admin</option>
          </select>
        </div>
        <div className="min-w-[150px]">
          <label htmlFor="tenantId" className="block text-sm font-medium text-dark-600 mb-1">
            Tenant ID
          </label>
          <input
            type="text"
            id="tenantId"
            value={tenantId}
            onChange={(e) => setTenantId(e.target.value)}
            placeholder="Filter by tenant..."
            className="block w-full px-3 py-2 bg-dark-50 border border-dark-200 rounded-lg text-dark-900 placeholder-dark-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
          />
        </div>
        <div className="flex gap-2">
          <button
            type="submit"
            className="px-4 py-2 bg-primary-600 text-white text-sm font-medium rounded-lg hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
          >
            Filter
          </button>
          {hasFilters && (
            <button
              type="button"
              onClick={handleClear}
              className="px-4 py-2 bg-dark-100 text-dark-700 text-sm font-medium rounded-lg hover:bg-dark-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-dark-500 transition-colors"
            >
              Clear
            </button>
          )}
        </div>
      </div>
    </form>
  );
}
