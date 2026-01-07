'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions } from "../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import FormInput from "@/_components/FormInput";
import FormTextArea from "@/_components/FormTextArea";
import FormSwitch from "@/_components/FormSwitch";
import FormLabel from "@/_components/FormLabel";
import { updateFooter } from "../../branding/actions";
import Link from "next/link";

export default async function Page() {
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const footer = content.footer;

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

            <form action={updateFooter}>
                <div className="space-y-12">
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900 my-4">Footer Settings</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Customize the footer of your landing page.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                            <div className="sm:col-span-6">
                                <FormTextArea
                                    name="companyDescription"
                                    label="Company Description"
                                    rows={3}
                                    defaultValue={footer.companyDescription || ''}
                                    placeholder="A brief description of your company for the footer..."
                                />
                            </div>

                            <div className="sm:col-span-6">
                                <FormInput
                                    name="copyrightText"
                                    label="Copyright Text"
                                    defaultValue={footer.copyrightText || ''}
                                    placeholder="e.g., © 2024 Your Company. All rights reserved."
                                />
                                <p className="mt-1 text-xs text-gray-500">
                                    Leave empty to use default copyright text
                                </p>
                            </div>
                        </div>
                    </div>

                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900">Footer Sections</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Choose which sections to display in the footer.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                            <div className="sm:col-span-3">
                                <FormLabel name="showQuickLinks" label="Show Quick Links" />
                                <div className="mt-3">
                                    <FormSwitch name="showQuickLinks" defaultChecked={footer.showQuickLinks} />
                                </div>
                                <p className="mt-1 text-xs text-gray-500">
                                    Display navigation links (Services, About, Contact)
                                </p>
                            </div>

                            <div className="sm:col-span-3">
                                <FormLabel name="showContactInfo" label="Show Contact Info" />
                                <div className="mt-3">
                                    <FormSwitch name="showContactInfo" defaultChecked={footer.showContactInfo} />
                                </div>
                                <p className="mt-1 text-xs text-gray-500">
                                    Display contact information from company settings
                                </p>
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
                        Save Changes
                    </button>
                </div>
            </form>
        </Main>
    );
}
