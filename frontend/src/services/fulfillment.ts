import { API_CONFIG } from "@/config/api";
import { api } from "@/lib/api";
import type { FulfillmentOrder, FulfillmentProvider, ProductFulfillmentMapping } from "@/types/fulfillment";
import type { PaginatedResponse } from "@/types/admin";

function buildQueryString(params: Record<string, string | number | boolean | undefined>): string {
  const searchParams = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      searchParams.append(key, String(value));
    }
  });
  const str = searchParams.toString();
  return str ? `?${str}` : "";
}

export const fulfillmentService = {
  getOrder: async (orderId: string): Promise<FulfillmentOrder | null> => {
    try {
      const response = await api.get<{ data: FulfillmentOrder }>(
        `${API_CONFIG.ENDPOINTS.ADMIN.ORDERS}/${orderId}/fulfillment`
      );
      return response.data.data;
    } catch {
      return null;
    }
  },

  listOrders: async (params: Record<string, string | number | boolean | undefined> = {}) => {
    const query = buildQueryString(params);
    const response = await api.get<{ data: PaginatedResponse<FulfillmentOrder> }>(
      `/api/admin/fulfillment${query}`
    );
    return response.data.data;
  },

  getProviders: async (): Promise<FulfillmentProvider[]> => {
    try {
      const response = await api.get<{ data: FulfillmentProvider[] }>(
        "/api/admin/fulfillment/providers"
      );
      return response.data.data;
    } catch {
      return [];
    }
  },

  listMappings: async (params: Record<string, string | number | boolean | undefined> = {}) => {
    const query = buildQueryString(params);
    const response = await api.get<{ data: PaginatedResponse<ProductFulfillmentMapping> }>(
      `/api/admin/fulfillment/mappings${query}`
    );
    return response.data.data;
  },

  createMapping: async (data: Record<string, unknown>) => {
    const response = await api.post<{ data: ProductFulfillmentMapping }>(
      "/api/admin/fulfillment/mappings",
      data
    );
    return response.data.data;
  },

  updateMapping: async (id: string, data: Record<string, unknown>) => {
    const response = await api.put<{ data: ProductFulfillmentMapping }>(
      `/api/admin/fulfillment/mappings/${id}`,
      data
    );
    return response.data.data;
  },

  deleteMapping: async (id: string) => {
    await api.delete(`/api/admin/fulfillment/mappings/${id}`);
  },

  retry: async (fulfillmentOrderId: string) => {
    const response = await api.post<{ data: FulfillmentOrder }>(
      `/api/admin/fulfillment/${fulfillmentOrderId}/retry`
    );
    return response.data.data;
  },
};
