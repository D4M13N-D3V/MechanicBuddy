'use client'
import { changePasswordOnLogin } from './actions'
import { useActionState } from 'react'

const initialState = {
  error: '',
};

export default function ChangePasswordPage() {

  const [state, action] = useActionState(changePasswordOnLogin, initialState);

  return (
    <>
      <div className="bg-slate-900 flex min-h-full flex-1">
        <div className="flex flex-1 flex-col justify-center px-4 py-12 sm:px-6 lg:flex-none lg:px-20 xl:px-24">
          <div className="mx-auto w-full max-w-sm lg:w-96">
            <div>
              <h2 className="mt-8 text-2xl/9 font-bold tracking-tight text-white">Change Your Password</h2>
              <p className="mt-2 text-sm/6 text-slate-400">
                For security reasons, you must change your password before continuing.
              </p>
            </div>

            <div className="mt-10">
              <div>
                {state?.error && <p className="text-red-400 text-sm mb-4">{state.error}</p>}
                <form action={action} className="space-y-6">
                  <div>
                    <label htmlFor="currentPassword" className="block text-sm/6 font-medium text-slate-200">
                      Current Password
                    </label>
                    <div className="mt-2">
                      <input
                        id="currentPassword"
                        name="currentPassword"
                        type="password"
                        required
                        autoComplete="current-password"
                        className="block w-full rounded-md bg-slate-800 px-3 py-1.5 text-base text-white outline-1 -outline-offset-1 outline-slate-600 placeholder:text-slate-500 focus:outline-2 focus:-outline-offset-2 focus:outline-blue-500 text-sm/6"
                      />
                    </div>
                  </div>

                  <div>
                    <label htmlFor="newPassword" className="block text-sm/6 font-medium text-slate-200">
                      New Password
                    </label>
                    <div className="mt-2">
                      <input
                        id="newPassword"
                        name="newPassword"
                        type="password"
                        required
                        autoComplete="new-password"
                        className="block w-full rounded-md bg-slate-800 px-3 py-1.5 text-base text-white outline-1 -outline-offset-1 outline-slate-600 placeholder:text-slate-500 focus:outline-2 focus:-outline-offset-2 focus:outline-blue-500 text-sm/6"
                      />
                    </div>
                  </div>

                  <div>
                    <label htmlFor="confirmPassword" className="block text-sm/6 font-medium text-slate-200">
                      Confirm New Password
                    </label>
                    <div className="mt-2">
                      <input
                        id="confirmPassword"
                        name="confirmPassword"
                        type="password"
                        required
                        autoComplete="new-password"
                        className="block w-full rounded-md bg-slate-800 px-3 py-1.5 text-base text-white outline-1 -outline-offset-1 outline-slate-600 placeholder:text-slate-500 focus:outline-2 focus:-outline-offset-2 focus:outline-blue-500 text-sm/6"
                      />
                    </div>
                  </div>

                  <div>
                    <button
                      type="submit"
                      className="flex w-full justify-center rounded-md bg-blue-600 px-3 py-1.5 text-sm/6 font-semibold text-white shadow-xs hover:bg-blue-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500"
                    >
                      Change Password
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
        <div className="relative hidden w-0 flex-1 lg:block bg-slate-800">
          <div className="absolute inset-0 flex items-center justify-center">
            <div className="text-center">
              <div className="text-6xl font-bold text-slate-700">MB</div>
              <div className="text-slate-600 mt-2">MechanicBuddy</div>
            </div>
          </div>
        </div>
      </div>
    </>
  )
}
