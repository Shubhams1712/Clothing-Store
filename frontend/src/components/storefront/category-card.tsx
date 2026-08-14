import Link from "next/link";
import Image from "next/image";
import { StorefrontCategory } from "@/types/storefront";
import { getSafeImageUrl } from "@/lib/utils";

interface CategoryCardProps {
  category: StorefrontCategory;
}

export function CategoryCard({ category }: CategoryCardProps) {
  const imageUrl = getSafeImageUrl(category.imageUrl);

  return (
    <Link href={`/categories/${category.slug}`}>
      <div className="group overflow-hidden">
        <div className="relative aspect-square overflow-hidden bg-neutral-100">
          <Image
            src={imageUrl}
            alt={category.name}
            fill
            className="object-cover transition-transform duration-500 group-hover:scale-110"
            sizes="(max-width: 768px) 50vw, 25vw"
          />

          <div className="absolute inset-0 bg-black/0 transition-colors group-hover:bg-black/20" />
        </div>

        <div className="pt-4 text-center">
          <h3 className="text-sm font-bold uppercase tracking-wider">{category.name}</h3>
          <p className="mt-1 text-xs text-neutral-400">
            {category.productCount} {category.productCount === 1 ? "product" : "products"}
          </p>
        </div>
      </div>
    </Link>
  );
}
