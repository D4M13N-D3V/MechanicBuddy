'use client'

import { formatDistanceToNow } from 'date-fns';
import clsx from 'clsx';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';

interface AuditLog {
  id: string;
  userName: string;
  actionType: string;
  httpMethod: string;
  endpoint: string;
  resourceType: string | null;
  actionDescription: string;
  timestamp: string;
  statusCode: number;
  wasSuccessful: boolean;
  durationMs: number | null;
  ipAddress: string;
}

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
    return `/home/settings/audit-logs?${params.toString()}`;
  };

  if (logs.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow p-8 text-center">
        <p className="text-gray-500">No audit logs found matching your criteria.</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Time</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Endpoint</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Duration</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">IP</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {logs.map((log) => (
              <tr key={log.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-500">
                  <span title={new Date(log.timestamp).toLocaleString()}>
                    {formatDistanceToNow(new Date(log.timestamp), { addSuffix: true })}
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm font-medium text-gray-900">
                  {log.userName}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm">
                  <ActionTypeBadge type={log.actionType} />
                </td>
                <td className="px-4 py-3 text-sm text-gray-500 max-w-xs truncate" title={log.actionDescription}>
                  {log.actionDescription}
                </td>
                <td className="px-4 py-3 text-sm text-gray-500 font-mono max-w-xs truncate" title={`${log.httpMethod} ${log.endpoint}`}>
                  <span className="font-semibold text-gray-700">{log.httpMethod}</span>{' '}
                  <span className="text-gray-500">{log.endpoint}</span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap">
                  <StatusBadge statusCode={log.statusCode} wasSuccessful={log.wasSuccessful} />
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-500">
                  {log.durationMs !== null ? `${log.durationMs}ms` : '-'}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-500 font-mono">
                  {log.ipAddress}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200">
        <div className="text-sm text-gray-700">
          Showing <span className="font-medium">{currentOffset + 1}</span> to{' '}
          <span className="font-medium">{Math.min(currentOffset + limit, total)}</span> of{' '}
          <span className="font-medium">{total}</span> results
        </div>
        <div className="flex gap-2">
          {currentOffset > 0 && (
            <Link
              href={buildPaginationUrl(Math.max(0, currentOffset - limit))}
              className="px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Previous
            </Link>
          )}
          {hasMore && (
            <Link
              href={buildPaginationUrl(currentOffset + limit)}
              className="px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
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
  const styles = {
    auth: 'bg-purple-100 text-purple-800',
    crud: 'bg-blue-100 text-blue-800',
    admin: 'bg-orange-100 text-orange-800',
    api_request: 'bg-gray-100 text-gray-800',
  };

  const labels = {
    auth: 'Auth',
    crud: 'CRUD',
    admin: 'Admin',
    api_request: 'API',
  };

  return (
    <span className={clsx(
      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
      styles[type as keyof typeof styles] || styles.api_request
    )}>
      {labels[type as keyof typeof labels] || type}
    </span>
  );
}

function StatusBadge({ statusCode, wasSuccessful }: { statusCode: number; wasSuccessful: boolean }) {
  return (
    <span className={clsx(
      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
      wasSuccessful ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
    )}>
      {statusCode}
    </span>
  );
}
