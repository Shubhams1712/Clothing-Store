import Link from "next/link";
import Image from "next/image";
import { Card, CardContent } from "@/components/ui/card";
import { StorefrontCategory } from "@/types/storefront";
import { getSafeImageUrl } from "@/lib/utils";

interface CategoryCardProps {
  category: StorefrontCategory;
}

export function CategoryCard({ category }: CategoryCardProps) {
  const imageUrl = getSafeImageUrl(category.imageUrl);

  return (
    <Link href={`/categories/${category.slug}`}>
      <Card className="group overflow-hidden border-0 bg-transparent shadow-none transition-all hover:shadow-md">
        <div className="relative aspect-square overflow-hidden rounded-full bg-muted">
          <Image
            src={imageUrl}
            alt={category.name}
            fill
            className="object-cover transition-transform duration-500 group-hover:scale-110"
            sizes="(max-width: 768px) 50vw, 25vw"
          />

          <div className="absolute inset-0 bg-black/0 transition-colors group-hover:bg-black/20" />
        </div>

        <CardContent className="pt-4 text-center">
          <h3 className="font-medium">{category.name}</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            {category.productCount} {category.productCount === 1 ? "product" : "products"}
          </p>
        </CardContent>
      </Card>
    </Link>
  );
}
