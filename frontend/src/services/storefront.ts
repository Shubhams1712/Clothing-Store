import { api } from "@/lib/api";
import { API_CONFIG } from "@/config/api";
import {
  StorefrontProduct,
  StorefrontCategory,
  StorefrontCollection,
  ProductFilterParams,
  PaginatedProducts,
  StoreSettings,
} from "@/types/storefront";

const ENDPOINTS = API_CONFIG.ENDPOINTS.STOREFRONT;

export const storefrontService = {
  async getProducts(params?: ProductFilterParams): Promise<PaginatedProducts> {
    const searchParams = new URLSearchParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== "") {
          searchParams.append(key, String(value));
        }
      });
    }
    const query = searchParams.toString();
    const response = await api.get(`${ENDPOINTS.PRODUCTS}${query ? `?${query}` : ""}`);
    return response.data.data;
  },

  async getProductBySlug(slug: string): Promise<StorefrontProduct | null> {
    try {
      const response = await api.get(ENDPOINTS.PRODUCT_BY_SLUG(slug));
      return response.data.data;
    } catch {
      return null;
    }
  },

  async getFeaturedProducts(page = 1, pageSize = 8): Promise<PaginatedProducts> {
    const response = await api.get(ENDPOINTS.FEATURED_PRODUCTS, { params: { page, pageSize } });
    return response.data.data;
  },

  async getNewArrivals(page = 1, pageSize = 20): Promise<PaginatedProducts> {
    const response = await api.get(ENDPOINTS.NEW_ARRIVALS, { params: { page, pageSize } });
    return response.data.data;
  },

  async getBestSellers(page = 1, pageSize = 20): Promise<PaginatedProducts> {
    const response = await api.get(ENDPOINTS.BEST_SELLERS, { params: { page, pageSize } });
    return response.data.data;
  },

  async getAvailableSizes(): Promise<string[]> {
    const response = await api.get(ENDPOINTS.SIZES);
    return response.data.data;
  },

  async getAvailableColors(): Promise<string[]> {
    const response = await api.get(ENDPOINTS.COLORS);
    return response.data.data;
  },

  async getCategories(): Promise<StorefrontCategory[]> {
    const response = await api.get(ENDPOINTS.CATEGORIES);
    return response.data.data;
  },

  async getCategoryBySlug(slug: string): Promise<StorefrontCategory | null> {
    try {
      const response = await api.get(ENDPOINTS.CATEGORY_BY_SLUG(slug));
      return response.data.data;
    } catch {
      return null;
    }
  },

  async getCollections(): Promise<StorefrontCollection[]> {
    const response = await api.get(ENDPOINTS.COLLECTIONS);
    return response.data.data;
  },

  async getFeaturedCollections(): Promise<StorefrontCollection[]> {
    const response = await api.get(ENDPOINTS.FEATURED_COLLECTIONS);
    return response.data.data;
  },

  async getCollectionBySlug(slug: string): Promise<StorefrontCollection | null> {
    try {
      const response = await api.get(ENDPOINTS.COLLECTION_BY_SLUG(slug));
      return response.data.data;
    } catch {
      return null;
    }
  },

  async getSettings(): Promise<StoreSettings> {
    try {
      const response = await api.get(ENDPOINTS.SETTINGS);
      return response.data.data;
    } catch {
      return { id: "", storeName: "The Freak Store" };
    }
  },
};
