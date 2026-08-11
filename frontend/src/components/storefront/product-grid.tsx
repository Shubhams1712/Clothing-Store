import { PackageOpen } from "lucide-react";
import { StorefrontProduct } from "@/types/storefront";
import { ProductCard } from "./product-card";
import { Button } from "@/components/ui/button";
import Link from "next/link";

interface ProductGridProps {
  products: StorefrontProduct[];
  emptyMessage?: string;
  showShopLink?: boolean;
}

export function ProductGrid({ products, emptyMessage = "No products found", showShopLink = false }: ProductGridProps) {
  if (products.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <PackageOpen className="mb-4 h-12 w-12 text-muted-foreground" />
        <p className="text-lg font-medium">{emptyMessage}</p>
        <p className="mt-1 text-sm text-muted-foreground">Try adjusting your filters or search terms.</p>
        {showShopLink && (
          <Link href="/shop" className="mt-4">
            <Button variant="outline" size="sm">Browse All Products</Button>
          </Link>
        )}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-4">
      {products.map((product) => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
}
