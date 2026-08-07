export const API_CONFIG = {
  BASE_URL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000",
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
    },
  },
} as const;
