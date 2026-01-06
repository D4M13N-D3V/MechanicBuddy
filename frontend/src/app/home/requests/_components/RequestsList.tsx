"use client"

import { useState } from "react"
import { updateRequestStatus, deleteServiceRequest } from "../actions"

interface ServiceRequest {
  id: string
  customerName: string
  phone: string
  email: string
  vehicleInfo: string
  serviceType: string
  message: string
  status: string
  submittedAt: string
  notes: string
}

const statusColors: Record<string, string> = {
  New: "bg-blue-100 text-blue-800",
  Contacted: "bg-yellow-100 text-yellow-800",
  Scheduled: "bg-purple-100 text-purple-800",
  Completed: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
}

export default function RequestsList({ initialRequests }: { initialRequests: ServiceRequest[] }) {
  const [requests, setRequests] = useState<ServiceRequest[]>(initialRequests)
  const [selectedRequest, setSelectedRequest] = useState<ServiceRequest | null>(null)
  const [updating, setUpdating] = useState(false)

  const handleUpdateStatus = async (id: string, status: string) => {
    setUpdating(true)
    try {
      const result = await updateRequestStatus(id, status)
      if (result.success) {
        setRequests(requests.map(r => r.id === id ? { ...r, status } : r))
        if (selectedRequest?.id === id) {
          setSelectedRequest({ ...selectedRequest, status })
        }
      } else {
        alert("Failed to update status: " + result.error)
      }
    } catch (error) {
      console.error("Failed to update status:", error)
      alert("Failed to update status")
    } finally {
      setUpdating(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this request?")) return
    try {
      const result = await deleteServiceRequest(id)
      if (result.success) {
        setRequests(requests.filter(r => r.id !== id))
        if (selectedRequest?.id === id) {
          setSelectedRequest(null)
        }
      } else {
        alert("Failed to delete: " + result.error)
      }
    } catch (error) {
      console.error("Failed to delete:", error)
      alert("Failed to delete")
    }
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit"
    })
  }

  return (
    <>
      <div className="mb-4 flex gap-4 items-center">
        <span className="text-sm text-gray-500">
          {requests.length} request{requests.length !== 1 ? "s" : ""}
        </span>
      </div>

      {requests.length === 0 ? (
        <div className="text-center py-8 text-gray-500">No service requests found.</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <div className="space-y-3">
            {requests.map((request) => (
              <div
                key={request.id}
                onClick={() => setSelectedRequest(request)}
                className={`p-4 bg-gray-50 rounded-lg shadow cursor-pointer transition-all hover:shadow-md border-2 ${
                  selectedRequest?.id === request.id ? "border-purple-500" : "border-transparent"
                }`}
              >
                <div className="flex justify-between items-start mb-2">
                  <div>
                    <h3 className="font-semibold text-gray-900">{request.customerName}</h3>
                    <p className="text-sm text-gray-500">{formatDate(request.submittedAt)}</p>
                  </div>
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${statusColors[request.status] || "bg-gray-100 text-gray-800"}`}>
                    {request.status}
                  </span>
                </div>
                {request.vehicleInfo && (
                  <p className="text-sm text-gray-600 mb-1">{request.vehicleInfo}</p>
                )}
                {request.serviceType && (
                  <p className="text-sm text-purple-600 font-medium">{request.serviceType}</p>
                )}
              </div>
            ))}
          </div>

          {selectedRequest && (
            <div className="bg-gray-50 rounded-lg shadow p-6 sticky top-4">
              <div className="flex justify-between items-start mb-4">
                <h2 className="text-xl font-bold text-gray-900">{selectedRequest.customerName}</h2>
                <button
                  onClick={() => setSelectedRequest(null)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  ✕
                </button>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="text-xs font-medium text-gray-500 uppercase">Status</label>
                  <select
                    value={selectedRequest.status}
                    onChange={(e) => handleUpdateStatus(selectedRequest.id, e.target.value)}
                    disabled={updating}
                    className={`mt-1 w-full px-3 py-2 rounded-lg border-2 font-medium ${statusColors[selectedRequest.status] || "bg-gray-100"} ${updating ? "opacity-50" : ""}`}
                  >
                    <option value="New">New</option>
                    <option value="Contacted">Contacted</option>
                    <option value="Scheduled">Scheduled</option>
                    <option value="Completed">Completed</option>
                    <option value="Cancelled">Cancelled</option>
                  </select>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  {selectedRequest.phone && (
                    <div>
                      <label className="text-xs font-medium text-gray-500 uppercase">Phone</label>
                      <p className="mt-1">
                        <a href={`tel:${selectedRequest.phone}`} className="text-purple-600 hover:underline">
                          {selectedRequest.phone}
                        </a>
                      </p>
                    </div>
                  )}
                  {selectedRequest.email && (
                    <div>
                      <label className="text-xs font-medium text-gray-500 uppercase">Email</label>
                      <p className="mt-1">
                        <a href={`mailto:${selectedRequest.email}`} className="text-purple-600 hover:underline">
                          {selectedRequest.email}
                        </a>
                      </p>
                    </div>
                  )}
                </div>

                {selectedRequest.vehicleInfo && (
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase">Vehicle</label>
                    <p className="mt-1 text-gray-900">{selectedRequest.vehicleInfo}</p>
                  </div>
                )}

                {selectedRequest.serviceType && (
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase">Service Requested</label>
                    <p className="mt-1 text-gray-900 font-medium">{selectedRequest.serviceType}</p>
                  </div>
                )}

                {selectedRequest.message && (
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase">Message</label>
                    <p className="mt-1 text-gray-700 bg-white p-3 rounded-lg">{selectedRequest.message}</p>
                  </div>
                )}

                <div>
                  <label className="text-xs font-medium text-gray-500 uppercase">Submitted</label>
                  <p className="mt-1 text-gray-600">{formatDate(selectedRequest.submittedAt)}</p>
                </div>

                <div className="pt-4 border-t flex gap-3">
                  {selectedRequest.phone && (
                    <a
                      href={`tel:${selectedRequest.phone}`}
                      className="flex-1 py-2 px-4 bg-green-600 text-white rounded-lg font-medium text-center hover:bg-green-700 transition-colors"
                    >
                      Call
                    </a>
                  )}
                  <button
                    onClick={() => handleDelete(selectedRequest.id)}
                    className="py-2 px-4 bg-red-100 text-red-700 rounded-lg font-medium hover:bg-red-200 transition-colors"
                  >
                    Delete
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
    </>
  )
}
