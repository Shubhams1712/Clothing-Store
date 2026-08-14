import { api } from "@/lib/api";

export interface PaymentOrder {
  orderId: string;
  amount: number;
  currency: string;
  keyId: string;
}

export interface OrderItem {
  productId: string;
  variantId?: string;
  productName: string;
  sku?: string;
  imageUrl?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  size?: string;
  color?: string;
}

export interface CustomerOrder {
  id: string;
  orderNumber: string;
  status: string;
  subTotal: number;
  taxAmount: number;
  shippingAmount: number;
  discountAmount: number;
  totalAmount: number;
  currency?: string;
  paymentMethod?: string;
  paymentStatus?: string;
  paymentId?: string;
  shippingName?: string;
  shippingAddress?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingPostalCode?: string;
  shippingCountry?: string;
  shippingPhone?: string;
  notes?: string;
  createdAt: string;
  shippedAt?: string;
  deliveredAt?: string;
  items: OrderItem[];
}

export interface OrderTrackingEvent {
  status: string;
  timestamp: string;
  description?: string;
}

export interface OrderTracking {
  orderNumber: string;
  currentStatus: string;
  createdAt: string;
  shippedAt?: string;
  deliveredAt?: string;
  trackingNumber?: string;
  timeline: OrderTrackingEvent[];
}

export interface Invoice {
  orderNumber: string;
  invoiceDate: string;
  customerName: string;
  customerEmail: string;
  shippingName: string;
  shippingAddress?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingPostalCode?: string;
  shippingCountry?: string;
  items: Array<{
    productName: string;
    sku?: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
  }>;
  subTotal: number;
  taxAmount: number;
  shippingAmount: number;
  discountAmount: number;
  totalAmount: number;
  paymentMethod?: string;
  paymentId?: string;
}

export interface CreateOrderRequest {
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
  items: Array<{
    productId: string;
    variantId?: string;
    quantity: number;
  }>;
  couponCode?: string;
  shippingAddressId?: string;
  shippingMethod?: string;
  notes?: string;
}

export interface CreateCodOrderRequest {
  items: Array<{
    productId: string;
    variantId?: string;
    quantity: number;
  }>;
  couponCode?: string;
  shippingAddressId?: string;
  shippingMethod?: string;
  notes?: string;
}

export const paymentService = {
  async createRazorpayOrder(amount: number, currency = "INR", receipt?: string): Promise<PaymentOrder> {
    const response = await api.post("/api/payments/create-order", { amount, currency, receipt });
    return response.data.data;
  },

  async createOrderAfterPayment(request: CreateOrderRequest): Promise<CustomerOrder> {
    const response = await api.post("/api/payments/create-order-after-payment", request);
    return response.data.data;
  },

  async createCodOrder(request: CreateCodOrderRequest): Promise<CustomerOrder> {
    const response = await api.post("/api/payments/cod", request);
    return response.data.data;
  },
};

export const orderService = {
  async getOrders(page = 1, pageSize = 20): Promise<CustomerOrder[]> {
    const response = await api.get("/api/orders", { params: { page, pageSize } });
    return response.data.data;
  },

  async getOrderById(id: string): Promise<CustomerOrder> {
    const response = await api.get(`/api/orders/${id}`);
    return response.data.data;
  },

  async cancelOrder(id: string): Promise<void> {
    await api.put(`/api/orders/${id}/cancel`);
  },

  async requestRefund(id: string, reason?: string): Promise<void> {
    await api.put(`/api/orders/${id}/refund`, { reason });
  },

  async getOrderTracking(id: string): Promise<OrderTracking> {
    const response = await api.get(`/api/orders/${id}/tracking`);
    return response.data.data;
  },

  async getOrderInvoice(id: string): Promise<Invoice> {
    const response = await api.get(`/api/orders/${id}/invoice`);
    return response.data.data;
  },
};
