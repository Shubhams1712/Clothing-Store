"use client";

import { useState } from "react";
import { use } from "react";
import { useQuery } from "@tanstack/react-query";
import Image from "next/image";
import Link from "next/link";
import { Heart, ShoppingBag, Star, ChevronRight, Minus, Plus, Truck, RotateCcw, Shield } from "lucide-react";
import { storefrontService } from "@/services/storefront";
import { useCart } from "@/hooks/use-cart";
import { Button, buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { ProductGrid } from "@/components/storefront/product-grid";
import { getSafeImageUrl, formatPrice } from "@/lib/utils";
import { toast } from "sonner";

export default function ProductDetailPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = use(params);
  const { addItem } = useCart();
  const [selectedColor, setSelectedColor] = useState<string>("");
  const [selectedSize, setSelectedSize] = useState<string>("");
  const [quantity, setQuantity] = useState(1);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);

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

  const handleAddToCart = () => {
    if (!product) return;
    addItem({
      productId: product.id,
      variantId: product.id,
      name: product.name,
      slug: product.slug,
      price: product.price,
      imageUrl: product.primaryImageUrl || "",
      size: selectedSize || "",
      color: selectedColor || "",
      stock: 999,
    });
    toast.success("Added to cart");
  };

  const handleQuantityChange = (delta: number) => {
    const newQty = quantity + delta;
    if (newQty >= 1 && newQty <= 99) {
      setQuantity(newQty);
    }
  };

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

  const allImages = product.images?.length > 0
    ? product.images
    : [
        ...(product.primaryImageUrl ? [{ id: "primary", url: product.primaryImageUrl, altText: product.name, sortOrder: 0, isFeatured: true }] : []),
        ...(product.secondaryImageUrl ? [{ id: "secondary", url: product.secondaryImageUrl, altText: `${product.name} alternate`, sortOrder: 1, isFeatured: false }] : []),
      ];

  const currentImage = allImages[selectedImageIndex] || allImages[0];

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
        {/* Image Gallery */}
        <div className="space-y-4">
          {/* Main Image */}
          <div className="relative aspect-[3/4] overflow-hidden rounded-lg bg-muted">
            <Image
              src={getSafeImageUrl(currentImage?.url)}
              alt={currentImage?.altText || product.name}
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

          {/* Thumbnails */}
          {allImages.length > 1 && (
            <div className="flex gap-2 overflow-x-auto pb-2">
              {allImages.map((image, index) => (
                <button
                  key={image.id}
                  onClick={() => setSelectedImageIndex(index)}
                  className={`relative h-20 w-20 shrink-0 overflow-hidden rounded-md border-2 transition-all ${
                    selectedImageIndex === index
                      ? "border-primary"
                      : "border-transparent hover:border-muted-foreground/50"
                  }`}
                >
                  <Image
                    src={getSafeImageUrl(image.url)}
                    alt={image.altText || `${product.name} ${index + 1}`}
                    fill
                    className="object-cover"
                    sizes="80px"
                  />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Details */}
        <div className="space-y-6">
          <div>
            {product.brand && <p className="text-sm text-muted-foreground">{product.brand}</p>}
            <h1 className="text-3xl font-bold">{product.name}</h1>
            {product.sku && (
              <p className="mt-1 text-sm text-muted-foreground">SKU: {product.sku}</p>
            )}
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
            <span className="text-3xl font-bold">{formatPrice(product.price)}</span>
            {hasDiscount && (
              <span className="text-lg text-muted-foreground line-through">
                {formatPrice(product.comparePrice!)}
              </span>
            )}
          </div>

          {product.shortDescription && (
            <p className="text-muted-foreground">{product.shortDescription}</p>
          )}

          {/* Colors */}
          {product.colors.length > 0 && (
            <div>
              <p className="mb-2 text-sm font-medium">Color</p>
              <div className="flex flex-wrap gap-2">
                {product.colors.map((color) => (
                  <Button
                    key={color}
                    variant={selectedColor === color ? "default" : "outline"}
                    size="sm"
                    onClick={() => setSelectedColor(color)}
                  >
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
              <p className="mb-2 text-sm font-medium">Size</p>
              <div className="flex flex-wrap gap-2">
                {product.sizes.map((size) => (
                  <Button
                    key={size}
                    variant={selectedSize === size ? "default" : "outline"}
                    size="sm"
                    onClick={() => setSelectedSize(size)}
                  >
                    {size}
                  </Button>
                ))}
              </div>
            </div>
          )}

          {/* Quantity Selector */}
          <div>
            <p className="mb-2 text-sm font-medium">Quantity</p>
            <div className="flex items-center gap-3">
              <div className="flex items-center border rounded-md">
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-10 w-10 rounded-none"
                  onClick={() => handleQuantityChange(-1)}
                  disabled={quantity <= 1}
                >
                  <Minus className="h-4 w-4" />
                </Button>
                <span className="w-12 text-center text-sm font-medium">{quantity}</span>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-10 w-10 rounded-none"
                  onClick={() => handleQuantityChange(1)}
                  disabled={quantity >= 99}
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>
              {product.isInStock && (
                <Badge variant="outline" className="text-xs">
                  In Stock
                </Badge>
              )}
            </div>
          </div>

          {/* Actions */}
          <div className="flex gap-3">
            <Button size="lg" className="flex-1 gap-2" disabled={!product.isInStock} onClick={handleAddToCart}>
              <ShoppingBag className="h-5 w-5" />
              {product.isInStock ? "Add to Cart" : "Out of Stock"}
            </Button>
            <Button size="lg" variant="outline">
              <Heart className="h-5 w-5" />
            </Button>
          </div>

          {/* Delivery Info */}
          <Card>
            <CardContent className="space-y-3 p-4">
              <div className="flex items-center gap-3 text-sm">
                <Truck className="h-4 w-4 text-muted-foreground" />
                <span>Free shipping on orders over ₹2,000</span>
              </div>
              <div className="flex items-center gap-3 text-sm">
                <RotateCcw className="h-4 w-4 text-muted-foreground" />
                <span>Easy 30-day returns & exchanges</span>
              </div>
              <div className="flex items-center gap-3 text-sm">
                <Shield className="h-4 w-4 text-muted-foreground" />
                <span>Secure checkout</span>
              </div>
            </CardContent>
          </Card>

          {/* Product Info */}
          <Card>
            <CardContent className="space-y-2 p-4 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Category</span>
                <span>{product.categoryName || "N/A"}</span>
              </div>
              {product.sku && (
                <div className="flex justify-between">
                  <span className="text-muted-foreground">SKU</span>
                  <span>{product.sku}</span>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Product Description Tabs */}
      {product.description && (
        <div className="mt-16">
          <Tabs defaultValue="description">
            <TabsList>
              <TabsTrigger value="description">Description</TabsTrigger>
              <TabsTrigger value="shipping">Shipping & Returns</TabsTrigger>
            </TabsList>
            <TabsContent value="description" className="mt-4">
              <div className="prose max-w-none text-muted-foreground">
                <p>{product.description}</p>
              </div>
            </TabsContent>
            <TabsContent value="shipping" className="mt-4">
              <div className="space-y-4 text-muted-foreground">
                <div>
                  <h3 className="font-medium text-foreground">Shipping</h3>
                  <p>Free standard shipping on orders over ₹2,000. Express shipping available at checkout.</p>
                </div>
                <div>
                  <h3 className="font-medium text-foreground">Returns</h3>
                  <p>Easy returns within 30 days of purchase. Items must be unworn with tags attached.</p>
                </div>
              </div>
            </TabsContent>
          </Tabs>
        </div>
      )}

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
