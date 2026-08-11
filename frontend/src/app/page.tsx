"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Truck, Shield, RotateCcw, Headphones } from "lucide-react";
import { storefrontService } from "@/services/storefront";
import { ProductCard } from "@/components/storefront/product-card";
import { CollectionCard } from "@/components/storefront/collection-card";
import { CategoryCard } from "@/components/storefront/category-card";
import { buttonVariants } from "@/components/ui/button";
import { SectionWrapper } from "@/components/layout/section-wrapper";
import { ProductGridSkeleton } from "@/components/storefront/product-grid-skeleton";

export default function Home() {
  const { data: featuredProducts, isLoading: loadingProducts } = useQuery({
    queryKey: ["featured-products"],
    queryFn: () => storefrontService.getFeaturedProducts(1, 8),
  });

  const { data: collections, isLoading: loadingCollections } = useQuery({
    queryKey: ["featured-collections"],
    queryFn: () => storefrontService.getFeaturedCollections(),
  });

  const { data: categories, isLoading: loadingCategories } = useQuery({
    queryKey: ["categories"],
    queryFn: () => storefrontService.getCategories(),
  });

  const isLoading = loadingProducts || loadingCollections || loadingCategories;

  return (
    <div className="flex flex-col">
      {/* Hero Section */}
      <section className="relative flex min-h-[80vh] items-center bg-gradient-to-br from-muted/50 via-background to-muted/30">
        <div className="mx-auto w-full max-w-7xl px-4 py-20 sm:px-6 lg:px-8">
          <div className="max-w-2xl space-y-6">
            <p className="text-sm font-medium uppercase tracking-widest text-muted-foreground">
              New Season 2026
            </p>
            <h1 className="text-4xl font-bold tracking-tight sm:text-5xl lg:text-6xl">
              Elevate Your
              <br />
              <span className="text-primary">Everyday Style</span>
            </h1>
            <p className="text-lg text-muted-foreground">
              Discover premium clothing crafted with intention. Minimalist designs, exceptional quality,
              and timeless pieces that define your wardrobe.
            </p>
            <div className="flex flex-wrap gap-4">
              <Link href="/shop" className={buttonVariants({ size: "lg", className: "gap-2" })}>
                Shop Now <ArrowRight className="h-4 w-4" />
              </Link>
              <Link href="/collections" className={buttonVariants({ size: "lg", variant: "outline" })}>
                View Collections
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Trust Badges */}
      <section className="border-y bg-muted/30">
        <div className="mx-auto grid max-w-7xl grid-cols-2 gap-4 px-4 py-6 sm:grid-cols-4 sm:px-6 lg:px-8">
          {[
            { icon: Truck, label: "Free Shipping", desc: "On orders over ₹1,000" },
            { icon: Shield, label: "Secure Payment", desc: "100% protected" },
            { icon: RotateCcw, label: "Easy Returns", desc: "30-day policy" },
            { icon: Headphones, label: "24/7 Support", desc: "Always here to help" },
          ].map(({ icon: Icon, label, desc }) => (
            <div key={label} className="flex items-center gap-3">
              <Icon className="h-8 w-8 text-muted-foreground" strokeWidth={1.5} />
              <div>
                <p className="text-sm font-medium">{label}</p>
                <p className="text-xs text-muted-foreground">{desc}</p>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Categories */}
      {categories && categories.length > 0 && (
        <SectionWrapper title="Shop by Category" description="Find your perfect fit">
          <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
            {categories.slice(0, 4).map((category) => (
              <CategoryCard key={category.id} category={category} />
            ))}
          </div>
          {categories.length > 4 && (
            <div className="mt-8 text-center">
              <Link href="/categories" className={buttonVariants({ variant: "outline" })}>
                View All Categories
              </Link>
            </div>
          )}
        </SectionWrapper>
      )}

      {/* Featured Collections */}
      {collections && collections.length > 0 && (
        <SectionWrapper title="Featured Collections" description="Curated looks for every occasion">
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {collections.slice(0, 3).map((collection) => (
              <CollectionCard key={collection.id} collection={collection} />
            ))}
          </div>
          <div className="mt-8 text-center">
            <Link href="/collections" className={buttonVariants({ variant: "outline" })}>
              View All Collections
            </Link>
          </div>
        </SectionWrapper>
      )}

      {/* Featured Products */}
      <SectionWrapper title="Featured Products" description="Hand-picked just for you">
        {isLoading ? (
          <ProductGridSkeleton count={8} />
        ) : featuredProducts?.items && featuredProducts.items.length > 0 ? (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {featuredProducts.items.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        ) : (
          <div className="py-12 text-center">
            <p className="text-muted-foreground">No featured products yet. Check back soon!</p>
          </div>
        )}
        <div className="mt-8 text-center">
          <Link href="/shop" className={buttonVariants({ variant: "outline" })}>
            Browse All Products
          </Link>
        </div>
      </SectionWrapper>

      {/* Newsletter CTA */}
      <section className="bg-muted/30">
        <div className="mx-auto max-w-7xl px-4 py-16 text-center sm:px-6 lg:px-8">
          <h2 className="text-2xl font-bold sm:text-3xl">Stay in the Loop</h2>
          <p className="mx-auto mt-3 max-w-md text-muted-foreground">
            Subscribe for exclusive drops, styling tips, and early access to new arrivals.
          </p>
          <div className="mx-auto mt-6 flex max-w-sm gap-2">
            <label htmlFor="newsletter-email" className="sr-only">
              Email address
            </label>
            <input
              id="newsletter-email"
              type="email"
              placeholder="Enter your email"
              className="h-10 flex-1 rounded-md border bg-background px-4 text-sm"
            />
            <button type="button" className={buttonVariants()}>
              Subscribe
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}
