"use client"

import { Container } from "@/_components/layout/Container"
import Link from "next/link"
import { useState } from "react"
import {
  WrenchScrewdriverIcon,
  PhoneIcon,
  MapPinIcon,
  ClockIcon,
  CheckCircleIcon,
  LightBulbIcon,
  CogIcon,
  BeakerIcon,
  TruckIcon,
} from "@heroicons/react/24/outline"

// Joker color theme
const PURPLE = "#7c3aed"
const PURPLE_DARK = "#5b21b6"
const PURPLE_LIGHT = "#ede9fe"
const GREEN = "#22c55e"
const GREEN_DARK = "#15803d"

// Navigation component
function Navigation() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)

  const navLinks = [
    { name: "Services", href: "#services" },
    { name: "About Us", href: "#about" },
    { name: "Auto Tips", href: "#tips" },
    { name: "Contact", href: "#contact" },
  ]

  return (
    <header className="bg-slate-900 text-white">
      <div style={{ backgroundColor: PURPLE }} className="py-2">
        <Container>
          <div className="flex flex-wrap justify-between items-center text-sm">
            <div className="flex items-center gap-6">
              <a href="tel:+13366898898" className="flex items-center gap-2 hover:text-slate-200 transition-colors">
                <PhoneIcon className="h-4 w-4" />
                <span>(336) 689-8898</span>
              </a>
              <span className="hidden sm:flex items-center gap-2">
                <MapPinIcon className="h-4 w-4" />
                <span>4610 West Gate City, Greensboro, NC</span>
              </span>
            </div>
            <span className="hidden md:flex items-center gap-2">
              <ClockIcon className="h-4 w-4" />
              <span>Mon-Fri: 10:30am - 6pm | Sat: 10:30am - 5pm</span>
            </span>
          </div>
        </Container>
      </div>

      <Container>
        <nav className="relative flex justify-between items-center py-4">
          <Link href="/" className="flex items-center gap-3">
            <span className="text-xl font-bold tracking-tight">3J&apos;s Auto Repairs</span>
          </Link>

          <div className="hidden md:flex items-center gap-6">
            {navLinks.map((link) => (
              <a
                key={link.name}
                href={link.href}
                className="text-sm font-medium transition-colors hover:opacity-80 px-2"
              >
                {link.name}
              </a>
            ))}
            <Link
              href="/auth/login"
              style={{ backgroundColor: GREEN }}
              className="ml-2 px-5 py-2.5 rounded-lg text-sm font-semibold transition-all shadow-lg hover:opacity-90"
            >
              Mechanic Portal
            </Link>
          </div>

          {/* Mobile menu button */}
          <button
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="md:hidden p-2 hover:bg-slate-800 rounded-lg transition-colors"
            aria-label="Toggle menu"
          >
            {mobileMenuOpen ? (
              <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            ) : (
              <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            )}
          </button>

          {/* Mobile menu panel */}
          {mobileMenuOpen && (
            <>
              {/* Backdrop */}
              <div
                className="fixed inset-0 z-40 bg-black/50"
                onClick={() => setMobileMenuOpen(false)}
              />
              {/* Menu */}
              <div className="absolute top-full left-0 right-0 z-50 mt-2 mx-4 bg-slate-800 rounded-xl shadow-xl p-4">
                <div className="flex flex-col gap-2">
                  {navLinks.map((link) => (
                    <a
                      key={link.name}
                      href={link.href}
                      onClick={() => setMobileMenuOpen(false)}
                      className="text-base font-medium py-3 px-4 rounded-lg hover:bg-slate-700 transition-colors"
                    >
                      {link.name}
                    </a>
                  ))}
                  <Link
                    href="/auth/login"
                    onClick={() => setMobileMenuOpen(false)}
                    style={{ backgroundColor: GREEN }}
                    className="mt-2 px-5 py-3 rounded-lg text-base font-semibold text-center transition-all shadow-lg hover:opacity-90"
                  >
                    Mechanic Portal
                  </Link>
                </div>
              </div>
            </>
          )}
        </nav>
      </Container>
    </header>
  )
}

