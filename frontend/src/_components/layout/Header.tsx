'use client'

import Link from 'next/link'
import { useState } from 'react'
import clsx from 'clsx'

import { Button } from '@/_components/layout/Button'
import { Container } from '@/_components/layout/Container'
import { NavLink } from '@/_components/layout/NavLink'

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

function MobileNavigation() {
  const [isOpen, setIsOpen] = useState(false)

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="relative z-10 flex h-8 w-8 items-center justify-center"
        aria-label="Toggle Navigation"
      >
        <MobileNavIcon open={isOpen} />
      </button>

      {/* Backdrop */}
      {isOpen && (
        <div
          className="fixed inset-0 z-40 bg-slate-300/50"
          onClick={() => setIsOpen(false)}
        />
      )}

      {/* Panel */}
      {isOpen && (
        <div className="absolute right-0 top-full z-50 mt-4 w-72 origin-top-right rounded-2xl bg-white p-4 text-lg tracking-tight text-slate-900 ring-1 shadow-xl ring-slate-900/5">
          <div className="space-y-2 mb-4">
            <a href="#features" className="block py-2 hover:text-blue-600" onClick={() => setIsOpen(false)}>Features</a>
            <a href="#pricing" className="block py-2 hover:text-blue-600" onClick={() => setIsOpen(false)}>Pricing</a>
          </div>
          <hr className="m-2 border-slate-300/40" />
          <Button
            href="https://github.com/d4m13n-de/MechanicBuddy"
            variant="outline"
            onClick={() => setIsOpen(false)}
          >
            View on GitHub
          </Button>
          <Button
            href="/auth/login"
            color="blue"
            className="mt-2"
            onClick={() => setIsOpen(false)}
          >
            Log in
          </Button>
        </div>
      )}
    </div>
  )
}

export function Header() {
  return (
    <header className="py-10">
      <Container>
        <nav className="relative z-50 flex justify-between">
          <div className="flex items-center md:gap-x-12">
            <Link href="/" aria-label="Home" className="flex items-center gap-2">
              <span className="text-xl font-bold text-slate-900">MechanicBuddy</span>
            </Link>
            <div className="hidden md:flex md:gap-x-6">
              <NavLink href="#features">Features</NavLink>
              <NavLink href="#pricing">Pricing</NavLink>
            </div>
          </div>
          <div className="flex items-center gap-x-5 md:gap-x-8">
            <div className="hidden md:block">
              <Button
                href="https://github.com/d4m13n-de/MechanicBuddy"
                variant="outline"
              >
                View on GitHub
              </Button>
            </div>
            <Button href="/auth/login" color="blue">
              <span>Log in</span>
            </Button>
            <div className="-mr-1 md:hidden">
              <MobileNavigation />
            </div>
          </div>
        </nav>
      </Container>
    </header>
  )
}
