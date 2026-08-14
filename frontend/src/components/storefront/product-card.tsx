"use client";

import { memo } from "react";
import Link from "next/link";
import Image from "next/image";
import { Heart } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { StorefrontProduct } from "@/types/storefront";
import { getSafeImageUrl, formatPrice } from "@/lib/utils";

interface ProductCardProps {
  product: StorefrontProduct;
}

export const ProductCard = memo(function ProductCard({ product }: ProductCardProps) {
  const hasDiscount = product.comparePrice && product.comparePrice > product.price;
  const discountPercent = hasDiscount
    ? Math.round(((product.comparePrice! - product.price) / product.comparePrice!) * 100)
    : 0;

  const imageUrl = getSafeImageUrl(product.primaryImageUrl);

  return (
    <Link href={`/shop/${product.slug}`} className="group block">
      <div className="overflow-hidden bg-neutral-100">
        <div className="relative aspect-[3/4] overflow-hidden bg-neutral-100">
          <Image
            src={imageUrl}
            alt={product.name}
            fill
            className="object-cover transition-transform duration-500 group-hover:scale-105"
            sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 25vw"
          />

          <div className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent opacity-0 transition-opacity group-hover:opacity-100" />

          {hasDiscount && (
            <Badge className="absolute left-2 top-2 bg-[#E10600] text-white border-0 text-[10px] font-bold uppercase tracking-wider">
              -{discountPercent}%
            </Badge>
          )}

          {product.isFeatured && (
            <Badge className="absolute right-2 top-2 bg-black text-white border-0 text-[10px] font-bold uppercase tracking-wider">
              Featured
            </Badge>
          )}

          <button
            type="button"
            className="absolute right-2 bottom-2 flex h-8 w-8 items-center justify-center rounded-full bg-white/90 opacity-0 backdrop-blur transition-opacity group-hover:opacity-100"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
          >
            <Heart className="h-4 w-4" />
          </button>
        </div>

        <div className="p-3">
          <div className="flex items-center justify-between">
            <p className="text-[11px] font-medium uppercase tracking-wider text-neutral-400">
              {product.brand || product.categoryName}
            </p>
            {product.colors.length > 0 && (
              <div className="flex gap-1">
                {product.colors.slice(0, 3).map((color) => (
                  <div
                    key={color}
                    className="h-2 w-2 rounded-full border border-neutral-200"
                    style={{ backgroundColor: color.toLowerCase() }}
                  />
                ))}
              </div>
            )}
          </div>
          <h3 className="mt-1 line-clamp-1 text-sm font-semibold">{product.name}</h3>
          <div className="mt-1 flex items-center gap-2">
            <span className="text-sm font-bold">{formatPrice(product.price)}</span>
            {hasDiscount && (
              <span className="text-xs text-neutral-400 line-through">
                {formatPrice(product.comparePrice!)}
              </span>
            )}
          </div>
          {!product.isInStock && (
            <p className="mt-2 text-[10px] font-semibold uppercase tracking-wider text-neutral-400">
              Out of Stock
            </p>
          )}
        </div>
      </div>
    </Link>
  );
});
