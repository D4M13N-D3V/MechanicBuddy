'use server'

import UserInput from '../_components/UserInput';
import { createOrUpdate } from '../createOrUpdate';
import Main from '../../../_components/Main';
import { CardHeader } from '@/_components/Card';

export default async function Page() {

    return (
        <Main header={
            <CardHeader title='New User' description='Enter user details'>
            </CardHeader>}>
            <form action={createOrUpdate}>
                <input type="hidden" name='id'></input>
                <UserInput></UserInput>
            </form>
        </Main>
    )
}
