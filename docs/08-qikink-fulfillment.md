# Phase 8 --- Qikink Fulfillment Integration

## Purpose

Integrate Qikink as the print-on-demand and fulfillment provider for the
eCommerce store.

The store remains the source of truth for customers, products, variants,
prices, payments, orders, and customer-facing order history.

Qikink is responsible for production and fulfillment after a paid order
is eligible for fulfillment.

Qikink confirms that custom-built websites can integrate through its
Open API options. The exact API endpoints, authentication method,
request schemas, response schemas, webhook capabilities, and rate limits
must be verified from the current Qikink API documentation and merchant
credentials during implementation. Do not invent API contracts.

------------------------------------------------------------------------

## Objectives

By the end of this phase:

-   Store products can be mapped to Qikink products/variants.
-   Qikink fulfillment configuration is stored securely.
-   Eligible paid orders can be submitted to Qikink.
-   Qikink order identifiers are stored against local orders.
-   Qikink fulfillment status can be synchronized.
-   Tracking information can be synchronized.
-   Fulfillment failures are visible to administrators.
-   Duplicate fulfillment submissions are prevented.
-   Qikink failures do not corrupt the local order.
-   Administrators can manually retry eligible failed fulfillment
    operations.
-   Customers can see meaningful fulfillment and shipping status through
    their order page.

------------------------------------------------------------------------

## Important Architecture Rule

The store database remains the source of truth for:

-   Customer
-   Product
-   Product Variant
-   Price
-   Order
-   Order Item
-   Payment
-   Customer-facing order status

Qikink is an external fulfillment provider.

Do not make the storefront dependent on Qikink being available for
normal product browsing.

------------------------------------------------------------------------

## Fulfillment Flow

### Prepaid Order

``` text
Customer
   ↓
Cart
   ↓
Checkout
   ↓
Razorpay Payment
   ↓
Payment Verification
   ↓
Local Order Created / Confirmed
   ↓
Fulfillment Eligibility Check
   ↓
Qikink Order Submission
   ↓
Qikink Order ID Stored
   ↓
Qikink Processing
   ↓
Status Synchronization
   ↓
Tracking Information
   ↓
Shipment
   ↓
Delivered
```

Qikink's current documentation describes prepaid fulfillment as the
customer paying the store through its payment gateway, followed by the
merchant paying Qikink using available credits for fulfillment.

------------------------------------------------------------------------

## Fulfillment Eligibility

Only submit an order to Qikink when:

-   Payment is successfully verified.
-   Local order is confirmed.
-   Order contains at least one Qikink-enabled product.
-   Every Qikink-enabled item has a valid Qikink mapping.
-   Required customer shipping information is complete.
-   Required design/print information is available.
-   The order has not already been submitted.
-   The order has not been cancelled.

Orders that fail these conditions must remain locally recorded and
clearly identify why fulfillment cannot proceed.

------------------------------------------------------------------------

## Product Mapping

Each store product variant that uses Qikink fulfillment must have an
explicit Qikink mapping.

The architecture should support:

-   Local Product ID
-   Local Variant ID
-   Qikink Product ID or equivalent identifier
-   Qikink Variant ID or equivalent identifier
-   Qikink SKU / Design SKU where applicable
-   Printing Type
-   Printing Placement
-   Design Reference
-   Design File URL
-   Active Mapping Status

Do not assume that a store SKU is automatically a Qikink SKU.

------------------------------------------------------------------------

## Design Files

For print-on-demand fulfillment, the design file must be accessible to
Qikink according to its current requirements.

Support:

-   Design file URL
-   Design format
-   Design dimensions where required
-   Design validation status
-   Design accessibility status

Verify the current API-specific requirements before enforcing final file
limits.

------------------------------------------------------------------------

## Database

Add fulfillment-related entities without disrupting existing order data.

### FulfillmentProvider

Support:

-   Id
-   Name
-   IsActive
-   CreatedAt
-   UpdatedAt

