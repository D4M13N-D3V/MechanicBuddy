'use client'

import FormLabel from "./FormLabel"

interface ColorInputProps {
    name: string
    label: string
    defaultValue: string
}

export default function ColorInput({ name, label, defaultValue }: ColorInputProps) {
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
    )
}
