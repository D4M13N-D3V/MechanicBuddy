'use client'

import { useState } from "react"
import { CheckCircleIcon } from "@heroicons/react/24/outline"
import { IServiceItem } from "@/app/home/settings/branding/model"

interface ServiceRequestFormProps {
    services: IServiceItem[];
}

export function ServiceRequestForm({ services }: ServiceRequestFormProps) {
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
                <div className="w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4 bg-landing-secondary">
                    <CheckCircleIcon className="h-8 w-8 text-white" />
                </div>
                <h3 className="text-xl font-bold text-slate-900 mb-2">Request Submitted!</h3>
                <p className="text-slate-600">Thank you! We&apos;ll contact you soon to schedule your service.</p>
                <button
                    onClick={() => setStatus("idle")}
                    className="mt-6 px-6 py-2 rounded-lg text-white font-semibold hover:opacity-90 transition-all bg-landing-primary"
                >
                    Submit Another Request
                </button>
            </div>
        )
    }

    const activeServices = services.filter(s => s.isActive);

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
                            placeholder="(555) 555-0123"
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
                        placeholder="2020 Honda Accord"
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
                        {activeServices.map((service) => (
                            <option key={service.id} value={service.title}>{service.title}</option>
                        ))}
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
                    className="w-full py-3 rounded-lg text-white font-semibold hover:opacity-90 transition-all disabled:opacity-50 bg-landing-primary"
                >
                    {status === "loading" ? "Submitting..." : "Submit Request"}
                </button>
            </div>
        </form>
    )
}
