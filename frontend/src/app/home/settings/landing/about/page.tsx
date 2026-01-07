'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions } from "../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import FormInput from "@/_components/FormInput";
import FormTextArea from "@/_components/FormTextArea";
import { updateAbout, createAboutFeature, deleteAboutFeature } from "../../branding/actions";
import Link from "next/link";
import { PlusIcon, TrashIcon } from "@heroicons/react/24/outline";

export default async function Page() {
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const about = content.about;
    const features = about.features.sort((a, b) => a.sortOrder - b.sortOrder);

    return (
        <Main header={<SettingsTabs />} narrow={true}>
            <div className="mb-6">
                <Link
                    href="/home/settings/landing"
                    className="text-sm text-indigo-600 hover:text-indigo-500"
                >
                    ← Back to Landing Page Settings
                </Link>
            </div>

            <form action={updateAbout}>
                <div className="space-y-12">
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900 my-4">About Section</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Customize the About Us section of your landing page.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                            <div className="sm:col-span-3">
                                <FormInput
                                    name="sectionLabel"
                                    label="Section Label"
                                    defaultValue={about.sectionLabel}
                                    placeholder="e.g., About Us"
                                />
                            </div>

                            <div className="sm:col-span-6">
                                <FormInput
                                    name="headline"
                                    label="Headline"
                                    defaultValue={about.headline}
                                    placeholder="e.g., Excellence in Auto Care"
                                />
                            </div>

                            <div className="sm:col-span-6">
                                <FormTextArea
                                    name="description"
                                    label="Primary Description"
                                    rows={3}
                                    defaultValue={about.description || ''}
                                />
                            </div>

                            <div className="sm:col-span-6">
                                <FormTextArea
                                    name="secondaryDescription"
                                    label="Secondary Description"
                                    rows={3}
                                    defaultValue={about.secondaryDescription || ''}
                                />
                            </div>
                        </div>
                    </div>
                </div>

                <div className="mt-6 flex items-center justify-end gap-x-6">
                    <Link
                        href="/home/settings/landing"
                        className="text-sm font-semibold text-gray-900"
                    >
                        Cancel
                    </Link>
                    <button
                        type="submit"
                        className="rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                    >
                        Save About Section
                    </button>
                </div>
            </form>

            {/* Features Section */}
            <div className="mt-12 pt-8 border-t border-gray-200">
                <div className="flex items-center justify-between">
                    <div>
                        <h3 className="text-base/7 font-semibold text-gray-900">Features</h3>
                        <p className="mt-1 text-sm text-gray-500">
                            Checkmark features displayed in the about section.
                        </p>
                    </div>
                </div>

                <div className="mt-6 bg-white rounded-lg border border-gray-200 shadow-sm">
                    {features.length === 0 ? (
                        <div className="p-8 text-center">
                            <p className="text-sm text-gray-500">No features yet.</p>
                        </div>
                    ) : (
                        <ul className="divide-y divide-gray-100">
                            {features.map((feature) => (
                                <li key={feature.id} className="flex items-center justify-between px-4 py-3">
                                    <span className="text-sm text-gray-900">{feature.text}</span>
                                    <form action={deleteAboutFeature}>
                                        <input type="hidden" name="id" value={feature.id} />
                                        <button
                                            type="submit"
                                            className="p-2 text-gray-400 hover:text-red-600"
                                        >
                                            <TrashIcon className="h-5 w-5" />
                                        </button>
                                    </form>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>

                <form action={createAboutFeature} className="mt-4">
                    <div className="flex gap-3">
                        <input
                            type="text"
                            name="text"
                            placeholder="Enter a new feature..."
                            className="block flex-1 rounded-md bg-white px-3 py-2 text-sm text-gray-900 outline-1 -outline-offset-1 outline-gray-300 placeholder:text-gray-400 focus:outline-2 focus:-outline-offset-2 focus:outline-indigo-600"
                        />
                        <button
                            type="submit"
                            className="inline-flex items-center gap-x-1.5 rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500"
                        >
                            <PlusIcon className="h-5 w-5" />
                            Add
                        </button>
                    </div>
                </form>
            </div>
        </Main>
    );
}
