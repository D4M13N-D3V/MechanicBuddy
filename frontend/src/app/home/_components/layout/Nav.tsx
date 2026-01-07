'use client'
import Image from "next/image"
import ProfileMenu from "./ProfileMenu"
import { InboxIcon,
    Cog6ToothIcon,
    QueueListIcon,
    TruckIcon,
    UsersIcon,
  } from '@heroicons/react/24/outline'
import clsx from "clsx";
import { usePathname } from "next/navigation"

const navigationIconClass = "size-6 shrink-0";
const navigation = [
    // { name: 'Dashboard', href: '/home', icon: <HomeIcon aria-hidden="true" className={navigationIconClass}></HomeIcon>},
    { name: 'Work', href: '/home/work', icon: <QueueListIcon aria-hidden="true" className={navigationIconClass}></QueueListIcon> },
    { name: 'Clients', href: '/home/clients', icon: <UsersIcon aria-hidden="true" className={navigationIconClass}></UsersIcon>  },
    { name: 'Vehicles', href: '/home/vehicles', icon: <TruckIcon aria-hidden="true" className={navigationIconClass}></TruckIcon>  },
    { name: 'Inventory', href: '/home/inventory', icon: <Cog6ToothIcon aria-hidden="true" className={navigationIconClass}></Cog6ToothIcon>  },
    { name: 'Requests', href: '/home/requests', icon: <InboxIcon aria-hidden="true" className={navigationIconClass}></InboxIcon>  },
    // { name: 'Services', href: '/home/services', icon: <WrenchScrewdriverIcon aria-hidden="true" className={navigationIconClass}></WrenchScrewdriverIcon>  },
]


export default function Nav({
    onSmallScreen,
    fullName,
    imageUrl,
    logoUrl,
}:{
    onSmallScreen: boolean,
    fullName: string,
    imageUrl: string,
    logoUrl: string,
}) {
    const currentPath = usePathname();

    return (
        <>
            <div className="flex h-16 shrink-0 items-center">
                {logoUrl.startsWith('data:') ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img
                        alt="Logo"
                        className="h-8 w-auto"
                        src={logoUrl}
                    />
                ) : (
                    <Image
                        alt="Logo"
                        width="50"
                        height="50"
                        className="h-8 w-auto"
                        src={logoUrl}
                        unoptimized
                    />
                )}
            </div>
            <nav className="flex flex-1 flex-col">
                <ul role="list" className="flex flex-1 flex-col gap-y-7">
                    <li>
                        <ul role="list" className="-mx-2 space-y-1">
                            {navigation.map((item) => (
                                <li key={item.name}>
                                    <a
                                        href={item.href}
                                        className={clsx(
                                               (item.href !=='/home'  &&currentPath?.startsWith(item.href) || item.href =='/home'&& currentPath === '/home') //home is ambigous
                                                ? 'text-white'
                                                : 'hover:text-white',
                                            'group flex gap-x-3 rounded-md p-2 text-sm/6 font-semibold',
                                        )}
                                        style={{
                                            backgroundColor: (item.href !=='/home' && currentPath?.startsWith(item.href) || item.href =='/home'&& currentPath === '/home')
                                                ? 'var(--portal-sidebar-active-bg, #1f2937)'
                                                : 'transparent',
                                            color: (item.href !=='/home' && currentPath?.startsWith(item.href) || item.href =='/home'&& currentPath === '/home')
                                                ? 'var(--portal-sidebar-active-text, #ffffff)'
                                                : 'var(--portal-sidebar-text, #9ca3af)',
                                        }}
                                    >
                                        {item.icon}
                                        {item.name}
                                    </a>
                                </li>
                            ))}
                        </ul>
                    </li>
                    {!onSmallScreen && <li className="mt-auto flex flex-col mb-5">
                        <a
                            href="/home/settings"
                            className="group -mx-2 flex gap-x-3 rounded-md p-2 text-sm/6 font-semibold hover:text-white"
                            style={{
                                color: 'var(--portal-sidebar-text, #9ca3af)',
                            }}
                        >
                            <Cog6ToothIcon aria-hidden="true" className="size-6 shrink-0" />
                            Settings
                        </a>
                        <ProfileMenu fullName={fullName} imageUrl={imageUrl} onSmallScreen={false}></ProfileMenu>
                    </li>}
                </ul>
            </nav>

        </>
    )
}