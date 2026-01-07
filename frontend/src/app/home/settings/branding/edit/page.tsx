'use server'

import { httpGet } from "@/_lib/server/query-api";
import { IBrandingOptions } from "../model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import FormLabel from "@/_components/FormLabel";
import { updateBranding } from "../actions";
import Link from "next/link";

function ColorInput({ name, label, defaultValue }: { name: string; label: string; defaultValue: string }) {
    return (
        <div>
            <FormLabel name={name} label={label} />
            <div className="mt-2 flex items-center gap-3">
                <input
                    type="color"
                    id={name}
                    name={name}
                    defaultValue={defaultValue}
                    className="h-10 w-14 cursor-pointer rounded border border-gray-300 p-1"
                />
                <input
                    type="text"
                    defaultValue={defaultValue}
                    className="block w-28 rounded-md bg-white px-3 py-1.5 text-sm text-gray-900 outline-1 -outline-offset-1 outline-gray-300 focus:outline-2 focus:-outline-offset-2 focus:outline-indigo-600"
                    onChange={(e) => {
                        const colorInput = document.getElementById(name) as HTMLInputElement;
                        if (colorInput && /^#[0-9A-Fa-f]{6}$/.test(e.target.value)) {
                            colorInput.value = e.target.value;
                        }
                    }}
                />
            </div>
        </div>
    );
}

export default async function Page() {
    const data = await httpGet('branding');
    const branding = await data.json() as IBrandingOptions;

    return (
        <Main header={<SettingsTabs />} narrow={true}>
            <form action={updateBranding}>
                <div className="space-y-12">
                    {/* Logo Section */}
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900 my-4">Company Logo</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Upload your company logo. Recommended size: 200x60 pixels.
                        </p>

                        <div className="mt-6">
                            <div className="flex items-center gap-6">
                                {branding.logoBase64 ? (
                                    <img
                                        src={`data:${branding.logoMimeType};base64,${branding.logoBase64}`}
                                        alt="Current Logo"
                                        className="h-16 w-auto rounded border border-gray-200 bg-white p-2"
                                    />
                                ) : (
                                    <div className="flex h-16 w-32 items-center justify-center rounded border border-dashed border-gray-300 bg-gray-50">
                                        <span className="text-xs text-gray-400">No logo</span>
                                    </div>
                                )}
                                <div>
                                    <label
                                        htmlFor="logo"
                                        className="cursor-pointer rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50"
                                    >
                                        Change Logo
                                    </label>
                                    <input
                                        type="file"
                                        id="logo"
                                        name="logo"
                                        accept="image/*"
                                        className="sr-only"
                                    />
                                    <p className="mt-2 text-xs text-gray-500">PNG, JPG, or SVG up to 2MB</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Portal Colors Section */}
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900">Admin Portal Colors</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Customize the colors used in the admin dashboard sidebar and accent elements.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-3">
                            <ColorInput
                                name="portalSidebarBg"
                                label="Sidebar Background"
                                defaultValue={branding.portalColors.sidebarBg}
                            />
                            <ColorInput
                                name="portalSidebarText"
                                label="Sidebar Text"
                                defaultValue={branding.portalColors.sidebarText}
                            />
                            <ColorInput
                                name="portalSidebarActiveBg"
                                label="Active Item Background"
                                defaultValue={branding.portalColors.sidebarActiveBg}
                            />
                            <ColorInput
                                name="portalSidebarActiveText"
                                label="Active Item Text"
                                defaultValue={branding.portalColors.sidebarActiveText}
                            />
                            <ColorInput
                                name="portalAccentColor"
                                label="Accent Color"
                                defaultValue={branding.portalColors.accentColor}
                            />
                            <ColorInput
                                name="portalContentBg"
                                label="Content Background"
                                defaultValue={branding.portalColors.contentBg}
                            />
                        </div>
                    </div>

                    {/* Landing Page Colors Section */}
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900">Landing Page Colors</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Customize the colors used on your public landing page.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-3">
                            <ColorInput
                                name="landingPrimaryColor"
                                label="Primary Color"
                                defaultValue={branding.landingColors.primaryColor}
                            />
                            <ColorInput
                                name="landingSecondaryColor"
                                label="Secondary Color"
                                defaultValue={branding.landingColors.secondaryColor}
                            />
                            <ColorInput
                                name="landingAccentColor"
                                label="Accent Color"
                                defaultValue={branding.landingColors.accentColor}
                            />
                            <ColorInput
                                name="landingHeaderBg"
                                label="Header Background"
                                defaultValue={branding.landingColors.headerBg}
                            />
                            <ColorInput
                                name="landingFooterBg"
                                label="Footer Background"
                                defaultValue={branding.landingColors.footerBg}
                            />
                        </div>
                    </div>
                </div>

                <div className="mt-6 flex items-center justify-end gap-x-6">
                    <Link
                        href="/home/settings/branding"
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
