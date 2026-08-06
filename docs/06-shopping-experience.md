# 06 - Shopping Experience Specification

## Purpose

This phase defines the complete shopping journey from the moment a
customer adds a product to the cart until the order is ready for
payment. The experience should be intuitive, responsive, and
frictionless while giving customers confidence before checkout.

------------------------------------------------------------------------

# Objectives

By the end of this phase:

-   Customers can manage their cart.
-   Customers can manage their wishlist.
-   Customers can apply coupons.
-   Customers can estimate shipping.
-   Customers can manage delivery addresses.
-   Customers can complete the checkout flow up to payment.

------------------------------------------------------------------------

# Customer Flow

1.  Add product to cart
2.  Review cart
3.  Update quantity
4.  Apply coupon
5.  Estimate shipping
6.  Login or continue (guest checkout if enabled)
7.  Select or add address
8.  Review order summary
9.  Continue to payment

------------------------------------------------------------------------

# Shopping Cart

## Functional Requirements

FR-001 Add product to cart

FR-002 Remove product

FR-003 Update quantity

FR-004 Save for later

FR-005 Move saved item back to cart

FR-006 Clear cart

FR-007 Display subtotal

FR-008 Display estimated tax

FR-009 Display shipping estimate

FR-010 Display grand total

------------------------------------------------------------------------

# Cart Item

Each item displays:

-   Product image
-   Product name
-   Selected size
-   Selected color
-   SKU
-   Unit price
-   Quantity selector
-   Stock status
-   Line total
-   Remove action
-   Save for later

------------------------------------------------------------------------

# Wishlist

Requirements

-   Add item
-   Remove item
-   Move item to cart
-   Share wishlist (future-ready)

Display:

-   Image
-   Name
-   Price
-   Availability

------------------------------------------------------------------------

# Coupon System

Supported types

-   Percentage discount
-   Fixed discount

Validation

-   Expiry date
-   Usage limit
-   Minimum order value
-   Eligible products
-   Eligible collections

Display discount breakdown clearly.

------------------------------------------------------------------------

# Shipping Estimate

Customer can calculate estimated shipping using:

-   Country
-   State
-   City
-   Postal code

Display:

-   Shipping cost
-   Estimated delivery window

------------------------------------------------------------------------

# Addresses

Customers can:

-   Add address
-   Edit address
-   Delete address
-   Set default address
-   Select shipping address
-   Select billing address

Address Fields

-   Full Name
-   Phone
-   Email
-   Address Line 1
-   Address Line 2
-   Landmark
-   City
-   State
-   Country
-   Postal Code

------------------------------------------------------------------------

# Checkout

## Step 1

Customer Information

## Step 2

Shipping Address

## Step 3

Billing Address

## Step 4

Shipping Method

## Step 5

Order Review

## Step 6

Proceed to Payment

------------------------------------------------------------------------

# Order Summary

Display

-   Products
-   Quantities
-   Discounts
-   Shipping
-   Tax
-   Grand Total

------------------------------------------------------------------------

# Validation Rules

-   Cart cannot be empty
-   Variant must exist
-   Quantity must not exceed stock
-   Valid shipping address required
-   Coupon validation before checkout

------------------------------------------------------------------------

# API Requirements

Cart

GET /api/cart POST /api/cart PUT /api/cart/{id} DELETE /api/cart/{id}

Wishlist

GET /api/wishlist POST /api/wishlist DELETE /api/wishlist/{id}

Coupons

POST /api/coupons/apply

Addresses

GET /api/addresses POST /api/addresses PUT /api/addresses/{id} DELETE
/api/addresses/{id}

Checkout

POST /api/checkout/review

------------------------------------------------------------------------

# UI Requirements

-   Responsive cart page
-   Sticky order summary on desktop
-   Mobile-friendly checkout
-   Clear progress indicator
-   Toast notifications
-   Confirmation dialogs where required
-   Empty cart state
-   Empty wishlist state

------------------------------------------------------------------------

# Performance

-   Persist cart between sessions
-   Fast cart updates
-   Optimistic UI where appropriate
-   Skeleton loading
-   Efficient API calls

------------------------------------------------------------------------

# Security

-   Validate prices on server
-   Validate stock before checkout
-   Validate coupon on server
-   Never trust client totals
-   Protect customer addresses

------------------------------------------------------------------------

# Acceptance Criteria

-   Cart works correctly
-   Wishlist works correctly
-   Coupons validate correctly
-   Shipping estimate works
-   Address management works
-   Checkout flow reaches payment successfully
-   Responsive layout works

------------------------------------------------------------------------

# Manual Testing Checklist

-   Add product to cart
-   Update quantity
-   Remove product
-   Save item for later
-   Add to wishlist
-   Apply valid coupon
-   Test invalid coupon
-   Add address
-   Edit address
-   Delete address
-   Complete checkout review
-   Test mobile layout

Phase 6 is complete when customers can confidently prepare an order for
payment without errors.
