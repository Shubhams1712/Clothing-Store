"use client";

import { useState, useEffect } from "react";
import { use } from "react";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  Heart,
  Share2,
  ShoppingBag,
  Zap,
  Star,
  ChevronRight,
  Minus,
  Plus,
  Truck,
  RotateCcw,
  Shield,
  Clock,
  RefreshCw,
} from "lucide-react";
import { storefrontService } from "@/services/storefront";
import { useCart } from "@/hooks/use-cart";
import { useWishlist } from "@/hooks/use-wishlist";
import { useRecentlyViewed } from "@/hooks/use-recently-viewed";
import { Button, buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { ProductGrid } from "@/components/storefront/product-grid";
import { ProductImageGallery } from "@/components/storefront/product-image-gallery";
import { ReviewsSection } from "@/components/storefront/reviews-section";
import { Specifications } from "@/components/storefront/specifications";
import { formatPrice } from "@/lib/utils";
import { toast } from "sonner";

export default function ProductDetailPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = use(params);
  const router = useRouter();
  const { addItem } = useCart();
  const { toggleItem, isInWishlist } = useWishlist();
  const { addItem: addRecentlyViewed } = useRecentlyViewed();

  const [selectedColor, setSelectedColor] = useState<string>("");
  const [selectedSize, setSelectedSize] = useState<string>("");
  const [quantity, setQuantity] = useState(1);

  const { data: product, isLoading } = useQuery({
    queryKey: ["product", slug],
    queryFn: () => storefrontService.getProductBySlug(slug),
  });

  const { data: relatedProducts } = useQuery({
    queryKey: ["related-products", product?.categorySlug],
    queryFn: () =>
      storefrontService.getProducts({
        categorySlug: product?.categorySlug,
        pageSize: 4,
      }),
    enabled: !!product?.categorySlug,
  });

  // Track recently viewed
  useEffect(() => {
    if (product) {
      addRecentlyViewed({
        productId: product.id,
        name: product.name,
        slug: product.slug,
        price: product.price,
        imageUrl: product.primaryImageUrl || "",
        brand: product.brand,
        categoryName: product.categoryName,
      });
    }
  }, [product?.id]);

  // Find selected variant based on color/size
  const selectedVariant = product?.variants?.find(
    v =>
      (!selectedColor || v.color === selectedColor) &&
      (!selectedSize || v.size === selectedSize) &&
      v.isAvailable
  );

  const maxStock = selectedVariant?.stock ?? 99;
  const currentPrice = selectedVariant?.price ?? product?.price ?? 0;

  const handleQuantityChange = (delta: number) => {
    const newQty = quantity + delta;
    if (newQty >= 1 && newQty <= maxStock) {
      setQuantity(newQty);
    }
  };

  const handleQuantityInput = (value: string) => {
    const num = parseInt(value, 10);
    if (!isNaN(num) && num >= 1 && num <= maxStock) {
      setQuantity(num);
    }
  };

  const handleAddToCart = () => {
    if (!product) return;
    if (product.colors.length > 0 && !selectedColor) {
      toast.error("Please select a color");
      return;
    }
    if (product.sizes.length > 0 && !selectedSize) {
      toast.error("Please select a size");
      return;
    }
    addItem({
      productId: product.id,
      variantId: selectedVariant?.id ?? product.id,
      name: product.name,
      slug: product.slug,
      price: currentPrice,
      imageUrl: product.primaryImageUrl || "",
      size: selectedSize || "",
      color: selectedColor || "",
      stock: maxStock,
      quantity,
    });
    toast.success("Added to cart");
  };

  const handleBuyNow = () => {
    handleAddToCart();
    setTimeout(() => {
      router.push("/checkout");
    }, 300);
  };

  const handleToggleWishlist = () => {
    if (!product) return;
    toggleItem({
      productId: product.id,
      name: product.name,
      slug: product.slug,
      price: product.price,
      imageUrl: product.primaryImageUrl || "",
      brand: product.brand,
    });
    toast.success(isInWishlist(product.id) ? "Removed from wishlist" : "Added to wishlist");
  };

  const handleShare = async () => {
    const url = window.location.href;
    if (navigator.share) {
      try {
        await navigator.share({ title: product?.name, url });
      } catch {
        // user cancelled
      }
    } else {
      await navigator.clipboard.writeText(url);
      toast.success("Link copied to clipboard");
    }
  };

  if (isLoading) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        {/* Breadcrumb skeleton */}
        <div className="mb-6 flex items-center gap-2">
          <Skeleton className="h-4 w-12" />
          <Skeleton className="h-4 w-1" />
          <Skeleton className="h-4 w-12" />
          <Skeleton className="h-4 w-1" />
          <Skeleton className="h-4 w-32" />
        </div>

        {/* Product layout skeleton - matches final 2-column grid */}
        <div className="grid gap-8 lg:grid-cols-2">
          {/* Image gallery skeleton */}
          <div className="space-y-4">
            <Skeleton className="aspect-[3/4] w-full rounded-lg" />
            <div className="flex gap-2">
              {[1, 2, 3, 4].map((i) => (
                <Skeleton key={i} className="h-20 w-20 rounded" />
              ))}
            </div>
          </div>

          {/* Details skeleton */}
          <div className="space-y-6">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-9 w-3/4" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-8 w-32" />
            <Skeleton className="h-4 w-48" />
            <div className="space-y-2">
              <Skeleton className="h-4 w-16" />
              <div className="flex gap-2">
                {[1, 2, 3].map((i) => (
                  <Skeleton key={i} className="h-9 w-9 rounded" />
                ))}
              </div>
            </div>
            <div className="space-y-2">
              <Skeleton className="h-4 w-12" />
              <div className="flex gap-2">
                {[1, 2, 3, 4].map((i) => (
                  <Skeleton key={i} className="h-9 w-14 rounded" />
                ))}
              </div>
            </div>
            <div className="flex gap-3">
              <Skeleton className="h-12 flex-1 rounded" />
              <Skeleton className="h-12 w-12 rounded" />
            </div>
          </div>
        </div>

        {/* Tabs skeleton */}
        <div className="mt-16 space-y-4">
          <Skeleton className="h-10 w-full max-w-md" />
          <Skeleton className="h-48 w-full rounded" />
        </div>

        {/* Reviews skeleton */}
        <div className="mt-16 space-y-4">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-32 w-full rounded" />
        </div>

        {/* Related products skeleton */}
        <div className="mt-16 space-y-4">
          <Skeleton className="h-8 w-48" />
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="space-y-2">
                <Skeleton className="aspect-[3/4] w-full rounded-lg" />
                <Skeleton className="h-4 w-3/4" />
                <Skeleton className="h-4 w-1/2" />
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }
  if (!product) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <div className="text-center">
          <h2 className="text-2xl font-bold">Product not found</h2>
          <Link href="/shop" className={buttonVariants({ variant: "link", className: "mt-4" })}>
            Back to Shop
          </Link>
        </div>
      </div>
    );
  }

  const hasDiscount = product.comparePrice && product.comparePrice > product.price;
  const discountPercent = hasDiscount
    ? Math.round(((product.comparePrice! - product.price) / product.comparePrice!) * 100)
    : 0;

  const inWishlist = isInWishlist(product.id);

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Breadcrumb */}
      <nav className="mb-6 flex items-center gap-2 text-xs text-neutral-400" aria-label="Breadcrumb">
        <Link href="/" className="hover:text-black transition-colors">
          Home
        </Link>
        <ChevronRight className="h-3 w-3" aria-hidden="true" />
        <Link href="/shop" className="hover:text-black transition-colors">
          Shop
        </Link>
        {product.categoryName && (
          <>
            <ChevronRight className="h-3 w-3" aria-hidden="true" />
            <Link
              href={`/categories/${product.categorySlug}`}
              className="hover:text-black transition-colors"
            >
              {product.categoryName}
            </Link>
          </>
        )}
        <ChevronRight className="h-3 w-3" aria-hidden="true" />
        <span className="text-black font-medium" aria-current="page">{product.name}</span>
      </nav>

      <div className="grid gap-8 lg:grid-cols-2">
        {/* Image Gallery */}
        <ProductImageGallery
          images={
            product.images?.length > 0
              ? product.images
              : [
                  ...(product.primaryImageUrl
                    ? [{ id: "primary", url: product.primaryImageUrl, altText: product.name, sortOrder: 0, isFeatured: true }]
                    : []),
                  ...(product.secondaryImageUrl
                    ? [{ id: "secondary", url: product.secondaryImageUrl, altText: `${product.name} alternate`, sortOrder: 1, isFeatured: false }]
                    : []),
                ]
          }
          productName={product.name}
        />

        {/* Details */}
        <div className="space-y-6">
          {/* Title & Brand */}
          <div>
            {product.brand && (
              <p className="text-[11px] font-semibold text-neutral-400 uppercase tracking-widest">{product.brand}</p>
            )}
            <h1 className="mt-1 text-3xl font-bold uppercase tracking-tight">{product.name}</h1>
            {product.sku && (
              <p className="mt-1 text-xs text-neutral-400">SKU: {product.sku}</p>
            )}

            {/* Rating */}
            {product.reviewCount > 0 && (
              <div className="mt-2 flex items-center gap-2">
                <div className="flex" role="img" aria-label={`${product.averageRating.toFixed(1)} out of 5 stars`}>
                  {[...Array(5)].map((_, i) => (
                    <Star
                      key={i}
                      className={`h-4 w-4 ${
                        i < Math.round(product.averageRating)
                          ? "fill-[#E10600] text-[#E10600]"
                          : "text-neutral-300"
                      }`}
                    />
                  ))}
                </div>
                <span className="text-sm text-neutral-500">
                  {product.averageRating.toFixed(1)} ({product.reviewCount} reviews)
                </span>
              </div>
            )}
          </div>

          {/* Price */}
          <div className="flex items-baseline gap-3">
            <span className="text-3xl font-bold">{formatPrice(currentPrice)}</span>
            {hasDiscount && currentPrice === product.price && (
              <span className="text-lg text-neutral-400 line-through">
                {formatPrice(product.comparePrice!)}
              </span>
            )}
            {hasDiscount && (
              <Badge className="bg-[#E10600] text-white border-0 text-[10px] font-bold uppercase tracking-wider">
                -{discountPercent}%
              </Badge>
            )}
          </div>

          {/* Short Description */}
          {product.shortDescription && (
            <p className="text-sm text-neutral-500">{product.shortDescription}</p>
          )}

          <Separator className="bg-black/10" />

          {/* Colors */}
          {product.colors.length > 0 && (
            <div>
              <label className="mb-2 block text-xs font-semibold uppercase tracking-wider text-neutral-500">
                Color {selectedColor && `- ${selectedColor}`}
              </label>
              <div className="flex flex-wrap gap-2" role="radiogroup" aria-label="Select color">
                {product.colors.map(color => (
                  <button
                    key={color}
                    type="button"
                    role="radio"
                    aria-checked={selectedColor === color}
                    onClick={() => {
                      setSelectedColor(color);
                      setQuantity(1);
                    }}
                    className={`flex items-center gap-2 border px-3 py-2 text-sm transition-all ${
                      selectedColor === color
                        ? "border-black bg-black text-white"
                        : "border-black/10 hover:border-black/30"
                    }`}
                  >
                    <div
                      className="h-4 w-4 rounded-full border border-white/20"
                      style={{ backgroundColor: color.toLowerCase() }}
                    />
                    {color}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Sizes */}
          {product.sizes.length > 0 && (
            <div>
              <label className="mb-2 block text-xs font-semibold uppercase tracking-wider text-neutral-500">
                Size {selectedSize && `- ${selectedSize}`}
              </label>
              <div className="flex flex-wrap gap-2" role="radiogroup" aria-label="Select size">
                {product.sizes.map(size => (
                  <button
                    key={size}
                    type="button"
                    role="radio"
                    aria-checked={selectedSize === size}
                    onClick={() => {
                      setSelectedSize(size);
                      setQuantity(1);
                    }}
                    className={`border px-4 py-2 text-sm font-medium transition-all ${
                      selectedSize === size
                        ? "border-black bg-black text-white"
                        : "border-black/10 hover:border-black/30"
                    }`}
                  >
                    {size}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Quantity Selector */}
          <div>
            <label htmlFor="quantity-input" className="mb-2 block text-xs font-semibold uppercase tracking-wider text-neutral-500">
              Quantity {selectedVariant && `(Max: ${maxStock})`}
            </label>
            <div className="flex items-center gap-3">
              <div className="flex items-center border border-black/10">
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-10 w-10 rounded-none"
                  onClick={() => handleQuantityChange(-1)}
                  disabled={quantity <= 1}
                  aria-label="Decrease quantity"
                >
                  <Minus className="h-4 w-4" />
                </Button>
                <input
                  id="quantity-input"
                  type="number"
                  min={1}
                  max={maxStock}
                  value={quantity}
                  onChange={e => handleQuantityInput(e.target.value)}
                  className="h-10 w-12 border-0 bg-transparent text-center text-sm font-medium [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none"
                  aria-label="Quantity"
                />
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-10 w-10 rounded-none"
                  onClick={() => handleQuantityChange(1)}
                  disabled={quantity >= maxStock}
                  aria-label="Increase quantity"
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>
              {product.isInStock ? (
                <Badge variant="outline" className="border-black/10 text-xs uppercase tracking-wider">
                  In Stock
                </Badge>
              ) : (
                <Badge variant="destructive" className="border-0 bg-[#E10600] text-white text-[10px] font-bold uppercase tracking-wider">
                  Out of Stock
                </Badge>
              )}
            </div>
          </div>

          <Separator className="bg-black/10" />

          {/* Purchase Actions */}
          <div className="flex gap-3">
            <Button
              size="lg"
              className="flex-1 gap-2 bg-black text-white hover:bg-neutral-800"
              disabled={!product.isInStock}
              onClick={handleAddToCart}
              aria-label="Add to cart"
            >
              <ShoppingBag className="h-5 w-5" />
              Add to Cart
            </Button>
            <Button
              size="lg"
              className="gap-2 bg-[#E10600] text-white hover:bg-[#c40500]"
              disabled={!product.isInStock}
              onClick={handleBuyNow}
              aria-label="Buy now"
            >
              <Zap className="h-5 w-5" />
              Buy Now
            </Button>
          </div>

          <div className="flex gap-3">
            <Button
              variant="outline"
              size="lg"
              className="flex-1 gap-2 border-black/10"
              onClick={handleToggleWishlist}
              aria-label={inWishlist ? "Remove from wishlist" : "Add to wishlist"}
            >
              <Heart className={`h-5 w-5 ${inWishlist ? "fill-[#E10600] text-[#E10600]" : ""}`} />
              {inWishlist ? "In Wishlist" : "Add to Wishlist"}
            </Button>
            <Button
              variant="outline"
              size="lg"
              className="gap-2 border-black/10"
              onClick={handleShare}
              aria-label="Share product"
            >
              <Share2 className="h-5 w-5" />
              Share
            </Button>
          </div>

          {/* Delivery & Returns */}
          <div className="border border-black/10 p-4">
            <div className="space-y-3">
              <div className="flex items-center gap-3 text-sm">
                <Truck className="h-4 w-4 text-neutral-400 shrink-0" />
                <div>
                  <p className="font-semibold">Free Shipping</p>
                  <p className="text-xs text-neutral-500">On orders over ₹2,000</p>
                </div>
              </div>
              <div className="flex items-center gap-3 text-sm">
                <Clock className="h-4 w-4 text-neutral-400 shrink-0" />
                <div>
                  <p className="font-semibold">Estimated Delivery</p>
                  <p className="text-xs text-neutral-500">3-7 business days</p>
                </div>
              </div>
              <div className="flex items-center gap-3 text-sm">
                <RotateCcw className="h-4 w-4 text-neutral-400 shrink-0" />
                <div>
                  <p className="font-semibold">Easy Returns & Exchanges</p>
                  <p className="text-xs text-neutral-500">30-day return policy</p>
                </div>
              </div>
              <div className="flex items-center gap-3 text-sm">
                <RefreshCw className="h-4 w-4 text-neutral-400 shrink-0" />
                <div>
                  <p className="font-semibold">Exchange Available</p>
                  <p className="text-xs text-neutral-500">Swap for a different size or color</p>
                </div>
              </div>
              <div className="flex items-center gap-3 text-sm">
                <Shield className="h-4 w-4 text-neutral-400 shrink-0" />
                <div>
                  <p className="font-semibold">Secure Checkout</p>
                  <p className="text-xs text-neutral-500">SSL encrypted payment</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Product Details Tabs */}
      <div className="mt-16">
        <Tabs defaultValue="description">
          <TabsList>
            <TabsTrigger value="description">Description</TabsTrigger>
            <TabsTrigger value="specifications">Specifications</TabsTrigger>
            <TabsTrigger value="shipping">Shipping & Returns</TabsTrigger>
          </TabsList>

          <TabsContent value="description" className="mt-6">
            <div className="prose max-w-none">
              {product.description ? (
                <div className="text-muted-foreground whitespace-pre-line">{product.description}</div>
              ) : (
                <p className="text-muted-foreground">No description available for this product.</p>
              )}
            </div>
          </TabsContent>

          <TabsContent value="specifications" className="mt-6">
            <Specifications
              sku={product.sku}
              brand={product.brand}
              category={product.categoryName}
            />
          </TabsContent>

          <TabsContent value="shipping" className="mt-6">
            <div className="space-y-6 text-muted-foreground">
              <div>
                <h3 className="font-semibold text-foreground mb-2">Shipping</h3>
                <ul className="space-y-1 text-sm">
                  <li>Free standard shipping on orders over ₹2,000</li>
                  <li>Express shipping available at checkout (₹150)</li>
                  <li>Orders are processed within 1-2 business days</li>
                  <li>Tracking information provided via email</li>
                </ul>
              </div>
              <div>
                <h3 className="font-semibold text-foreground mb-2">Returns & Exchanges</h3>
                <ul className="space-y-1 text-sm">
                  <li>30-day return policy from date of delivery</li>
                  <li>Items must be unworn, unwashed, with original tags</li>
                  <li>Free exchange for different size or color</li>
                  <li>Refunds processed within 5-7 business days</li>
                </ul>
              </div>
              <div>
                <h3 className="font-semibold text-foreground mb-2">Secure Payment</h3>
                <p className="text-sm">
                  We use industry-standard SSL encryption to protect your payment information.
                  All transactions are processed securely through Razorpay.
                </p>
              </div>
            </div>
          </TabsContent>
        </Tabs>
      </div>

      {/* Reviews Section */}
      <div className="mt-16">
        <ReviewsSection
          productId={product.id}
          reviewCount={product.reviewCount}
          averageRating={product.averageRating}
        />
      </div>

      {/* Related Products */}
      <div className="mt-16">
        <h2 className="mb-6 text-2xl font-bold">You May Also Like</h2>
        {relatedProducts?.items && relatedProducts.items.length > 0 ? (
          <ProductGrid products={relatedProducts.items.filter(p => p.id !== product.id)} />
        ) : (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="space-y-2">
                <Skeleton className="aspect-[3/4] w-full rounded-lg" />
                <Skeleton className="h-4 w-3/4" />
                <Skeleton className="h-4 w-1/2" />
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
