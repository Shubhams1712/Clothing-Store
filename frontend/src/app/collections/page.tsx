"use client";

import { useQuery } from "@tanstack/react-query";
import { storefrontService } from "@/services/storefront";
import { CollectionCard } from "@/components/storefront/collection-card";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { SectionWrapper } from "@/components/layout/section-wrapper";

export default function CollectionsPage() {
  const { data: collections, isLoading } = useQuery({
    queryKey: ["collections"],
    queryFn: () => storefrontService.getCollections(),
  });

  if (isLoading) return <LoadingOverlay text="Loading collections..." />;

  return (
    <SectionWrapper
      title="Collections"
      description="Explore our curated collections for every style and occasion"
      className="py-8"
    >
      {collections && collections.length > 0 ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {collections.map((collection) => (
            <CollectionCard key={collection.id} collection={collection} />
          ))}
        </div>
      ) : (
        <div className="py-16 text-center">
          <p className="text-muted-foreground">No collections available yet.</p>
        </div>
      )}
    </SectionWrapper>
  );
}
