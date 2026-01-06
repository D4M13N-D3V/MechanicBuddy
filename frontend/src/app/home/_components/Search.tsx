import clsx from "clsx";
import Link from "next/link";
import React  from "react";
import { httpGet } from "@/_lib/server/query-api"; 
interface DataResult {
  hasMore: boolean,
  items: Record<string, string>[]
}

interface DataRowModel {
  dataField: string,
  headerText?: string | undefined,
  headerClasses?(index: number): string | undefined,
  dataClasses?(item: Record<string, any>, index: number): string | undefined,// eslint-disable-line @typescript-eslint/no-explicit-any
  sort?: boolean | undefined,
  dataFormatter?(item: Record<string, any>, colIndex: number): React.ReactNode | undefined, // eslint-disable-line @typescript-eslint/no-explicit-any
}

export default async function Search(
  {
    searchParams,
    resourceName,
    pageName,
    columns = [],
    rowClass,
    idField = 'id',
    children,
  }: {
    searchParams: Promise<Record<string, string>>
    resourceName: string,
    pageName?: string | undefined | null,
    idField?: string | undefined,
    columns?: DataRowModel[],
    rowClass? (item:any):string // eslint-disable-line @typescript-eslint/no-explicit-any
    children?: React.ReactNode
  }) {

  if (!pageName) pageName = resourceName;
  let options = (await searchParams);
  const offset = parseInt(options.offset ?? 0);
  const limit = parseInt(options.limit ?? 30);
  options = {
    ...options,
    offset: offset.toString(),
    limit: limit.toString()
  };
  const queryString = new URLSearchParams(options).toString();
  const page = '/home/' + pageName + '?';
  const nextPage = page + new URLSearchParams({ ...options, offset: (offset + limit).toString() }).toString();
  const prevPage = page + new URLSearchParams({ ...options, offset: (offset - limit).toString() }).toString();

  const response = await httpGet(`${resourceName}/page?${queryString}`);
  const data = (await response.json() as DataResult);
  
  if (data.items.length > 0) {
    //if no columns defined show all what data has
    if (!columns || columns.length === 0) {
      columns = Object.getOwnPropertyNames(data.items[0]).map((item) => {
        return {
          dataField: item,
        } as DataRowModel
      })
    }
    //populate defaults
    columns.forEach((col) => {
      if (col.headerText === undefined) col.headerText = String(col.dataField).charAt(0).toUpperCase() + String(col.dataField).slice(1);
      if (!col.dataFormatter) {
        col.dataFormatter = (item) => {
          return item[col.dataField];
        }
      }
      if (!col.headerClasses) {
        col.headerClasses = (index) => {
          return clsx(index === 0 ? "py-3.5 pr-3 pl-4 text-left text-sm font-semibold text-gray-900 sm:pl-0" : "px-3 py-3.5 text-left text-sm font-semibold text-gray-900")
        }
      }
      if (!col.dataClasses) {
        col.dataClasses = (item, index) => {
          return clsx(index === 0 ?
            "py-4 pr-3 pl-4 text-sm font-medium whitespace-nowrap text-gray-900 sm:pl-0" :
            "px-3 py-4 text-sm whitespace-nowrap text-gray-500");
        }
      }
    })
  }

  /* useEffect(() => {
    if (typeof window !== "undefined") {
      const searchPath = localStorage.getItem("searchPath");
      if (searchPath) {
        
      }
    }
  }, []); */
  return (

    <> 
    
    {children}

    <div className="sm:flex sm:items-center">
      <div className="sm:flex-auto">
        
        {/* <h1 className="text-base font-semibold text-gray-900">{displayName}</h1>
                            <p className="mt-2 text-sm text-gray-700">
                               
                            </p> */}
          
      </div>
      
      <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">

      </div>
    </div>
   
      <div className="-mx-4 sm:mx-0 mt-4 flow-root">
        {data.items.length===0? 
         <div className="text-center"> 
           <h3 className="mt-2 pb-6 text-sm font-semibold text-gray-900">Nothing found</h3> 
        </div>:
          <div className="overflow-hidden">
          <div className=" overflow-x-auto  ">
            <div className="inline-block min-w-full   align-middle  ">

              <table className="min-w-full divide-y divide-gray-300">
                <thead>
                  <tr>
                    {
                      columns?.map((val, index) => {
                        return <th key={'th' + index} className={val.headerClasses && val.headerClasses(index)}>{val.headerText}</th>
                      })
                    }
                    <th scope="col" className="relative py-3.5 pr-4 pl-3 sm:pr-0">
                      <span className="sr-only">Edit</span>
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {data.items.map((item, rowindex) => (
                    <tr key={'tr' + item[idField]}  className={rowClass && rowClass(item)}>
                      {
                        columns?.map((col, colindex) => {
                          return <td key={'td' + colindex + item[idField] + rowindex}
                            className={col.dataClasses && col.dataClasses(item, colindex)}>
                            {col.dataFormatter && col.dataFormatter(item, colindex)}
                          </td>
                        })
                      }
                      <td className="relative py-4 pr-4 pl-3 text-right text-sm font-medium whitespace-nowrap sm:pr-0">
                        <Link href={`/home/${pageName}/edit/${item[idField]}`} className="text-indigo-900 hover:text-indigo-500">
                          Edit
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
          <nav
            aria-label="Pagination"
            className="flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3 sm:px-6"
          >
            <div className="hidden sm:block">
              <p className="text-sm text-gray-700">
                Showing <span className="font-medium">{offset + 1}</span> to <span className="font-medium">{offset + limit}</span>
                {/* of{' '} <span className="font-medium">{limit}</span> results */}
              </p>
            </div>
            <div className="flex flex-1 justify-between sm:justify-end">
              <Link href={prevPage}
                className={
                  clsx(offset <= 0 ? "pointer-events-none text-gray-400" : "text-gray-900", "relative inline-flex items-center rounded-md bg-white px-3 py-2 text-sm font-semibold ring-1 ring-gray-300 ring-inset hover:bg-gray-50 focus-visible:outline-offset-0")} >Previous</Link>
              <Link href={nextPage}
                className={
                  clsx(!data.hasMore ? "pointer-events-none text-gray-400" : "text-gray-900", " relative ml-3 inline-flex items-center rounded-md bg-white px-3 py-2 text-sm font-semibold ring-1 ring-gray-300 ring-inset hover:bg-gray-50 focus-visible:outline-offset-0")}>Next</Link>
            </div>
          </nav>
        </div>
        }
      
      </div>
      
       </>

  )
}