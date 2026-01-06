"use server"

import { httpPut, httpDelete } from "@/_lib/server/query-api"
import { revalidatePath } from "next/cache"

export async function updateRequestStatus(id: string, status: string) {
  try {
    await httpPut({
      url: `servicerequest/${id}/status`,
      body: { status }
    })
    revalidatePath("/home/requests")
    return { success: true }
  } catch (error) {
    console.error("Failed to update status:", error)
    return { success: false, error: String(error) }
  }
}

export async function deleteServiceRequest(id: string) {
  try {
    await httpDelete({
      url: `servicerequest/${id}`,
      body: {}
    })
    revalidatePath("/home/requests")
    return { success: true }
  } catch (error) {
    console.error("Failed to delete:", error)
    return { success: false, error: String(error) }
  }
}
