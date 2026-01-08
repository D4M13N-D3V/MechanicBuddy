'use client'

import { useRouter } from 'next/navigation';
import { useState } from 'react';

interface Props {
  searchParams: Record<string, string>;
}

export default function AuditLogsFilters({ searchParams }: Props) {
  const router = useRouter();
  const [searchText, setSearchText] = useState(searchParams.searchText || '');
  const [actionType, setActionType] = useState(searchParams.actionType || '');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const params = new URLSearchParams();
    if (searchText) params.set('searchText', searchText);
    if (actionType) params.set('actionType', actionType);
    params.set('offset', '0'); // Reset to first page on filter change
    router.push(`/home/settings/audit-logs?${params.toString()}`);
  };

  const handleClear = () => {
    setSearchText('');
    setActionType('');
    router.push('/home/settings/audit-logs');
  };

  return (
    <form onSubmit={handleSubmit} className="mb-6 bg-white rounded-lg shadow p-4 border border-gray-200">
      <div className="flex flex-wrap gap-4 items-end">
        <div className="flex-1 min-w-[200px]">
          <label htmlFor="searchText" className="block text-sm font-medium text-gray-700 mb-1">
            Search
          </label>
          <input
            type="text"
            id="searchText"
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            placeholder="Search by user, endpoint, or description..."
            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
          />
        </div>
        <div className="min-w-[150px]">
          <label htmlFor="actionType" className="block text-sm font-medium text-gray-700 mb-1">
            Action Type
          </label>
          <select
            id="actionType"
            value={actionType}
            onChange={(e) => setActionType(e.target.value)}
            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
          >
            <option value="">All Types</option>
            <option value="api_request">API Request</option>
            <option value="crud">CRUD</option>
            <option value="auth">Auth</option>
            <option value="admin">Admin</option>
          </select>
        </div>
        <div className="flex gap-2">
          <button
            type="submit"
            className="px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
          >
            Filter
          </button>
          {(searchText || actionType) && (
            <button
              type="button"
              onClick={handleClear}
              className="px-4 py-2 bg-gray-100 text-gray-700 text-sm font-medium rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
            >
              Clear
            </button>
          )}
        </div>
      </div>
    </form>
  );
}
