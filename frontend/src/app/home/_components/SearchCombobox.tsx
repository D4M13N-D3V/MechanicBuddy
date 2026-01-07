'use client'

import { dataPage } from "@/_lib/client/query-api";
import TypeAheadCombobox, { ISearchComboboxOnItemChange } from "./TypeAheadCombobox"
import { useState } from "react";
import { IVehicleData } from "../vehicles/model";

interface ISearchComboItem
{
    value:string,
    text:string
}

export function VehiclesCombobox( {
    name,
    defaultValue,
    className, 
}:{
    name:string, 
    defaultValue?:ISearchComboItem | null |undefined,
    className?:string |undefined, 
}){
    return (<SearchCombobox 
        name={name} 
        defaultValue={defaultValue} 
        className={className}  
        displayFormatter={(item:IVehicleData)=>{
            return [item.producer,item.model].filter(x=>x).join(' ')+ (!item.regNr?'':` (${item.regNr})`)
        }}
        resourceName="vehicles"
        placeholder="vin, reg nr., owner or make ..." ></SearchCombobox>)
}

export function ClientsCombobox( {
    name,
    defaultValue,
    className, 
    onItemChange
}:{
    name:string, 
    defaultValue?:ISearchComboItem | null |undefined,
    className?:string |undefined, 
    onItemChange?: ISearchComboboxOnItemChange<ISearchComboItem>,
}){
    return (<SearchCombobox 
        name={name} 
        defaultValue={defaultValue} 
        className={className}  
        displayFormatter={(item:any)=>{  // eslint-disable-line @typescript-eslint/no-explicit-any
            return item.name;
        }}
        onItemChange={onItemChange}
        resourceName="clients"
        placeholder="name ..." ></SearchCombobox>)
}

interface ISearchResultDisplayTextFormatter{
    (item:any):string // eslint-disable-line @typescript-eslint/no-explicit-any
}

export default function SearchCombobox({
    name,
    defaultValue,
    className, 
    resourceName,
    placeholder,
    onItemChange,
    displayFormatter,
}:{
    name:string,
    resourceName: string,
    placeholder: string,
    defaultValue?:ISearchComboItem | null |undefined,
    className?:string |undefined, 
    onItemChange?: ISearchComboboxOnItemChange<ISearchComboItem>,
    displayFormatter?: ISearchResultDisplayTextFormatter
}){
    
    const [selectedItem,setSelectedItem] = useState(defaultValue);


    return (
        <>
        <input type="hidden" name={`${name}[value]`} value={selectedItem?.value ?? ''} />
        <input type="hidden" name={`${name}[text]`} value={selectedItem?.text ?? ''} />
        <TypeAheadCombobox
        placeholder={placeholder}
        defaultValue={selectedItem}
        name={name}
        className={className} 
        displayFormatter={(item)=>{ return !item?'': item.text; }}
        optionFormatter={(item)=>{ return !item?'':item.text; }}
        onItemChange={(item)=>{
           setSelectedItem(item);
            if(onItemChange) onItemChange(item??null);
        }}
        onSearch={(e,target)=>{

            const inputValue =e.currentTarget.value;
            if(!inputValue) return;
            dataPage({
                resourceName:resourceName,
                searchText: inputValue  ,
                whenReady:(items)=>{
                   const data = items.map((item:any)=>{  // eslint-disable-line @typescript-eslint/no-explicit-any
                        return {
                          text: !displayFormatter?item : displayFormatter(item),
                          value:item.id
                        };
                      });
                    target(data);

                },
                onFailure:({url,status,text})=>{
                    console.log(url);
                    console.log(status);
                    console.log(text);
                }
              })   
        }}
        >
        </TypeAheadCombobox>
        </>
    )
}