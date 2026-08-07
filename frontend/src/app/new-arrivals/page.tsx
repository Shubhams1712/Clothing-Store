"use client";

import { Suspense } from "react";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "next/navigation";
import { storefrontService } from "@/services/storefront";
import { ProductGrid } from "@/components/storefront/product-grid";
import { Pagination } from "@/components/storefront/pagination";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { SectionWrapper } from "@/components/layout/section-wrapper";

function NewArrivalsContent() {
  const searchParams = useSearchParams();
  const page = Number(searchParams.get("page")) || 1;

  const { data: products, isLoading } = useQuery({
    queryKey: ["new-arrivals", page],
    queryFn: () => storefrontService.getNewArrivals(page, 20),
  });

  return (
    <SectionWrapper
      title="New Arrivals"
      description="The latest additions to our collection"
      className="py-8"
    >
      {isLoading ? (
        <LoadingOverlay text="Loading new arrivals..." />
      ) : (
        <>
          <ProductGrid products={products?.items || []} emptyMessage="No new arrivals yet." />
          {products && products.totalPages > 1 && (
            <div className="mt-8">
              <Pagination currentPage={page} totalPages={products.totalPages} basePath="/new-arrivals" />
            </div>
          )}
        </>
      )}
    </SectionWrapper>
  );
}

export default function NewArrivalsPage() {
  return (
    <Suspense fallback={<LoadingOverlay text="Loading..." />}>
      <NewArrivalsContent />
    </Suspense>
  );
}
