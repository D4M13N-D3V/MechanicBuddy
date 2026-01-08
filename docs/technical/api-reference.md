# API Reference

MechanicBuddy provides a RESTful API for all workshop management operations. The API is documented with Swagger/OpenAPI and available at `/swagger` when running the backend.

## Base URL

| Environment | URL |
|-------------|-----|
| Local Development | `http://localhost:15567` |
| Production | `https://api.yourdomain.com` |

## Authentication

### Authenticate User

Obtain JWT tokens for API access.

```http
POST /api/users/authenticate
Content-Type: application/json

{
  "username": "admin",
  "password": "carcare",
  "consumerSecret": "your-consumer-secret"
}
```

**Response:**

```json
{
  "jwt": "eyJhbGciOiJIUzI1NiIs...",
  "publicJwt": "eyJhbGciOiJIUzI1NiIs...",
  "mustChangePassword": false
}
```

**Rate Limit:** 10 requests per 60 seconds per IP

### Using JWT Token

Include the JWT token in the `Authorization` header:

```http
GET /api/work/page
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Extend Session

Refresh an expiring JWT token.

```http
POST /api/users/extendsession
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## Work Orders

### List Work Orders

Get paginated list of work orders with filtering.

```http
GET /api/work/page
Authorization: Bearer {jwt}
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `offset` | int | Number of records to skip (default: 0) |
| `limit` | int | Number of records to return (default: 20) |
| `search` | string | Search in client name, vehicle reg, work number |
| `status` | string | Filter by status: `Created`, `InProgress`, `Completed` |
| `hasInvoice` | bool | Filter by invoice presence |
| `dateFrom` | date | Start date filter (YYYY-MM-DD) |
| `dateTo` | date | End date filter (YYYY-MM-DD) |

**Response:**

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "number": 1234,
      "startedOn": "2024-01-15T10:30:00Z",
      "completedOn": null,
      "status": "InProgress",
      "client": {
        "id": "...",
        "name": "John Doe"
      },
      "vehicle": {
        "id": "...",
        "regNr": "ABC-123",
        "producer": "Toyota",
        "model": "Corolla"
      },
      "hasInvoice": false
    }
  ],
  "total": 156,
  "offset": 0,
  "limit": 20
}
```

### Get Work Order

Retrieve a single work order with full details.

```http
GET /api/work/{id}
Authorization: Bearer {jwt}
```

**Response:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "number": 1234,
  "startedOn": "2024-01-15T10:30:00Z",
  "completedOn": null,
  "odo": 125000,
  "notes": "Customer reported engine noise",
  "userStatus": "Waiting for parts",
  "client": { ... },
  "vehicle": { ... },
  "invoice": null,
  "starter": { "firstName": "Mike", "lastName": "Smith" }
}
```

### Create Work Order

Create a new work order.

```http
POST /api/work
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "vehicleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "odo": 125000,
  "notes": "Customer reported engine noise"
}
```

**Response:** `201 Created` with created work order

!!! warning "Free Tier Limit"
    Free tier is limited to 1000 work orders. Returns `403 Forbidden` when limit exceeded.

### Update Work Order

Update an existing work order.

```http
PUT /api/work/{id}
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "clientId": "...",
  "vehicleId": "...",
  "odo": 125500,
  "notes": "Updated notes"
}
```

### Delete Work Orders

Delete one or more work orders.

```http
DELETE /api/work
Authorization: Bearer {jwt}
Content-Type: application/json

