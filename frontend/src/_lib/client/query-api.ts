import {
    getCookie
  } from 'cookies-next/client';

// Use relative path for client-side API calls - Next.js rewrites will proxy to backend
const basePath = '/backend-api';

const getJwtToken =()=>{
  
    const jwt = getCookie('jwt');
    return jwt;
}

 

const dataPage =({
    resourceName,
    searchText,
    whenReady,
    onFailure
}:{
    resourceName:string,
    searchText:string,
    whenReady:IOnQuerySuccess,
    onFailure:IOnQueryFailure
})=> {
 
    const url = resourceName+'/page?'+new URLSearchParams({
      orderby: 'id',
      searchText: searchText,
      limit: '20',
      offset: '0'
    })
    query({
      url: url,
      method : 'GET',
      onSuccess: (result) => { 
        if (result.items) {
          whenReady(result.items); 
        } 
        else  {
          console.log(result); 
          return [];
        }
      },
      onFailure: onFailure
    });
  }

  export interface IOnQuerySuccess
  {
    (json:any):void // eslint-disable-line @typescript-eslint/no-explicit-any
  }
  export interface IOnQueryFailure
  {
    ({
        url,
        status,
        text, 
    }:{
        url:string,
        status?:number |undefined,
        text:string, 
    }):void
  }

  const query = ({ 
     url, 
     method, 
     body, 
     onSuccess, 
     onFailure 
    }:{
        url:string,
        method:string,
        body?: any | null, // eslint-disable-line @typescript-eslint/no-explicit-any
        onSuccess:IOnQuerySuccess,
        onFailure:IOnQueryFailure

    }) => {
   
    const jwt = getJwtToken();
    const headers:HeadersInit = {
      'Content-Type': 'application/json',
    };
    if (jwt) {
      headers['Authorization'] = 'Bearer ' + jwt;
    }
     
    // basePath is now a relative path that gets rewritten by Next.js to the backend API
    url = `${basePath}/${url}`;
     
    body = !body ? undefined : JSON.stringify(body);
    fetch(url, {
      method: method,
      body: body,
      headers: headers
    }).then(response => {
   
     
      if (response.ok) {
        return response.json().then(json => {
            onSuccess(json);
          });
      } else {
        return response.text().then(text => {
          onFailure({
            url: url,
            status: response.status,
            text: text
          });
        });
      }
    }).catch((reason) => { 
      onFailure({
        url: url,
        text: reason.toString()
      });
    });
  };
   
  export {
      query,dataPage 
  }