'use server'

import Main from "../../_components/Main";
import Search from "../../_components/Search";
import { SearchCardHeader } from "../../_components/SearchCardHeader";
import SimpleSearchBar from "../../_components/SimpleSearchBar";
import Link from "next/link";
import { httpGet } from "@/_lib/server/query-api";
import BlueBadge from "@/_components/BlueBadge";

export default async function Page(
  { searchParams }: { searchParams: Promise<Record<string, string>> }) {

  // Check if user can manage users
  const canManageResponse = await httpGet('usermanagement/canmanage');
  const canManageUsers = await canManageResponse.json() as boolean;

  // If user cannot manage users, show upgrade prompt
  if (!canManageUsers) {
    return <Main header={
      <SearchCardHeader title="Users" pageName="settings/users">
      </SearchCardHeader>
    } narrow={false}>
      <div className="text-center py-12">
        <h3 className="mt-2 text-sm font-semibold text-gray-900">User Management Unavailable</h3>
        <p className="mt-1 text-sm text-gray-500">
          Please upgrade your plan to manage multiple users.
        </p>
      </div>
    </Main>
  }

  return <Main header={
    <SearchCardHeader title="Find Users" pageName="settings/users">
    </SearchCardHeader>
  } narrow={false}>
    <form method="GET">
      <Search
        searchParams={searchParams}
        resourceName="usermanagement"
        pageName="settings/users"
        columns={[{
          dataField: "name",
          dataFormatter: ({ id, firstName, lastName }) => {
            return (
              <Link href={'/home/settings/users/' + id}>
                {firstName} {lastName}
              </Link>
            );
          }
        }, {
          dataField: "userName",
          dataFormatter: ({ userName, isDefaultAdmin }) => {
            return (
              <div className="flex items-center gap-2">
                {userName}
                {isDefaultAdmin && <BlueBadge text="Default Admin" />}
              </div>
            );
          }
        }, {
          dataField: "email",
        }, {
          dataField: "phone",
        }]}>

        <SimpleSearchBar searchParams={searchParams} placeholder="name, username or email ..."></SimpleSearchBar>
      </Search>
    </form>

  </Main>
}
