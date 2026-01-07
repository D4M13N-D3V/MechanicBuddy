'use client'

import { TrashIcon } from "@heroicons/react/24/outline";

export default function DeletePhotoButton() {
    return (
        <button
            type="submit"
            className="p-2 text-gray-400 hover:text-red-600"
            onClick={(e) => {
                if (!confirm('Are you sure you want to delete this photo?')) {
                    e.preventDefault();
                }
            }}
        >
            <TrashIcon className="h-5 w-5" />
        </button>
    );
}
