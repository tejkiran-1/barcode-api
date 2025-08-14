# Shipment Delivery API - Complete Endpoint Documentation

## API Base Configuration
- **Base URL:** `http://localhost:5144`
- **Content-Type:** `application/json`
- **Authentication:** None required

---

## 1. CREATE SHIPMENT DELIVERY

### Endpoint Details
- **Method:** `POST`
- **URL:** `/api/shipmentdelivery`
- **Description:** Creates a new shipment delivery with container or bulk items

### Request Parameters
**Body Parameters (JSON):**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `shipmentNumber` | string | ✅ Yes | Unique identifier for the shipment |
| `deliveryNumber` | string | ✅ Yes | Unique identifier for the delivery |
| `deliveryType` | number | ✅ Yes | 0 = Container, 1 = Bulk |
| `containerItems` | array | Conditional | Required if deliveryType = 0 |
| `bulkItems` | array | Conditional | Required if deliveryType = 1 |

### Request Model Structure

```typescript
interface CreateShipmentDeliveryRequest {
  shipmentNumber: string;        // Required, non-empty
  deliveryNumber: string;        // Required, non-empty
  deliveryType: DeliveryType;    // Required, 0 or 1
  containerItems?: ContainerItem[]; // Required if deliveryType = 0
  bulkItems?: BulkItem[];          // Required if deliveryType = 1
}

interface ContainerItem {
  materialNumber: string;  // Required, non-empty
  serialNumber: string;    // Required, non-empty
}

interface BulkItem {
  materialNumber: string;  // Required, non-empty
  evdSealNumber: string;   // Required, non-empty
}

enum DeliveryType {
  Container = 0,
  Bulk = 1
}
```

### Request Examples

**Container Delivery Request:**
```json
{
  "shipmentNumber": "SH001",
  "deliveryNumber": "DL001",
  "deliveryType": 0,
  "containerItems": [
    {
      "materialNumber": "MAT001",
      "serialNumber": "SER001"
    },
    {
      "materialNumber": "MAT002", 
      "serialNumber": "SER002"
    }
  ]
}
```

**Bulk Delivery Request:**
```json
{
  "shipmentNumber": "SH002",
  "deliveryNumber": "DL003",
  "deliveryType": 1,
  "bulkItems": [
    {
      "materialNumber": "MAT003",
      "evdSealNumber": "EVD001"
    },
    {
      "materialNumber": "MAT004",
      "evdSealNumber": "EVD002"
    }
  ]
}
```

### Response
**Success Response (201 Created):**
```json
{
  // Returns the created request object
}
```

**Error Responses:**
- `400 Bad Request` - Missing required fields or validation errors
- `409 Conflict` - Delivery number already exists
- `500 Internal Server Error` - Server error

### Validation Rules
- `shipmentNumber`: Required, non-empty string
- `deliveryNumber`: Required, non-empty string, must be unique
- `deliveryType`: Required, must be 0 (Container) or 1 (Bulk)
- If `deliveryType = 0`: `containerItems` array required with at least 1 item
- If `deliveryType = 1`: `bulkItems` array required with at least 1 item
- Each container item must have both `materialNumber` and `serialNumber`
- Each bulk item must have both `materialNumber` and `evdSealNumber`

---

## 2. GET SHIPMENT BY SHIPMENT NUMBER

### Endpoint Details
- **Method:** `GET`
- **URL:** `/api/shipmentdelivery/shipment/{shipmentNumber}`
- **Description:** Retrieves complete shipment details with all associated deliveries

### Request Parameters
**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `shipmentNumber` | string | ✅ Yes | The shipment number to search for |

**Example URL:**
```
GET /api/shipmentdelivery/shipment/SH001
```

### Response Model Structure

```typescript
interface ShipmentDeliveryResponse {
  shipmentNumber: string;         // Shipment identifier
  shipmentCreatedAt: string;      // ISO 8601 DateTime string
  deliveries: DeliveryResponse[]; // Array of all deliveries in this shipment
}

interface DeliveryResponse {
  deliveryNumber: string;           // Delivery identifier
  deliveryType: DeliveryType;       // 0 = Container, 1 = Bulk
  createdAt: string;               // ISO 8601 DateTime string
  containerItems?: ContainerItemResponse[]; // Only present if deliveryType = 0
  bulkItems?: BulkItemResponse[];          // Only present if deliveryType = 1
}

interface ContainerItemResponse {
  materialNumber: string;  // Material number
  serialNumber: string;    // Serial number
  createdAt: string;      // ISO 8601 DateTime string
}

interface BulkItemResponse {
  materialNumber: string;  // Material number
  evdSealNumber: string;   // EVD seal number
  createdAt: string;      // ISO 8601 DateTime string
}
```

