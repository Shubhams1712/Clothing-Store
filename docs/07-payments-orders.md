# 07 - Payments & Orders Specification

## Purpose

This phase defines the complete payment and order lifecycle. From the
moment a customer confirms checkout until the order is delivered or
refunded, every action must be reliable, secure, traceable, and easy to
manage.

------------------------------------------------------------------------

# Objectives

By the end of this phase:

-   Customers can pay securely.
-   Orders are created only after successful validation.
-   Payments are verified server-side.
-   Invoices are generated.
-   Customers can track orders.
-   Administrators can manage the complete order lifecycle.
-   Refunds and cancellations are supported.

------------------------------------------------------------------------

# Supported Payment Methods

Initial

-   Razorpay
-   UPI
-   Debit Card
-   Credit Card
-   Net Banking
-   Wallets
-   Cash on Delivery (COD)

Future Ready

-   Stripe
-   PayPal
-   International Payments

------------------------------------------------------------------------

# Customer Order Flow

1.  Checkout review
2.  Select payment method
3.  Initiate payment
4.  Verify payment
5.  Create order
6.  Generate invoice
7.  Send confirmation
8.  Order processing
9.  Shipping
10. Delivery

------------------------------------------------------------------------

# Order Lifecycle

Statuses

-   Pending Payment
-   Payment Failed
-   Payment Successful
-   Order Confirmed
-   Packed
-   Shipped
-   Out for Delivery
-   Delivered
-   Cancelled
-   Refund Requested
-   Refunded

Every status change must be recorded in order history.

------------------------------------------------------------------------

# Payment Requirements

FR-001 Initiate payment

FR-002 Verify payment signature

FR-003 Prevent duplicate payments

FR-004 Prevent duplicate orders

FR-005 Store transaction details

FR-006 Record payment status

FR-007 Retry failed payment

FR-008 Support COD orders

------------------------------------------------------------------------

# Order Details

Each order stores:

-   Order Number
-   Customer
-   Items
-   Variants
-   Prices
-   Discounts
-   Shipping
-   Tax
-   Grand Total
-   Payment Method
-   Payment Status
-   Order Status
-   Tracking Number
-   Invoice Reference
-   Timestamps

------------------------------------------------------------------------

# Invoice

Generate invoice after successful order creation.

Include

-   Order Number
-   Customer Details
-   Billing Address
-   Shipping Address
-   Itemized Products
-   Tax
-   Shipping
-   Discounts
-   Total Amount
-   Payment Method
-   Invoice Date

------------------------------------------------------------------------

# Order Tracking

Customers can view:

-   Current status
-   Timeline
-   Tracking number
-   Estimated delivery
-   Courier information (future)

------------------------------------------------------------------------

# Cancellation & Refunds

Customer

-   Request cancellation (eligible states only)
-   Request refund (eligible states only)

Admin

-   Approve / reject request
-   Process refund
-   Update status
-   Record reason

------------------------------------------------------------------------

# Notifications

Customer notifications

-   Order confirmed
-   Payment received
-   Payment failed
-   Order packed
-   Order shipped
-   Out for delivery
-   Delivered
-   Cancelled
-   Refunded

Notification channels should be extensible (email, SMS, WhatsApp).

------------------------------------------------------------------------

# API Requirements

Payments

POST /api/payments/create-order POST /api/payments/verify POST
/api/payments/retry

Orders

GET /api/orders GET /api/orders/{id} POST /api/orders PUT
/api/orders/{id}/cancel PUT /api/orders/{id}/refund GET
/api/orders/{id}/invoice GET /api/orders/{id}/tracking

Admin

GET /api/admin/orders PUT /api/admin/orders/{id}/status

------------------------------------------------------------------------

# Validation Rules

-   Verify payment on the server
-   Validate order totals on the server
-   Validate stock before creating order
-   Prevent payment amount tampering
-   Prevent duplicate order creation

------------------------------------------------------------------------

# Security Requirements

-   Never trust client-side payment status
-   Verify Razorpay signatures
-   Store transaction references
-   Protect invoice endpoints
-   Log payment failures
-   Audit order status changes

------------------------------------------------------------------------

# Performance

-   Fast payment confirmation
-   Reliable webhook processing
-   Idempotent payment handling
-   Efficient order queries

------------------------------------------------------------------------

# Acceptance Criteria

-   Payment succeeds securely
-   Failed payments handled correctly
-   Orders created correctly
-   Invoice generated
-   Tracking timeline displayed
-   Admin manages order lifecycle
-   Refund workflow functions
-   Notifications triggered

------------------------------------------------------------------------

# Manual Testing Checklist

-   Successful Razorpay payment
-   Failed payment retry
-   COD order
-   Order creation
-   Invoice generation
-   Order tracking
-   Cancel eligible order
-   Request refund
-   Admin updates status
-   Verify duplicate payment protection

Phase 7 is complete when the complete payment and order lifecycle
operates securely from checkout to delivery.
