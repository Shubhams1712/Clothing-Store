import type { NextConfig } from "next";

const isDev = process.env.NODE_ENV === "development";

const nextConfig: NextConfig = {
  images: {
    dangerouslyAllowLocalIP: isDev,
    formats: ["image/avif", "image/webp"],
    deviceSizes: [640, 750, 828, 1080, 1200, 1920],
    imageSizes: [16, 32, 48, 64, 96, 128, 256, 384],
    remotePatterns: isDev
      ? [
          { protocol: "https", hostname: "res.cloudinary.com" },
          { protocol: "http", hostname: "localhost", port: "5124" },
          { protocol: "https", hostname: "localhost", port: "5124" },
        ]
      : [{ protocol: "https", hostname: "res.cloudinary.com" }],
  },
  compress: true,
  poweredByHeader: false,
};

export default nextConfig;