### Response Example
**Success Response (200 OK):**
```json
{
  "shipmentNumber": "SH001",
  "shipmentCreatedAt": "2025-08-14T08:30:00.000Z",
  "deliveries": [
    {
      "deliveryNumber": "DL001",
      "deliveryType": 0,
      "createdAt": "2025-08-14T08:35:00.000Z",
      "containerItems": [
        {
          "materialNumber": "MAT001",
          "serialNumber": "SER001",
          "createdAt": "2025-08-14T08:35:00.000Z"
        },
        {
          "materialNumber": "MAT002",
          "serialNumber": "SER002", 
          "createdAt": "2025-08-14T08:35:00.000Z"
        }
      ]
    },
    {
      "deliveryNumber": "DL002",
      "deliveryType": 1,
      "createdAt": "2025-08-14T08:40:00.000Z",
      "bulkItems": [
        {
          "materialNumber": "MAT003",
          "evdSealNumber": "EVD001",
          "createdAt": "2025-08-14T08:40:00.000Z"
        }
      ]
    }
  ]
}
```

**Error Responses:**
- `400 Bad Request` - Invalid or empty shipment number
- `404 Not Found` - Shipment number not found
- `500 Internal Server Error` - Server error

### Validation Rules
- `shipmentNumber` must be provided and non-empty
- Returns complete shipment data including all deliveries and their items

---

## 3. GET SHIPMENT BY DELIVERY NUMBER

### Endpoint Details
- **Method:** `GET`
- **URL:** `/api/shipmentdelivery/delivery/{deliveryNumber}`
- **Description:** Retrieves complete shipment details by searching with delivery number

### Request Parameters
**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `deliveryNumber` | string | ✅ Yes | The delivery number to search for |

**Example URL:**
```
GET /api/shipmentdelivery/delivery/DL001
```

### Response Model Structure
**Same as "Get Shipment by Shipment Number"** - Returns the complete shipment data that contains the specified delivery.

```typescript
// Uses the same ShipmentDeliveryResponse interface as endpoint #2
interface ShipmentDeliveryResponse {
  shipmentNumber: string;
  shipmentCreatedAt: string;
  deliveries: DeliveryResponse[];
}
```

### Response Example
**Success Response (200 OK):**
```json
{
  "shipmentNumber": "SH001",
  "shipmentCreatedAt": "2025-08-14T08:30:00.000Z",
  "deliveries": [
    {
      "deliveryNumber": "DL001", // This is the delivery we searched for
      "deliveryType": 0,
      "createdAt": "2025-08-14T08:35:00.000Z",
      "containerItems": [
        {
          "materialNumber": "MAT001",
          "serialNumber": "SER001",
          "createdAt": "2025-08-14T08:35:00.000Z"
        }
      ]
    },
    {
      "deliveryNumber": "DL002", // Other deliveries in the same shipment
      "deliveryType": 1,
      "createdAt": "2025-08-14T08:40:00.000Z",
      "bulkItems": [
        {
          "materialNumber": "MAT003",
          "evdSealNumber": "EVD001",
          "createdAt": "2025-08-14T08:40:00.000Z"
        }
      ]
    }
  ]
}
```

**Error Responses:**
- `400 Bad Request` - Invalid or empty delivery number
- `404 Not Found` - Delivery number not found
- `500 Internal Server Error` - Server error

### Validation Rules
- `deliveryNumber` must be provided and non-empty
- Returns the entire shipment that contains the specified delivery
- Includes all deliveries in that shipment, not just the searched delivery

---

## 4. GET ALL SHIPMENTS

### Endpoint Details
- **Method:** `GET`
- **URL:** `/api/shipmentdelivery`
- **Description:** Retrieves all shipments with their complete delivery details

### Request Parameters
**No parameters required**

**Example URL:**
```
GET /api/shipmentdelivery
```

### Response Model Structure

```typescript
// Returns an array of ShipmentDeliveryResponse objects
type GetAllShipmentsResponse = ShipmentDeliveryResponse[];

interface ShipmentDeliveryResponse {
  shipmentNumber: string;
  shipmentCreatedAt: string;
  deliveries: DeliveryResponse[];
}
```

