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
        <h3 className="font-semibold">Filters</h3>
        {hasActiveFilters && (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.push("/shop")}
            className="h-auto p-0 text-xs"
          >
            Clear all
          </Button>
        )}
      </div>

      <Accordion defaultValue={["category", "size", "color", "price"]}>
        <AccordionItem value="category">
          <AccordionTrigger className="text-sm">Category</AccordionTrigger>
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
                <span className="text-xs text-muted-foreground">{cat.productCount}</span>
              </Button>
            ))}
          </AccordionContent>
        </AccordionItem>

        <AccordionItem value="size">
          <AccordionTrigger className="text-sm">Size</AccordionTrigger>
          <AccordionContent>
            <div className="flex flex-wrap gap-2">
              {sizes.map((size) => (
                <Button
                  key={size}
                  variant={selectedSize === size ? "default" : "outline"}
                  size="sm"
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
          <AccordionTrigger className="text-sm">Color</AccordionTrigger>
          <AccordionContent>
            <div className="flex flex-wrap gap-2">
              {colors.map((color) => (
                <Button
                  key={color}
                  variant={selectedColor === color ? "default" : "outline"}
                  size="sm"
                  className="gap-2"
                  onClick={() =>
                    router.push(createFilterURL("color", selectedColor === color ? null : color))
                  }
                >
                  <div
                    className="h-3 w-3 rounded-full border border-border"
                    style={{ backgroundColor: color.toLowerCase() }}
                  />
                  {color}
                </Button>
              ))}
            </div>
          </AccordionContent>
        </AccordionItem>

        <AccordionItem value="price">
          <AccordionTrigger className="text-sm">Price Range</AccordionTrigger>
          <AccordionContent className="space-y-2">
            <div className="flex gap-2">
              <Input
                type="number"
                placeholder="Min"
                defaultValue={minPrice}
                className="h-8"
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
                className="h-8"
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
            <Badge variant="secondary" className="gap-1">
              {categories.find((c) => c.slug === selectedCategory)?.name}
              <Button
                variant="ghost"
                size="sm"
                className="ml-1 h-auto p-0"
                onClick={() => router.push(createFilterURL("category", null))}
              >
                ×
              </Button>
            </Badge>
          )}
          {selectedSize && (
            <Badge variant="secondary" className="gap-1">
              Size: {selectedSize}
              <Button
                variant="ghost"
                size="sm"
                className="ml-1 h-auto p-0"
                onClick={() => router.push(createFilterURL("size", null))}
              >
                ×
              </Button>
            </Badge>
          )}
          {selectedColor && (
            <Badge variant="secondary" className="gap-1">
              Color: {selectedColor}
              <Button
                variant="ghost"
                size="sm"
                className="ml-1 h-auto p-0"
                onClick={() => router.push(createFilterURL("color", null))}
              >
                ×
              </Button>
            </Badge>
          )}
        </div>
      )}
    </div>
  );
}
