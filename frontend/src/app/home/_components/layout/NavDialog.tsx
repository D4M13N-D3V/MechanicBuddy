'use client'
import {  useState } from 'react'
import { Dialog, DialogBackdrop, DialogPanel, TransitionChild } from "@headlessui/react";
import { Bars3Icon, XMarkIcon } from "@heroicons/react/24/outline";
import Nav from './Nav';
import ProfileMenu from './ProfileMenu';

export default function NavDialog({
  fullName,
  imageUrl,
}:{
  fullName: string,
  imageUrl: string,
})
{
 
    const [sidebarOpen, setSidebarOpen] = useState(false)
    return (<>
           <Dialog open={sidebarOpen} onClose={()=>setSidebarOpen(false)} className="relative z-50 lg:hidden">
                    <DialogBackdrop
                      transition
                      className="fixed inset-0 bg-gray-900/80 transition-opacity duration-300 ease-linear data-closed:opacity-0"
                    />
          
                    <div className="fixed inset-0 flex">
                      <DialogPanel
                        transition
                        className="relative mr-16 flex w-full max-w-xs flex-1 transform transition duration-300 ease-in-out data-closed:-translate-x-full"
                      >
                        <TransitionChild>
                          <div className="absolute top-0 left-full flex w-16 justify-center pt-5 duration-300 ease-in-out data-closed:opacity-0">
                            <button type="button" onClick={() => setSidebarOpen(false)} className="-m-2.5 p-2.5">
                              <span className="sr-only">Close sidebar</span>
                              <XMarkIcon aria-hidden="true" className="size-6 text-white" />
                            </button>
                          </div>
                        </TransitionChild>
                        {/* Sidebar component, swap this element with another sidebar if you like */}
                        <div className="flex grow flex-col gap-y-5 overflow-y-auto px-6 pb-2 ring-1 ring-white/10" style={{ backgroundColor: 'var(--portal-sidebar-bg, #111827)' }}>
                          <Nav fullName={fullName} imageUrl={imageUrl} onSmallScreen={true}></Nav>
                        </div>
                      </DialogPanel>
                    </div>
                  </Dialog>
                  <div className="sticky top-0 z-40 flex items-center gap-x-6 px-4 py-4 shadow-xs sm:px-6 lg:hidden" style={{ backgroundColor: 'var(--portal-sidebar-bg, #111827)' }}>
          <button type="button" onClick={() => setSidebarOpen(true)} className="-m-2.5 p-2.5 text-gray-400 lg:hidden">
            <span className="sr-only">Open sidebar</span>
            <Bars3Icon aria-hidden="true" className="size-6" />
          </button>
          <div className="flex-1 text-sm/6 font-semibold text-white"></div>
         
             {/* Profile dropdown */}
             <ProfileMenu fullName={fullName} imageUrl={imageUrl}  onSmallScreen={true}></ProfileMenu> 
        </div>
    </>)
}