function HeroSection() {
  return (
    <section 
      className="relative text-white py-20 lg:py-28"
      style={{ background: `linear-gradient(135deg, #1a1a2e 0%, ${PURPLE_DARK} 50%, #1a1a2e 100%)` }}
    >
      <div 
        className="absolute inset-0 bg-cover bg-center opacity-15"
        style={{ backgroundImage: "url(/charger-interior.jpg)" }}
      />
      <div className="absolute inset-0 bg-gradient-to-b from-black/50 to-black/70" />
      
      <Container className="relative z-10">
        <div className="max-w-4xl mx-auto text-center">
          <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold mb-6 leading-tight tracking-tight">
            Professional Auto Repair You Can Trust
          </h1>
          <p className="text-lg md:text-xl text-slate-300 mb-4 leading-relaxed max-w-3xl mx-auto">
            Serving Greensboro with honest service, expert technicians, and reliable repairs. Your car deserves the best care at 3J&apos;s Auto Repairs.
          </p>
          <p className="text-base md:text-lg text-slate-400 mb-8 leading-relaxed max-w-2xl mx-auto">
            <span style={{ color: GREEN }} className="font-semibold">Specializing in Chryslers, Chargers &amp; Challengers</span> — We know your Mopar inside and out!
          </p>
          <div className="flex flex-wrap justify-center gap-4">
            <a
              href="#services"
              style={{ backgroundColor: PURPLE }}
              className="px-6 py-3 rounded-lg text-base font-semibold transition-all shadow-xl inline-flex items-center gap-2 hover:opacity-90"
            >
              <WrenchScrewdriverIcon className="h-5 w-5" />
              Our Services
            </a>
            <a
              href="#contact"
              style={{ backgroundColor: GREEN }}
              className="px-6 py-3 rounded-lg text-base font-semibold transition-all shadow-xl inline-flex items-center gap-2 hover:opacity-90"
            >
              <PhoneIcon className="h-5 w-5" />
              Contact Us
            </a>
          </div>
        </div>
      </Container>
    </section>
  )
}

function ServiceCard({ icon: Icon, title, description, isGreen }: { icon: React.ElementType; title: string; description: string; isGreen?: boolean }) {
  return (
    <div className="bg-white rounded-2xl p-6 shadow-lg hover:shadow-xl transition-all duration-300 group">
      <div 
        style={{ backgroundColor: isGreen ? GREEN : PURPLE }}
        className="w-14 h-14 rounded-xl flex items-center justify-center mb-4 shadow-md group-hover:scale-105 transition-transform duration-300"
      >
        <Icon className="h-7 w-7 text-white" />
      </div>
      <h3 className="text-lg font-bold text-slate-900 mb-2">{title}</h3>
      <p className="text-slate-600 text-sm leading-relaxed">{description}</p>
    </div>
  )
}

function ServicesSection() {
  const services = [
    { icon: CogIcon, title: "Chrysler/Dodge Specialists", description: "Expert service for Chryslers, Chargers, and Challengers. We know Mopar vehicles inside and out — trust the specialists.", isGreen: true },
    { icon: CogIcon, title: "Oil Change", description: "Regular oil changes to keep your engine running smoothly. We use quality oils and filters for all makes and models.", isGreen: false },
    { icon: WrenchScrewdriverIcon, title: "Brake Service", description: "Complete brake inspections, pad replacements, rotor resurfacing, and brake fluid flushes for your safety.", isGreen: true },
    { icon: CogIcon, title: "Engine Repair", description: "From minor tune-ups to major engine overhauls, our certified technicians handle it all.", isGreen: false },
    { icon: WrenchScrewdriverIcon, title: "Transmission", description: "Transmission fluid changes, repairs, and rebuilds. We keep your vehicle shifting smoothly.", isGreen: true },
    { icon: BeakerIcon, title: "Tire Service", description: "Tire rotations, balancing, and new tire installations. Keep your ride smooth and safe. Note: We do not offer alignments.", isGreen: false },
    { icon: BeakerIcon, title: "Diagnostics", description: "State-of-the-art diagnostic equipment to quickly identify and resolve any vehicle issues.", isGreen: false },
    { icon: TruckIcon, title: "Towing Service", description: "Stranded? We offer reliable towing services to get your vehicle to our shop safely. Call us anytime!", isGreen: true },
  ]

  return (
    <section id="services" className="py-16 bg-slate-100">
      <Container>
        <div className="text-center mb-12">
          <span style={{ color: PURPLE }} className="text-sm font-bold uppercase tracking-wider mb-2 block">What We Offer</span>
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">Our Services</h2>
          <p className="text-base text-slate-600 max-w-2xl mx-auto leading-relaxed">
            We offer comprehensive auto repair and maintenance services to keep your vehicle running at its best. Specializing in Chryslers, Chargers, and Challengers.
          </p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {services.map((service, index) => (
            <ServiceCard key={index} {...service} />
          ))}
        </div>
      </Container>
    </section>
  )
}

