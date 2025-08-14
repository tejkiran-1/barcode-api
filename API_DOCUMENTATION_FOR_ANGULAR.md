# Shipment Delivery API - Angular Integration Guide

## Base Configuration

**Base URL:** `http://localhost:5144`  
**Content-Type:** `application/json`  
**API Version:** v1  

## API Endpoints

### 1. Create Shipment Delivery
**Method:** `POST`  
**URL:** `/api/shipmentdelivery`  
**Description:** Creates a new shipment delivery entry with container or bulk items

#### Request Body Structure
```typescript
interface CreateShipmentDeliveryRequest {
  shipmentNumber: string;        // Required - Unique shipment identifier
  deliveryNumber: string;        // Required - Unique delivery identifier
  deliveryType: DeliveryType;    // Required - 0 = Container, 1 = Bulk
  containerItems?: ContainerItem[]; // Required if deliveryType = 0
  bulkItems?: BulkItem[];          // Required if deliveryType = 1
}

interface ContainerItem {
  materialNumber: string;  // Required
  serialNumber: string;    // Required
}

interface BulkItem {
  materialNumber: string;  // Required
  evdSealNumber: string;   // Required
}

enum DeliveryType {
  Container = 0,
  Bulk = 1
}
```

#### Request Examples

**Container Delivery:**
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

**Bulk Delivery:**
```json
{
  "shipmentNumber": "SH001",
  "deliveryNumber": "DL002",
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

#### Response Codes
- `201 Created` - Successfully created
- `400 Bad Request` - Validation error
- `409 Conflict` - Delivery number already exists
- `500 Internal Server Error` - Server error

#### Angular Service Method
```typescript
createShipmentDelivery(request: CreateShipmentDeliveryRequest): Observable<any> {
  return this.http.post(`${this.baseUrl}/api/shipmentdelivery`, request);
}
```

---

### 2. Get Shipment by Shipment Number
**Method:** `GET`  
**URL:** `/api/shipmentdelivery/shipment/{shipmentNumber}`  
**Description:** Retrieves shipment details with all associated deliveries

#### Parameters
- `shipmentNumber` (string, required) - Path parameter

#### Response Structure
```typescript
interface ShipmentDeliveryResponse {
  shipmentNumber: string;
  shipmentCreatedAt: string;    // ISO DateTime
  deliveries: DeliveryResponse[];
}

interface DeliveryResponse {
  deliveryNumber: string;
  deliveryType: DeliveryType;   // 0 = Container, 1 = Bulk
  createdAt: string;           // ISO DateTime
  containerItems?: ContainerItemResponse[];  // Only if deliveryType = 0
  bulkItems?: BulkItemResponse[];           // Only if deliveryType = 1
}

interface ContainerItemResponse {
  materialNumber: string;
  serialNumber: string;
  createdAt: string;          // ISO DateTime
}

