"use client";

import Link from "next/link";
import Image from "next/image";
import { Heart, ShoppingBag, Trash2, ArrowLeft } from "lucide-react";
import { useWishlist } from "@/hooks/use-wishlist";
import { useCart } from "@/hooks/use-cart";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { formatPrice, getSafeImageUrl } from "@/lib/utils";
import { toast } from "sonner";

export default function WishlistPage() {
  const { items, removeItem } = useWishlist();
  const { addItem } = useCart();

  const handleMoveToCart = (item: typeof items[0]) => {
    addItem({
      productId: item.productId,
      variantId: item.productId,
      name: item.name,
      slug: item.slug,
      price: item.price,
      imageUrl: item.imageUrl,
      size: "",
      color: "",
      stock: 999,
    });
    removeItem(item.productId);
    toast.success("Moved to cart");
  };

  if (items.length === 0) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center space-y-6 text-center">
          <Heart className="h-16 w-16 text-muted-foreground" />
          <h1 className="text-3xl font-bold">Your wishlist is empty</h1>
          <p className="text-muted-foreground">Save items you love to your wishlist.</p>
          <Link href="/shop" className={buttonVariants({ size: "lg" })}>
            Explore Products
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold">My Wishlist</h1>
        <p className="mt-2 text-muted-foreground">{items.length} item{items.length !== 1 ? "s" : ""} saved</p>
      </div>

      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {items.map(item => (
          <Card key={item.productId} className="group overflow-hidden">
            <div className="relative aspect-[3/4] overflow-hidden bg-muted">
              <Image
                src={getSafeImageUrl(item.imageUrl)}
                alt={item.name}
                fill
                className="object-cover transition-transform duration-300 group-hover:scale-105"
                sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 25vw"
              />
              <Button
                variant="ghost"
                size="icon"
                className="absolute right-2 top-2 h-8 w-8 bg-background/80 backdrop-blur-sm text-destructive hover:text-destructive"
                onClick={() => {
                  removeItem(item.productId);
                  toast.success("Removed from wishlist");
                }}
                aria-label="Remove from wishlist"
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>

            <CardContent className="space-y-3 p-4">
              <div>
                <Link href={`/shop/${item.slug}`} className="font-medium hover:underline line-clamp-1">
                  {item.name}
                </Link>
                {item.brand && (
                  <p className="text-xs text-muted-foreground">{item.brand}</p>
                )}
              </div>
              <p className="font-semibold">{formatPrice(item.price)}</p>
              <Button
                size="sm"
                className="w-full gap-2"
                onClick={() => handleMoveToCart(item)}
              >
                <ShoppingBag className="h-4 w-4" />
                Move to Cart
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="mt-8">
        <Link href="/shop" className={buttonVariants({ variant: "ghost" })}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Continue Shopping
        </Link>
      </div>
    </div>
  );
}
