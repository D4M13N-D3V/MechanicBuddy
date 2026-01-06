 
import Main from "../_components/Main";
import Search from "../_components/Search";
import { SearchCardHeader } from "../_components/SearchCardHeader";
import SimpleSearchBar from "../_components/SimpleSearchBar";
import Link from "next/link";

export default async function Page(
  { searchParams }: { searchParams: Promise<Record<string, string>> }) {


  return <Main header={
    <SearchCardHeader title="Find Clients" pageName="clients">
    </SearchCardHeader>
  } narrow={false}>
    <form method="GET" > <Search
      searchParams={searchParams}
      resourceName="clients"
      columns={[{
        dataField: "name",
        dataFormatter: ({ id, name }) => {
          return (
            <Link href={'/home/clients/' + id}>
              {name}
            </Link>
          );
        }
      }, {
        dataField: "phone",
      }, {
        dataField: "email",
      }, {
        dataField: "address",
      }]}>

      <SimpleSearchBar searchParams={searchParams} placeholder="name, address or phone ..."></SimpleSearchBar>
    </Search></form>

  </Main>


}