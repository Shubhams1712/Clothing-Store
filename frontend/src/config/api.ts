function getBaseUrl(): string {
  const url = process.env.NEXT_PUBLIC_API_URL;
  if (url) return url;

  if (process.env.NODE_ENV === "production") {
    throw new Error(
      "NEXT_PUBLIC_API_URL is not set. " +
      "Define it in your environment variables for production builds."
    );
  }

  return "http://localhost:5000";
}

export const API_CONFIG = {
  BASE_URL: getBaseUrl(),
  TIMEOUT: 15000,
  ENDPOINTS: {
    HEALTH: "/api/health",
    AUTH: {
      REGISTER: "/api/auth/register",
      LOGIN: "/api/auth/login",
      LOGOUT: "/api/auth/logout",
      REFRESH: "/api/auth/refresh",
      FORGOT_PASSWORD: "/api/auth/forgot-password",
      RESET_PASSWORD: "/api/auth/reset-password",
      VERIFY_EMAIL: "/api/auth/verify-email",
      ADMIN_LOGIN: "/api/auth/admin/login",
    },
    USERS: {
      ME: "/api/users/me",
      CHANGE_PASSWORD: "/api/users/change-password",
    },
    STOREFRONT: {
      PRODUCTS: "/api/products",
      PRODUCT_BY_SLUG: (slug: string) => `/api/products/${slug}`,
      FEATURED_PRODUCTS: "/api/products/featured",
      NEW_ARRIVALS: "/api/products/new-arrivals",
      BEST_SELLERS: "/api/products/best-sellers",
      SIZES: "/api/products/sizes",
      COLORS: "/api/products/colors",
      CATEGORIES: "/api/categories",
      CATEGORY_BY_SLUG: (slug: string) => `/api/categories/${slug}`,
      COLLECTIONS: "/api/collections",
      FEATURED_COLLECTIONS: "/api/collections/featured",
      COLLECTION_BY_SLUG: (slug: string) => `/api/collections/${slug}`,
      SETTINGS: "/api/storefront/settings",
    },
    ADMIN: {
      DASHBOARD: "/api/admin/dashboard",
      PRODUCTS: "/api/admin/products",
      CATEGORIES: "/api/admin/categories",
      COLLECTIONS: "/api/admin/collections",
      ORDERS: "/api/admin/orders",
      CUSTOMERS: "/api/admin/customers",
      COUPONS: "/api/admin/coupons",
      REVIEWS: "/api/admin/reviews",
      SETTINGS: "/api/admin/settings",
      INVENTORY: "/api/admin/inventory",
      ANALYTICS: "/api/admin/analytics",
    },
    MEDIA: {
      UPLOAD: "/api/media/upload",
      DELETE: "/api/media",
    },
  },
} as const;