### ProductFulfillmentMapping

Support:

-   Id
-   ProductId
-   ProductVariantId
-   ProviderId
-   ExternalProductId
-   ExternalVariantId
-   ExternalSku
-   DesignReference
-   DesignFileUrl
-   PrintingType
-   PrintingPlacement
-   IsActive
-   CreatedAt
-   UpdatedAt

### FulfillmentOrder

Support:

-   Id
-   OrderId
-   ProviderId
-   ExternalOrderId
-   Status
-   LastSyncedAt
-   SubmittedAt
-   FailureReason
-   RetryCount
-   CreatedAt
-   UpdatedAt

### FulfillmentOrderItem

Support:

-   Id
-   FulfillmentOrderId
-   OrderItemId
-   ExternalProductId
-   ExternalVariantId
-   ExternalSku
-   Quantity
-   Status
-   FailureReason

### Shipment

Support:

-   Id
-   OrderId
-   FulfillmentOrderId
-   TrackingNumber
-   CourierName
-   TrackingUrl
-   ShippingStatus
-   ShippedAt
-   DeliveredAt
-   LastSyncedAt

Use the existing database conventions and avoid unnecessary duplicate
order data.

------------------------------------------------------------------------

## Order State Model

Maintain separate concepts for local order status and fulfillment
status.

### Local Order Status

Examples:

-   PendingPayment
-   Paid
-   Processing
-   Shipped
-   Delivered
-   Cancelled
-   Refunded

### Fulfillment Status

Examples:

-   NotSubmitted
-   PendingSubmission
-   Submitted
-   Processing
-   ActionRequired
-   Shipped
-   Delivered
-   Failed
-   Cancelled

Do not replace local order status with a raw Qikink status.

------------------------------------------------------------------------

## Qikink Status Mapping

The integration must support the current Qikink fulfillment lifecycle.

Qikink currently documents statuses including:

-   On Hold
-   Live OOS
-   Live
-   To be Printed
-   Partially Picklisted
-   Printed
-   Manifested
-   In-Transit
-   Exception
-   Delivered
-   RTO Initiated
-   Returned
-   Cancelled

Qikink also documents Action Required for issues such as invalid design
files, incomplete shipping information, copyright/design issues, or
stock problems.

Map provider-specific statuses into normalized application fulfillment
states while preserving the original provider status.

------------------------------------------------------------------------

## Shipping Tracking

Synchronize:

-   AWB / Tracking ID
-   Courier
-   Tracking URL where available
-   Shipment status
-   Shipment timestamps

Customers should see a simplified status while administrators can see
the provider's original status.

------------------------------------------------------------------------

## API Integration Layer

Create a dedicated Qikink integration service.

Suggested architecture:

-   IQikinkClient
-   QikinkClient
-   QikinkOrderService
-   QikinkProductMappingService
-   QikinkStatusService
-   QikinkTrackingService

Keep Qikink-specific logic isolated from general order business logic.

The application should be replaceable with another fulfillment provider
later.

------------------------------------------------------------------------

## API Contract Verification

Before implementing the integration:

1.  Verify current Qikink Open API documentation.
2.  Verify authentication requirements.
3.  Verify API base URL.
4.  Verify order creation endpoint.
5.  Verify order status endpoint.
6.  Verify tracking endpoint.
7.  Verify product/variant identifiers.
8.  Verify design file requirements.
9.  Verify rate limits.
10. Verify webhook support, if available.
11. Verify sandbox/test capabilities, if available.

Record the verified contract in the project's technical documentation.

Never invent endpoint names, authentication headers, request fields, or
response fields.

------------------------------------------------------------------------

## Authentication & Secrets

Store Qikink credentials only in server-side environment configuration.

Suggested configuration structure:

``` text
Qikink:
    ApiBaseUrl
    ApiKey
    ApiSecret
```

Use the exact configuration required by the verified Qikink API.

Never expose Qikink credentials to:

