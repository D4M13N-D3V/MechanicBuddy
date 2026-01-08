'use server'

import { httpGet } from '@/_lib/server/query-api';
import UserInput from '../../_components/UserInput';
import { createOrUpdate } from '../../createOrUpdate';
import { IUser } from '../../model';
import Main from '@/app/home/_components/Main';
import { CardHeader } from '@/_components/Card';


export default async function Page({
    params,
}: {
    params: Promise<{ id: string }>
}) {


    const id = (await params).id;
    const data = await httpGet('usermanagement/' + id);
    const user = await data.json() as IUser;

    return (
        <Main header={
            <CardHeader title='User Information' description='Edit details'>
            </CardHeader>}>
            <form action={createOrUpdate}>
                <input type="hidden" name='id' value={id}></input>
                <UserInput user={user}></UserInput>
            </form>
        </Main>
    )
}