### Response Example
**Success Response (200 OK):**
```json
[
  {
    "shipmentNumber": "SH001",
    "shipmentCreatedAt": "2025-08-14T08:30:00.000Z",
    "deliveries": [
      {
        "deliveryNumber": "DL001",
        "deliveryType": 0,
        "createdAt": "2025-08-14T08:35:00.000Z",
        "containerItems": [
          {
            "materialNumber": "MAT001",
            "serialNumber": "SER001",
            "createdAt": "2025-08-14T08:35:00.000Z"
          }
        ]
      }
    ]
  },
  {
    "shipmentNumber": "SH002", 
    "shipmentCreatedAt": "2025-08-14T09:00:00.000Z",
    "deliveries": [
      {
        "deliveryNumber": "DL003",
        "deliveryType": 1,
        "createdAt": "2025-08-14T09:05:00.000Z",
        "bulkItems": [
          {
            "materialNumber": "MAT003",
            "evdSealNumber": "EVD001",
            "createdAt": "2025-08-14T09:05:00.000Z"
          }
        ]
      }
    ]
  }
]
```

**Empty Response (200 OK):**
```json
[]
```

**Error Responses:**
- `500 Internal Server Error` - Server error

### Validation Rules
- No input validation required
- Returns empty array if no shipments exist
- Returns complete data for all shipments including all deliveries and items

---

## Complete Angular Service Implementation

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

// Interfaces
export interface CreateShipmentDeliveryRequest {
  shipmentNumber: string;
  deliveryNumber: string;
  deliveryType: DeliveryType;
  containerItems?: ContainerItem[];
  bulkItems?: BulkItem[];
}

export interface ContainerItem {
  materialNumber: string;
  serialNumber: string;
}

export interface BulkItem {
  materialNumber: string;
  evdSealNumber: string;
}

export interface ShipmentDeliveryResponse {
  shipmentNumber: string;
  shipmentCreatedAt: string;
  deliveries: DeliveryResponse[];
}

export interface DeliveryResponse {
  deliveryNumber: string;
  deliveryType: DeliveryType;
  createdAt: string;
  containerItems?: ContainerItemResponse[];
  bulkItems?: BulkItemResponse[];
}

export interface ContainerItemResponse {
  materialNumber: string;
  serialNumber: string;
  createdAt: string;
}

export interface BulkItemResponse {
  materialNumber: string;
  evdSealNumber: string;
  createdAt: string;
}

export enum DeliveryType {
  Container = 0,
  Bulk = 1
}

@Injectable({
  providedIn: 'root'
})
export class ShipmentDeliveryService {
  private baseUrl = 'http://localhost:5144';
  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  constructor(private http: HttpClient) { }

  // 1. Create Shipment Delivery
  createShipmentDelivery(request: CreateShipmentDeliveryRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/shipmentdelivery`, request, this.httpOptions)
      .pipe(catchError(this.handleError));
  }

  // 2. Get Shipment by Shipment Number
  getShipmentByNumber(shipmentNumber: string): Observable<ShipmentDeliveryResponse> {
    return this.http.get<ShipmentDeliveryResponse>(`${this.baseUrl}/api/shipmentdelivery/shipment/${encodeURIComponent(shipmentNumber)}`)
      .pipe(catchError(this.handleError));
  }

  // 3. Get Shipment by Delivery Number
  getShipmentByDeliveryNumber(deliveryNumber: string): Observable<ShipmentDeliveryResponse> {
    return this.http.get<ShipmentDeliveryResponse>(`${this.baseUrl}/api/shipmentdelivery/delivery/${encodeURIComponent(deliveryNumber)}`)
      .pipe(catchError(this.handleError));
  }

  // 4. Get All Shipments
  getAllShipments(): Observable<ShipmentDeliveryResponse[]> {
    return this.http.get<ShipmentDeliveryResponse[]>(`${this.baseUrl}/api/shipmentdelivery`)
      .pipe(catchError(this.handleError));
  }

  // Error handling
  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An error occurred';
    
    if (error.error instanceof ErrorEvent) {
      errorMessage = error.error.message;
    } else {
      switch (error.status) {
        case 400:
          errorMessage = 'Bad Request - Please check your input';
          break;
        case 404:
          errorMessage = 'Not Found - The requested item does not exist';
          break;
        case 409:
          errorMessage = 'Conflict - Item already exists';
          break;
        case 500:
          errorMessage = 'Internal Server Error - Please try again later';
          break;
        default:
          errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
      }
    }
    
    return throwError(() => new Error(errorMessage));
  }
}
```

## Quick Reference Summary

| Endpoint | Method | URL | Parameters | Response |
|----------|--------|-----|------------|----------|
| Create | POST | `/api/shipmentdelivery` | Body: CreateShipmentDeliveryRequest | 201 Created |
| Get by Shipment | GET | `/api/shipmentdelivery/shipment/{shipmentNumber}` | Path: shipmentNumber | ShipmentDeliveryResponse |
| Get by Delivery | GET | `/api/shipmentdelivery/delivery/{deliveryNumber}` | Path: deliveryNumber | ShipmentDeliveryResponse |
| Get All | GET | `/api/shipmentdelivery` | None | ShipmentDeliveryResponse[] |
