'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions } from "../../../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../../../_components/Main";
import FormInput from "@/_components/FormInput";
import { updateStat } from "../../../../branding/actions";
import Link from "next/link";
import { notFound } from "next/navigation";

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params;
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const stat = content.stats.find(s => s.id === id);

    if (!stat) {
        notFound();
    }

    return (
        <Main header={<SettingsTabs />} narrow={true}>
            <div className="mb-6">
                <Link
                    href="/home/settings/landing/stats"
                    className="text-sm text-indigo-600 hover:text-indigo-500"
                >
                    ← Back to Statistics
                </Link>
            </div>

            <form action={updateStat}>
                <input type="hidden" name="id" value={id} />
                <div className="space-y-12">
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900 my-4">Edit Statistic</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Update this statistic&apos;s information.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                            <div className="sm:col-span-3">
                                <FormInput
                                    name="value"
                                    label="Value"
                                    defaultValue={stat.value}
                                />
                            </div>

                            <div className="sm:col-span-3">
                                <FormInput
                                    name="label"
                                    label="Label"
                                    defaultValue={stat.label}
                                />
                            </div>
                        </div>
                    </div>
                </div>

                <div className="mt-6 flex items-center justify-end gap-x-6">
                    <Link
                        href="/home/settings/landing/stats"
                        className="text-sm font-semibold text-gray-900"
                    >
                        Cancel
                    </Link>
                    <button
                        type="submit"
                        className="rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                    >
                        Save Changes
                    </button>
                </div>
            </form>
        </Main>
    );
}
