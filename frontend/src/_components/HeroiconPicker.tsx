'use client'

import { useState, useRef, useEffect } from 'react'
import * as OutlineIcons from '@heroicons/react/24/outline'
import { ChevronDownIcon } from '@heroicons/react/24/outline'

// Get all available icon names
const iconNames = Object.keys(OutlineIcons).filter(name => name.endsWith('Icon'))

// Common icons to show at the top - automotive/service related first
const commonIcons = [
    // Automotive & Tools
    'WrenchIcon',
    'WrenchScrewdriverIcon',
    'CogIcon',
    'Cog6ToothIcon',
    'Cog8ToothIcon',
    'TruckIcon',
    'KeyIcon',
    'BoltIcon',
    'FireIcon',
    'BeakerIcon',
    'CpuChipIcon',
    'CircleStackIcon',
    'Battery0Icon',
    'Battery50Icon',
    'Battery100Icon',
    'SignalIcon',
    'WifiIcon',
    // Service & Support
    'PhoneIcon',
    'PhoneArrowDownLeftIcon',
    'PhoneArrowUpRightIcon',
    'ChatBubbleLeftIcon',
    'ChatBubbleLeftRightIcon',
    'ChatBubbleOvalLeftIcon',
    'EnvelopeIcon',
    'EnvelopeOpenIcon',
    'InboxIcon',
    'InboxArrowDownIcon',
    'InboxStackIcon',
    'ClipboardIcon',
    'ClipboardDocumentIcon',
    'ClipboardDocumentCheckIcon',
    'ClipboardDocumentListIcon',
    'DocumentIcon',
    'DocumentTextIcon',
    'DocumentCheckIcon',
    'DocumentDuplicateIcon',
    'DocumentMagnifyingGlassIcon',
    // Time & Scheduling
    'ClockIcon',
    'CalendarIcon',
    'CalendarDaysIcon',
    'CalendarDateRangeIcon',
    // Money & Business
    'CurrencyDollarIcon',
    'CurrencyEuroIcon',
    'CurrencyPoundIcon',
    'CurrencyYenIcon',
    'BanknotesIcon',
    'CreditCardIcon',
    'ReceiptPercentIcon',
    'ReceiptRefundIcon',
    'CalculatorIcon',
    'PresentationChartLineIcon',
    'PresentationChartBarIcon',
    'ChartBarIcon',
    'ChartPieIcon',
    'BuildingOfficeIcon',
    'BuildingOffice2Icon',
    'BuildingStorefrontIcon',
    'BriefcaseIcon',
    // Verification & Security
    'CheckIcon',
    'CheckCircleIcon',
    'CheckBadgeIcon',
    'ShieldCheckIcon',
    'ShieldExclamationIcon',
    'LockClosedIcon',
    'LockOpenIcon',
    'FingerPrintIcon',
    'IdentificationIcon',
    // Users & People
    'UserIcon',
    'UserCircleIcon',
    'UserGroupIcon',
    'UserPlusIcon',
    'UsersIcon',
    // Navigation & Location
    'HomeIcon',
    'HomeModernIcon',
    'MapIcon',
    'MapPinIcon',
    'GlobeAltIcon',
    'GlobeAmericasIcon',
    'GlobeAsiaAustraliaIcon',
    'GlobeEuropeAfricaIcon',
    // Actions & UI
    'MagnifyingGlassIcon',
    'MagnifyingGlassPlusIcon',
    'MagnifyingGlassMinusIcon',
    'AdjustmentsHorizontalIcon',
    'AdjustmentsVerticalIcon',
    'ArrowPathIcon',
    'ArrowsRightLeftIcon',
    'ArrowTrendingUpIcon',
    'ArrowTrendingDownIcon',
    'ArrowUpIcon',
    'ArrowDownIcon',
    'ArrowLeftIcon',
    'ArrowRightIcon',
    'PlusIcon',
    'PlusCircleIcon',
    'MinusIcon',
    'MinusCircleIcon',
    'XMarkIcon',
    'XCircleIcon',
    // Status & Info
    'InformationCircleIcon',
    'ExclamationCircleIcon',
    'ExclamationTriangleIcon',
    'QuestionMarkCircleIcon',
    'BellIcon',
    'BellAlertIcon',
    'BellSlashIcon',
    'FlagIcon',
    'BookmarkIcon',
    'BookmarkSquareIcon',
    // Items & Objects
    'ShoppingCartIcon',
    'ShoppingBagIcon',
    'GiftIcon',
    'GiftTopIcon',
    'ArchiveBoxIcon',
    'ArchiveBoxArrowDownIcon',
    'CubeIcon',
    'CubeTransparentIcon',
    'TagIcon',
    'RectangleStackIcon',
    'Squares2X2Icon',
    'SquaresPlusIcon',
    'ViewColumnsIcon',
    'TableCellsIcon',
    'ListBulletIcon',
    'QueueListIcon',
    // Stars & Ratings
    'StarIcon',
    'SparklesIcon',
    'SunIcon',
    'MoonIcon',
    'HeartIcon',
    'HandThumbUpIcon',
    'HandThumbDownIcon',
    'TrophyIcon',
    // Media & Files
    'PhotoIcon',
    'CameraIcon',
    'VideoCameraIcon',
    'FilmIcon',
    'MusicalNoteIcon',
    'MicrophoneIcon',
    'SpeakerWaveIcon',
    'SpeakerXMarkIcon',
    // Tech & Devices
    'ComputerDesktopIcon',
    'DevicePhoneMobileIcon',
    'DeviceTabletIcon',
    'PrinterIcon',
    'ServerIcon',
    'ServerStackIcon',
    'CloudIcon',
    'CloudArrowUpIcon',
    'CloudArrowDownIcon',
    'CommandLineIcon',
    'CodeBracketIcon',
    'CodeBracketSquareIcon',
    // Misc
    'LightBulbIcon',
    'RocketLaunchIcon',
    'PaperAirplaneIcon',
    'PuzzlePieceIcon',
    'ScissorsIcon',
    'PaintBrushIcon',
    'SwatchIcon',
    'EyeIcon',
    'EyeSlashIcon',
    'PencilIcon',
    'PencilSquareIcon',
    'TrashIcon',
    'FolderIcon',
    'FolderOpenIcon',
    'FolderPlusIcon',
    'LinkIcon',
    'PaperClipIcon',
    'WindowIcon',
    'Bars3Icon',
    'Bars4Icon',
    'EllipsisHorizontalIcon',
    'EllipsisVerticalIcon',
]

