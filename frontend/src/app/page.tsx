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

  return (
    <div className="flex flex-col">
      {/* Hero Section */}
      <section className="relative flex min-h-[85vh] items-center bg-black text-white">
        <div className="absolute inset-0 bg-gradient-to-br from-black via-neutral-900 to-black" />
        <div className="relative mx-auto w-full max-w-7xl px-4 py-20 sm:px-6 lg:px-8">
          <div className="max-w-2xl space-y-6">
            <p className="text-[11px] font-semibold uppercase tracking-[0.2em] text-white/50">
              New Season 2026
            </p>
            <h1 className="text-5xl font-bold uppercase tracking-tight sm:text-6xl lg:text-7xl">
              Wear Your
              <br />
              <span className="text-[#E10600]">Attitude</span>
            </h1>
            <p className="max-w-md text-base text-white/60">
              Premium streetwear for the bold. Minimal designs, maximum impact.
            </p>
            <div className="flex flex-wrap gap-4 pt-2">
              <Link
                href="/shop"
                className="inline-flex h-12 items-center gap-2 bg-white px-6 text-sm font-bold uppercase tracking-wider text-black transition-colors hover:bg-neutral-200"
              >
                Shop Now <ArrowRight className="h-4 w-4" />
              </Link>
              <Link
                href="/collections"
                className="inline-flex h-12 items-center gap-2 border border-white/30 px-6 text-sm font-bold uppercase tracking-wider text-white transition-colors hover:bg-white/10"
              >
                Collections
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Trust Badges */}
      <section className="border-y border-black/10 bg-white">
        <div className="mx-auto grid max-w-7xl grid-cols-2 gap-4 px-4 py-5 sm:grid-cols-4 sm:px-6 lg:px-8">
          {[
            { icon: Truck, label: "Free Shipping", desc: "On orders over ₹1,000" },
            { icon: Shield, label: "Secure Payment", desc: "100% protected" },
            { icon: RotateCcw, label: "Easy Returns", desc: "30-day policy" },
            { icon: Headphones, label: "24/7 Support", desc: "Always here to help" },
          ].map(({ icon: Icon, label, desc }) => (
            <div key={label} className="flex items-center gap-3">
              <Icon className="h-5 w-5 text-neutral-400" strokeWidth={1.5} />
              <div>
                <p className="text-xs font-semibold uppercase tracking-wider">{label}</p>
                <p className="text-xs text-neutral-500">{desc}</p>
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
              <Link
                href="/categories"
                className="inline-flex h-10 items-center border border-black/20 px-5 text-xs font-semibold uppercase tracking-wider transition-colors hover:bg-black hover:text-white"
              >
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
            <Link
              href="/collections"
              className="inline-flex h-10 items-center border border-black/20 px-5 text-xs font-semibold uppercase tracking-wider transition-colors hover:bg-black hover:text-white"
            >
              View All Collections
            </Link>
          </div>
        </SectionWrapper>
      )}

      {/* Featured Products */}
      <SectionWrapper title="Featured Products" description="Hand-picked just for you">
        {loadingProducts ? (
          <ProductGridSkeleton count={8} />
        ) : featuredProducts?.items && featuredProducts.items.length > 0 ? (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {featuredProducts.items.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        ) : (
          <div className="py-12 text-center">
            <p className="text-neutral-500">No featured products yet. Check back soon!</p>
          </div>
        )}
        <div className="mt-8 text-center">
          <Link
            href="/shop"
            className="inline-flex h-10 items-center border border-black/20 px-5 text-xs font-semibold uppercase tracking-wider transition-colors hover:bg-black hover:text-white"
          >
            Browse All Products
          </Link>
        </div>
      </SectionWrapper>

      {/* Brand Statement */}
      <section className="bg-black py-20 text-white">
        <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
          <p className="text-[11px] font-semibold uppercase tracking-[0.2em] text-white/40">
            Our Philosophy
          </p>
          <h2 className="mt-4 text-3xl font-bold uppercase tracking-tight sm:text-4xl lg:text-5xl">
            Not For Everyone
          </h2>
          <p className="mx-auto mt-6 max-w-xl text-sm text-white/50">
            The Freak Store is for those who refuse to blend in. We create premium streetwear
            that speaks louder than words. Every piece is designed with intention, built with
            quality, and worn with confidence.
          </p>
          <Link
            href="/shop"
            className="mt-8 inline-flex h-12 items-center gap-2 bg-[#E10600] px-6 text-sm font-bold uppercase tracking-wider text-white transition-colors hover:bg-[#c40500]"
          >
            Shop the Drop
          </Link>
        </div>
      </section>

      {/* Newsletter CTA */}
      <section className="border-t border-black/10 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-16 text-center sm:px-6 lg:px-8">
          <h2 className="text-2xl font-bold uppercase tracking-tight sm:text-3xl">Stay in the Loop</h2>
          <p className="mx-auto mt-3 max-w-md text-sm text-neutral-500">
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
              className="h-12 flex-1 border border-black/20 bg-white px-4 text-sm transition-colors focus:border-black focus:outline-none"
            />
            <button
              type="button"
              className="h-12 bg-black px-6 text-xs font-bold uppercase tracking-wider text-white transition-colors hover:bg-neutral-800"
            >
              Subscribe
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}
