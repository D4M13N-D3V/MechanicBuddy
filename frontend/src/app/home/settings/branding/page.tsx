import { httpGet } from "@/_lib/server/query-api";
import { IBrandingOptions } from "./model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../_components/Main";
import Link from "next/link";
import { DescriptionItem } from "@/_components/DescriptionItem";

function ColorSwatch({ color, label }: { color: string; label: string }) {
    return (
        <div className="flex items-center gap-3">
            <div
                className="w-8 h-8 rounded border border-gray-300"
                style={{ backgroundColor: color }}
            />
            <span className="text-sm text-gray-600">{color}</span>
        </div>
    );
}

export default async function Page() {
    const data = await httpGet('branding');
    const branding = await data.json() as IBrandingOptions;

    return (
        <Main header={<SettingsTabs />} narrow={true}>
            {/* Logo Section */}
            <div className="px-0">
                <h3 className="text-base/7 font-semibold text-gray-900 my-4">Logo</h3>
            </div>
            <div className="mt-6 border-t border-gray-100">
                <dl className="divide-y divide-gray-100">
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Company Logo</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            {branding.logoBase64 ? (
                                <img
                                    src={`data:${branding.logoMimeType};base64,${branding.logoBase64}`}
                                    alt="Company Logo"
                                    className="h-16 w-auto"
                                />
                            ) : (
                                <span className="text-gray-400">No logo uploaded</span>
                            )}
                        </dd>
                    </div>
                </dl>
            </div>

            {/* Portal Colors Section */}
            <div className="pt-8 px-0">
                <h3 className="text-base/7 font-semibold text-gray-900">Admin Portal Colors</h3>
                <p className="mt-1 max-w-2xl text-sm/6 text-gray-500">Colors used in the admin dashboard</p>
            </div>
            <div className="mt-6 border-t border-gray-100">
                <dl className="divide-y divide-gray-100">
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Sidebar Background</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.portalColors.sidebarBg} label="Sidebar Background" />
                        </dd>
                    </div>
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Sidebar Text</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.portalColors.sidebarText} label="Sidebar Text" />
                        </dd>
                    </div>
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Active Item Background</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.portalColors.sidebarActiveBg} label="Active Background" />
                        </dd>
                    </div>
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Active Item Text</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.portalColors.sidebarActiveText} label="Active Text" />
                        </dd>
                    </div>
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Accent Color</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.portalColors.accentColor} label="Accent Color" />
                        </dd>
                    </div>
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Content Background</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.portalColors.contentBg} label="Content Background" />
                        </dd>
                    </div>
                </dl>
            </div>

            {/* Landing Page Colors Section */}
            <div className="pt-8 px-0">
                <h3 className="text-base/7 font-semibold text-gray-900">Landing Page Colors</h3>
                <p className="mt-1 max-w-2xl text-sm/6 text-gray-500">Colors used on the public landing page</p>
            </div>
            <div className="mt-6 border-t border-gray-100">
                <dl className="divide-y divide-gray-100">
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Primary Color</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.landingColors.primaryColor} label="Primary Color" />
                        </dd>
                    </div>
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Secondary Color</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.landingColors.secondaryColor} label="Secondary Color" />
                        </dd>
                    </div>
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Accent Color</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.landingColors.accentColor} label="Accent Color" />
                        </dd>
                    </div>
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Header Background</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.landingColors.headerBg} label="Header Background" />
                        </dd>
                    </div>
                    <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                        <dt className="text-sm/6 font-medium text-gray-900">Footer Background</dt>
                        <dd className="mt-1 text-sm/6 text-gray-700 sm:col-span-2 sm:mt-0">
                            <ColorSwatch color={branding.landingColors.footerBg} label="Footer Background" />
                        </dd>
                    </div>
                </dl>
            </div>

            <div className="mt-6 flex items-center justify-end gap-x-6">
                <Link
                    href="/home/settings/branding/edit"
                    className="inline-flex items-center gap-x-1.5 rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                >
                    Edit
                </Link>
            </div>
        </Main>
    );
}