["id1", "id2", "id3"]
```

### Get Work Activities

List all offers and repair jobs for a work order.

```http
GET /api/work/{id}/activities
Authorization: Bearer {jwt}
```

**Response:**

```json
{
  "offers": [
    {
      "id": "...",
      "orderNr": 0,
      "startedOn": "2024-01-15T11:00:00Z",
      "acceptedOn": null,
      "hasEstimate": true
    }
  ],
  "repairJobs": [
    {
      "id": "...",
      "orderNr": 0,
      "startedOn": "2024-01-15T14:00:00Z"
    }
  ]
}
```

### Change Work Status

Update the status of a work order.

```http
PUT /api/work/{id}/status/{status}
Authorization: Bearer {jwt}
```

**Status Values:** `Created`, `InProgress`, `Completed`, `Cancelled`

---

## Offers & Estimates

### Get Offer Products/Services

```http
GET /api/work/offer/{offerId}/productsorservices
Authorization: Bearer {jwt}
```

**Response:**

```json
{
  "products": [
    {
      "id": "...",
      "code": "BRK-001",
      "name": "Brake Pad Set",
      "quantity": 1,
      "price": 45.00,
      "discount": 0
    }
  ],
  "services": [
    {
      "id": "...",
      "name": "Brake Pad Replacement",
      "quantity": 1,
      "price": 80.00
    }
  ]
}
```

### Update Offer Products/Services

```http
PUT /api/work/offer/{offerId}/productsorservices
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "products": [...],
  "services": [...]
}
```

### Issue Estimate

Generate a PDF estimate from an offer.

```http
PUT /api/work/{workId}/estimate/issue/{offerNumber}
Authorization: Bearer {jwt}
```

**Response:**

```json
{
  "estimateNumber": "1234-0",
  "pdfUrl": "/api/pricings/estimate/1234-0/pdf"
}
```

### Send Estimate by Email

```http
PUT /api/work/estimate/send/{offerId}
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "email": "customer@example.com",
  "subject": "Your Estimate from Workshop",
  "body": "Please find attached..."
}
```

### Accept Estimate

Mark an estimate as accepted, creating a repair job.

```http
PUT /api/work/{workId}/estimate/{offerNumber}/accepted
Authorization: Bearer {jwt}
```

---

## Repair Jobs

### Get Repair Job Products/Services

```http
GET /api/work/repairjob/{jobId}/productsorservices
Authorization: Bearer {jwt}
```

### Update Repair Job Products/Services

```http
PUT /api/work/repairjob/{jobId}/productsorservices
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "products": [
    {
      "code": "BRK-001",
      "name": "Brake Pad Set",
      "quantity": 1,
      "price": 45.00,
      "status": "Installed"
    }
  ],
  "services": [
    {
      "name": "Brake Pad Replacement",
      "quantity": 1,
      "price": 80.00,
      "mechanicId": "..."
    }
  ]
}
```

**Product Status Values:** `Unordered`, `Ordered`, `Arrived`, `Installed`

---

## Invoices

### Issue Invoice

Generate an invoice for a work order.

```http
PUT /api/work/{workId}/invoice/issue
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "paymentType": "BankTransfer",
  "dueDays": 14
}
```

**Payment Types:** `Cash`, `Card`, `BankTransfer`

### Send Invoice by Email

```http
PUT /api/work/{workId}/invoice/send
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "email": "customer@example.com"
}
```

### Download Invoice PDF

```http
GET /api/pricings/invoice/{invoiceNumber}/pdf
Authorization: Bearer {jwt}
```

**Response:** PDF file (application/pdf)

### Mark Invoice as Paid

```http
PUT /api/pricings/invoice/{id}/paid
Authorization: Bearer {jwt}
```

---

## Clients

### List Clients

```http
GET /api/clients/page
Authorization: Bearer {jwt}
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `search` | string | Search in name, phone, email |
| `type` | string | `private` or `legal` |
| `offset` | int | Pagination offset |
| `limit` | int | Page size |

### Get Client

```http
GET /api/clients/{id}
Authorization: Bearer {jwt}
```

### Create Private Client

```http
POST /api/clients/private
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+1234567890",
  "email": "john@example.com",
  "address": {
    "street": "123 Main St",
    "city": "Anytown",
    "postalCode": "12345",
    "country": "USA"
  }
}
```

### Create Legal Client

```http
POST /api/clients/legal
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "name": "ACME Corporation",
  "regNr": "12345678",
  "phone": "+1234567890",
  "email": "contact@acme.com",
  "address": { ... }
}
```

### Update Client

```http
PUT /api/clients/{type}/{id}
Authorization: Bearer {jwt}
```

Where `type` is `private` or `legal`.

---

## Vehicles

### List Vehicles

