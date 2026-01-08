'use client'

import { useState } from 'react'
import FormInput from '@/_components/FormInput';
import { useRouter } from 'next/navigation';
import PrimaryButton from '@/_components/PrimaryButton';
import SecondaryButton from '@/_components/SecondaryButton';
import { IUser } from '../model';

export default function UserInput({
    user
}: {
    user?: IUser | undefined
}) {

    const router = useRouter()

    const [firstName, setFirstName] = useState(user?.firstName || '');
    const [lastName, setLastName] = useState(user?.lastName || '');
    const [userName, setUserName] = useState(user?.userName || '');
    const [email, setEmail] = useState(user?.email || '');

    const [firstNameError, setFirstNameError] = useState("");
    const [lastNameError, setLastNameError] = useState("");
    const [userNameError, setUserNameError] = useState("");
    const [emailError, setEmailError] = useState("");
    const [passwordError, setPasswordError] = useState("");

    function validate(event: React.MouseEvent) {
        let hasError = false;

        if (!firstName) {
            setFirstNameError("First name is required");
            hasError = true;
        } else {
            setFirstNameError("");
        }

        if (!lastName) {
            setLastNameError("Last name is required");
            hasError = true;
        } else {
            setLastNameError("");
        }

        if (!userName) {
            setUserNameError("Username is required");
            hasError = true;
        } else {
            setUserNameError("");
        }

        if (!email) {
            setEmailError("Email is required");
            hasError = true;
        } else {
            setEmailError("");
        }

        // Password validation only for new users or if password fields are filled
        if (!user) {
            const password = (document.querySelector('[name="password"]') as HTMLInputElement)?.value;
            const confirmPassword = (document.querySelector('[name="confirmPassword"]') as HTMLInputElement)?.value;

            if (!password) {
                setPasswordError("Password is required");
                hasError = true;
            } else if (password !== confirmPassword) {
                setPasswordError("Passwords do not match");
                hasError = true;
            } else {
                setPasswordError("");
            }
        } else {
            const password = (document.querySelector('[name="password"]') as HTMLInputElement)?.value;
            const confirmPassword = (document.querySelector('[name="confirmPassword"]') as HTMLInputElement)?.value;

            if (password || confirmPassword) {
                if (password !== confirmPassword) {
                    setPasswordError("Passwords do not match");
                    hasError = true;
                } else {
                    setPasswordError("");
                }
            } else {
                setPasswordError("");
            }
        }

        if (hasError) {
            event.preventDefault();
        }
    }

    return (
        <>
            <div className="space-y-12">
                <div className="border-b border-gray-900/10 pb-12">
                    <div className="grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                        <div className="sm:col-span-3">
                            <FormInput
                                name='firstName'
                                inputError={firstNameError}
                                onInputChange={(e) => setFirstName(e.currentTarget.value)}
                                defaultValue={firstName}
                                label='First name'
                                required
                            />
                        </div>
                        <div className="sm:col-span-3">
                            <FormInput
                                name='lastName'
                                inputError={lastNameError}
                                onInputChange={(e) => setLastName(e.currentTarget.value)}
                                defaultValue={lastName}
                                label='Last name'
                                required
                            />
                        </div>

                        <div className="sm:col-span-3">
                            <FormInput
                                name='userName'
                                inputError={userNameError}
                                onInputChange={(e) => setUserName(e.currentTarget.value)}
                                defaultValue={userName}
                                label='Username'
                                required
                                disabled={user?.isDefaultAdmin}
                            />
                        </div>

                        <div className="sm:col-span-3">
                            <FormInput
                                name='email'
                                type='email'
                                inputError={emailError}
                                onInputChange={(e) => setEmail(e.currentTarget.value)}
                                defaultValue={email}
                                label='Email'
                                required
                            />
                        </div>

                        <div className="sm:col-span-3">
                            <FormInput
                                name='phone'
                                defaultValue={user?.phone}
                                label='Phone'
                            />
                        </div>

                        <div className="sm:col-span-3"></div>

                        {!user && (
                            <>
                                <div className="sm:col-span-3">
                                    <FormInput
                                        name='password'
                                        type='password'
                                        inputError={passwordError}
                                        label='Password'
                                        required
                                    />
                                </div>

                                <div className="sm:col-span-3">
                                    <FormInput
                                        name='confirmPassword'
                                        type='password'
                                        label='Confirm Password'
                                        required
                                    />
                                </div>
                            </>
                        )}

                        {user && (
                            <>
                                <div className="sm:col-span-6">
                                    <h3 className="text-base font-semibold text-gray-900 mb-4">
                                        Change Password (Optional)
                                    </h3>
                                </div>
                                <div className="sm:col-span-3">
                                    <FormInput
                                        name='password'
                                        type='password'
                                        inputError={passwordError}
                                        label='New Password'
                                    />
                                </div>

                                <div className="sm:col-span-3">
                                    <FormInput
                                        name='confirmPassword'
                                        type='password'
                                        label='Confirm New Password'
                                    />
                                </div>
                            </>
                        )}
                    </div>
                </div>
            </div>
            <div className="mt-6 flex items-center justify-end gap-x-6">
                <SecondaryButton onClick={() => router.back()}>Cancel</SecondaryButton>
                <PrimaryButton onClick={validate}>Save</PrimaryButton>
            </div>
        </>
    )
}
