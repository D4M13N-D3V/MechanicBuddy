'use client'

import { formatDistanceToNow } from 'date-fns';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { cn } from '@/_lib/utils';
import type { AuditLog } from '@/_lib/api';

interface Props {
  logs: AuditLog[];
  total: number;
  hasMore: boolean;
  currentOffset: number;
  limit: number;
}

export default function AuditLogsTable({ logs, total, hasMore, currentOffset, limit }: Props) {
  const searchParams = useSearchParams();

  const buildPaginationUrl = (newOffset: number) => {
    const params = new URLSearchParams(searchParams.toString());
    params.set('offset', newOffset.toString());
    params.set('limit', limit.toString());
    return `/dashboard/audit-logs?${params.toString()}`;
  };

  if (logs.length === 0) {
    return (
      <div className="py-12 text-center">
        <p className="text-dark-500">No audit logs found matching your criteria.</p>
      </div>
    );
  }

  return (
    <div>
      <div className="overflow-x-auto">
        <table className="min-w-full">
          <thead>
            <tr className="border-b border-dark-200">
              <th className="px-4 py-3 text-left text-xs font-medium text-dark-500 uppercase tracking-wider">Time</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-dark-500 uppercase tracking-wider">Admin</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-dark-500 uppercase tracking-wider">Type</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-dark-500 uppercase tracking-wider">Action</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-dark-500 uppercase tracking-wider">Endpoint</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-dark-500 uppercase tracking-wider">Tenant</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-dark-500 uppercase tracking-wider">Status</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-dark-500 uppercase tracking-wider">Duration</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-dark-100">
            {logs.map((log) => (
              <tr key={log.id} className="hover:bg-dark-50 transition-colors">
                <td className="px-4 py-3 whitespace-nowrap text-sm text-dark-500">
                  <span title={new Date(log.timestamp).toLocaleString()}>
                    {formatDistanceToNow(new Date(log.timestamp), { addSuffix: true })}
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap">
                  <div className="text-sm font-medium text-dark-900">{log.adminEmail}</div>
                  {log.adminRole && (
                    <div className="text-xs text-dark-500">{log.adminRole}</div>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap">
                  <ActionTypeBadge type={log.actionType} />
                </td>
                <td className="px-4 py-3 text-sm text-dark-600 max-w-xs truncate" title={log.actionDescription || ''}>
                  {log.actionDescription || '-'}
                </td>
                <td className="px-4 py-3 text-sm font-mono max-w-xs truncate" title={`${log.httpMethod} ${log.endpoint}`}>
                  <span className="font-semibold text-dark-700">{log.httpMethod}</span>{' '}
                  <span className="text-dark-500">{log.endpoint}</span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm text-dark-500 font-mono">
                  {log.tenantId || '-'}
                </td>
                <td className="px-4 py-3 whitespace-nowrap">
                  <StatusBadge statusCode={log.statusCode} wasSuccessful={log.wasSuccessful} />
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm text-dark-500">
                  {log.durationMs !== null ? `${log.durationMs}ms` : '-'}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="mt-4 flex items-center justify-between border-t border-dark-200 pt-4">
        <div className="text-sm text-dark-600">
          Showing <span className="font-medium">{currentOffset + 1}</span> to{' '}
          <span className="font-medium">{Math.min(currentOffset + limit, total)}</span> of{' '}
          <span className="font-medium">{total}</span> results
        </div>
        <div className="flex gap-2">
          {currentOffset > 0 && (
            <Link
              href={buildPaginationUrl(Math.max(0, currentOffset - limit))}
              className="px-3 py-2 border border-dark-300 rounded-lg text-sm font-medium text-dark-700 hover:bg-dark-50 transition-colors"
            >
              Previous
            </Link>
          )}
          {hasMore && (
            <Link
              href={buildPaginationUrl(currentOffset + limit)}
              className="px-3 py-2 border border-dark-300 rounded-lg text-sm font-medium text-dark-700 hover:bg-dark-50 transition-colors"
            >
              Next
            </Link>
          )}
        </div>
      </div>
    </div>
  );
}

function ActionTypeBadge({ type }: { type: string }) {
  const styles: Record<string, string> = {
    auth: 'bg-purple-100 text-purple-800',
    crud: 'bg-blue-100 text-blue-800',
    admin: 'bg-orange-100 text-orange-800',
    api_request: 'bg-dark-100 text-dark-800',
  };

  const labels: Record<string, string> = {
    auth: 'Auth',
    crud: 'CRUD',
    admin: 'Admin',
    api_request: 'API',
  };

  return (
    <span className={cn(
      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
      styles[type] || styles.api_request
    )}>
      {labels[type] || type}
    </span>
  );
}

function StatusBadge({ statusCode, wasSuccessful }: { statusCode: number; wasSuccessful: boolean }) {
  return (
    <span className={cn(
      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
      wasSuccessful ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
    )}>
      {statusCode}
    </span>
  );
}
