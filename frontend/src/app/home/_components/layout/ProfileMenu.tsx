
import {  Menu, MenuButton, MenuItem, MenuItems  } from '@headlessui/react'
import {  
  EllipsisVerticalIcon, 
} from '@heroicons/react/24/outline'
import clsx from "clsx" 
import Image from 'next/image' 
const userNavigation = [
    { name: 'My profile', href: '/home/profile' },
    { name: 'Sign out', href: '/home/logout' },
]

 
export default function ProfileMenu({
    onSmallScreen, 
    fullName,
    imageUrl,
}:{
    onSmallScreen:boolean, 
    fullName:string,
    imageUrl:string
})

{     
   
    return (
        <> 
          <Menu as="div" className="relative">
                               <MenuButton className={clsx(onSmallScreen&&"-m-1.5","flex items-center p-1.5")}>
                                   <span className="sr-only">Open user menu</span>

                                    <Image alt={fullName}   
                                       src={imageUrl}
                                       width="100"
                                       height="100"
                                       className="size-8 rounded-full bg-gray-50" />  

                                   <span className="hidden lg:flex lg:items-center min-w-0">
                                       <span aria-hidden="true" className="ml-4 text-sm/6 font-semibold text-white truncate max-w-[120px]">
                                           {fullName}
                                       </span>
                                       <EllipsisVerticalIcon aria-hidden="true" className="ml-2 size-5 text-white shrink-0" />
                                   </span>
                               </MenuButton>
                               <MenuItems
                               modal={false}
                                   transition
                                   className={clsx(!onSmallScreen&&"bottom-full","absolute right-0 z-10 mt-2.5 w-32 origin-top-right rounded-md bg-white py-2 ring-1 shadow-lg ring-gray-900/5 transition focus:outline-hidden data-closed:scale-95 data-closed:transform data-closed:opacity-0 data-enter:duration-100 data-enter:ease-out data-leave:duration-75 data-leave:ease-in")}
                               >
                                   {userNavigation.map((item) => (
                                       <MenuItem key={item.name}>
                                           <a  href={item.href} 
                                               className="block px-3 py-1 text-sm/6 text-gray-900 data-focus:bg-gray-50 data-focus:outline-hidden"
                                           >{item.name}
                                           </a>
                                       </MenuItem>
                                   ))}
                               </MenuItems>
                           </Menu>
                           </>
    )
}