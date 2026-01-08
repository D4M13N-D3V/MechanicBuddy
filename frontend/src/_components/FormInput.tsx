import { ExclamationCircleIcon } from "@heroicons/react/16/solid"
import clsx from "clsx"
import { ChangeEvent } from "react";
import FormLabel from "./FormLabel";


export interface IInputOnChange {
    (event: ChangeEvent<HTMLInputElement>): void
}

export default function FormInput({
    name,
    label,
    defaultValue,
    value,
    type,
    inputError,
    placeholder,
    onInputChange,
    step,
    className,
    required,
    disabled,
}: {
    name: string,
    label?: string | undefined,
    defaultValue?: string | number | readonly string[] | undefined,
    value?: string | number | readonly string[] | undefined,
    type?: string | undefined,
    inputError?: string | undefined,
    placeholder?: string | undefined,
    onInputChange?: IInputOnChange,
    step?: string | undefined,
    className?: string | undefined,
    required?: boolean | undefined,
    disabled?: boolean | undefined,
}) {
     
    let hasError = false;
    if (inputError) {
        hasError = true;
    } 
    return (
        <> 
            {label&&<FormLabel name={name} label={label}></FormLabel>}
            <div className="mt-2   grid grid-cols-1">
                <input
                    id={name}
                    name={name}
                    type={type}
                    step={step}
                    onChange={onInputChange}
                    defaultValue={defaultValue}
                    value={value}
                    required={required}
                    disabled={disabled}
                    placeholder={placeholder}
                    autoComplete={name}
                    aria-invalid={hasError}
                    aria-describedby={name + '-error'}
                    className={clsx(className,
                        hasError ? "col-start-1 row-start-1 text-red-900 outline-red-300 placeholder:text-red-400 focus:outline-red-600"
                            : "text-gray-900 outline-gray-300 placeholder:text-gray-400 focus:outline-indigo-600"
                        , "block w-full   rounded-md bg-white px-3 py-1.5 text-base  outline-1 -outline-offset-1  focus:outline-2 focus:-outline-offset-2 text-sm/6")}
                />
                {hasError && <ExclamationCircleIcon
                    aria-hidden="true"
                    className="pointer-events-none col-start-1 row-start-1 mr-3 size-5 self-center justify-self-end text-red-500 sm:size-4"
                />}
            </div>
            {hasError && <p id={name + '-error'} className="mt-2 text-sm text-red-600">
                {inputError}
            </p>}

        </>
    )
}

export function FormRadio({
    id,
    name,
    label,
    defaultChecked ,
    value,
    onChange,
}: {
    id: string,
    name: string,
    label?: string | undefined,
    defaultChecked?: boolean| undefined,
    value: string,
    onChange? : React.ChangeEventHandler<HTMLInputElement>
}){
    
    return (
       <>
        <input
        defaultChecked={defaultChecked}
        id={id}
        name={name}
        value={value}
        onChange={onChange}
        type="radio"
        className="relative size-4 appearance-none rounded-full border border-gray-300 bg-white before:absolute before:inset-1 before:rounded-full before:bg-white not-checked:before:hidden checked:border-indigo-600 checked:bg-indigo-600 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 disabled:border-gray-300 disabled:bg-gray-100 disabled:before:bg-gray-400 forced-colors:appearance-auto forced-colors:before:hidden"
      />
      <label htmlFor={name} className="block text-sm/6 text-nowrap font-medium text-gray-900">
      {label}
      </label>
      </>
    )
}