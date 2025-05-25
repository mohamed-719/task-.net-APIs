# Country Blocking API

A RESTful API for managing country-based IP blocking with geolocation lookup and logging capabilities.

## Features

- Block/unblock countries permanently or temporarily
- Check if an IP address originates from a blocked country
- View logs of access attempts from blocked countries
- Automatic cleanup of expired temporary blocks

## API Endpoints

### Country Management

#### Temporarily Block a Country
`POST /api/countries/temporal-block`

Request:
```json
{
  "countryCode": "CN",
  "durationMinutes": 30
}
GET ==> /api/ip/lookup

{
  "countryCode": "US",
  "countryName": "United States",
  "ISP": "Google LLC"
}
GET ==> /api/ip/check-block


{
  "isBlocked": true,
  "countryCode": "CN",
  "countryName": "China",
  "ipAddress": "123.45.67.89"
}
GET ==> /api/logs/blocked-attempts

[
  {
    "ipAddress": "123.45.67.89",
    "countryCode": "CN",
    "isBlocked": true,
    "userAgent": "Mozilla/5.0...",
    "timestamp": "2024-05-26T12:34:56Z"
  }
]
