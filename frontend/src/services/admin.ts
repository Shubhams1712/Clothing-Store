import { API_CONFIG } from "@/config/api";
import { api } from "@/lib/api";
import type {
  PaginatedResponse,
  PaginatedRequest,
  Product,
  Category,
  Collection,
  Order,
  Customer,
  CustomerDetail,
  Coupon,
  Review,
  DashboardStats,
  StoreSettings,
  AnalyticsDateRange,
  DashboardAnalytics,
  SalesAnalytics,
  ProductAnalytics,
  CustomerAnalytics,
  InventoryAnalytics,
  OrderAnalytics,
  ReportExportRequest,
} from "@/types/admin";

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

export const adminApi = {
  dashboard: {
    getStats: async () => {
      const response = await api.get<{ data: DashboardStats }>(API_CONFIG.ENDPOINTS.ADMIN.DASHBOARD);
      return response.data.data;
    },
  },

  products: {
    list: async (params: PaginatedRequest = {}) => {
      const query = buildQueryString(params as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: PaginatedResponse<Product> }>(`${API_CONFIG.ENDPOINTS.ADMIN.PRODUCTS}${query}`);
      return response.data.data;
    },
    get: async (id: string) => {
      const response = await api.get<{ data: Product }>(`${API_CONFIG.ENDPOINTS.ADMIN.PRODUCTS}/${id}`);
      return response.data.data;
    },
    create: async (data: Record<string, unknown>) => {
      const response = await api.post<{ data: Product }>(API_CONFIG.ENDPOINTS.ADMIN.PRODUCTS, data);
      return response.data.data;
    },
    update: async (id: string, data: Record<string, unknown>) => {
      const response = await api.put<{ data: Product }>(`${API_CONFIG.ENDPOINTS.ADMIN.PRODUCTS}/${id}`, data);
      return response.data.data;
    },
    delete: async (id: string) => {
      await api.delete(`${API_CONFIG.ENDPOINTS.ADMIN.PRODUCTS}/${id}`);
    },
    togglePublish: async (id: string) => {
      await api.patch(`${API_CONFIG.ENDPOINTS.ADMIN.PRODUCTS}/${id}/toggle-publish`);
    },
  },

  categories: {
    list: async (params: PaginatedRequest = {}) => {
      const query = buildQueryString(params as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: PaginatedResponse<Category> }>(`${API_CONFIG.ENDPOINTS.ADMIN.CATEGORIES}${query}`);
      return response.data.data;
    },
    get: async (id: string) => {
      const response = await api.get<{ data: Category }>(`${API_CONFIG.ENDPOINTS.ADMIN.CATEGORIES}/${id}`);
      return response.data.data;
    },
    create: async (data: Record<string, unknown>) => {
      const response = await api.post<{ data: Category }>(API_CONFIG.ENDPOINTS.ADMIN.CATEGORIES, data);
      return response.data.data;
    },
    update: async (id: string, data: Record<string, unknown>) => {
      const response = await api.put<{ data: Category }>(`${API_CONFIG.ENDPOINTS.ADMIN.CATEGORIES}/${id}`, data);
      return response.data.data;
    },
    delete: async (id: string) => {
      await api.delete(`${API_CONFIG.ENDPOINTS.ADMIN.CATEGORIES}/${id}`);
    },
  },

  collections: {
    list: async (params: PaginatedRequest = {}) => {
      const query = buildQueryString(params as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: PaginatedResponse<Collection> }>(`${API_CONFIG.ENDPOINTS.ADMIN.COLLECTIONS}${query}`);
      return response.data.data;
    },
    get: async (id: string) => {
      const response = await api.get<{ data: Collection }>(`${API_CONFIG.ENDPOINTS.ADMIN.COLLECTIONS}/${id}`);
      return response.data.data;
    },
    create: async (data: Record<string, unknown>) => {
      const response = await api.post<{ data: Collection }>(API_CONFIG.ENDPOINTS.ADMIN.COLLECTIONS, data);
      return response.data.data;
    },
    update: async (id: string, data: Record<string, unknown>) => {
      const response = await api.put<{ data: Collection }>(`${API_CONFIG.ENDPOINTS.ADMIN.COLLECTIONS}/${id}`, data);
      return response.data.data;
    },
    delete: async (id: string) => {
      await api.delete(`${API_CONFIG.ENDPOINTS.ADMIN.COLLECTIONS}/${id}`);
    },
  },

  orders: {
    list: async (params: PaginatedRequest = {}) => {
      const query = buildQueryString(params as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: PaginatedResponse<Order> }>(`${API_CONFIG.ENDPOINTS.ADMIN.ORDERS}${query}`);
      return response.data.data;
    },
    get: async (id: string) => {
      const response = await api.get<{ data: Order }>(`${API_CONFIG.ENDPOINTS.ADMIN.ORDERS}/${id}`);
      return response.data.data;
    },
    updateStatus: async (id: string, data: { status: string; internalNotes?: string }) => {
      const response = await api.put<{ data: Order }>(`${API_CONFIG.ENDPOINTS.ADMIN.ORDERS}/${id}/status`, data);
      return response.data.data;
    },
  },

  customers: {
    list: async (params: PaginatedRequest = {}) => {
      const query = buildQueryString(params as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: PaginatedResponse<Customer> }>(`${API_CONFIG.ENDPOINTS.ADMIN.CUSTOMERS}${query}`);
      return response.data.data;
    },
    get: async (id: string) => {
      const response = await api.get<{ data: CustomerDetail }>(`${API_CONFIG.ENDPOINTS.ADMIN.CUSTOMERS}/${id}`);
      return response.data.data;
    },
    toggleActive: async (id: string) => {
      await api.patch(`${API_CONFIG.ENDPOINTS.ADMIN.CUSTOMERS}/${id}/toggle-active`);
    },
  },

  coupons: {
    list: async (params: PaginatedRequest = {}) => {
      const query = buildQueryString(params as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: PaginatedResponse<Coupon> }>(`${API_CONFIG.ENDPOINTS.ADMIN.COUPONS}${query}`);
      return response.data.data;
    },
    get: async (id: string) => {
      const response = await api.get<{ data: Coupon }>(`${API_CONFIG.ENDPOINTS.ADMIN.COUPONS}/${id}`);
      return response.data.data;
    },
    create: async (data: Record<string, unknown>) => {
      const response = await api.post<{ data: Coupon }>(API_CONFIG.ENDPOINTS.ADMIN.COUPONS, data);
      return response.data.data;
    },
    update: async (id: string, data: Record<string, unknown>) => {
      const response = await api.put<{ data: Coupon }>(`${API_CONFIG.ENDPOINTS.ADMIN.COUPONS}/${id}`, data);
      return response.data.data;
    },
    delete: async (id: string) => {
      await api.delete(`${API_CONFIG.ENDPOINTS.ADMIN.COUPONS}/${id}`);
    },
  },

  reviews: {
    list: async (params: PaginatedRequest & { isApproved?: boolean } = {}) => {
      const { isApproved, ...rest } = params;
      const query = buildQueryString({ ...rest, isApproved: isApproved !== undefined ? String(isApproved) : undefined } as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: PaginatedResponse<Review> }>(`${API_CONFIG.ENDPOINTS.ADMIN.REVIEWS}${query}`);
      return response.data.data;
    },
    update: async (id: string, data: { isApproved: boolean; isHidden: boolean }) => {
      const response = await api.patch<{ data: Review }>(`${API_CONFIG.ENDPOINTS.ADMIN.REVIEWS}/${id}`, data);
      return response.data.data;
    },
    reply: async (id: string, data: { adminReply: string }) => {
      const response = await api.post<{ data: Review }>(`${API_CONFIG.ENDPOINTS.ADMIN.REVIEWS}/${id}/reply`, data);
      return response.data.data;
    },
    delete: async (id: string) => {
      await api.delete(`${API_CONFIG.ENDPOINTS.ADMIN.REVIEWS}/${id}`);
    },
  },

  settings: {
    get: async () => {
      const response = await api.get<{ data: StoreSettings }>(API_CONFIG.ENDPOINTS.ADMIN.SETTINGS);
      return response.data.data;
    },
    update: async (data: Record<string, unknown>) => {
      const response = await api.put<{ data: StoreSettings }>(API_CONFIG.ENDPOINTS.ADMIN.SETTINGS, data);
      return response.data.data;
    },
  },

  inventory: {
    list: async (params: PaginatedRequest = {}) => {
      const query = buildQueryString(params as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: PaginatedResponse<Product> }>(`${API_CONFIG.ENDPOINTS.ADMIN.INVENTORY}${query}`);
      return response.data.data;
    },
    updateStock: async (variantId: string, stock: number) => {
      await api.put(`${API_CONFIG.ENDPOINTS.ADMIN.INVENTORY}/${variantId}`, { stock });
    },
  },

  analytics: {
    getDashboard: async (dateRange: AnalyticsDateRange = {}) => {
      const query = buildQueryString(dateRange as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: DashboardAnalytics }>(`${API_CONFIG.ENDPOINTS.ADMIN.ANALYTICS}/dashboard${query}`);
      return response.data.data;
    },
    getSales: async (dateRange: AnalyticsDateRange = {}) => {
      const query = buildQueryString(dateRange as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: SalesAnalytics }>(`${API_CONFIG.ENDPOINTS.ADMIN.ANALYTICS}/sales${query}`);
      return response.data.data;
    },
    getProducts: async (dateRange: AnalyticsDateRange = {}) => {
      const query = buildQueryString(dateRange as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: ProductAnalytics }>(`${API_CONFIG.ENDPOINTS.ADMIN.ANALYTICS}/products${query}`);
      return response.data.data;
    },
    getCustomers: async (dateRange: AnalyticsDateRange = {}) => {
      const query = buildQueryString(dateRange as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: CustomerAnalytics }>(`${API_CONFIG.ENDPOINTS.ADMIN.ANALYTICS}/customers${query}`);
      return response.data.data;
    },
    getInventory: async (dateRange: AnalyticsDateRange = {}) => {
      const query = buildQueryString(dateRange as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: InventoryAnalytics }>(`${API_CONFIG.ENDPOINTS.ADMIN.ANALYTICS}/inventory${query}`);
      return response.data.data;
    },
    getOrders: async (dateRange: AnalyticsDateRange = {}) => {
      const query = buildQueryString(dateRange as Record<string, string | number | boolean | undefined>);
      const response = await api.get<{ data: OrderAnalytics }>(`${API_CONFIG.ENDPOINTS.ADMIN.ANALYTICS}/orders${query}`);
      return response.data.data;
    },
    exportReport: async (request: ReportExportRequest) => {
      const response = await api.post(`${API_CONFIG.ENDPOINTS.ADMIN.ANALYTICS}/reports/export`, request, {
        responseType: "blob",
      });
      return response.data;
    },
  },
};
