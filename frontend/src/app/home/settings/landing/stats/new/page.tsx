'use server'

import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../../_components/Main";
import FormInput from "@/_components/FormInput";
import { createStat } from "../../../branding/actions";
import Link from "next/link";

export default async function Page() {
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

            <form action={createStat}>
                <div className="space-y-12">
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900 my-4">New Statistic</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Add a new statistic to display on your landing page.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                            <div className="sm:col-span-3">
                                <FormInput
                                    name="value"
                                    label="Value"
                                    placeholder="e.g., 15+"
                                />
                                <p className="mt-1 text-xs text-gray-500">
                                    The number or value to display (e.g., &quot;15+&quot;, &quot;5000+&quot;, &quot;98%&quot;)
                                </p>
                            </div>

                            <div className="sm:col-span-3">
                                <FormInput
                                    name="label"
                                    label="Label"
                                    placeholder="e.g., Years of Experience"
                                />
                                <p className="mt-1 text-xs text-gray-500">
                                    Description of what the value represents
                                </p>
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
                        Create Statistic
                    </button>
                </div>
            </form>
        </Main>
    );
}