-   Browser JavaScript
-   Frontend environment variables
-   Customer API responses
-   Logs
-   Git repositories

------------------------------------------------------------------------

## Order Submission

Implement an idempotent fulfillment submission process.

Before creating a Qikink order:

-   Check whether a fulfillment record already exists.
-   Check whether the order has already been submitted.
-   Use a deterministic local idempotency key where supported.
-   Persist the submission state.

Possible lifecycle:

``` text
PendingSubmission
        ↓
Submitting
        ↓
Submitted
        ↓
Processing
```

If the external request times out, do not blindly submit again. First
determine whether the provider accepted the previous request.

------------------------------------------------------------------------

## Failure Handling

Handle:

-   Authentication failures
-   Validation failures
-   Product mapping failures
-   Missing design files
-   Out-of-stock products
-   Insufficient provider credits
-   Network failures
-   Timeout
-   Rate limiting
-   Provider server errors
-   Duplicate order responses
-   Invalid shipping information

The local customer order must remain intact when fulfillment fails.

Store:

-   Error category
-   Safe provider response details
-   Failure reason
-   Timestamp
-   Retry count

Never store provider secrets in error logs.

------------------------------------------------------------------------

## Retry Strategy

Retries should only happen for retryable failures.

Retryable examples:

-   Temporary network failure
-   Timeout
-   Provider 5xx response
-   Temporary rate limit

Do not automatically retry permanent failures such as:

-   Invalid product mapping
-   Invalid design
-   Missing required customer information
-   Invalid credentials

Provide an administrator-controlled retry mechanism for failed
fulfillment operations.

------------------------------------------------------------------------

## Admin Panel

Extend the existing Admin Panel with fulfillment information.

### Product Management

Support:

-   Enable Qikink fulfillment
-   Map product to Qikink product
-   Map variants
-   Store design reference
-   Store design file
-   View mapping status

### Order Management

Show:

-   Fulfillment provider
-   Fulfillment status
-   Qikink order ID
-   Provider status
-   Tracking ID
-   Courier
-   Failure reason
-   Last synchronization time
-   Retry action when appropriate

------------------------------------------------------------------------

## Customer Order Page

Customers should see:

-   Order status
-   Shipment status
-   Tracking number when available
-   Courier when available
-   Tracking link when available

Do not expose internal provider errors or unnecessary technical
identifiers.

------------------------------------------------------------------------

## Synchronization

Prefer webhooks if the verified Qikink API provides a reliable webhook
mechanism.

Otherwise implement secure polling/synchronization.

Synchronization must:

-   Be idempotent.
-   Update only valid state transitions.
-   Store the provider's latest status.
-   Update tracking data.
-   Record synchronization timestamp.
-   Handle provider downtime gracefully.

------------------------------------------------------------------------

## Security

Protect the integration against:

-   Credential leakage
-   Unauthorized fulfillment actions
-   Duplicate submissions
-   Request replay
-   Malicious webhook requests
-   Untrusted provider data
-   Log leakage

If Qikink webhooks are supported:

-   Verify webhook authenticity using the provider's documented
    mechanism.
-   Validate payloads.
-   Reject unauthorized requests.
-   Make webhook processing idempotent.

------------------------------------------------------------------------

## Financial Safety

Never automatically submit an unpaid or unverified order to Qikink.

Before fulfillment:

-   Verify payment server-side.
-   Verify payment status.
-   Verify local order integrity.
-   Verify fulfillment eligibility.

Qikink fulfillment costs are separate from the customer's store payment.
The customer pays the store; Qikink charges the merchant for production
and shipping according to the merchant's Qikink balance/credits.

------------------------------------------------------------------------

## Cancellation

Support cancellation only when both the store and provider permit it.

Never assume that a local cancellation automatically cancels an external
fulfillment order.

Verify the current Qikink cancellation rules before exposing
cancellation actions.

------------------------------------------------------------------------

## Returns & RTO

Prepare the architecture to handle:

