'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions } from "../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import FormInput from "@/_components/FormInput";
import FormTextArea from "@/_components/FormTextArea";
import FormSwitch from "@/_components/FormSwitch";
import FormLabel from "@/_components/FormLabel";
import Link from "next/link";
import { PlusIcon, PencilIcon, TrashIcon, ArrowUpIcon, ArrowDownIcon } from "@heroicons/react/24/outline";
import { updateTipsSection, deleteTip, reorderTips } from "../../branding/actions";

export default async function Page() {
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const tipsSection = content.tipsSection;
    const tips = content.tips.sort((a, b) => a.sortOrder - b.sortOrder);

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

            {/* Tips Section Settings */}
            <form action={updateTipsSection}>
                <div className="space-y-12">
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900 my-4">Tips Section Settings</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Configure the tips section of your landing page.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                            <div className="sm:col-span-3">
                                <FormLabel name="isVisible" label="Show Tips Section" />
                                <div className="mt-3">
                                    <FormSwitch name="isVisible" defaultChecked={tipsSection.isVisible} />
                                </div>
                                <p className="mt-1 text-xs text-gray-500">
                                    Toggle visibility of the entire tips section
                                </p>
                            </div>

                            <div className="sm:col-span-3">
                                <FormInput
                                    name="sectionLabel"
                                    label="Section Label"
                                    defaultValue={tipsSection.sectionLabel}
                                    placeholder="e.g., Expert Advice"
                                />
                            </div>

                            <div className="sm:col-span-4">
                                <FormInput
                                    name="headline"
                                    label="Headline"
                                    defaultValue={tipsSection.headline}
                                    placeholder="e.g., Auto Care Tips"
                                />
                            </div>

                            <div className="sm:col-span-6">
                                <FormTextArea
                                    name="description"
                                    label="Description"
                                    rows={2}
                                    defaultValue={tipsSection.description || ''}
                                />
                            </div>
                        </div>
                    </div>
                </div>

                <div className="mt-6 flex items-center justify-end gap-x-6">
                    <button
                        type="submit"
                        className="rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                    >
                        Save Section Settings
                    </button>
                </div>
            </form>

            {/* Tips List */}
            <div className="mt-12 pt-8 border-t border-gray-200">
                <div className="flex items-center justify-between">
                    <div>
                        <h3 className="text-base/7 font-semibold text-gray-900">Tips</h3>
                        <p className="mt-1 text-sm text-gray-500">
                            Manage individual tips displayed in this section.
                        </p>
                    </div>
                    <Link
                        href="/home/settings/landing/tips/new"
                        className="inline-flex items-center gap-x-1.5 rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                    >
                        <PlusIcon className="h-5 w-5" />
                        Add Tip
                    </Link>
                </div>

                <div className="mt-6 bg-white rounded-lg border border-gray-200 shadow-sm">
                    {tips.length === 0 ? (
                        <div className="p-8 text-center">
                            <p className="text-sm text-gray-500">No tips yet. Add your first tip to get started.</p>
                        </div>
                    ) : (
                        <div className="divide-y divide-gray-100">
                            {tips.map((tip, index) => (
                                <div key={tip.id} className="flex items-center justify-between px-4 py-4">
                                    <div className="flex items-center gap-4">
                                        <div className="flex flex-col gap-1">
                                            <form action={reorderTips}>
                                                <input type="hidden" name="order" value={JSON.stringify(
                                                    index > 0 ? { id: tip.id, direction: 'up' } : null
                                                )} />
                                                <button
                                                    type="submit"
                                                    disabled={index === 0}
                                                    className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30 disabled:cursor-not-allowed"
                                                >
                                                    <ArrowUpIcon className="h-4 w-4" />
                                                </button>
                                            </form>
                                            <form action={reorderTips}>
                                                <input type="hidden" name="order" value={JSON.stringify(
                                                    index < tips.length - 1 ? { id: tip.id, direction: 'down' } : null
                                                )} />
                                                <button
                                                    type="submit"
                                                    disabled={index === tips.length - 1}
                                                    className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30 disabled:cursor-not-allowed"
                                                >
                                                    <ArrowDownIcon className="h-4 w-4" />
                                                </button>
                                            </form>
                                        </div>
                                        <div>
                                            <div className="flex items-center gap-2">
                                                <h4 className="text-sm font-medium text-gray-900">{tip.title}</h4>
                                                {!tip.isActive && (
                                                    <span className="inline-flex items-center rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
                                                        Inactive
                                                    </span>
                                                )}
                                            </div>
                                            <p className="text-sm text-gray-500 mt-1 line-clamp-1">{tip.description}</p>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <Link
                                            href={`/home/settings/landing/tips/${tip.id}/edit`}
                                            className="p-2 text-gray-400 hover:text-indigo-600"
                                        >
                                            <PencilIcon className="h-5 w-5" />
                                        </Link>
                                        <form action={deleteTip}>
                                            <input type="hidden" name="id" value={tip.id} />
                                            <button
                                                type="submit"
                                                className="p-2 text-gray-400 hover:text-red-600"
                                            >
                                                <TrashIcon className="h-5 w-5" />
                                            </button>
                                        </form>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </Main>
    );
}
