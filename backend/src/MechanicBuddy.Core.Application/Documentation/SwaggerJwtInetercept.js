(request) => {
if (!request.url.endsWith('swagger.json')) return request;
var json = window.localStorage?.authorized;
if (!json) return request;
var auth = JSON.parse(json);
var token = auth?.Bearer?.value;
if (!token) return request;
request.headers.Authorization = 'Bearer ' + token;
return request;
}