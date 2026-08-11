export interface FulfillmentOrder {
  id: string;
  orderId: string;
  providerId: string;
  providerName?: string;
  externalOrderId?: string;
  status: string;
  providerStatus?: string;
  failureReason?: string;
  errorCategory?: string;
  submittedAt?: string;
  completedAt?: string;
  items: FulfillmentOrderItem[];
  shipment?: Shipment;
}

export interface FulfillmentOrderItem {
  id: string;
  fulfillmentOrderId: string;
  orderItemId: string;
  externalProductId: string;
  externalVariantId?: string;
  externalSku: string;
  quantity: number;
  status?: string;
  failureReason?: string;
  designReference?: string;
  designFileUrl?: string;
  mockupUrl?: string;
}

export interface Shipment {
  id: string;
  fulfillmentOrderId: string;
  trackingNumber?: string;
  courierName?: string;
  trackingUrl?: string;
  providerShippingStatus?: string;
}

export interface FulfillmentProvider {
  id: string;
  name: string;
  code?: string;
  apiBaseUrl?: string;
  isEnabled: boolean;
}

export interface ProductFulfillmentMapping {
  id: string;
  productId: string;
  productName?: string;
  productVariantId?: string;
  variantSku?: string;
  providerId: string;
  providerName?: string;
  externalProductId: string;
  externalVariantId?: string;
  externalSku: string;
  designReference?: string;
  designFileUrl?: string;
  printingType?: string;
  printingPlacement?: string;
  isActive: boolean;
}
