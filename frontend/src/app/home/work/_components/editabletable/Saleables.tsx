'use client'

import clsx from "clsx";
import {   IPriceSummary, IProduct  } from "../../model";
import React, { useRef }  from "react";
import { DataItemRow, DataItemRowHandle } from "./DataIItemRow"; 
import { dragAndDrop } from "../activity/dragAndDrop"; 

export default function Saleables({
    edit, 
    data,
    priceSummary,
    tableRef, 
    removeItem,
    refreshData
}: {
    edit: boolean, 
    data: IProduct[],
    priceSummary: IPriceSummary,
    tableRef: React.RefObject<DataItemRowHandle<IProduct>[] | null[]>, 
    removeItem: (id: string) => void,
    refreshData: (data: IProduct[]) => void
}) {

     
    const moneyFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });
 
    const dragItem = useRef<string | undefined>(null);
    const dragOverItem = useRef<string | undefined>(null);
    
    const dnd = dragAndDrop(refreshData, tableRef,dragItem,dragOverItem);
 
    return (
        <>
         
            <div className="flow-root">
                <div className="  overflow-x-auto  ">
                    <div className=" inline-block  min-w-full py-2 align-middle  ">
                        <table id="items" border={1} className=" min-w-full divide-y divide-gray-300">
                            <thead>
                                <tr>
                                    {edit && <th></th>}
                                    <th className="py-3.5 pr-3 pl-4   text-sm font-semibold whitespace-nowrap text-gray-900 sm:pl-0"
                                    >Code
                                    </th>
                                    <th className="px-2 py-3.5  text-sm font-semibold whitespace-nowrap text-gray-900">
                                        Name
                                    </th>
                                    <th className="px-2 py-3.5 text-end  text-sm font-semibold whitespace-nowrap text-gray-900">
                                        Price
                                    </th>
                                    <th className="px-2 py-3.5 text-end   text-sm font-semibold whitespace-nowrap text-gray-900">
                                        Quantity
                                    </th>
                                    <th className="px-2 py-3.5 text-end  text-sm font-semibold whitespace-nowrap text-gray-900">
                                        Unit
                                    </th>
                                    <th className="px-2 py-3.5 text-end  text-sm font-semibold whitespace-nowrap text-gray-900">
                                        Discount
                                    </th>
                                    {edit && <th className="px-2 py-3.5 text-end text-sm font-semibold whitespace-nowrap text-gray-900"></th>}
                                </tr>
                            </thead>
                            <tbody className={clsx(!edit && "divide-y divide-gray-200", " bg-white")}>
                                {data.filter(x=>x).map((product, index) => {
                                    
                                    return (
                                        <DataItemRow key={'dr' + product.id}
                                            isEditing={edit}
                                            index={index}
                                            ref={el => {
                                                if (tableRef?.current) tableRef.current[index] = el;
                                            }}
                                            item={product}
                                            onDragStart={e => dnd.handleDragStart(e)}
                                            onDragEnter={e => dnd.handleDragEnter(e)}
                                            removeWorkItem={(id) => {
                                                removeItem(id);
                                            }}>
                                        </DataItemRow>
                                    )
                                })}
                            </tbody>
                            {!edit && priceSummary && <tfoot>
                                <tr>
                                    <th colSpan={6}>
                                        <div className="grid grid-rows-3 gap-0">
                                            <div className="flex flex-row-reverse " >

                                                <div className="hidden pt-4 pr-3 pl-4 text-right text-sm font-normal text-gray-500 sm:table-cell sm:pl-0 w-25">{moneyFormatter.format(priceSummary.totalWithoutVat)}</div>
                                                <div className="hidden pt-4 pr-3 pl-4 text-right text-sm font-normal text-gray-500 sm:table-cell sm:pl-0">Subtotal</div>
                                            </div>
                                            <div className="flex flex-row-reverse " >
                                                <div className="hidden pt-4 pr-3 pl-4 text-right text-sm font-normal text-gray-500 sm:table-cell sm:pl-0 w-25">{moneyFormatter.format(priceSummary.totalWithVat - priceSummary.totalWithoutVat)}</div>
                                                <div className="hidden pt-4 pr-3 pl-4 text-right text-sm font-normal text-gray-500 sm:table-cell sm:pl-0">Tax</div>
                                            </div>
                                            <div className="flex flex-row-reverse  " >

                                                <div className="hidden pt-4 pr-3 pl-4 text-right text-sm font-semibold text-gray-900 sm:table-cell sm:pl-0 w-25">{moneyFormatter.format(priceSummary.totalWithVat)}</div>
                                                <div className="hidden pt-4 pr-3 pl-4 text-right text-sm font-semibold text-gray-900 sm:table-cell sm:pl-0">Total</div>
                                            </div>
                                        </div>
                                    </th>
                                </tr>
                            </tfoot>}
                        </table>
                    </div>
                </div>
            </div>
           </>)

}