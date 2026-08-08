import { api } from "@/lib/api";

export interface StorefrontReview {
  id: string;
  userName: string;
  rating: number;
  title: string;
  comment?: string;
  adminReply?: string;
  createdAt: string;
}

export interface RatingDistribution {
  fiveStar: number;
  fourStar: number;
  threeStar: number;
  twoStar: number;
  oneStar: number;
}

export interface PaginatedReviews {
  items: StorefrontReview[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateReviewPayload {
  rating: number;
  title: string;
  comment?: string;
}

export const reviewService = {
  async getReviews(productId: string, page = 1, pageSize = 10, sortBy?: string): Promise<PaginatedReviews> {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (sortBy) params.set("sortBy", sortBy);
    const response = await api.get(`/api/products/${productId}/reviews?${params.toString()}`);
    return response.data.data;
  },

  async getRatingDistribution(productId: string): Promise<RatingDistribution> {
    const response = await api.get(`/api/products/${productId}/rating-distribution`);
    return response.data.data;
  },

  async createReview(productId: string, payload: CreateReviewPayload): Promise<StorefrontReview> {
    const response = await api.post(`/api/products/${productId}/reviews`, payload);
    return response.data.data;
  },
};
