'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions } from "../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import Link from "next/link";
import { PlusIcon, PencilIcon, TrashIcon, ArrowUpIcon, ArrowDownIcon } from "@heroicons/react/24/outline";
import { deleteStat, reorderStats } from "../../branding/actions";

export default async function Page() {
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const stats = content.stats.sort((a, b) => a.sortOrder - b.sortOrder);

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

            <div className="flex items-center justify-between my-4">
                <div>
                    <h2 className="text-base/7 font-semibold text-gray-900">Statistics</h2>
                    <p className="mt-1 text-sm text-gray-500">
                        Manage the statistics displayed on your landing page.
                    </p>
                </div>
                <Link
                    href="/home/settings/landing/stats/new"
                    className="inline-flex items-center gap-x-1.5 rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                >
                    <PlusIcon className="h-5 w-5" />
                    Add Stat
                </Link>
            </div>

            <div className="mt-6 bg-white rounded-lg border border-gray-200 shadow-sm">
                {stats.length === 0 ? (
                    <div className="p-8 text-center">
                        <p className="text-sm text-gray-500">No statistics yet. Add your first stat to get started.</p>
                    </div>
                ) : (
                    <div className="divide-y divide-gray-100">
                        {stats.map((stat, index) => (
                            <div key={stat.id} className="flex items-center justify-between px-4 py-4">
                                <div className="flex items-center gap-4">
                                    <div className="flex flex-col gap-1">
                                        <form action={reorderStats}>
                                            <input type="hidden" name="order" value={JSON.stringify(
                                                index > 0 ? { id: stat.id, direction: 'up' } : null
                                            )} />
                                            <button
                                                type="submit"
                                                disabled={index === 0}
                                                className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30 disabled:cursor-not-allowed"
                                            >
                                                <ArrowUpIcon className="h-4 w-4" />
                                            </button>
                                        </form>
                                        <form action={reorderStats}>
                                            <input type="hidden" name="order" value={JSON.stringify(
                                                index < stats.length - 1 ? { id: stat.id, direction: 'down' } : null
                                            )} />
                                            <button
                                                type="submit"
                                                disabled={index === stats.length - 1}
                                                className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30 disabled:cursor-not-allowed"
                                            >
                                                <ArrowDownIcon className="h-4 w-4" />
                                            </button>
                                        </form>
                                    </div>
                                    <div>
                                        <div className="text-2xl font-bold text-indigo-600">{stat.value}</div>
                                        <div className="text-sm text-gray-500">{stat.label}</div>
                                    </div>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Link
                                        href={`/home/settings/landing/stats/${stat.id}/edit`}
                                        className="p-2 text-gray-400 hover:text-indigo-600"
                                    >
                                        <PencilIcon className="h-5 w-5" />
                                    </Link>
                                    <form action={deleteStat}>
                                        <input type="hidden" name="id" value={stat.id} />
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
        </Main>
    );
}
