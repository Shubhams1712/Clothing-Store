# 01 - Foundation Specification

> **Phase Goal:** Build a production-ready foundation for the eCommerce
> platform. This phase establishes the architecture, development
> standards, project structure, design system, backend infrastructure,
> and frontend infrastructure. No business features (authentication,
> products, payments, admin, etc.) are implemented in this phase.

------------------------------------------------------------------------

# Success Criteria

Phase 1 is complete only when:

-   Frontend builds successfully.
-   Backend builds successfully.
-   PostgreSQL connection is working.
-   Backend exposes a health endpoint.
-   Frontend successfully calls the backend.
-   Shared UI system is established.
-   Clean Architecture backend is configured.
-   Project is ready for future modules without restructuring.

------------------------------------------------------------------------

# Technology Stack

## Frontend

-   Next.js 15 (App Router)
-   TypeScript
-   Tailwind CSS v4
-   shadcn/ui
-   Framer Motion
-   Axios
-   TanStack React Query
-   React Hook Form (future-ready)
-   Zod (future-ready)

## Backend

-   ASP.NET Core 8 Web API
-   Entity Framework Core
-   PostgreSQL
-   FluentValidation (future-ready)
-   Serilog

------------------------------------------------------------------------

# Repository Structure

    /
    ├── frontend/
    ├── backend/
    ├── docs/
    ├── .github/
    ├── README.md
    └── docker-compose.yml (optional)

------------------------------------------------------------------------

# Frontend Folder Structure

    src/
    ├── app/
    ├── components/
    │   ├── ui/
    │   ├── common/
    │   ├── layout/
    │   └── feedback/
    ├── features/
    ├── hooks/
    ├── lib/
    ├── providers/
    ├── services/
    ├── styles/
    ├── types/
    ├── utils/
    └── config/

Every folder must have a single responsibility. Components should be
reusable rather than page-specific.

------------------------------------------------------------------------

# Backend Solution Structure

    backend/
    ├── API/
    ├── Application/
    ├── Domain/
    ├── Infrastructure/
    └── Tests/ (placeholder)

Responsibilities:

## API

-   Controllers
-   Middleware
-   Dependency Injection
-   Configuration
-   Endpoints

## Application

-   Use cases
-   DTOs
-   Interfaces
-   Validation
-   Mapping

## Domain

-   Entities
-   Value Objects
-   Domain Events
-   Enums
-   Repository Contracts

## Infrastructure

-   Entity Framework Core
-   PostgreSQL
-   Repository Implementations
-   External Services

------------------------------------------------------------------------

# UI Foundation

Create reusable primitives instead of page-specific components.

Required components:

-   Button
-   Input
-   Textarea
-   Select
-   Checkbox
-   Radio
-   Badge
-   Card
-   Dialog
-   Drawer
-   Sheet
-   Tooltip
-   Dropdown
-   Pagination
-   Breadcrumb
-   Tabs
-   Accordion
-   Skeleton
-   Spinner
-   Toast
-   Alert
-   Empty State
-   Error State
-   Loading Overlay

These components form the foundation for future modules.

------------------------------------------------------------------------

# Layout System

Create reusable layout components:

-   Root Layout
-   Navbar Shell
-   Footer Shell
-   Page Container
-   Section Wrapper
-   Grid System
-   Responsive Spacing Utilities

No business content should exist yet.

------------------------------------------------------------------------

# Design Tokens

Define a centralized design language.

## Typography

-   Display
-   Heading
-   Title
-   Body
-   Caption

## Spacing

Use a consistent spacing scale.

## Radius

Use a unified border-radius system.

## Shadows

Create standardized elevation levels.

## Motion

Use subtle animations with consistent timing.

## Color Palette

Neutral, premium palette prepared for brand customization.

------------------------------------------------------------------------

# API Layer

Create a reusable API abstraction.

Include:

-   Axios instance
-   Base URL
-   Request interceptor
-   Response interceptor
-   Error normalization
-   Timeout configuration

No feature-specific endpoints except Health.

------------------------------------------------------------------------

# React Query

Configure:

-   Query Client
-   Provider
-   Default retry policy
-   Cache defaults
-   Devtools (development only)

------------------------------------------------------------------------

# Environment Variables

## Frontend

    NEXT_PUBLIC_API_URL=

## Backend

    ConnectionStrings__DefaultConnection=
    JWT__Secret=
    Cloudinary__CloudName=
    Cloudinary__ApiKey=
    Cloudinary__ApiSecret=
    Razorpay__KeyId=
    Razorpay__KeySecret=

Keep payment and cloud services as placeholders.

------------------------------------------------------------------------

# Database

Configure PostgreSQL.

Requirements:

-   Initial DbContext
-   Migrations enabled
-   Connection verified

No business tables yet.

------------------------------------------------------------------------

# Health Endpoint

Create:

    GET /api/health

Response example:

``` json
{
  "status": "Healthy",
  "service": "API",
  "timestamp": "2026-01-01T00:00:00Z"
}
```

Frontend should display successful communication.

------------------------------------------------------------------------

# Error Handling

## Backend

-   Global exception middleware
-   Consistent error response
-   Validation response format
-   Structured logging

## Frontend

-   Error Boundary
-   API error handler
-   Toast notifications
-   Retry handling

------------------------------------------------------------------------

# Performance Foundation

Configure:

-   Route groups
-   Metadata
-   Image optimization
-   Font optimization
-   Lazy loading support
-   Dynamic imports where appropriate

------------------------------------------------------------------------

# Code Standards

Every contribution must follow:

-   SOLID principles
-   Strong typing
-   Dependency Injection
-   Single Responsibility Principle
-   Reusable architecture
-   Descriptive naming
-   No duplicated logic
-   Small focused components

------------------------------------------------------------------------

# Deliverables Checklist

## Frontend

-   [ ] Next.js configured
-   [ ] Tailwind configured
-   [ ] shadcn/ui installed
-   [ ] React Query configured
-   [ ] Axios configured
-   [ ] Global providers created
-   [ ] Shared layout created
-   [ ] UI foundation created

## Backend

-   [ ] ASP.NET Core solution created
-   [ ] Clean Architecture configured
-   [ ] EF Core configured
-   [ ] PostgreSQL connected
-   [ ] Health endpoint implemented
-   [ ] Swagger configured
-   [ ] Logging configured
-   [ ] Exception middleware configured

## Integration

-   [ ] Frontend communicates with backend
-   [ ] Health endpoint visible in UI

------------------------------------------------------------------------

# Manual Verification

Before Phase 2:

1.  Install dependencies.
2.  Start PostgreSQL.
3.  Run backend.
4.  Verify Swagger.
5.  Run frontend.
6.  Verify health endpoint response.
7.  Verify frontend displays backend health.
8.  Ensure build succeeds without errors.

Only after every item passes should Phase 2 begin.