function AboutSection() {
  const features = [
    "Chrysler/Charger/Challenger Experts",
    "State-of-the-Art Equipment",
    "Quality Parts & Materials",
    "Transparent Pricing",
    "Towing Available",
    "Family Owned & Operated",
  ]

  return (
    <section id="about" className="py-16 bg-slate-900 text-white">
      <Container>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          <div>
            <span style={{ color: GREEN }} className="text-sm font-bold uppercase tracking-wider mb-2 block">About Us</span>
            <h2 className="text-3xl md:text-4xl font-bold mb-6 leading-tight">Your Mopar Specialists in Greensboro</h2>
            <p className="text-slate-300 mb-4 text-base leading-relaxed">
              3J&apos;s Auto Repairs is dedicated to providing honest, reliable service to our Greensboro community. We specialize in Chryslers, Chargers, and Challengers — if you drive a Mopar, you&apos;ve found the right shop.
            </p>
            <p className="text-slate-400 mb-8 text-sm leading-relaxed">
              Our technicians deliver skilled diagnostics, preventive maintenance, and quality repairs for all makes and models. We also offer towing services to get your vehicle to us safely. Your satisfaction and safety are our top priorities.
            </p>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              {features.map((feature, index) => (
                <div key={index} className="flex items-center gap-2 p-2 bg-slate-800/50 rounded-lg">
                  <CheckCircleIcon style={{ color: index % 2 === 0 ? PURPLE : GREEN }} className="h-5 w-5 flex-shrink-0" />
                  <span className="text-slate-200 text-sm font-medium">{feature}</span>
                </div>
              ))}
            </div>
          </div>
          <div className="relative">
            <div style={{ background: `linear-gradient(135deg, ${PURPLE} 0%, ${GREEN_DARK} 100%)` }} className="rounded-2xl p-8 text-center shadow-2xl">
              <div className="text-6xl font-bold mb-2">22+</div>
              <div className="text-lg font-medium opacity-90">Years Combined Experience</div>
              <div className="mt-6 pt-6 border-t border-white/20">
                <div className="text-4xl font-bold mb-2">20,000+</div>
                <div className="text-base font-medium opacity-90">Satisfied Customers</div>
              </div>
            </div>
          </div>
        </div>
      </Container>
    </section>
  )
}

function TipsSection() {
  const tips = [
    { title: "Check Your Oil Regularly", description: "Check your oil level at least once a month. Low oil can cause serious engine damage." },
    { title: "Monitor Tire Pressure", description: "Proper tire pressure improves fuel economy and extends tire life. Check monthly." },
    { title: "Listen to Your Brakes", description: "Squealing or grinding sounds indicate worn brake pads. Don't ignore warning signs." },
    { title: "Keep Up with Fluid Changes", description: "Transmission fluid, coolant, and brake fluid all need periodic replacement." },
    { title: "Watch Your Warning Lights", description: "If your check engine light comes on, get it diagnosed promptly." },
    { title: "Replace Wipers & Filters", description: "Change wiper blades every 6-12 months and air filters every 12,000-15,000 miles." },
  ]

  return (
    <section id="tips" className="py-16 bg-white">
      <Container>
        <div className="text-center mb-12">
          <div style={{ backgroundColor: PURPLE_LIGHT }} className="inline-flex items-center justify-center w-14 h-14 rounded-full mb-4">
            <LightBulbIcon style={{ color: PURPLE }} className="h-7 w-7" />
          </div>
          <span style={{ color: GREEN }} className="text-sm font-bold uppercase tracking-wider mb-2 block">Expert Advice</span>
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">Auto Care Tips</h2>
          <p className="text-base text-slate-600 max-w-2xl mx-auto leading-relaxed">
            Keep your vehicle in top shape with these helpful maintenance tips from our experts.
          </p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {tips.map((tip, index) => (
            <div key={index} className="bg-slate-50 rounded-2xl p-6 shadow-md hover:shadow-lg transition-all duration-300">
              <div style={{ backgroundColor: index % 2 === 0 ? PURPLE : GREEN }} className="w-10 h-10 rounded-full flex items-center justify-center mb-4 text-white font-bold text-sm">
                {index + 1}
              </div>
              <h3 className="text-base font-bold text-slate-900 mb-2">{tip.title}</h3>
              <p className="text-slate-600 text-sm leading-relaxed">{tip.description}</p>
            </div>
          ))}
        </div>
      </Container>
    </section>
  )
}