```http
GET /api/vehicles/page
Authorization: Bearer {jwt}
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `search` | string | Search in regNr, VIN, producer, model |
| `offset` | int | Pagination offset |
| `limit` | int | Page size |

### Get Vehicle

```http
GET /api/vehicles/{id}
Authorization: Bearer {jwt}
```

### Create Vehicle

```http
POST /api/vehicles
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "regNr": "ABC-123",
  "vin": "1HGBH41JXMN109186",
  "producer": "Toyota",
  "model": "Corolla",
  "engine": "1.8L",
  "transmission": "Automatic",
  "body": "Sedan",
  "year": 2020,
  "color": "Silver"
}
```

### Register Vehicle to Owner

```http
POST /api/vehicles/{vehicleId}/register
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "clientId": "..."
}
```

---

## Inventory (Spare Parts)

### List Spare Parts

```http
GET /api/spareparts/page
Authorization: Bearer {jwt}
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `search` | string | Search in code, name |
| `storageId` | guid | Filter by storage location |
| `lowStock` | bool | Filter low stock items |

### Get Spare Part

```http
GET /api/spareparts/{id}
Authorization: Bearer {jwt}
```

### Create Spare Part

```http
POST /api/spareparts
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "code": "BRK-001",
  "name": "Brake Pad Set - Front",
  "price": 45.00,
  "quantity": 10,
  "storageId": "...",
  "discount": 0
}
```

### Update Spare Part

```http
PUT /api/spareparts/{id}
Authorization: Bearer {jwt}
```

---

## User Management

### List Users

```http
GET /api/usermanagement
Authorization: Bearer {jwt}
```

### Create User

```http
POST /api/usermanagement
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "username": "newuser",
  "email": "user@example.com",
  "employeeId": "..."
}
```

### Reset User Password

```http
POST /api/usermanagement/{id}/reset-password
Authorization: Bearer {jwt}
```

### Disable User

```http
PUT /api/usermanagement/{id}/disable
Authorization: Bearer {jwt}
```

---

## Settings & Configuration

### Get Company Requisites

```http
GET /api/options/requisites
Authorization: Bearer {jwt}
```

### Update Company Requisites

```http
PUT /api/options/requisites
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "name": "My Workshop",
  "address": "123 Service Road",
  "phone": "+1234567890",
  "email": "workshop@example.com",
  "bankAccount": "XX12 3456 7890",
  "regNr": "12345678",
  "taxId": "TAX123456"
}
```

### Get Pricing Settings

```http
GET /api/options/pricing
Authorization: Bearer {jwt}
```

### Update Pricing Settings

```http
PUT /api/options/pricing
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "vatRate": 20,
  "surcharge": 0,
  "disclaimer": "All prices include VAT",
  "signatureLine": "Authorized Signature"
}
```

---

## Branding

### Get Branding

```http
GET /api/branding
Authorization: Bearer {jwt}
```

### Update Logo

```http
PUT /api/branding/logo
Authorization: Bearer {jwt}
Content-Type: multipart/form-data

logo: [binary file]
```

### Update Portal Colors

```http
PUT /api/branding/portal-colors
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "sidebarColor": "#1a1a2e",
  "accentColor": "#4f46e5",
  "contentBackground": "#ffffff"
}
```

---

## Service Requests (Landing Page)

### Submit Service Request

Public endpoint for landing page form.

```http
POST /api/servicerequest
Content-Type: application/json

{
  "customerName": "John Doe",
  "phone": "+1234567890",
  "email": "john@example.com",
  "vehicleInfo": "2020 Toyota Corolla",
  "serviceType": "Oil Change",
  "message": "Looking for routine maintenance"
}
```

### List Service Requests

```http
GET /api/servicerequest/page
Authorization: Bearer {jwt}
```

### Update Service Request Status

```http
PUT /api/servicerequest/{id}/status/{status}
Authorization: Bearer {jwt}
```

**Status Values:** `New`, `Contacted`, `Scheduled`, `Completed`, `Cancelled`

---

## Error Responses

All errors follow a consistent format:

```json
{
  "error": "Error message description",
  "code": "ERROR_CODE"
}
```

**Common HTTP Status Codes:**

| Code | Description |
|------|-------------|
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Missing or invalid JWT |
| 403 | Forbidden - Insufficient permissions or limit exceeded |
| 404 | Not Found - Resource doesn't exist |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error |
