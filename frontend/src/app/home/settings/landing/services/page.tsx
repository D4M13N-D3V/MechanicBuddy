'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions, IServiceItem } from "../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import Link from "next/link";
import { PlusIcon, PencilIcon, TrashIcon, ArrowUpIcon, ArrowDownIcon } from "@heroicons/react/24/outline";
import { deleteService, reorderServices } from "../../branding/actions";

function ServiceRow({ service, index, total }: { service: IServiceItem; index: number; total: number }) {
    return (
        <div className="flex items-center justify-between py-4 border-b border-gray-100 last:border-0">
            <div className="flex items-center gap-4">
                <div className="flex flex-col gap-1">
                    <form action={reorderServices}>
                        <input type="hidden" name="order" value={JSON.stringify(
                            index > 0 ? { id: service.id, direction: 'up' } : null
                        )} />
                        <button
                            type="submit"
                            disabled={index === 0}
                            className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30 disabled:cursor-not-allowed"
                        >
                            <ArrowUpIcon className="h-4 w-4" />
                        </button>
                    </form>
                    <form action={reorderServices}>
                        <input type="hidden" name="order" value={JSON.stringify(
                            index < total - 1 ? { id: service.id, direction: 'down' } : null
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
                        <h4 className="text-sm font-medium text-gray-900">{service.title}</h4>
                        {!service.isActive && (
                            <span className="inline-flex items-center rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
                                Inactive
                            </span>
                        )}
                        {service.usePrimaryColor && (
                            <span className="inline-flex items-center rounded-full bg-purple-100 px-2 py-0.5 text-xs font-medium text-purple-600">
                                Primary
                            </span>
                        )}
                    </div>
                    <p className="text-sm text-gray-500 mt-1 line-clamp-1">{service.description}</p>
                </div>
            </div>
            <div className="flex items-center gap-2">
                <Link
                    href={`/home/settings/landing/services/${service.id}/edit`}
                    className="p-2 text-gray-400 hover:text-indigo-600"
                >
                    <PencilIcon className="h-5 w-5" />
                </Link>
                <form action={deleteService}>
                    <input type="hidden" name="id" value={service.id} />
                    <button
                        type="submit"
                        className="p-2 text-gray-400 hover:text-red-600"
                        onClick={(e) => {
                            if (!confirm('Are you sure you want to delete this service?')) {
                                e.preventDefault();
                            }
                        }}
                    >
                        <TrashIcon className="h-5 w-5" />
                    </button>
                </form>
            </div>
        </div>
    );
}

export default async function Page() {
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const services = content.services.sort((a, b) => a.sortOrder - b.sortOrder);

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
                    <h2 className="text-base/7 font-semibold text-gray-900">Services</h2>
                    <p className="mt-1 text-sm text-gray-500">
                        Manage the services displayed on your landing page.
                    </p>
                </div>
                <Link
                    href="/home/settings/landing/services/new"
                    className="inline-flex items-center gap-x-1.5 rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-xs hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                >
                    <PlusIcon className="h-5 w-5" />
                    Add Service
                </Link>
            </div>

            <div className="mt-6 bg-white rounded-lg border border-gray-200 shadow-sm">
                {services.length === 0 ? (
                    <div className="p-8 text-center">
                        <p className="text-sm text-gray-500">No services yet. Add your first service to get started.</p>
                    </div>
                ) : (
                    <div className="divide-y divide-gray-100 px-4">
                        {services.map((service, index) => (
                            <ServiceRow
                                key={service.id}
                                service={service}
                                index={index}
                                total={services.length}
                            />
                        ))}
                    </div>
                )}
            </div>
        </Main>
    );
}
