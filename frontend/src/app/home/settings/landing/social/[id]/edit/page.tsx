'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ISocialLinkItem } from "../../../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../../../_components/Main";
import Link from "next/link";
import { updateSocialLink } from "../../../../branding/actions";
import FormSwitch from "@/_components/FormSwitch";
import PlatformSelect from "../../_components/PlatformSelect";
import { notFound } from "next/navigation";

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params;
    const data = await httpGet(`branding/social-links`);
    const links = await data.json() as ISocialLinkItem[];
    const link = links.find(l => l.id === id);

    if (!link) {
        notFound();
    }

    return (
        <Main header={<SettingsTabs />} narrow={true}>
            <div className="mb-6">
                <Link
                    href="/home/settings/landing/social"
                    className="text-sm text-indigo-600 hover:text-indigo-500"
                >
                    &larr; Back to Social Links
                </Link>
            </div>

            <div className="mb-6">
                <h2 className="text-base/7 font-semibold text-gray-900">Edit Social Link</h2>
                <p className="mt-1 text-sm text-gray-500">
                    Update this social link&apos;s settings.
                </p>
            </div>

            <form action={updateSocialLink} className="bg-white rounded-lg border border-gray-200 shadow-sm p-6">
                <input type="hidden" name="id" value={id} />

                <div className="space-y-6">
                    <div>
                        <label htmlFor="platform" className="block text-sm font-medium text-gray-700">
                            Platform *
                        </label>
                        <PlatformSelect defaultValue={link.platform} />
                    </div>

                    <div>
                        <label htmlFor="url" className="block text-sm font-medium text-gray-700">
                            URL *
                        </label>
                        <input
                            type="url"
                            id="url"
                            name="url"
                            required
                            defaultValue={link.url}
                            placeholder="https://facebook.com/yourpage"
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                        />
                    </div>

                    <div>
                        <label htmlFor="displayName" className="block text-sm font-medium text-gray-700">
                            Display Name (for custom links)
                        </label>
                        <input
                            type="text"
                            id="displayName"
                            name="displayName"
                            defaultValue={link.displayName || ''}
                            placeholder="My Website"
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                        />
                        <p className="mt-1 text-xs text-gray-500">
                            Only used for custom links. Leave empty for standard platforms.
                        </p>
                    </div>

                    <div className="flex items-center justify-between">
                        <div>
                            <label className="text-sm font-medium text-gray-700">Active</label>
                            <p className="text-sm text-gray-500">Show this link on the landing page</p>
                        </div>
                        <FormSwitch name="isActive" defaultChecked={link.isActive} />
                    </div>

                    <div className="flex items-center justify-between">
                        <div>
                            <label className="text-sm font-medium text-gray-700">Show in Header</label>
                            <p className="text-sm text-gray-500">Display this link in the site header</p>
                        </div>
                        <FormSwitch name="showInHeader" defaultChecked={link.showInHeader} />
                    </div>

                    <div className="flex items-center justify-between">
                        <div>
                            <label className="text-sm font-medium text-gray-700">Show in Footer</label>
                            <p className="text-sm text-gray-500">Display this link in the site footer</p>
                        </div>
                        <FormSwitch name="showInFooter" defaultChecked={link.showInFooter} />
                    </div>
                </div>

                <div className="mt-6 flex justify-end gap-3">
                    <Link
                        href="/home/settings/landing/social"
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