function ServiceRequestForm() {
  const [formData, setFormData] = useState({
    customerName: "",
    phone: "",
    email: "",
    vehicleInfo: "",
    serviceType: "",
    message: ""
  })
  const [status, setStatus] = useState<"idle" | "loading" | "success" | "error">("idle")
  const [errorMessage, setErrorMessage] = useState("")

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setStatus("loading")
    setErrorMessage("")

    try {
      const response = await fetch("/api/servicerequest/submit", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData)
      })

      if (response.ok) {
        setStatus("success")
        setFormData({ customerName: "", phone: "", email: "", vehicleInfo: "", serviceType: "", message: "" })
      } else {
        const data = await response.json()
        setErrorMessage(data.message || "Something went wrong. Please try again.")
        setStatus("error")
      }
    } catch {
      setErrorMessage("Unable to submit. Please call us instead.")
      setStatus("error")
    }
  }

  if (status === "success") {
    return (
      <div className="bg-white rounded-2xl p-8 shadow-xl text-center">
        <div style={{ backgroundColor: GREEN }} className="w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4">
          <CheckCircleIcon className="h-8 w-8 text-white" />
        </div>
        <h3 className="text-xl font-bold text-slate-900 mb-2">Request Submitted!</h3>
        <p className="text-slate-600">Thank you! We&apos;ll contact you soon to schedule your service.</p>
        <button
          onClick={() => setStatus("idle")}
          style={{ backgroundColor: PURPLE }}
          className="mt-6 px-6 py-2 rounded-lg text-white font-semibold hover:opacity-90 transition-all"
        >
          Submit Another Request
        </button>
      </div>
    )
  }

  return (
    <form onSubmit={handleSubmit} className="bg-white rounded-2xl p-8 shadow-xl">
      <h3 className="text-xl font-bold text-slate-900 mb-6">Request Service</h3>
      
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Your Name *</label>
          <input
            type="text"
            required
            value={formData.customerName}
            onChange={(e) => setFormData({ ...formData, customerName: e.target.value })}
            className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent outline-none transition-all"
            placeholder="John Doe"
          />
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Phone</label>
            <input
              type="tel"
              value={formData.phone}
              onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
              className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent outline-none transition-all"
              placeholder="(336) 555-0123"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Email</label>
            <input
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent outline-none transition-all"
              placeholder="john@example.com"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Vehicle Info</label>
          <input
            type="text"
            value={formData.vehicleInfo}
            onChange={(e) => setFormData({ ...formData, vehicleInfo: e.target.value })}
            className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent outline-none transition-all"
            placeholder="2020 Dodge Charger R/T"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Service Needed</label>
          <select
            value={formData.serviceType}
            onChange={(e) => setFormData({ ...formData, serviceType: e.target.value })}
            className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent outline-none transition-all"
          >
            <option value="">Select a service...</option>
            <option value="Oil Change">Oil Change</option>
            <option value="Brake Service">Brake Service</option>
            <option value="Engine Repair">Engine Repair</option>
            <option value="Transmission">Transmission</option>
            <option value="Tire Service">Tire Service</option>
            <option value="Diagnostics">Diagnostics</option>
            <option value="Towing">Towing</option>
            <option value="Other">Other</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Message</label>
          <textarea
            value={formData.message}
            onChange={(e) => setFormData({ ...formData, message: e.target.value })}
            rows={3}
            className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent outline-none transition-all resize-none"
            placeholder="Tell us more about what you need..."
          />
        </div>

        {status === "error" && (
          <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {errorMessage}
          </div>
        )}

        <button
          type="submit"
          disabled={status === "loading"}
          style={{ backgroundColor: PURPLE }}
          className="w-full py-3 rounded-lg text-white font-semibold hover:opacity-90 transition-all disabled:opacity-50"
        >
          {status === "loading" ? "Submitting..." : "Submit Request"}
        </button>

        <p className="text-xs text-slate-500 text-center">
          Or call us directly at <a href="tel:+13366898898" className="font-semibold hover:underline">(336) 689-8898</a>
        </p>
      </div>
    </form>
  )
}

