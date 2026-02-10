# WSV - Monitoring Dashboard
## Overview

Web Supervisor is a backend-focused monitoring dashboard that simulates multiple data sources producing time-based readings.  
The system processes these readings through an event-driven pipeline, temporarily stores recent data in memory, and persists historical data into a database for long-term access.

The primary goal of this project was to design and implement a backend architecture that models real-world data-flow systems, focusing on service lifecycles, data consistency, authorization, and separation of concerns.

---

## Key Features

- Simulated data sources producing time-series readings
- Event-driven processing pipeline
- In-memory buffering + short-term cache for recent data
- Persistent storage for historical queries
- REST API (public + protected endpoints)
- JWT authentication + role-based authorization
- Angular UI with authentication-driven behavior

---

## Tech Stack

**Backend**
- ASP.NET Core Web API
- Entity Framework Core
- Background services (IHostedService)

**Frontend**
- Angular

**Other**
- JWT authentication
- Role-based authorization

---

## Data Flow (High Level)

1. Hosted service generates a reading for a given source.
2. Reading is pushed into an in-memory buffer / pipeline.
3. Recent readings are stored in short-term cache (for near real-time endpoints).
4. Readings are persisted into the database (for historical endpoints).
5. API exposes:
   - near real-time data (cache-based)
   - historical data (DB-based)
   - management endpoints (protected)

---

## Repository Structure

- `WSV.Api/` – ASP.NET Core backend (API + background services)
- `WSV.App/` – Angular frontend
- `WSV.sln` – .NET solution

---

## Demo Users & Authorization

The application demonstrates role-based authorization with multiple user roles.

On startup, demo users are seeded automatically.

### Public Access (No Login)
- Can access public endpoints
- Sees only the first data source

### Viewer
- Username: `viewer`
- Password: `Viewer123`
- Can view all data sources
- Cannot modify system state

### Operator
- Username: `operator`
- Password: `Operator123`
- Can view all data sources
- Can enable / disable sources

### Admin
- Username: `admin`
- Password: `Admin123`
- Full access to all protected endpoints
- Intended to manage advanced administrative operations (future extension)

---

## Getting Started

This setup demonstrates:
- Public vs protected endpoints
- JWT-based authentication
- Role-based authorization policies

### Prerequisites
- .NET SDK (matching the solution)
- Node.js + npm

### Restore .NET local tools
```bash
dotnet tool restore
```

### Run Backend (API)

```bash
cd WSV.Api
dotnet restore
dotnet run
```

### Run Frontend (Angular)
```bash
cd WSV.App
npm install
npm start
```
