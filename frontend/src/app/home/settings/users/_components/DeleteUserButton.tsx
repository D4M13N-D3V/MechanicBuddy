'use client'

import { TrashIcon } from "@heroicons/react/24/outline";

export default function DeleteUserButton() {
    return (
        <button
            type="submit"
            className="inline-flex items-center gap-x-1.5 rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 ring-1 shadow-xs ring-gray-300 ring-inset hover:bg-red-50 hover:text-red-600"
            onClick={(e) => {
                if (!confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
                    e.preventDefault();
                }
            }}
        >
            <TrashIcon className="h-5 w-5" />
            Delete
        </button>
    );
}
