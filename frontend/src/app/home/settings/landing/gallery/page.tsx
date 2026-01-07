'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions, IGalleryPhotoItem } from "../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import Link from "next/link";
import { PlusIcon, PencilIcon, ArrowUpIcon, ArrowDownIcon } from "@heroicons/react/24/outline";
import { deleteGalleryPhoto, reorderGalleryPhotos, updateGallerySection } from "../../branding/actions";
import DeletePhotoButton from "./_components/DeletePhotoButton";

function PhotoRow({ photo, index, total }: { photo: IGalleryPhotoItem; index: number; total: number }) {
    const imageUrl = photo.id ? `/api/branding/gallery-photos/${photo.id}/image` : null;

    return (
        <div className="flex items-center justify-between py-4 border-b border-gray-100 last:border-0">
            <div className="flex items-center gap-4">
                <div className="flex flex-col gap-1">
                    <form action={reorderGalleryPhotos}>
                        <input type="hidden" name="order" value={JSON.stringify(
                            index > 0 ? { id: photo.id, direction: 'up' } : null
                        )} />
                        <button
                            type="submit"
                            disabled={index === 0}
                            className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30 disabled:cursor-not-allowed"
                        >
                            <ArrowUpIcon className="h-4 w-4" />
                        </button>
                    </form>
                    <form action={reorderGalleryPhotos}>
                        <input type="hidden" name="order" value={JSON.stringify(
                            index < total - 1 ? { id: photo.id, direction: 'down' } : null
                        )} />
                        <button
                            type="submit"
                            disabled={index === total - 1}
                            className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30 disabled:cursor-not-allowed"
                        >
                            <ArrowDownIcon className="h-4 w-4" />
                        </button>
                    </form>
                </div>
                {imageUrl && (
                    <div className="h-16 w-24 rounded-md overflow-hidden bg-gray-100">
                        <img
                            src={imageUrl}
                            alt={photo.caption || 'Gallery photo'}
                            className="h-full w-full object-cover"
                        />
                    </div>
                )}
                <div>
                    <div className="flex items-center gap-2">
                        <h4 className="text-sm font-medium text-gray-900">
                            {photo.caption || 'No caption'}
                        </h4>
                        {!photo.isActive && (
                            <span className="inline-flex items-center rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
                                Inactive
                            </span>
                        )}
                    </div>
                </div>
            </div>
            <div className="flex items-center gap-2">
                <Link
                    href={`/home/settings/landing/gallery/${photo.id}/edit`}
                    className="p-2 text-gray-400 hover:text-indigo-600"
                >
                    <PencilIcon className="h-5 w-5" />
                </Link>
                <form action={deleteGalleryPhoto}>
                    <input type="hidden" name="id" value={photo.id} />
                    <DeletePhotoButton />
                </form>
            </div>
        </div>
    );
}

export default async function Page() {
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const photos = (content.galleryPhotos || []).sort((a, b) => a.sortOrder - b.sortOrder);
    const gallerySection = content.gallerySection;

    return (
        <Main header={<SettingsTabs />} narrow={true}>
            <div className="mb-6">
                <Link
                    href="/home/settings/landing"
                    className="text-sm text-indigo-600 hover:text-indigo-500"
                >
                    &larr; Back to Landing Page Settings
                </Link>
            </div>

            {/* Gallery Section Settings */}
            <div className="mb-8">
                <h2 className="text-base/7 font-semibold text-gray-900">Gallery Section Settings</h2>
                <p className="mt-1 text-sm text-gray-500">
                    Configure the gallery section header.
                </p>
                <form action={updateGallerySection} className="mt-4 bg-white rounded-lg border border-gray-200 shadow-sm p-4">
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                        <div>
                            <label htmlFor="sectionLabel" className="block text-sm font-medium text-gray-700">
                                Section Label
                            </label>
                            <input
                                type="text"
                                id="sectionLabel"
                                name="sectionLabel"
                                defaultValue={gallerySection?.sectionLabel || 'Our Work'}
                                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                            />
                        </div>
                        <div>
                            <label htmlFor="headline" className="block text-sm font-medium text-gray-700">
                                Headline
                            </label>
                            <input
                                type="text"
                                id="headline"
                                name="headline"
                                defaultValue={gallerySection?.headline || 'Photo Gallery'}
                                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                            />
                        </div>
                    </div>
                    <div className="mt-4">
                        <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                            Description
                        </label>
                        <textarea
                            id="description"
                            name="description"
                            rows={2}
                            defaultValue={gallerySection?.description || ''}
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                        />
                    </div>
                    <div className="mt-4 flex justify-end">
                        <button
                            type="submit"
                            className="rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500"
                        >
                            Save Section Settings
                        </button>
                    </div>
                </form>
            </div>

            {/* Photos List */}
            <div className="flex items-center justify-between my-4">
                <div>
                    <h2 className="text-base/7 font-semibold text-gray-900">Photos</h2>
                    <p className="mt-1 text-sm text-gray-500">
                        Manage the photos displayed in your gallery.
                    </p>
                </div>
                <Link
                    href="/home/settings/landing/gallery/new"
                    className="inline-flex items-center gap-x-1.5 rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                >
                    <PlusIcon className="h-5 w-5" />
                    Add Photo
                </Link>
            </div>

            <div className="mt-6 bg-white rounded-lg border border-gray-200 shadow-sm">
                {photos.length === 0 ? (
                    <div className="p-8 text-center">
                        <p className="text-sm text-gray-500">No photos yet. Add your first photo to get started.</p>
                    </div>
                ) : (
                    <div className="divide-y divide-gray-100 px-4">
                        {photos.map((photo, index) => (
                            <PhotoRow
                                key={photo.id}
                                photo={photo}
                                index={index}
                                total={photos.length}
                            />
                        ))}
                    </div>
                )}
            </div>
        </Main>
    );
}
