'use client'

import Link from 'next/link'
import {
  Popover,
  PopoverButton,
  PopoverBackdrop,
  PopoverPanel,
} from '@headlessui/react'
import clsx from 'clsx'

import { Button } from '@/_components/layout/Button'
import { Container } from '@/_components/layout/Container'
 
 
function MobileNavIcon({ open }: { open: boolean }) {
  return (
    <svg
      aria-hidden="true"
      className="h-3.5 w-3.5 overflow-visible stroke-slate-700"
      fill="none"
      strokeWidth={2}
      strokeLinecap="round"
    >
      <path
        d="M0 1H14M0 7H14M0 13H14"
        className={clsx(
          'origin-center transition',
          open && 'scale-90 opacity-0',
        )}
      />
      <path
        d="M2 2L12 12M12 2L2 12"
        className={clsx(
          'origin-center transition',
          !open && 'scale-90 opacity-0',
        )}
      />
    </svg>
  )
}

interface MobileNavigationProps {
  onTryDemoClick?: () => void;
}

function MobileNavigation({ onTryDemoClick }: MobileNavigationProps) {
  return (
    <Popover>
      <PopoverButton
        className="relative z-10 flex h-8 w-8 items-center justify-center focus:not-data-focus:outline-hidden"
        aria-label="Toggle Navigation"
      >
        {({ open }) => <MobileNavIcon open={open} />}
      </PopoverButton>
      <PopoverBackdrop
        transition
        className="fixed inset-0 bg-slate-300/50 duration-150 data-closed:opacity-0 data-enter:ease-out data-leave:ease-in"
      />
      <PopoverPanel
        transition
        className="absolute inset-x-0 top-full mt-4 flex origin-top flex-col rounded-2xl bg-white p-4 text-lg tracking-tight text-slate-900 ring-1 shadow-xl ring-slate-900/5 data-closed:scale-95 data-closed:opacity-0 data-enter:duration-150 data-enter:ease-out data-leave:duration-100 data-leave:ease-in"
      >
        <hr className="m-2 border-slate-300/40" />
        <Button
          href="https://github.com/rene98c/carcareco"
          variant="outline"
        >
          View on GitHub
        </Button>
        <Button 
          href="/auth/login"
          color="blue"
          className="mt-2"
        >
          Log in
        </Button>
        {onTryDemoClick && (
          <button
            onClick={onTryDemoClick}
            className="mt-2 rounded-md bg-gray-200 px-3 py-2 text-sm font-semibold text-gray-900 shadow-xs hover:bg-gray-300"
          >
            Try the demo
          </button>
        )}
      </PopoverPanel>
    </Popover>
  )
}

interface HeaderProps {
  onTryDemoClick?: () => void;
}

export function  Header({ onTryDemoClick }: HeaderProps) {
  return (
    <header className="py-10">
      <Container>
        <nav className="relative z-50 flex justify-between">
          <div className="flex items-center md:gap-x-12">
            <Link href="#" aria-label="Home">
               
            </Link>
            <div className="hidden md:flex md:gap-x-6">
              {/* Add navigation links here if needed */}
            </div>
          </div>
          <div className="flex items-center gap-x-5 md:gap-x-8">
            <div className="hidden md:block">
              <Button
                href="https://github.com/rene98c/carcareco"
                variant="outline"
              >
                View on GitHub
              </Button>
            </div>
            {onTryDemoClick ? (
              <Button href="/auth/login" color="blue">
                <span>Log in</span>
              </Button>
            ) : (
              <Button href="/auth/login" color="blue">
                <span>Log in</span>
              </Button>
            )}
            <div className="-mr-1 md:hidden">
              <MobileNavigation onTryDemoClick={onTryDemoClick} />
            </div>
          </div>
        </nav>
      </Container>
    </header>
  )
}