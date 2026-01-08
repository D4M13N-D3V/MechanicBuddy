'use client'
import { authenticate } from './authenticate'
import { useActionState } from 'react'

const initialState = {
  error: '',
};

export default function LoginPage() {

  const [state, action] = useActionState(authenticate, initialState);

  return (
    <>
      <div className="bg-slate-900 flex min-h-full flex-1">
        <div className="flex flex-1 flex-col justify-center px-4 py-12 sm:px-6 lg:flex-none lg:px-20 xl:px-24">
          <div className="mx-auto w-full max-w-sm lg:w-96">
            <div>
              <h2 className="mt-8 text-2xl/9 font-bold tracking-tight text-white">Mechanic Portal</h2>
              <p className="mt-2 text-sm/6 text-slate-400">
                Sign in to access the management system
              </p>
            </div>

            <div className="mt-10">
              <div>
                {state?.error && <p className="text-red-400 text-sm mb-4">{state.error}</p>}
                <form action={action} className="space-y-6">
                  <div>
                    <label htmlFor="username" className="block text-sm/6 font-medium text-slate-200">
                      Username
                    </label>
                    <div className="mt-2">
                      <input
                        id="username"
                        name="username"
                        type="text"
                        required
                        className="block w-full rounded-md bg-slate-800 px-3 py-1.5 text-base text-white outline-1 -outline-offset-1 outline-slate-600 placeholder:text-slate-500 focus:outline-2 focus:-outline-offset-2 focus:outline-blue-500 text-sm/6"
                      />
                    </div>
                  </div>

                  <div>
                    <label htmlFor="password" className="block text-sm/6 font-medium text-slate-200">
                      Password
                    </label>
                    <div className="mt-2">
                      <input
                        id="password"
                        name="password"
                        type="password"
                        required
                        autoComplete="current-password"
                        className="block w-full rounded-md bg-slate-800 px-3 py-1.5 text-base text-white outline-1 -outline-offset-1 outline-slate-600 placeholder:text-slate-500 focus:outline-2 focus:-outline-offset-2 focus:outline-blue-500 text-sm/6"
                      />
                    </div>
                  </div>

                  <div className="flex items-center justify-between">
                    <div className="flex gap-3">
                      <div className="flex h-6 shrink-0 items-center">
                        <div className="group grid size-4 grid-cols-1">
                          <input
                            id="remember-me"
                            name="remember-me"
                            type="checkbox"
                            className="col-start-1 row-start-1 appearance-none rounded-sm border border-slate-600 bg-slate-800 checked:border-blue-500 checked:bg-blue-500 indeterminate:border-blue-500 indeterminate:bg-blue-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 disabled:border-slate-600 disabled:bg-slate-700 disabled:checked:bg-slate-700 forced-colors:appearance-auto"
                          />
                          <svg
                            fill="none"
                            viewBox="0 0 14 14"
                            className="pointer-events-none col-start-1 row-start-1 size-3.5 self-center justify-self-center stroke-white group-has-disabled:stroke-slate-400"
                          >
                            <path
                              d="M3 8L6 11L11 3.5"
                              strokeWidth={2}
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              className="opacity-0 group-has-checked:opacity-100"
                            />
                            <path
                              d="M3 7H11"
                              strokeWidth={2}
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              className="opacity-0 group-has-indeterminate:opacity-100"
                            />
                          </svg>
                        </div>
                      </div>
                      <label htmlFor="remember-me" className="block text-sm/6 text-slate-300">
                        Remember me
                      </label>
                    </div>
                  </div>

                  <div>
                    <button
                      type="submit"
                      className="flex w-full justify-center rounded-md bg-blue-600 px-3 py-1.5 text-sm/6 font-semibold text-white shadow-xs hover:bg-blue-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500"
                    >
                      Sign in
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