function ContactSection() {
  return (
    <section id="contact" className="py-16 bg-slate-100">
      <Container>
        <div className="text-center mb-12">
          <span style={{ color: PURPLE }} className="text-sm font-bold uppercase tracking-wider mb-2 block">Get In Touch</span>
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">Contact Us</h2>
          <p className="text-base text-slate-600 max-w-2xl mx-auto leading-relaxed">
            Have questions or need to schedule service? Fill out the form or give us a call!
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <ServiceRequestForm />

          <div className="bg-slate-900 text-white rounded-2xl p-8 shadow-xl">
            <h3 className="text-xl font-bold mb-8">Contact Info</h3>
            <div className="space-y-6">
              <div className="flex items-start gap-4">
                <div style={{ backgroundColor: PURPLE }} className="p-3 rounded-xl">
                  <MapPinIcon className="h-5 w-5" />
                </div>
                <div>
                  <h4 className="font-semibold text-base mb-1">Address</h4>
                  <p className="text-slate-300 text-sm leading-relaxed">4610 West Gate City<br />Greensboro, NC</p>
                </div>
              </div>
              <div className="flex items-start gap-4">
                <div style={{ backgroundColor: GREEN }} className="p-3 rounded-xl">
                  <PhoneIcon className="h-5 w-5" />
                </div>
                <div>
                  <h4 className="font-semibold text-base mb-1">Phone</h4>
                  <a href="tel:+13366898898" className="text-slate-300 hover:text-white transition-colors">(336) 689-8898</a>
                </div>
              </div>
              <div className="flex items-start gap-4">
                <div style={{ backgroundColor: PURPLE }} className="p-3 rounded-xl">
                  <TruckIcon className="h-5 w-5" />
                </div>
                <div>
                  <h4 className="font-semibold text-base mb-1">Towing</h4>
                  <p className="text-slate-300 text-sm">Towing service available — call us!</p>
                </div>
              </div>
              <div className="flex items-start gap-4">
                <div style={{ backgroundColor: GREEN }} className="p-3 rounded-xl">
                  <ClockIcon className="h-5 w-5" />
                </div>
                <div>
                  <h4 className="font-semibold text-base mb-1">Hours</h4>
                  <div className="text-slate-300 text-sm space-y-0.5">
                    <p>Monday: 11:00am - 6:00pm</p>
                    <p>Tuesday: 10:30am - 5:00pm</p>
                    <p>Wednesday: 11:00am - 6:00pm</p>
                    <p>Thursday: 10:30am - 5:00pm</p>
                    <p>Friday: 11:00am - 6:00pm</p>
                    <p>Saturday: 10:30am - 5:00pm</p>
                    <p>Sunday: Closed</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Container>
    </section>
  )
}

function Footer() {
  return (
    <footer className="bg-slate-900 text-white">
      <Container>
        <div className="py-12 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
          <div className="lg:col-span-2">
            <div className="flex items-center gap-3 mb-4">
              <span className="text-lg font-bold tracking-tight">3J&apos;s Auto Repairs</span>
            </div>
            <p className="text-slate-400 text-sm leading-relaxed max-w-md">
              Professional auto repair and maintenance services you can trust. Specializing in Chryslers, Chargers, and Challengers. Proudly serving Greensboro, NC.
            </p>
          </div>
          <div>
            <h4 className="font-bold text-base mb-4">Quick Links</h4>
            <ul className="space-y-2 text-slate-400 text-sm">
              <li><a href="#services" className="hover:text-white transition-colors">Services</a></li>
              <li><a href="#about" className="hover:text-white transition-colors">About Us</a></li>
              <li><a href="#tips" className="hover:text-white transition-colors">Auto Tips</a></li>
              <li><a href="#contact" className="hover:text-white transition-colors">Contact</a></li>
              <li><Link href="/auth/login" className="hover:text-white transition-colors">Mechanic Portal</Link></li>
            </ul>
          </div>
          <div>
            <h4 className="font-bold text-base mb-4">Contact Info</h4>
            <ul className="space-y-2 text-slate-400 text-sm">
              <li className="flex items-start gap-2">
                <MapPinIcon className="h-4 w-4 flex-shrink-0 mt-0.5" />
                <span>4610 West Gate City, Greensboro, NC</span>
              </li>
              <li>
                <a href="tel:+13366898898" className="flex items-center gap-2 hover:text-white transition-colors">
                  <PhoneIcon className="h-4 w-4 flex-shrink-0" />
                  <span>(336) 689-8898</span>
                </a>
              </li>
              <li className="flex items-center gap-2">
                <TruckIcon className="h-4 w-4 flex-shrink-0" />
                <span>Towing Available</span>
              </li>
            </ul>
          </div>
        </div>
        <div className="border-t border-slate-800 py-6 text-center text-slate-500 text-sm">
          <p>© {new Date().getFullYear()} 3J&apos;s Auto Repairs. All rights reserved.</p>
        </div>
      </Container>
    </footer>
  )
}

export default function Home() {
  return (
    <>
      <Navigation />
      <main>
        <HeroSection />
        <ServicesSection />
        <AboutSection />
        <TipsSection />
        <ContactSection />
      </main>
      <Footer />
    </>
  )
}
