import { PaginatedResponse } from "./admin";

export interface StorefrontProduct {
  id: string;
  name: string;
  slug: string;
  shortDescription?: string;
  price: number;
  comparePrice?: number;
  brand?: string;
  isFeatured: boolean;
  categoryName?: string;
  categorySlug?: string;
  createdAt: string;
  primaryImageUrl?: string;
  secondaryImageUrl?: string;
  colors: string[];
  sizes: string[];
  reviewCount: number;
  averageRating: number;
  isInStock: boolean;
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
