export const API_CONFIG = {
  BASE_URL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000",
  TIMEOUT: 15000,
  ENDPOINTS: {
    HEALTH: "/api/health",
  },
} as const;