interface HeroiconPickerProps {
    name: string
    label?: string
    defaultValue?: string
}

export default function HeroiconPicker({ name, label, defaultValue = 'WrenchIcon' }: HeroiconPickerProps) {
    const [isOpen, setIsOpen] = useState(false)
    const [selectedIcon, setSelectedIcon] = useState(defaultValue)
    const [search, setSearch] = useState('')
    const dropdownRef = useRef<HTMLDivElement>(null)

    // Close dropdown when clicking outside
    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsOpen(false)
            }
        }
        document.addEventListener('mousedown', handleClickOutside)
        return () => document.removeEventListener('mousedown', handleClickOutside)
    }, [])

    // Filter icons based on search
    const filteredIcons = search
        ? iconNames.filter(name => name.toLowerCase().includes(search.toLowerCase()))
        : [...commonIcons, ...iconNames.filter(name => !commonIcons.includes(name))]

    // Get the icon component
    const getIconComponent = (iconName: string) => {
        const Icon = (OutlineIcons as Record<string, React.ComponentType<{ className?: string }>>)[iconName]
        return Icon ? <Icon className="h-5 w-5" /> : null
    }

    return (
        <div className="relative" ref={dropdownRef}>
            {label && (
                <label className="block text-sm font-medium text-gray-900 mb-2">
                    {label}
                </label>
            )}
            <input type="hidden" name={name} value={selectedIcon} />
            <button
                type="button"
                onClick={() => setIsOpen(!isOpen)}
                className="relative w-full cursor-pointer rounded-md bg-white py-2 pl-3 pr-10 text-left text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-600 sm:text-sm"
            >
                <span className="flex items-center gap-2">
                    {getIconComponent(selectedIcon)}
                    <span>{selectedIcon.replace('Icon', '')}</span>
                </span>
                <span className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2">
                    <ChevronDownIcon className="h-5 w-5 text-gray-400" />
                </span>
            </button>

            {isOpen && (
                <div className="absolute z-10 mt-1 max-h-80 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
                    <div className="sticky top-0 bg-white px-2 py-2 border-b">
                        <input
                            type="text"
                            placeholder="Search icons..."
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            className="w-full rounded-md border-gray-300 text-sm focus:border-indigo-500 focus:ring-indigo-500"
                            onClick={(e) => e.stopPropagation()}
                        />
                    </div>
                    <div className="grid grid-cols-6 gap-1 p-2">
                        {filteredIcons.slice(0, 120).map((iconName) => (
                            <button
                                key={iconName}
                                type="button"
                                onClick={() => {
                                    setSelectedIcon(iconName)
                                    setIsOpen(false)
                                    setSearch('')
                                }}
                                className={`flex flex-col items-center justify-center p-2 rounded hover:bg-indigo-50 ${
                                    selectedIcon === iconName ? 'bg-indigo-100 ring-2 ring-indigo-500' : ''
                                }`}
                                title={iconName}
                            >
                                {getIconComponent(iconName)}
                                <span className="text-xs mt-1 truncate w-full text-center text-gray-600">
                                    {iconName.replace('Icon', '').slice(0, 8)}
                                </span>
                            </button>
                        ))}
                    </div>
                    {filteredIcons.length > 60 && (
                        <p className="text-xs text-gray-500 text-center py-2">
                            Showing 60 of {filteredIcons.length} icons. Use search to find more.
                        </p>
                    )}
                </div>
            )}
        </div>
    )
}
