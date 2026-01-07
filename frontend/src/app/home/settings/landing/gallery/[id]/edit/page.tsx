'use server'

import { httpGet } from "@/_lib/server/query-api";
import { IGalleryPhotoItem } from "../../../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../../../_components/Main";
import Link from "next/link";
import { updateGalleryPhoto } from "../../../../branding/actions";
import FormSwitch from "@/_components/FormSwitch";
import { notFound } from "next/navigation";

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params;
    const data = await httpGet(`branding/gallery-photos`);
    const photos = await data.json() as IGalleryPhotoItem[];
    const photo = photos.find(p => p.id === id);

    if (!photo) {
        notFound();
    }

    const imageUrl = `/api/branding/gallery-photos/${id}/image`;

    return (
        <Main header={<SettingsTabs />} narrow={true}>
            <div className="mb-6">
                <Link
                    href="/home/settings/landing/gallery"
                    className="text-sm text-indigo-600 hover:text-indigo-500"
                >
                    &larr; Back to Gallery
                </Link>
            </div>

            <div className="mb-6">
                <h2 className="text-base/7 font-semibold text-gray-900">Edit Photo</h2>
                <p className="mt-1 text-sm text-gray-500">
                    Update this photo&apos;s caption and settings.
                </p>
            </div>

            <form action={updateGalleryPhoto} className="bg-white rounded-lg border border-gray-200 shadow-sm p-6">
                <input type="hidden" name="id" value={id} />

                <div className="space-y-6">
                    {/* Current Photo Preview */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Current Photo
                        </label>
                        <div className="h-48 w-full max-w-md rounded-lg overflow-hidden bg-gray-100">
                            <img
                                src={imageUrl}
                                alt={photo.caption || 'Gallery photo'}
                                className="h-full w-full object-contain"
                            />
                        </div>
                    </div>

                    <div>
                        <label htmlFor="image" className="block text-sm font-medium text-gray-700">
                            Replace Photo (optional)
                        </label>
                        <input
                            type="file"
                            id="image"
                            name="image"
                            accept="image/*"
                            className="mt-1 block w-full text-sm text-gray-500
                                file:mr-4 file:py-2 file:px-4
                                file:rounded-md file:border-0
                                file:text-sm file:font-semibold
                                file:bg-indigo-50 file:text-indigo-700
                                hover:file:bg-indigo-100"
                        />
                        <p className="mt-1 text-xs text-gray-500">
                            Leave empty to keep the current photo
                        </p>
                    </div>

                    <div>
                        <label htmlFor="caption" className="block text-sm font-medium text-gray-700">
                            Caption
                        </label>
                        <input
                            type="text"
                            id="caption"
                            name="caption"
                            defaultValue={photo.caption || ''}
                            placeholder="Describe this photo..."
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                        />
                    </div>

                    <div className="flex items-center justify-between">
                        <div>
                            <label className="text-sm font-medium text-gray-700">Active</label>
                            <p className="text-sm text-gray-500">Show this photo on the landing page</p>
                        </div>
                        <FormSwitch name="isActive" defaultChecked={photo.isActive} />
                    </div>
                </div>

                <div className="mt-6 flex justify-end gap-3">
                    <Link
                        href="/home/settings/landing/gallery"
                        className="rounded-md bg-white px-4 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50"
                    >
                        Cancel
                    </Link>
                    <button
                        type="submit"
                        className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                    >
                        Save Changes
                    </button>
                </div>
            </form>
        </Main>
    );
}
