import { PaginatedResponse } from "./admin";

export interface StorefrontProduct {
  id: string;
  name: string;
  slug: string;
  description?: string;
  shortDescription?: string;
  sku?: string;
  price: number;
  comparePrice?: number;
  brand?: string;
  isFeatured: boolean;
  categoryName?: string;
  categorySlug?: string;
  createdAt: string;
  primaryImageUrl?: string;
  secondaryImageUrl?: string;
  images: StorefrontProductImage[];
  colors: string[];
  sizes: string[];
  variants: StorefrontProductVariant[];
  reviewCount: number;
  averageRating: number;
  isInStock: boolean;
}

export interface StorefrontProductImage {
  id: string;
  url: string;
  altText?: string;
  sortOrder: number;
  isFeatured: boolean;
}

export interface StorefrontProductVariant {
  id: string;
  size?: string;
  color?: string;
  sku: string;
  price: number;
  stock: number;
  isAvailable: boolean;
}

export interface StorefrontCategory {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  productCount: number;
}

export interface StorefrontCollection {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  isFeatured: boolean;
  productCount: number;
}

export interface ProductFilterParams {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  categoryId?: string;
  categorySlug?: string;
  collectionId?: string;
  collectionSlug?: string;
  size?: string;
  color?: string;
  minPrice?: number;
  maxPrice?: number;
  isFeatured?: boolean;
  isNewArrival?: boolean;
  isBestSeller?: boolean;
  inStock?: boolean;
}

export type PaginatedProducts = PaginatedResponse<StorefrontProduct>;
