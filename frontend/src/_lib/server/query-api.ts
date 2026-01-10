'server-only'
import { redirect } from "next/navigation";
import { getJwt } from "./session";
import { headers } from "next/headers";
import { pushToast } from "./pushToast";

interface IAPICall
{
  url:string,
  authorize? : boolean,
  body?: any | null, // eslint-disable-line @typescript-eslint/no-explicit-any
  method: string
}

// Extract tenant ID from hostname (e.g., "demo-1b94f2" from "demo-1b94f2.mechanicbuddy.app")
async function getTenantIdFromHost(): Promise<string | null> {
  const headersList = await headers();
  const host = headersList.get('host');
  if (!host) return null;

  const parts = host.split('.');
  if (parts.length >= 2) {
    const tenantId = parts[0];
    // Skip common subdomains that aren't tenant IDs
    if (tenantId && tenantId !== 'www' && tenantId !== 'api' && tenantId !== 'localhost') {
      return tenantId;
    }
  }
  return null;
}

async function apiCall({
  url,
  authorize=true,
  method,
  body =null
}:IAPICall) {
  const  requestHeaders:HeadersInit =   {
   "Content-Type": "application/json",
  };
  if(authorize) {
    const jwt = await getJwt();
    requestHeaders["Authorization"] =  'Bearer ' + jwt;
  }

  // Pass tenant ID to API for multi-tenant routing
  const tenantId = await getTenantIdFromHost();
  if (tenantId) {
    requestHeaders["X-Tenant-ID"] = tenantId;
  }

  const fullUrl = process.env.API_URL +`/api/${url}`;
  const request = {
    method,
    headers: requestHeaders,
    body : body? JSON.stringify(body):null
  };
   
  const response = await fetch(fullUrl,request);
  if (!response.ok) {
    debugger;
    const responseText = await response.text();
    console.log("API response content type header: " + response.headers.get('Content-Type'));
    console.log("API threw an exception: " + responseText);
    console.log(method+' request to: '+fullUrl);
    console.log('headers: '+JSON.stringify(requestHeaders));
    console.log('body: '+request.body);
    const hasContentType = response.headers.has('Content-Type');
    let message = 'API Error occurred server side';
    let isUserError = false;
    if(hasContentType){
      const contentType = response.headers.get('Content-Type');
      if(contentType?.startsWith('application/json'))
      {
        const responseJson = JSON.parse(responseText);
        debugger;
        if (responseJson.exceptionMessage) {
            message = responseJson.exceptionMessage;
        }
        if(responseJson.isUserError)
          {
            isUserError = true;
          }
      } 
    }
  
    if(isUserError)
      {
        const headersList = await headers()
        const currentPath = headersList.get('currentPath')
  
        if(currentPath){
          pushToast(message,true);
          redirect(currentPath)  
        }
      }

     redirect(`/error?code=${response.status}&statusText=${response.statusText}&text=${message}`)
  }
  return response;
}

export async function httpGet(url: string) {
    return apiCall({
      url,
      method:"GET"
    }); 
}

export async function httpDelete({
  url,
  body
}:{
  url:string,
  body?:any,// eslint-disable-line @typescript-eslint/no-explicit-any
}) {
  return apiCall({
    url,
    method:"DELETE", 
    body, 
  }); 
}

export async function httpPost({
  url,
  body, 
  authorize=true, 
}:{
  url:string,
  body:any,// eslint-disable-line @typescript-eslint/no-explicit-any
  authorize?: boolean | undefined 
}) {
  return apiCall({
    url,
    method:"POST",
    authorize,
    body, 
  }); 
}

export async function httpPut({
  url,
  body, 
  authorize=true, 
}:{
  url:string,
  body:any,// eslint-disable-line @typescript-eslint/no-explicit-any
  authorize?: boolean | undefined
  verboseLog?: boolean | undefined
}) {
  return apiCall({
    url,
    method:"PUT",
    authorize,
    body, 
  }); 
}