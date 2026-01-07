import { Combobox, ComboboxInput, ComboboxOption, ComboboxOptions } from "@headlessui/react";
import { CheckIcon, MagnifyingGlassIcon, XMarkIcon } from "@heroicons/react/20/solid";
import { ChangeEvent, useState } from "react"; 
import clsx from "clsx"; 


export interface IObjectToString<T> {
  (item: T): string
}
export interface ISearchComboboxFind<T> {
    (event: ChangeEvent<HTMLInputElement>,dataTarget:ISetDataSource<T>): void
}
export interface ISearchComboboxOnItemChange<T> {
  (selecteItem:NoInfer<T | null> | null): void
}

export interface ISetDataSource<T>{
  (data:T[]):void
}

export default function TypeAheadCombobox<T>({
  id,
  defaultValue, 
  name,
  placeholder,
  className, 
  comboboxOptionsAbsolute, 
  comboboxOptionsWidth,
  displayFormatter,
  optionFormatter , 
  onItemChange,
  onSearch,
  showLookingGlass,
  clearable,
}: {
  id?: string | undefined,
  name: string,
  defaultValue: T | null, 
  displayFormatter: IObjectToString<T>,
  optionFormatter: IObjectToString<T>, 
  placeholder: string,
  className?: string, 
  comboboxOptionsAbsolute?: boolean | undefined,
  comboboxOptionsWidth?: number | undefined,
  comboboxOptionClass?:string,
  onItemChange: ISearchComboboxOnItemChange<T>,
  onSearch: ISearchComboboxFind<T>,
  showLookingGlass?: boolean | undefined,
  clearable?: boolean | undefined
}) {
  const [datasource, setDatasource] = useState<T[]>([]);
  const [selectedValue, setSelectedValue] = useState<T | null>(defaultValue);
  const applyDatasource =(items:T[])=>{
    setDatasource(items);
  }

  if(!className){
    className = " block w-full rounded-md bg-white py-1.5 pr-12 pl-3 text-base text-gray-900 outline-1 -outline-offset-1 outline-gray-300 placeholder:text-gray-400 focus:outline-2 focus:-outline-offset-2 focus:outline-indigo-600 text-sm/6";
  }

  return (
    <Combobox
        as="div"
        id={id}
        name={name}

        value={selectedValue}
        onChange={(item) => {
          setSelectedValue(item);
          setDatasource([]);
          onItemChange(item);
        }}
      >

        <div className={(comboboxOptionsAbsolute?'':'mt-2 relative')}>  
          <div className="grid grid-cols-1">
          
          <ComboboxInput 
            defaultValue={defaultValue}
            placeholder={placeholder}
            className={clsx(showLookingGlass&&"col-start-1 row-start-1 pl-11",className)}
            onChange={(event) => {
               onSearch(event,applyDatasource);
            }}
            onBlur={() =>   {
              
              setDatasource([])
            }
            }
            displayValue={(item) => {
              if (!item) return '';
              return displayFormatter(item);
            }}
          />
          {showLookingGlass && <MagnifyingGlassIcon
            className="pointer-events-none col-start-1 row-start-1 ml-4 size-5 self-center text-gray-400"
            aria-hidden="true"
          />}

          {clearable && <XMarkIcon
            aria-hidden="true"
            onClick={() => { }}
            className="cursor-pointer col-start-1 row-start-1 mr-2 size-5 self-center justify-self-end text-gray-500  size-4"
          />}
          </div> 
          
          {datasource && datasource.length > 0 && (
            <ComboboxOptions modal={false}  className={clsx(comboboxOptionsAbsolute?("w-80  sm:w-"+comboboxOptionsWidth):"w-full" ,"absolute z-10 mt-1 max-h-60 overflow-auto rounded-md bg-white py-1 text-base ring-1 shadow-lg ring-black/5 focus:outline-hidden  text-sm")} >
              {datasource.map((item, index) => (
                <ComboboxOption
                  key={index}
                  value={item}
                  className="group relative cursor-default py-2 pr-9 pl-3 text-gray-900 select-none data-focus:bg-indigo-600 data-focus:text-white data-focus:outline-hidden"
                >
                  <span className="block truncate group-data-selected:font-semibold">
                    {optionFormatter(item)}
                  </span>
                  <span className="absolute inset-y-0 right-0 hidden items-center pr-4 text-indigo-600 group-data-focus:text-white group-data-selected:flex">
                    <CheckIcon className="size-5" aria-hidden="true" />
                  </span>
                </ComboboxOption>
              ))}
            </ComboboxOptions>
          )}
        </div>
      </Combobox>
  )
}