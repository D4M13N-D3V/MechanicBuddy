'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions, ISocialLinkItem } from "../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import Link from "next/link";
import { PlusIcon, PencilIcon, ArrowUpIcon, ArrowDownIcon } from "@heroicons/react/24/outline";
import { deleteSocialLink, reorderSocialLinks } from "../../branding/actions";
import DeleteSocialButton from "./_components/DeleteSocialButton";
import { SOCIAL_PLATFORMS } from "./_components/PlatformSelect";

function getPlatformLabel(platform: string): string {
    const found = SOCIAL_PLATFORMS.find(p => p.value === platform);
    return found ? found.label : platform;
}

function SocialLinkRow({ link, index, total }: { link: ISocialLinkItem; index: number; total: number }) {
    return (
        <div className="flex items-center justify-between py-4 border-b border-gray-100 last:border-0">
            <div className="flex items-center gap-4">
                <div className="flex flex-col gap-1">
                    <form action={reorderSocialLinks}>
                        <input type="hidden" name="order" value={JSON.stringify(
                            index > 0 ? { id: link.id, direction: 'up' } : null
                        )} />
                        <button
                            type="submit"
                            disabled={index === 0}
                            className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30 disabled:cursor-not-allowed"
                        >
                            <ArrowUpIcon className="h-4 w-4" />
                        </button>
                    </form>
                    <form action={reorderSocialLinks}>
                        <input type="hidden" name="order" value={JSON.stringify(
                            index < total - 1 ? { id: link.id, direction: 'down' } : null
                        )} />
                        <button
                            type="submit"
                            disabled={index === total - 1}
                            className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30 disabled:cursor-not-allowed"
                        >
                            <ArrowDownIcon className="h-4 w-4" />
                        </button>
                    </form>
                </div>
                <div>
                    <div className="flex items-center gap-2">
                        <h4 className="text-sm font-medium text-gray-900">
                            {link.platform === 'custom' ? (link.displayName || 'Custom Link') : getPlatformLabel(link.platform)}
                        </h4>
                        {!link.isActive && (
                            <span className="inline-flex items-center rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
                                Inactive
                            </span>
                        )}
                        {link.showInHeader && (
                            <span className="inline-flex items-center rounded-full bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-600">
                                Header
                            </span>
                        )}
                        {link.showInFooter && (
                            <span className="inline-flex items-center rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-600">
                                Footer
                            </span>
                        )}
                    </div>
                    <p className="text-sm text-gray-500 mt-0.5 truncate max-w-md">{link.url}</p>
                </div>
            </div>
            <div className="flex items-center gap-2">
                <Link
                    href={`/home/settings/landing/social/${link.id}/edit`}
                    className="p-2 text-gray-400 hover:text-indigo-600"
                >
                    <PencilIcon className="h-5 w-5" />
                </Link>
                <form action={deleteSocialLink}>
                    <input type="hidden" name="id" value={link.id} />
                    <DeleteSocialButton />
                </form>
            </div>
        </div>
    );
}

export default async function Page() {
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const socialLinks = (content.socialLinks || []).sort((a, b) => a.sortOrder - b.sortOrder);

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

            <div className="flex items-center justify-between my-4">
                <div>
                    <h2 className="text-base/7 font-semibold text-gray-900">Social Links</h2>
                    <p className="mt-1 text-sm text-gray-500">
                        Add links to your social media profiles. These will appear in the header and/or footer.
                    </p>
                </div>
                <Link
                    href="/home/settings/landing/social/new"
                    className="inline-flex items-center gap-x-1.5 rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                >
                    <PlusIcon className="h-5 w-5" />
                    Add Link
                </Link>
            </div>

            <div className="mt-6 bg-white rounded-lg border border-gray-200 shadow-sm">
                {socialLinks.length === 0 ? (
                    <div className="p-8 text-center">
                        <p className="text-sm text-gray-500">No social links yet. Add your first link to get started.</p>
                    </div>
                ) : (
                    <div className="divide-y divide-gray-100 px-4">
                        {socialLinks.map((link, index) => (
                            <SocialLinkRow
                                key={link.id}
                                link={link}
                                index={index}
                                total={socialLinks.length}
                            />
                        ))}
                    </div>
                )}
            </div>
        </Main>
    );
}
