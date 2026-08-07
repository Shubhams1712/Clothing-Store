"use client";

import { use, Suspense } from "react";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { ChevronRight } from "lucide-react";
import { storefrontService } from "@/services/storefront";
import { ProductGrid } from "@/components/storefront/product-grid";
import { Pagination } from "@/components/storefront/pagination";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { buttonVariants } from "@/components/ui/button";

function CollectionDetailContent({ slug }: { slug: string }) {
  const searchParams = useSearchParams();
  const page = Number(searchParams.get("page")) || 1;

  const { data: collection, isLoading: loadingCollection } = useQuery({
    queryKey: ["collection", slug],
    queryFn: () => storefrontService.getCollectionBySlug(slug),
  });

  const { data: products, isLoading: loadingProducts } = useQuery({
    queryKey: ["collection-products", slug, page],
    queryFn: () =>
      storefrontService.getProducts({
        collectionSlug: slug,
        page,
        pageSize: 20,
      }),
    enabled: !!slug,
  });

  const isLoading = loadingCollection || loadingProducts;

  if (isLoading) return <LoadingOverlay text="Loading collection..." />;

  if (!collection) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <div className="text-center">
          <h2 className="text-2xl font-bold">Collection not found</h2>
          <Link href="/collections" className={buttonVariants({ variant: "link", className: "mt-4" })}>
            Back to Collections
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <nav className="mb-6 flex items-center gap-2 text-sm text-muted-foreground">
        <Link href="/" className="hover:text-foreground">Home</Link>
        <ChevronRight className="h-3 w-3" />
        <Link href="/collections" className="hover:text-foreground">Collections</Link>
        <ChevronRight className="h-3 w-3" />
        <span className="text-foreground">{collection.name}</span>
      </nav>

      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">{collection.name}</h1>
        {collection.description && (
          <p className="mt-2 text-muted-foreground">{collection.description}</p>
        )}
        <p className="mt-1 text-sm text-muted-foreground">
          {collection.productCount} {collection.productCount === 1 ? "product" : "products"}
        </p>
      </div>

      <ProductGrid products={products?.items || []} emptyMessage="No products in this collection yet." />

      {products && products.totalPages > 1 && (
        <div className="mt-8">
          <Pagination currentPage={page} totalPages={products.totalPages} basePath={`/collections/${slug}`} />
        </div>
      )}
    </div>
  );
}

export default function CollectionDetailPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = use(params);
  return (
    <Suspense fallback={<LoadingOverlay text="Loading..." />}>
      <CollectionDetailContent slug={slug} />
    </Suspense>
  );
}
