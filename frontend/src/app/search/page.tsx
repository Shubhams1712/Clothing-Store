"use client";

import { Suspense } from "react";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "next/navigation";
import { storefrontService } from "@/services/storefront";
import { ProductGrid } from "@/components/storefront/product-grid";
import { Pagination } from "@/components/storefront/pagination";
import { SearchBar } from "@/components/storefront/search-bar";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";

function SearchContent() {
  const searchParams = useSearchParams();
  const query = searchParams.get("q") || "";
  const page = Number(searchParams.get("page")) || 1;

  const { data: products, isLoading } = useQuery({
    queryKey: ["search", query, page],
    queryFn: () =>
      storefrontService.getProducts({
        search: query,
        page,
        pageSize: 20,
      }),
    enabled: !!query,
  });

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Search</h1>
          {query && (
            <p className="mt-2 text-muted-foreground">
              {products?.totalCount ?? 0} results for &ldquo;{query}&rdquo;
            </p>
          )}
        </div>

        <SearchBar defaultValue={query} placeholder="Search products..." />

        {!query ? (
          <div className="py-16 text-center">
            <p className="text-muted-foreground">Enter a search term to find products.</p>
          </div>
        ) : isLoading ? (
          <LoadingOverlay text="Searching..." />
        ) : (
          <>
            <ProductGrid
              products={products?.items || []}
              emptyMessage={`No products found for "${query}"`}
            />
            {products && products.totalPages > 1 && (
              <div className="mt-8">
                <Pagination currentPage={page} totalPages={products.totalPages} basePath="/search" />
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

export default function SearchPage() {
  return (
    <Suspense fallback={<LoadingOverlay text="Loading..." />}>
      <SearchContent />
    </Suspense>
  );
}
