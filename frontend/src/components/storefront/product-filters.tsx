"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useCallback } from "react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import { StorefrontCategory } from "@/types/storefront";

interface ProductFiltersProps {
  categories: StorefrontCategory[];
  sizes: string[];
  colors: string[];
  selectedCategory?: string;
  selectedSize?: string;
  selectedColor?: string;
  minPrice?: string;
  maxPrice?: string;
}

export function ProductFilters({
  categories,
  sizes,
  colors,
  selectedCategory,
  selectedSize,
  selectedColor,
  minPrice,
  maxPrice,
}: ProductFiltersProps) {
  const router = useRouter();
  const searchParams = useSearchParams();

  const createFilterURL = useCallback(
    (key: string, value: string | null) => {
      const params = new URLSearchParams(searchParams.toString());
      if (value === null) {
        params.delete(key);
      } else {
        params.set(key, value);
      }
      params.delete("page");
      return `/shop?${params.toString()}`;
    },
    [searchParams]
  );

  const hasActiveFilters = selectedCategory || selectedSize || selectedColor || minPrice || maxPrice;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-bold uppercase tracking-wider">Filters</h3>
        {hasActiveFilters && (
          <button
            type="button"
            onClick={() => router.push("/shop")}
            className="text-[11px] font-semibold uppercase tracking-wider text-neutral-400 hover:text-black"
          >
            Clear all
          </button>
        )}
      </div>

      <Accordion defaultValue={["category", "size", "color", "price"]}>
        <AccordionItem value="category">
          <AccordionTrigger className="text-xs font-semibold uppercase tracking-wider">Category</AccordionTrigger>
          <AccordionContent className="space-y-1">
            <Button
              variant={!selectedCategory ? "secondary" : "ghost"}
              size="sm"
              className="w-full justify-start"
              onClick={() => router.push(createFilterURL("category", null))}
            >
              All Categories
            </Button>
            {categories.map((cat) => (
              <Button
                key={cat.id}
                variant={selectedCategory === cat.slug ? "secondary" : "ghost"}
                size="sm"
                className="w-full justify-between"
                onClick={() => router.push(createFilterURL("category", cat.slug))}
              >
                <span className="truncate">{cat.name}</span>
                <span className="text-xs text-neutral-400">{cat.productCount}</span>
              </Button>
            ))}
          </AccordionContent>
        </AccordionItem>

        <AccordionItem value="size">
          <AccordionTrigger className="text-xs font-semibold uppercase tracking-wider">Size</AccordionTrigger>
          <AccordionContent>
            <div className="flex flex-wrap gap-2">
              {sizes.map((size) => (
                <Button
                  key={size}
                  variant={selectedSize === size ? "default" : "outline"}
                  size="sm"
                  className={selectedSize === size ? "bg-black text-white hover:bg-neutral-800" : "border-black/10"}
                  onClick={() =>
                    router.push(createFilterURL("size", selectedSize === size ? null : size))
                  }
                >
                  {size}
                </Button>
              ))}
            </div>
          </AccordionContent>
        </AccordionItem>

        <AccordionItem value="color">
          <AccordionTrigger className="text-xs font-semibold uppercase tracking-wider">Color</AccordionTrigger>
          <AccordionContent>
            <div className="flex flex-wrap gap-2">
              {colors.map((color) => (
                <Button
                  key={color}
                  variant={selectedColor === color ? "default" : "outline"}
                  size="sm"
                  className={`gap-2 ${selectedColor === color ? "bg-black text-white hover:bg-neutral-800" : "border-black/10"}`}
                  onClick={() =>
                    router.push(createFilterURL("color", selectedColor === color ? null : color))
                  }
                >
                  <div
                    className="h-3 w-3 rounded-full border border-neutral-200"
                    style={{ backgroundColor: color.toLowerCase() }}
                  />
                  {color}
                </Button>
              ))}
            </div>
          </AccordionContent>
        </AccordionItem>

        <AccordionItem value="price">
          <AccordionTrigger className="text-xs font-semibold uppercase tracking-wider">Price Range</AccordionTrigger>
          <AccordionContent className="space-y-2">
            <div className="flex gap-2">
              <Input
                type="number"
                placeholder="Min"
                defaultValue={minPrice}
                className="h-8 border-black/10"
                onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                  if (e.key === "Enter") {
                    const value = (e.target as HTMLInputElement).value;
                    router.push(createFilterURL("minPrice", value || null));
                  }
                }}
              />
              <Input
                type="number"
                placeholder="Max"
                defaultValue={maxPrice}
                className="h-8 border-black/10"
                onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                  if (e.key === "Enter") {
                    const value = (e.target as HTMLInputElement).value;
                    router.push(createFilterURL("maxPrice", value || null));
                  }
                }}
              />
            </div>
          </AccordionContent>
        </AccordionItem>
      </Accordion>

      {hasActiveFilters && (
        <div className="flex flex-wrap gap-2 pt-2">
          {selectedCategory && (
            <Badge variant="secondary" className="gap-1 bg-neutral-100">
              {categories.find((c) => c.slug === selectedCategory)?.name}
              <button
                type="button"
                onClick={() => router.push(createFilterURL("category", null))}
                className="ml-1 text-neutral-400 hover:text-black"
              >
                ×
              </button>
            </Badge>
          )}
          {selectedSize && (
            <Badge variant="secondary" className="gap-1 bg-neutral-100">
              Size: {selectedSize}
              <button
                type="button"
                onClick={() => router.push(createFilterURL("size", null))}
                className="ml-1 text-neutral-400 hover:text-black"
              >
                ×
              </button>
            </Badge>
          )}
          {selectedColor && (
            <Badge variant="secondary" className="gap-1 bg-neutral-100">
              Color: {selectedColor}
              <button
                type="button"
                onClick={() => router.push(createFilterURL("color", null))}
                className="ml-1 text-neutral-400 hover:text-black"
              >
                ×
              </button>
            </Badge>
          )}
        </div>
      )}
    </div>
  );
}