interface BulkItemResponse {
  materialNumber: string;
  evdSealNumber: string;
  createdAt: string;          // ISO DateTime
}
```

#### Response Example
```json
{
  "shipmentNumber": "SH001",
  "shipmentCreatedAt": "2025-08-14T08:30:00Z",
  "deliveries": [
    {
      "deliveryNumber": "DL001",
      "deliveryType": 0,
      "createdAt": "2025-08-14T08:35:00Z",
      "containerItems": [
        {
          "materialNumber": "MAT001",
          "serialNumber": "SER001",
          "createdAt": "2025-08-14T08:35:00Z"
        }
      ]
    },
    {
      "deliveryNumber": "DL002",
      "deliveryType": 1,
      "createdAt": "2025-08-14T08:40:00Z",
      "bulkItems": [
        {
          "materialNumber": "MAT003",
          "evdSealNumber": "EVD001",
          "createdAt": "2025-08-14T08:40:00Z"
        }
      ]
    }
  ]
}
```

#### Response Codes
- `200 OK` - Success
- `400 Bad Request` - Invalid shipment number
- `404 Not Found` - Shipment not found
- `500 Internal Server Error` - Server error

#### Angular Service Method
```typescript
getShipmentByNumber(shipmentNumber: string): Observable<ShipmentDeliveryResponse> {
  return this.http.get<ShipmentDeliveryResponse>(`${this.baseUrl}/api/shipmentdelivery/shipment/${shipmentNumber}`);
}
```

---

### 3. Get Shipment by Delivery Number
**Method:** `GET`  
**URL:** `/api/shipmentdelivery/delivery/{deliveryNumber}`  
**Description:** Retrieves shipment details by delivery number (returns the entire shipment)

#### Parameters
- `deliveryNumber` (string, required) - Path parameter

#### Response Structure
Same as "Get Shipment by Shipment Number" - returns the complete shipment data

#### Response Codes
- `200 OK` - Success
- `400 Bad Request` - Invalid delivery number
- `404 Not Found` - Delivery not found
- `500 Internal Server Error` - Server error

#### Angular Service Method
```typescript
getShipmentByDeliveryNumber(deliveryNumber: string): Observable<ShipmentDeliveryResponse> {
  return this.http.get<ShipmentDeliveryResponse>(`${this.baseUrl}/api/shipmentdelivery/delivery/${deliveryNumber}`);
}
```

---

### 4. Get All Shipments
**Method:** `GET`  
**URL:** `/api/shipmentdelivery`  
**Description:** Retrieves all shipments with their deliveries

#### Parameters
None

#### Response Structure
```typescript
type GetAllShipmentsResponse = ShipmentDeliveryResponse[];
```

#### Response Codes
- `200 OK` - Success (returns array, can be empty)
- `500 Internal Server Error` - Server error

#### Angular Service Method
```typescript
getAllShipments(): Observable<ShipmentDeliveryResponse[]> {
  return this.http.get<ShipmentDeliveryResponse[]>(`${this.baseUrl}/api/shipmentdelivery`);
}
```

---

## Angular Service Implementation

### Complete Service Example
```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ShipmentDeliveryService {
  private baseUrl = environment.apiUrl || 'http://localhost:5144';

  constructor(private http: HttpClient) { }

  // HTTP Options
  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  // Create shipment delivery
  createShipmentDelivery(request: CreateShipmentDeliveryRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/shipmentdelivery`, request, this.httpOptions);
  }

  // Get by shipment number
  getShipmentByNumber(shipmentNumber: string): Observable<ShipmentDeliveryResponse> {
    return this.http.get<ShipmentDeliveryResponse>(`${this.baseUrl}/api/shipmentdelivery/shipment/${shipmentNumber}`);
  }

  // Get by delivery number
  getShipmentByDeliveryNumber(deliveryNumber: string): Observable<ShipmentDeliveryResponse> {
    return this.http.get<ShipmentDeliveryResponse>(`${this.baseUrl}/api/shipmentdelivery/delivery/${deliveryNumber}`);
  }

  // Get all shipments
  getAllShipments(): Observable<ShipmentDeliveryResponse[]> {
    return this.http.get<ShipmentDeliveryResponse[]>(`${this.baseUrl}/api/shipmentdelivery`);
  }
}
```

## Error Handling

### Error Response Structure
```typescript
interface ApiError {
  title?: string;
  status?: number;
  detail?: string;
  errors?: { [key: string]: string[] };
}
```

### Angular Error Handling Example
```typescript
import { catchError, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

private handleError(error: HttpErrorResponse) {
  let errorMessage = 'An error occurred';
  
  if (error.error instanceof ErrorEvent) {
    // Client-side error
    errorMessage = error.error.message;
  } else {
    // Server-side error
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

// Use in service methods
createShipmentDelivery(request: CreateShipmentDeliveryRequest): Observable<any> {
  return this.http.post(`${this.baseUrl}/api/shipmentdelivery`, request, this.httpOptions)
    .pipe(catchError(this.handleError));
}
```

## Validation Rules

### Frontend Validation
- **ShipmentNumber**: Required, non-empty string
- **DeliveryNumber**: Required, non-empty string  
- **DeliveryType**: Required, must be 0 (Container) or 1 (Bulk)
- **ContainerItems**: Required if DeliveryType = 0, each item must have materialNumber and serialNumber
- **BulkItems**: Required if DeliveryType = 1, each item must have materialNumber and evdSealNumber

### Form Validation Example
```typescript
// Angular Reactive Form
this.shipmentForm = this.fb.group({
  shipmentNumber: ['', [Validators.required, Validators.minLength(1)]],
  deliveryNumber: ['', [Validators.required, Validators.minLength(1)]],
  deliveryType: [0, [Validators.required]],
  containerItems: this.fb.array([]),
  bulkItems: this.fb.array([])
});
```

## CORS Configuration
If you're running Angular on a different port, make sure the API has CORS configured for your Angular development server (typically `http://localhost:4200`).
