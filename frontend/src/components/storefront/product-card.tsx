"use client";

import Link from "next/link";
import Image from "next/image";
import { Heart } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { buttonVariants } from "@/components/ui/button";
import { StorefrontProduct } from "@/types/storefront";
import { getSafeImageUrl } from "@/lib/utils";

interface ProductCardProps {
  product: StorefrontProduct;
}

export function ProductCard({ product }: ProductCardProps) {
  const hasDiscount = product.comparePrice && product.comparePrice > product.price;
  const discountPercent = hasDiscount
    ? Math.round(((product.comparePrice! - product.price) / product.comparePrice!) * 100)
    : 0;

  const imageUrl = getSafeImageUrl(product.primaryImageUrl);

  return (
    <Link href={`/shop/${product.slug}`} className="group block">
      <Card className="overflow-hidden border-0 bg-transparent shadow-none transition-all group-hover:shadow-md">
        <div className="relative aspect-[3/4] overflow-hidden rounded-lg bg-muted">
          <Image
            src={imageUrl}
            alt={product.name}
            fill
            className="object-cover transition-transform duration-500 group-hover:scale-105"
            sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 25vw"
          />

          <div className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent opacity-0 transition-opacity group-hover:opacity-100" />

          {hasDiscount && (
            <Badge className="absolute left-2 top-2 bg-destructive text-destructive-foreground">
              -{discountPercent}%
            </Badge>
          )}

          {product.isFeatured && (
            <Badge className="absolute right-2 top-2 bg-primary text-primary-foreground">
              Featured
            </Badge>
          )}

          <span
            role="button"
            tabIndex={0}
            className={buttonVariants({
              variant: "ghost",
              size: "icon",
              className:
                "absolute right-2 bottom-2 h-8 w-8 rounded-full bg-background/80 opacity-0 transition-opacity group-hover:opacity-100",
            })}
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
            onKeyDown={(e) => {
              if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                e.stopPropagation();
              }
            }}
          >
            <Heart className="h-4 w-4" />
          </span>
        </div>

        <CardContent className="space-y-1 p-2">
          <div className="flex items-center justify-between">
            <p className="text-xs text-muted-foreground">{product.brand || product.categoryName}</p>
            {product.colors.length > 0 && (
              <div className="flex gap-1">
                {product.colors.slice(0, 3).map((color) => (
                  <div
                    key={color}
                    className="h-2.5 w-2.5 rounded-full border border-border"
                    style={{ backgroundColor: color.toLowerCase() }}
                  />
                ))}
              </div>
            )}
          </div>
          <h3 className="line-clamp-1 text-sm font-medium">{product.name}</h3>
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold">${product.price.toFixed(2)}</span>
            {hasDiscount && (
              <span className="text-xs text-muted-foreground line-through">
                ${product.comparePrice!.toFixed(2)}
              </span>
            )}
          </div>
          {!product.isInStock && (
            <Badge variant="outline" className="text-xs">
              Out of Stock
            </Badge>
          )}
        </CardContent>
      </Card>
    </Link>
  );
}