-   Delivered
-   RTO initiated
-   Returned
-   Reverse pickup
-   Refund-related workflows

Store provider shipment states so later return workflows can use them.

------------------------------------------------------------------------

## Observability

Log safe operational events:

-   Fulfillment submission started
-   Fulfillment submitted
-   Fulfillment submission failed
-   Status synchronized
-   Tracking synchronized
-   Retry attempted
-   Cancellation requested
-   Webhook received

Never log:

-   API secrets
-   Passwords
-   Payment secrets
-   Full authentication tokens
-   Sensitive customer data unnecessarily

------------------------------------------------------------------------

## Testing

### Unit Tests

Test:

-   Product mapping
-   Variant mapping
-   Status mapping
-   Eligibility rules
-   Idempotency
-   Retry classification
-   Error classification

### Integration Tests

Test:

-   Qikink authentication
-   Order submission
-   Status retrieval
-   Tracking retrieval
-   Error responses

Use a sandbox/test environment when Qikink provides one.

Do not place real customer orders during automated tests.

------------------------------------------------------------------------

## Manual Test Scenarios

### Product Mapping

-   Create a Qikink-enabled product.
-   Map its variant.
-   Save mapping.
-   Reload.
-   Verify mapping persists.

### Successful Fulfillment

-   Create a test order.
-   Complete payment in the configured test environment.
-   Verify local order becomes eligible.
-   Submit fulfillment.
-   Verify Qikink order ID is stored.

### Duplicate Protection

-   Trigger fulfillment twice.
-   Verify only one provider order is created.

### Failure

-   Simulate an invalid mapping.
-   Verify local order remains intact.
-   Verify failure is visible to admin.
-   Verify retry becomes available only when appropriate.

### Status Synchronization

Verify provider statuses update the local fulfillment record.

### Tracking

Verify:

-   Tracking ID
-   Courier
-   Tracking URL
-   Shipment status

appear correctly.

### Cancellation

Test cancellation only where the provider allows it.

### Customer View

Verify customers can see shipment status and tracking information.

------------------------------------------------------------------------

## Performance

-   Do not block normal storefront requests on Qikink API calls.
-   Use background processing for fulfillment and synchronization where
    appropriate.
-   Avoid repeated provider requests.
-   Cache safe provider metadata when useful.
-   Respect Qikink API rate limits.

------------------------------------------------------------------------

## Acceptance Criteria

Phase 8 is complete when:

-   Qikink Open API contract has been verified from current provider
    documentation/credentials.
-   Credentials are stored securely server-side.
-   Product-to-Qikink mapping works.
-   Variant mapping works.
-   Design references are stored correctly.
-   Paid orders can be submitted to Qikink.
-   Duplicate submissions are prevented.
-   Qikink order IDs are stored.
-   Provider status synchronization works.
-   Tracking information synchronization works.
-   Fulfillment failures are handled safely.
-   Retry behavior works.
-   Admin can inspect fulfillment state.
-   Customers can see appropriate shipment information.
-   Local order data remains consistent if Qikink is unavailable.
-   No Qikink credentials are exposed to the frontend.
-   No sensitive information is leaked through logs.
-   Automated tests pass.
-   Manual testing passes.
-   Production build succeeds.
-   No TypeScript errors.
-   No backend compilation errors.
-   No runtime errors.
-   No application-generated console errors.

------------------------------------------------------------------------

## Phase Completion Rule

Do not mark Phase 8 complete merely because one test order reaches
Qikink.

The complete lifecycle must be verified:

``` text
Store Product
    ↓
Qikink Mapping
    ↓
Customer Order
    ↓
Payment Verification
    ↓
Fulfillment Eligibility
    ↓
Qikink Submission
    ↓
Qikink Order ID
    ↓
Production Status
    ↓
Shipment
    ↓
Tracking
    ↓
Delivery
```

Phase 8 is complete only when the integration is reliable, secure,
observable, idempotent, and recoverable from provider failures.
