"use client";

import { Suspense, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams, useRouter } from "next/navigation";
import { SlidersHorizontal } from "lucide-react";
import { storefrontService } from "@/services/storefront";
import { ProductGrid } from "@/components/storefront/product-grid";
import { ProductFilters } from "@/components/storefront/product-filters";
import { Pagination } from "@/components/storefront/pagination";
import { SearchBar } from "@/components/storefront/search-bar";
import { ProductGridSkeleton } from "@/components/storefront/product-grid-skeleton";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetHeader, SheetTitle } from "@/components/ui/sheet";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

function ShopContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const [filterOpen, setFilterOpen] = useState(false);

  const page = Number(searchParams.get("page")) || 1;
  const search = searchParams.get("q") || undefined;
  const sortBy = searchParams.get("sortBy") || undefined;
  const categorySlug = searchParams.get("category") || undefined;
  const size = searchParams.get("size") || undefined;
  const color = searchParams.get("color") || undefined;
  const minPrice = searchParams.get("minPrice") || undefined;
  const maxPrice = searchParams.get("maxPrice") || undefined;

  const { data: products, isLoading } = useQuery({
    queryKey: ["products", page, search, sortBy, categorySlug, size, color, minPrice, maxPrice],
    queryFn: () =>
      storefrontService.getProducts({
        page,
        pageSize: 20,
        search,
        sortBy,
        categorySlug,
        size,
        color,
        minPrice: minPrice ? Number(minPrice) : undefined,
        maxPrice: maxPrice ? Number(maxPrice) : undefined,
      }),
  });

  const { data: categories = [] } = useQuery({
    queryKey: ["categories-for-filters"],
    queryFn: () => storefrontService.getCategories(),
  });

  const { data: sizes = [] } = useQuery({
    queryKey: ["sizes"],
    queryFn: () => storefrontService.getAvailableSizes(),
  });

  const { data: colors = [] } = useQuery({
    queryKey: ["colors"],
    queryFn: () => storefrontService.getAvailableColors(),
  });

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Shop</h1>
          <p className="mt-2 text-muted-foreground">
            {products?.totalCount ?? 0} products
          </p>
        </div>

        <SearchBar placeholder="Search products..." />

        <div className="flex flex-col gap-6 lg:flex-row">
          {/* Desktop Sidebar Filters */}
          <aside className="hidden w-full shrink-0 lg:block lg:w-64">
            <ProductFilters
              categories={categories}
              sizes={sizes}
              colors={colors}
              selectedCategory={categorySlug}
              selectedSize={size}
              selectedColor={color}
              minPrice={minPrice}
              maxPrice={maxPrice}
            />
          </aside>

          {/* Mobile Filter Sheet */}
          <Sheet open={filterOpen} onOpenChange={setFilterOpen}>
            <SheetContent side="left" className="w-72">
              <SheetHeader>
                <SheetTitle>Filters</SheetTitle>
              </SheetHeader>
              <div className="mt-4">
                <ProductFilters
                  categories={categories}
                  sizes={sizes}
                  colors={colors}
                  selectedCategory={categorySlug}
                  selectedSize={size}
                  selectedColor={color}
                  minPrice={minPrice}
                  maxPrice={maxPrice}
                />
              </div>
            </SheetContent>
          </Sheet>

          {/* Products */}
          <main className="flex-1">
            <div className="mb-4 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <Button
                  variant="outline"
                  size="sm"
                  className="lg:hidden"
                  onClick={() => setFilterOpen(true)}
                >
                  <SlidersHorizontal className="mr-2 h-4 w-4" />
                  Filters
                </Button>
                <p className="text-sm text-muted-foreground">
                  Showing {products ? (page - 1) * 20 + 1 : 0}-
                  {products ? Math.min(page * 20, products.totalCount) : 0} of{" "}
                  {products?.totalCount ?? 0}
                </p>
              </div>
              <Select
                defaultValue={sortBy || "newest"}
                onValueChange={(value) => {
                  if (!value) return;
                  const params = new URLSearchParams(searchParams.toString());
                  if (value === "newest") {
                    params.delete("sortBy");
                  } else {
                    params.set("sortBy", value);
                  }
                  params.delete("page");
                  router.push(`/shop?${params.toString()}`);
                }}
              >
                <SelectTrigger className="w-[160px]">
                  <SelectValue placeholder="Sort by" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="newest">Newest</SelectItem>
                  <SelectItem value="price_asc">Price: Low to High</SelectItem>
                  <SelectItem value="price_desc">Price: High to Low</SelectItem>
                  <SelectItem value="name_asc">Name: A-Z</SelectItem>
                  <SelectItem value="name_desc">Name: Z-A</SelectItem>
                  <SelectItem value="best_sellers">Best Sellers</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {isLoading ? (
              <ProductGridSkeleton count={8} />
            ) : (
              <ProductGrid products={products?.items || []} emptyMessage="No products found" />
            )}

            {products && products.totalPages > 1 && (
              <div className="mt-8">
                <Pagination currentPage={page} totalPages={products.totalPages} basePath="/shop" />
              </div>
            )}
          </main>
        </div>
      </div>
    </div>
  );
}

export default function ShopPage() {
  return (
    <Suspense fallback={<div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8"><ProductGridSkeleton count={8} /></div>}>
      <ShopContent />
    </Suspense>
  );
}
