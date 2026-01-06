import clsx from "clsx";
import Search from "../_components/Search";
import 'car-makes-icons/dist/style.css';
import { SearchCardHeader } from "../_components/SearchCardHeader";
import Main from "../_components/Main";
import SimpleSearchBar from "../_components/SimpleSearchBar";
import Link from "next/link";


export default async function Page(
  { searchParams }: { searchParams: Promise<Record<string, string>> }) {

  return <Main header={
    <SearchCardHeader title="Find Vehicles" pageName="vehicles">
    </SearchCardHeader>
  } narrow={false}>
     <form method="GET" > <Search
      searchParams={searchParams}
      resourceName="vehicles"
      columns={[

        {
          dataField: 'producer',
          headerText: 'Producer',
          dataClasses: () => {
            return "pl-4 font-medium gray-900 whitespace-nowrap";
          },
          dataFormatter: ({ producer }) => {
            const producerName = producer.trim().replace(" ", "-").toLowerCase();
            return (
              <div className="flex items-center " >
                <i className={clsx("pr-2 text-2xl", "car-" + producerName)}>  </i>
                <span className="text-sm">{producer}</span>
              </div>
            );
          }
        },
        {
          dataField: 'model',
          headerText: 'Model',
        },
        {
          dataField: 'regNr',
          headerText: 'RegNr',
          dataFormatter: ({ regNr, id }) => {
            return (
              <Link href={'/home/vehicles/' + id} >
                <h5 className="font-semibold"> {regNr}</h5>
              </Link>
            );
          }
        },
        {
          dataField: 'ownerName',
          headerText: 'Owner',
          dataFormatter: ({ ownerName, ownerId }) => {
            if (!ownerName) return <p className="font-italic text-gray-400">No owner</p>;
            return (
              <Link href={'/home/clients/' + ownerId} >
                <h5 >{ownerName}</h5>
              </Link>
            );
          }
        },
        {
          dataField: 'vin',
          headerText: 'VIN',
          dataFormatter: ({ vin, id }) => {
            return (
              <Link href={'/home/vehicles/' + id} >
                <h5  >{vin}</h5>
              </Link>
            );
          }
        }
      ]}>
        <SimpleSearchBar searchParams={searchParams} placeholder="vin, reg nr., owner or make ..."></SimpleSearchBar> 
        </Search></form>
   
  </Main>



}