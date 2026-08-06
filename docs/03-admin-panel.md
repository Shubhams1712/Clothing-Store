# 03 - Admin Panel Specification

## Purpose

The Admin Panel is the operational center of the eCommerce platform.
Every business activity---products, orders, customers, inventory,
content, and store configuration---must be managed from here. The
interface should be clean, fast, responsive, and optimized for daily use
by a small team.

------------------------------------------------------------------------

# Objectives

By the end of this phase, administrators should be able to:

-   Monitor store activity from a dashboard
-   Manage products and variants
-   Manage categories and collections
-   Process orders
-   View and manage customers
-   Manage inventory
-   Create and manage coupons
-   Moderate reviews
-   Configure store settings

------------------------------------------------------------------------

# Layout

## Sidebar

-   Dashboard
-   Products
-   Categories
-   Collections
-   Orders
-   Customers
-   Inventory
-   Coupons
-   Reviews
-   Media Library
-   Settings
-   Profile

## Top Bar

-   Search
-   Notifications
-   Theme Toggle
-   Admin Profile Menu

------------------------------------------------------------------------

# Dashboard

## Functional Requirements

FR-001 Display total revenue

FR-002 Display today's orders

FR-003 Display pending orders

FR-004 Display low-stock products

FR-005 Display recent orders

FR-006 Display latest customer registrations

FR-007 Display sales overview chart

FR-008 Display top-selling products

FR-009 Display quick actions

Dashboard widgets should be configurable for future expansion.

------------------------------------------------------------------------

# Product Management

## Features

-   Create product
-   Edit product
-   Archive product
-   Delete product
-   Duplicate product
-   Draft products
-   Publish products

## Product Fields

-   Name
-   Slug
-   Description
-   Short Description
-   SKU
-   Price
-   Compare Price
-   Cost Price
-   Brand
-   Collection
-   Category
-   Tags
-   Status
-   Featured
-   SEO Title
-   SEO Description

## Images

-   Upload multiple images
-   Reorder images
-   Set featured image
-   Remove image

## Variants

Support:

-   Size
-   Color
-   Material (future)

Each variant has:

-   SKU
-   Price
-   Stock
-   Status

## Validation

-   Name required
-   SKU unique
-   Slug unique
-   Price greater than zero
-   Minimum one image
-   At least one variant

------------------------------------------------------------------------

# Category Management

Requirements

-   Create category
-   Edit category
-   Delete category
-   Parent categories
-   Category image
-   Category description
-   Display order

------------------------------------------------------------------------

# Collection Management

Requirements

-   Create collection
-   Edit collection
-   Delete collection
-   Assign products
-   Featured collections

------------------------------------------------------------------------

# Order Management

## Order Status

-   Pending
-   Confirmed
-   Packed
-   Shipped
-   Delivered
-   Cancelled
-   Refunded

## Features

FR-020 View order

FR-021 Update status

FR-022 Print invoice

FR-023 View payment

FR-024 View customer

FR-025 Add internal notes

FR-026 Cancel order

FR-027 Refund order

------------------------------------------------------------------------

# Customer Management

Customer Profile includes

-   Personal details
-   Contact details
-   Addresses
-   Order history
-   Lifetime spending
-   Notes
-   Account status

Actions

-   View
-   Edit
-   Disable account
-   View orders

------------------------------------------------------------------------

# Inventory

Features

-   Stock quantity
-   Variant stock
-   Low-stock alerts
-   Inventory history
-   Manual stock adjustment

Validation

Stock cannot be negative.

------------------------------------------------------------------------

# Coupons

Support

-   Percentage discount
-   Fixed amount discount

Fields

-   Code
-   Description
-   Discount
-   Expiry
-   Usage limit
-   Minimum order value
-   Active status

------------------------------------------------------------------------

# Reviews

Features

-   Approve
-   Hide
-   Delete
-   Reply
-   Filter by rating

------------------------------------------------------------------------

# Media Library

Store

-   Product images
-   Banner images
-   Collection images

Features

-   Upload
-   Delete
-   Preview
-   Search
-   Filter

------------------------------------------------------------------------

# Settings

Sections

-   Store Information
-   Branding
-   Shipping
-   Tax
-   Payment Methods
-   Email Configuration
-   SEO
-   Social Links
-   Legal Pages

------------------------------------------------------------------------

# API Requirements

Products

GET /api/admin/products POST /api/admin/products PUT
/api/admin/products/{id} DELETE /api/admin/products/{id}

Categories

GET /api/admin/categories POST /api/admin/categories

Orders

GET /api/admin/orders PUT /api/admin/orders/{id}

Customers

GET /api/admin/customers

Inventory

GET /api/admin/inventory PUT /api/admin/inventory/{id}

Coupons

CRUD endpoints

Reviews

Moderation endpoints

------------------------------------------------------------------------

# Permissions

Admin

-   Full access

Manager

-   Manage products
-   Manage orders
-   Manage inventory

Staff

-   View dashboard
-   Process orders

All permissions must be enforced on the backend.

------------------------------------------------------------------------

# UI Requirements

-   Responsive desktop-first interface
-   Consistent spacing
-   Reusable tables
-   Reusable forms
-   Search on listing pages
-   Filtering
-   Sorting
-   Pagination
-   Confirmation dialogs for destructive actions
-   Toast notifications for success and failure

------------------------------------------------------------------------

# Security

-   Admin routes require authentication
-   Role-based authorization
-   Audit log for critical actions
-   Validate all input server-side
-   Prevent unauthorized resource access

------------------------------------------------------------------------

# Acceptance Criteria

-   Dashboard displays business metrics
-   Product CRUD works
-   Category CRUD works
-   Collection CRUD works
-   Order management works
-   Customer management works
-   Inventory management works
-   Coupon management works
-   Review moderation works
-   Settings can be updated
-   Permissions enforced correctly

------------------------------------------------------------------------

# Manual Testing Checklist

-   Create product
-   Edit product
-   Delete product
-   Create category
-   Create collection
-   Update stock
-   Create coupon
-   Moderate review
-   Change order status
-   View customer profile
-   Verify permission restrictions
-   Verify responsive layout

Phase 3 is complete only after all checklist items succeed.
