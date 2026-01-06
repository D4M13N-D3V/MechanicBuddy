import Main from "../_components/Main"
import { CardHeader } from "@/_components/Card"
import { httpGet } from "@/_lib/server/query-api"
import RequestsList from "./_components/RequestsList"

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

export default async function RequestsPage() {
  let requests: ServiceRequest[] = []
  
  try {
    const response = await httpGet("servicerequest")
    requests = await response.json()
  } catch (error) {
    console.error("Failed to fetch requests:", error)
  }

  return (
    <Main
      header={
        <div className="sm:px-0">
          <CardHeader title="Service Requests" description="Customer service request submissions" />
        </div>
      }
      narrow={false}
    >
      <RequestsList initialRequests={requests} />
    </Main>
  )
}
