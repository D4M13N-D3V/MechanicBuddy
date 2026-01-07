'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions } from "../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import Link from "next/link";
import { updateSectionVisibility } from "../../branding/actions";
import FormSwitch from "@/_components/FormSwitch";

interface VisibilityRowProps {
    name: string;
    label: string;
    description: string;
    defaultChecked: boolean;
}

function VisibilityRow({ name, label, description, defaultChecked }: VisibilityRowProps) {
    return (
        <div className="flex items-center justify-between py-4 border-b border-gray-100 last:border-0">
            <div>
                <h4 className="text-sm font-medium text-gray-900">{label}</h4>
                <p className="text-sm text-gray-500 mt-0.5">{description}</p>
            </div>
            <FormSwitch
                name={name}
                defaultChecked={defaultChecked}
            />
        </div>
    );
}

export default async function Page() {
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const visibility = content.sectionVisibility;

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

            <div className="mb-6">
                <h2 className="text-base/7 font-semibold text-gray-900">Section Visibility</h2>
                <p className="mt-1 text-sm text-gray-500">
                    Control which sections are displayed on your public landing page.
                </p>
            </div>

            <form action={updateSectionVisibility}>
                <div className="bg-white rounded-lg border border-gray-200 shadow-sm px-4">
                    <VisibilityRow
                        name="heroVisible"
                        label="Hero Section"
                        description="Main banner with company name and call-to-action"
                        defaultChecked={visibility?.heroVisible ?? true}
                    />
                    <VisibilityRow
                        name="servicesVisible"
                        label="Services Section"
                        description="List of services you offer"
                        defaultChecked={visibility?.servicesVisible ?? true}
                    />
                    <VisibilityRow
                        name="aboutVisible"
                        label="About Section"
                        description="Company description and feature highlights"
                        defaultChecked={visibility?.aboutVisible ?? true}
                    />
                    <VisibilityRow
                        name="statsVisible"
                        label="Stats Section"
                        description="Key statistics and achievements"
                        defaultChecked={visibility?.statsVisible ?? true}
                    />
                    <VisibilityRow
                        name="tipsVisible"
                        label="Tips Section"
                        description="Auto care tips and advice"
                        defaultChecked={visibility?.tipsVisible ?? true}
                    />
                    <VisibilityRow
                        name="galleryVisible"
                        label="Gallery Section"
                        description="Photo gallery showcasing your work"
                        defaultChecked={visibility?.galleryVisible ?? true}
                    />
                    <VisibilityRow
                        name="contactVisible"
                        label="Contact Section"
                        description="Contact form and business hours"
                        defaultChecked={visibility?.contactVisible ?? true}
                    />
                </div>

                <div className="mt-6 flex justify-end">
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
