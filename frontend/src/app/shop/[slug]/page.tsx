"use client";

import { use } from "react";
import { useQuery } from "@tanstack/react-query";
import Image from "next/image";
import Link from "next/link";
import { Heart, ShoppingBag, Star, ChevronRight } from "lucide-react";
import { storefrontService } from "@/services/storefront";
import { Button, buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { ProductGrid } from "@/components/storefront/product-grid";
import { getSafeImageUrl } from "@/lib/utils";

export default function ProductDetailPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = use(params);

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

  if (isLoading) return <LoadingOverlay text="Loading product..." />;
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

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Breadcrumb */}
      <nav className="mb-6 flex items-center gap-2 text-sm text-muted-foreground">
        <Link href="/" className="hover:text-foreground">
          Home
        </Link>
        <ChevronRight className="h-3 w-3" />
        <Link href="/shop" className="hover:text-foreground">
          Shop
        </Link>
        {product.categoryName && (
          <>
            <ChevronRight className="h-3 w-3" />
            <Link href={`/categories/${product.categorySlug}`} className="hover:text-foreground">
              {product.categoryName}
            </Link>
          </>
        )}
        <ChevronRight className="h-3 w-3" />
        <span className="text-foreground">{product.name}</span>
      </nav>

      <div className="grid gap-8 lg:grid-cols-2">
        {/* Images */}
        <div className="space-y-4">
          <div className="relative aspect-[3/4] overflow-hidden rounded-lg bg-muted">
            <Image
              src={getSafeImageUrl(product.primaryImageUrl)}
              alt={product.name}
              fill
              className="object-cover"
              sizes="(max-width: 1024px) 100vw, 50vw"
              priority
            />
            {hasDiscount && (
              <Badge className="absolute left-4 top-4 bg-destructive text-destructive-foreground">
                -{discountPercent}%
              </Badge>
            )}
          </div>
          {product.secondaryImageUrl && (
            <div className="relative aspect-[3/4] w-1/2 overflow-hidden rounded-lg bg-muted">
              <Image
                src={getSafeImageUrl(product.secondaryImageUrl)}
                alt={`${product.name} alternate`}
                fill
                className="object-cover"
                sizes="25vw"
              />
            </div>
          )}
        </div>

        {/* Details */}
        <div className="space-y-6">
          <div>
            {product.brand && <p className="text-sm text-muted-foreground">{product.brand}</p>}
            <h1 className="text-3xl font-bold">{product.name}</h1>
            {product.reviewCount > 0 && (
              <div className="mt-2 flex items-center gap-2">
                <div className="flex">
                  {[...Array(5)].map((_, i) => (
                    <Star
                      key={i}
                      className={`h-4 w-4 ${
                        i < Math.round(product.averageRating)
                          ? "fill-primary text-primary"
                          : "text-muted-foreground"
                      }`}
                    />
                  ))}
                </div>
                <span className="text-sm text-muted-foreground">
                  {product.averageRating.toFixed(1)} ({product.reviewCount} reviews)
                </span>
              </div>
            )}
          </div>

          <div className="flex items-baseline gap-3">
            <span className="text-3xl font-bold">${product.price.toFixed(2)}</span>
            {hasDiscount && (
              <span className="text-lg text-muted-foreground line-through">
                ${product.comparePrice!.toFixed(2)}
              </span>
            )}
          </div>

          {product.shortDescription && (
            <p className="text-muted-foreground">{product.shortDescription}</p>
          )}

          {/* Colors */}
          {product.colors.length > 0 && (
            <div>
              <p className="mb-2 text-sm font-medium">Colors</p>
              <div className="flex flex-wrap gap-2">
                {product.colors.map((color) => (
                  <Button key={color} variant="outline" size="sm">
                    <div
                      className="mr-2 h-3 w-3 rounded-full border"
                      style={{ backgroundColor: color.toLowerCase() }}
                    />
                    {color}
                  </Button>
                ))}
              </div>
            </div>
          )}

          {/* Sizes */}
          {product.sizes.length > 0 && (
            <div>
              <p className="mb-2 text-sm font-medium">Sizes</p>
              <div className="flex flex-wrap gap-2">
                {product.sizes.map((size) => (
                  <Button key={size} variant="outline" size="sm">
                    {size}
                  </Button>
                ))}
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-3">
            <Button size="lg" className="flex-1 gap-2" disabled={!product.isInStock}>
              <ShoppingBag className="h-5 w-5" />
              {product.isInStock ? "Add to Cart" : "Out of Stock"}
            </Button>
            <Button size="lg" variant="outline">
              <Heart className="h-5 w-5" />
            </Button>
          </div>

          {/* Product Info */}
          <Card>
            <CardContent className="space-y-2 p-4 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Category</span>
                <span>{product.categoryName || "N/A"}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Availability</span>
                <Badge variant={product.isInStock ? "default" : "destructive"}>
                  {product.isInStock ? "In Stock" : "Out of Stock"}
                </Badge>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Related Products */}
      {relatedProducts?.items && relatedProducts.items.length > 0 && (
        <div className="mt-16">
          <h2 className="mb-6 text-2xl font-bold">You May Also Like</h2>
          <ProductGrid products={relatedProducts.items.filter((p) => p.id !== product.id)} />
        </div>
      )}
    </div>
  );
}
