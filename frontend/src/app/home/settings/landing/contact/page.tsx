'use server'

import { httpGet } from "@/_lib/server/query-api";
import { ILandingContentOptions } from "../../branding/model";
import SettingsTabs from "@/_components/SettingsTabs";
import Main from "../../../_components/Main";
import FormInput from "@/_components/FormInput";
import FormTextArea from "@/_components/FormTextArea";
import FormSwitch from "@/_components/FormSwitch";
import FormLabel from "@/_components/FormLabel";
import { updateContact } from "../../branding/actions";
import Link from "next/link";

const DAYS = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

function BusinessHoursRow({ day, open, close }: { day: string; open: string; close: string }) {
    const isClosed = open === 'Closed' || close === 'Closed';

    return (
        <div className="grid grid-cols-4 gap-4 items-center py-2">
            <div className="text-sm font-medium text-gray-900">{day}</div>
            <div>
                <input
                    type="text"
                    name={`hours_${day}_open`}
                    defaultValue={open}
                    placeholder="9:00 AM"
                    className="block w-full rounded-md bg-white px-3 py-1.5 text-sm text-gray-900 outline-1 -outline-offset-1 outline-gray-300 placeholder:text-gray-400 focus:outline-2 focus:-outline-offset-2 focus:outline-indigo-600"
                />
            </div>
            <div className="text-center text-sm text-gray-500">to</div>
            <div>
                <input
                    type="text"
                    name={`hours_${day}_close`}
                    defaultValue={close}
                    placeholder="5:00 PM"
                    className="block w-full rounded-md bg-white px-3 py-1.5 text-sm text-gray-900 outline-1 -outline-offset-1 outline-gray-300 placeholder:text-gray-400 focus:outline-2 focus:-outline-offset-2 focus:outline-indigo-600"
                />
            </div>
        </div>
    );
}

export default async function Page() {
    const data = await httpGet('branding/landing-content');
    const content = await data.json() as ILandingContentOptions;
    const contact = content.contact;

    // Create a map for easy lookup
    const hoursMap = new Map(contact.businessHours.map(h => [h.day, h]));

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

            <form action={updateContact}>
                <div className="space-y-12">
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900 my-4">Contact Section</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Customize the contact section of your landing page.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                            <div className="sm:col-span-3">
                                <FormInput
                                    name="sectionLabel"
                                    label="Section Label"
                                    defaultValue={contact.sectionLabel}
                                    placeholder="e.g., Get In Touch"
                                />
                            </div>

                            <div className="sm:col-span-3">
                                <FormInput
                                    name="headline"
                                    label="Headline"
                                    defaultValue={contact.headline}
                                    placeholder="e.g., Contact Us"
                                />
                            </div>

                            <div className="sm:col-span-6">
                                <FormTextArea
                                    name="description"
                                    label="Description"
                                    rows={2}
                                    defaultValue={contact.description || ''}
                                />
                            </div>
                        </div>
                    </div>

                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900">Towing Service</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Display a towing service notice on the contact section.
                        </p>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                            <div className="sm:col-span-3">
                                <FormLabel name="showTowing" label="Show Towing Notice" />
                                <div className="mt-3">
                                    <FormSwitch name="showTowing" defaultChecked={contact.showTowing} />
                                </div>
                            </div>

                            <div className="sm:col-span-6">
                                <FormInput
                                    name="towingText"
                                    label="Towing Text"
                                    defaultValue={contact.towingText}
                                    placeholder="e.g., Towing service available — call us!"
                                />
                            </div>
                        </div>
                    </div>

                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base/7 font-semibold text-gray-900">Business Hours</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Set your business hours. Use &quot;Closed&quot; for days you&apos;re not open.
                        </p>

                        <div className="mt-10 max-w-xl">
                            <div className="grid grid-cols-4 gap-4 pb-2 border-b border-gray-200">
                                <div className="text-xs font-medium text-gray-500 uppercase">Day</div>
                                <div className="text-xs font-medium text-gray-500 uppercase">Open</div>
                                <div></div>
                                <div className="text-xs font-medium text-gray-500 uppercase">Close</div>
                            </div>
                            {DAYS.map(day => {
                                const hours = hoursMap.get(day);
                                return (
                                    <BusinessHoursRow
                                        key={day}
                                        day={day}
                                        open={hours?.open || 'Closed'}
                                        close={hours?.close || 'Closed'}
                                    />
                                );
                            })}
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
