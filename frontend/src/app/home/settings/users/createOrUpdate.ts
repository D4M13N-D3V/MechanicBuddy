'use server'

import { httpPut, httpPost, httpDelete } from "@/_lib/server/query-api";
import { pushToast } from "@/_lib/server/pushToast";
import { redirect } from "next/navigation";

interface UserFormData {
    firstName: FormDataEntryValue | null;
    lastName: FormDataEntryValue | null;
    userName: FormDataEntryValue | null;
    email: FormDataEntryValue | null;
    phone: FormDataEntryValue | null;
    password?: FormDataEntryValue | null;
}

export async function createOrUpdate(
    formData: FormData
) {

    const id = formData.get('id');

    const body: UserFormData = {
        firstName: formData.get('firstName'),
        lastName: formData.get('lastName'),
        userName: formData.get('userName'),
        email: formData.get('email'),
        phone: formData.get('phone'),
    };

    // Add password if provided
    const password = formData.get('password');
    if (password) {
        body.password = password;
    }

    const url = "usermanagement";

    const isUpdating = !!id;
    const response = isUpdating ? await httpPut({ url: url + '/' + id, body }) : await httpPost({ url, body });

    const jsonResponse = await response.json();

    const userId = jsonResponse;

    pushToast(`User ${(isUpdating ? 'updated' : 'created')} successfully!`)

    redirect('/home/settings/users/' + userId)
}

export async function deleteUser(
    formData: FormData
) {

    const id = formData.get('id');

    if (!id) {
        throw new Error('User ID is required');
    }

    const url = "usermanagement/" + id;

    await httpDelete({ url });

    pushToast('User deleted successfully!')

    redirect('/home/settings/users')
}
