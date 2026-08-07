"use client";

import { useQuery } from "@tanstack/react-query";
import { storefrontService } from "@/services/storefront";
import { CategoryCard } from "@/components/storefront/category-card";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { SectionWrapper } from "@/components/layout/section-wrapper";

export default function CategoriesPage() {
  const { data: categories, isLoading } = useQuery({
    queryKey: ["categories"],
    queryFn: () => storefrontService.getCategories(),
  });

  if (isLoading) return <LoadingOverlay text="Loading categories..." />;

  return (
    <SectionWrapper
      title="Categories"
      description="Browse our product categories"
      className="py-8"
    >
      {categories && categories.length > 0 ? (
        <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
          {categories.map((category) => (
            <CategoryCard key={category.id} category={category} />
          ))}
        </div>
      ) : (
        <div className="py-16 text-center">
          <p className="text-muted-foreground">No categories available yet.</p>
        </div>
      )}
    </SectionWrapper>
  );
}
