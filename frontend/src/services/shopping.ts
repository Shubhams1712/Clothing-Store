import { api } from "@/lib/api";

export interface Address {
  id: string;
  fullName: string;
  phone: string;
  email?: string;
  addressLine1: string;
  addressLine2?: string;
  landmark?: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  isDefault: boolean;
}

export interface CreateAddressPayload {
  fullName: string;
  phone: string;
  email?: string;
  addressLine1: string;
  addressLine2?: string;
  landmark?: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  isDefault: boolean;
}

export interface CouponApplyResult {
  isValid: boolean;
  code?: string;
  description?: string;
  type: string;
  value: number;
  discountAmount: number;
  message?: string;
}

export interface CheckoutItem {
  productId: string;
  variantId?: string;
  quantity: number;
}

export interface CheckoutReviewResult {
  items: Array<{
    productId: string;
    variantId?: string;
    productName: string;
    sku?: string;
    imageUrl?: string;
    unitPrice: number;
    quantity: number;
    totalPrice: number;
    availableStock: number;
    isAvailable: boolean;
  }>;
  subTotal: number;
  taxAmount: number;
  shippingAmount: number;
  discountAmount: number;
  totalAmount: number;
  coupon?: CouponApplyResult;
  isValid: boolean;
  errors: string[];
}

export const addressService = {
  async getAddresses(): Promise<Address[]> {
    const response = await api.get("/api/addresses");
    return response.data.data;
  },

  async createAddress(payload: CreateAddressPayload): Promise<Address> {
    const response = await api.post("/api/addresses", payload);
    return response.data.data;
  },

  async updateAddress(id: string, payload: CreateAddressPayload): Promise<Address> {
    const response = await api.put(`/api/addresses/${id}`, payload);
    return response.data.data;
  },

  async deleteAddress(id: string): Promise<void> {
    await api.delete(`/api/addresses/${id}`);
  },
};

export const couponService = {
  async applyCoupon(code: string, orderSubtotal: number): Promise<CouponApplyResult> {
    const response = await api.post("/api/coupons/apply", { code, orderSubtotal });
    return response.data.data;
  },
};

export const checkoutService = {
  async reviewCheckout(items: CheckoutItem[], couponCode?: string): Promise<CheckoutReviewResult> {
    const response = await api.post("/api/checkout/review", { items, couponCode });
    return response.data.data;
  },
};
