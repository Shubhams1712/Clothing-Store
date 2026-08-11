"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Star, ThumbsUp, MessageSquare, ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Textarea } from "@/components/ui/textarea";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { reviewService, type StorefrontReview, type RatingDistribution } from "@/services/reviews";
import { useAuth } from "@/hooks/use-auth";
import { toast } from "sonner";

interface ReviewsSectionProps {
  productId: string;
  reviewCount: number;
  averageRating: number;
}

export function ReviewsSection({ productId, reviewCount, averageRating }: ReviewsSectionProps) {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [sortBy, setSortBy] = useState<string>("newest");
  const [formRating, setFormRating] = useState(0);
  const [formTitle, setFormTitle] = useState("");
  const [formComment, setFormComment] = useState("");
  const [hoveredStar, setHoveredStar] = useState(0);

  const { data: reviewsData, isLoading: reviewsLoading } = useQuery({
    queryKey: ["reviews", productId, sortBy],
    queryFn: () => reviewService.getReviews(productId, 1, 20, sortBy),
  });

  const { data: distribution } = useQuery({
    queryKey: ["rating-distribution", productId],
    queryFn: () => reviewService.getRatingDistribution(productId),
  });

  const createReviewMutation = useMutation({
    mutationFn: () => reviewService.createReview(productId, {
      rating: formRating,
      title: formTitle,
      comment: formComment || undefined,
    }),
    onSuccess: () => {
      toast.success("Review submitted for approval");
      setShowForm(false);
      setFormRating(0);
      setFormTitle("");
      setFormComment("");
      queryClient.invalidateQueries({ queryKey: ["reviews", productId] });
      queryClient.invalidateQueries({ queryKey: ["rating-distribution", productId] });
    },
    onError: (err: Error) => {
      toast.error(err.message || "Failed to submit review");
    },
  });

  const handleSubmitReview = () => {
    if (formRating === 0) {
      toast.error("Please select a rating");
      return;
    }
    if (!formTitle.trim()) {
      toast.error("Please enter a title");
      return;
    }
    createReviewMutation.mutate();
  };

  const distributionTotal = distribution
    ? distribution.fiveStar + distribution.fourStar + distribution.threeStar + distribution.twoStar + distribution.oneStar
    : 0;

  const getDistributionPercent = (count: number) =>
    distributionTotal > 0 ? Math.round((count / distributionTotal) * 100) : 0;

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Customer Reviews</h2>
        {user && (
          <Button variant="outline" onClick={() => setShowForm(!showForm)}>
            {showForm ? "Cancel" : "Write a Review"}
          </Button>
        )}
      </div>

      {/* Rating Summary */}
      <div className="grid gap-6 md:grid-cols-[200px_1fr]">
        <div className="flex flex-col items-center justify-center space-y-2">
          <span className="text-4xl font-bold">{averageRating.toFixed(1)}</span>
          <div className="flex">
            {[...Array(5)].map((_, i) => (
              <Star
                key={i}
                className={`h-5 w-5 ${
                  i < Math.round(averageRating) ? "fill-primary text-primary" : "text-muted-foreground"
                }`}
              />
            ))}
          </div>
          <span className="text-sm text-muted-foreground">{reviewCount} reviews</span>
        </div>

        {distribution && (
          <div className="space-y-2">
            {[
              { label: "5", count: distribution.fiveStar },
              { label: "4", count: distribution.fourStar },
              { label: "3", count: distribution.threeStar },
              { label: "2", count: distribution.twoStar },
              { label: "1", count: distribution.oneStar },
            ].map(({ label, count }) => (
              <div key={label} className="flex items-center gap-3">
                <span className="w-8 text-sm text-muted-foreground">{label} ★</span>
                <div className="flex-1 h-2 rounded-full bg-muted overflow-hidden">
                  <div
                    className="h-full bg-primary rounded-full transition-all"
                    style={{ width: `${getDistributionPercent(count)}%` }}
                  />
                </div>
                <span className="w-8 text-right text-sm text-muted-foreground">{count}</span>
              </div>
            ))}
          </div>
        )}
      </div>

      <Separator />

      {/* Review Form */}
      {showForm && (
        <Card>
          <CardContent className="space-y-4 p-6">
            <h3 className="font-semibold">Write Your Review</h3>

            <div>
              <label className="mb-2 block text-sm font-medium">Rating *</label>
              <div className="flex gap-1">
                {[1, 2, 3, 4, 5].map(star => (
                  <button
                    key={star}
                    type="button"
                    onClick={() => setFormRating(star)}
                    onMouseEnter={() => setHoveredStar(star)}
                    onMouseLeave={() => setHoveredStar(0)}
                    aria-label={`${star} star${star !== 1 ? "s" : ""}`}
                    className="p-0.5"
                  >
                    <Star
                      className={`h-6 w-6 transition-colors ${
                        star <= (hoveredStar || formRating)
                          ? "fill-primary text-primary"
                          : "text-muted-foreground"
                      }`}
                    />
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label htmlFor="review-title" className="mb-2 block text-sm font-medium">Title *</label>
              <Input
                id="review-title"
                value={formTitle}
                onChange={e => setFormTitle(e.target.value)}
                placeholder="Summarize your experience"
                maxLength={200}
              />
            </div>

            <div>
              <label htmlFor="review-comment" className="mb-2 block text-sm font-medium">Comment</label>
              <Textarea
                id="review-comment"
                value={formComment}
                onChange={e => setFormComment(e.target.value)}
                placeholder="Tell others about your experience with this product"
                rows={4}
                maxLength={2000}
              />
            </div>

            <Button
              onClick={handleSubmitReview}
              disabled={createReviewMutation.isPending}
            >
              {createReviewMutation.isPending ? "Submitting..." : "Submit Review"}
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Sort */}
      <div className="flex items-center gap-4">
        <span className="text-sm text-muted-foreground">Sort by:</span>
        <Select value={sortBy} onValueChange={(value) => value && setSortBy(value)}>
          <SelectTrigger className="w-[180px]" aria-label="Sort reviews">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="newest">Newest</SelectItem>
            <SelectItem value="oldest">Oldest</SelectItem>
            <SelectItem value="rating_high">Highest Rating</SelectItem>
            <SelectItem value="rating_low">Lowest Rating</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Reviews List */}
      {reviewsLoading ? (
        <div className="space-y-4">
          {[...Array(3)].map((_, i) => (
            <div key={i} className="animate-pulse space-y-2">
              <div className="h-4 w-32 rounded bg-muted" />
              <div className="h-4 w-48 rounded bg-muted" />
              <div className="h-16 rounded bg-muted" />
            </div>
          ))}
        </div>
      ) : reviewsData?.items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <MessageSquare className="mb-4 h-10 w-10 text-muted-foreground" />
          <p className="font-medium">No reviews yet</p>
          <p className="mt-1 text-sm text-muted-foreground">Be the first to review this product!</p>
          {user && !showForm && (
            <Button variant="outline" size="sm" className="mt-4" onClick={() => setShowForm(true)}>
              Write a Review
            </Button>
          )}
        </div>
      ) : (
        <div className="space-y-6">
          {reviewsData?.items.map((review: StorefrontReview) => (
            <div key={review.id} className="space-y-3">
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    <div className="flex">
                      {[...Array(5)].map((_, i) => (
                        <Star
                          key={i}
                          className={`h-4 w-4 ${
                            i < review.rating ? "fill-primary text-primary" : "text-muted-foreground"
                          }`}
                        />
                      ))}
                    </div>
                    <span className="font-medium">{review.title}</span>
                  </div>
                  <p className="mt-1 text-sm text-muted-foreground">
                    by {review.userName} on{" "}
                    {new Date(review.createdAt).toLocaleDateString("en-IN", {
                      year: "numeric",
                      month: "long",
                      day: "numeric",
                    })}
                  </p>
                </div>
              </div>

              {review.comment && (
                <p className="text-sm text-muted-foreground">{review.comment}</p>
              )}

              {review.adminReply && (
                <div className="ml-4 rounded-lg border-l-4 border-primary bg-muted/50 p-3">
                  <p className="mb-1 text-xs font-semibold text-primary">Store Reply</p>
                  <p className="text-sm">{review.adminReply}</p>
                </div>
              )}

              <Separator />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
