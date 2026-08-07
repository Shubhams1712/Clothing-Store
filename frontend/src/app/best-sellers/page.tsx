"use client";

import { Suspense } from "react";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "next/navigation";
import { storefrontService } from "@/services/storefront";
import { ProductGrid } from "@/components/storefront/product-grid";
import { Pagination } from "@/components/storefront/pagination";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { SectionWrapper } from "@/components/layout/section-wrapper";

function BestSellersContent() {
  const searchParams = useSearchParams();
  const page = Number(searchParams.get("page")) || 1;

  const { data: products, isLoading } = useQuery({
    queryKey: ["best-sellers", page],
    queryFn: () => storefrontService.getBestSellers(page, 20),
  });

  return (
    <SectionWrapper
      title="Best Sellers"
      description="Our most popular products"
      className="py-8"
    >
      {isLoading ? (
        <LoadingOverlay text="Loading best sellers..." />
      ) : (
        <>
          <ProductGrid products={products?.items || []} emptyMessage="No best sellers yet." />
          {products && products.totalPages > 1 && (
            <div className="mt-8">
              <Pagination currentPage={page} totalPages={products.totalPages} basePath="/best-sellers" />
            </div>
          )}
        </>
      )}
    </SectionWrapper>
  );
}

export default function BestSellersPage() {
  return (
    <Suspense fallback={<LoadingOverlay text="Loading..." />}>
      <BestSellersContent />
    </Suspense>
  );
}
