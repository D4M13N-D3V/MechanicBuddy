'use server'

import { httpPut,httpPost } from "@/_lib/server/query-api";
import {  pushToast } from "@/_lib/server/pushToast";
import { redirect } from "next/navigation"; 

export async function createOrUpdate(
    formData: FormData
    ) {
      
     
    const id = formData.get('id');
    let vehicleId = formData.get('vehicleId');
    if(formData.get('onlyClientVehicles')=='on'){
        vehicleId = formData.get('vehicleId[value]');
    }
    const clientId = formData.get('clientId[value]');
 //Guid? ClientId, string Description, Guid? VehicleId, Guid[] AssignedTo, int? Odo, bool StartWithOffer
    const body = {
       clientId: clientId == '' ? null : clientId,
       description: formData.get('about'),
       vehicleId: vehicleId == '' ? null : vehicleId ?? null,
       assignedTo: formData.getAll('mechanics'),
       odo: +(formData.get('odo')?.toString() ?? '0'),
       startWithOffer: formData.get('isOffer') == 'on'
    }; 
    const url = "work"; 
    
    const isUpdating = !!id;
    
    const response = isUpdating?await httpPut({url:url+'/'+id,body}) : await httpPost({url,body});

    const jsonResponse = await response.json();
       
    pushToast(`Work ${(isUpdating?'updated':'saved')} successfully!`)

    if(id)
        redirect(`/home/work/${jsonResponse}`) 

    if(isUpdating){
        redirect(`/home/work/${jsonResponse.workId}/${jsonResponse.activityId}`) 
    }
    redirect(`/home/work/${jsonResponse.workId}/${jsonResponse.activityId}/edit/startfresh`) 
    
}

