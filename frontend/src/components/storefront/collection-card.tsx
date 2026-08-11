import Link from "next/link";
import Image from "next/image";
import { Card } from "@/components/ui/card";
import { StorefrontCollection } from "@/types/storefront";
import { getSafeImageUrl } from "@/lib/utils";

interface CollectionCardProps {
  collection: StorefrontCollection;
}

export function CollectionCard({ collection }: CollectionCardProps) {
  const imageUrl = getSafeImageUrl(collection.imageUrl);

  return (
    <Link href={`/collections/${collection.slug}`}>
      <Card className="group overflow-hidden border-0 bg-transparent shadow-none transition-all hover:shadow-md">
        <div className="relative aspect-[4/5] overflow-hidden rounded-lg bg-muted">
          <Image
            src={imageUrl}
            alt={collection.name}
            fill
            className="object-cover transition-transform duration-500 group-hover:scale-105"
            sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
          />

          <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/20 to-transparent" />

          <div className="absolute right-0 bottom-0 left-0 p-4">
            <h3 className="text-lg font-semibold text-white">{collection.name}</h3>
            <p className="mt-1 line-clamp-2 text-sm text-white/80">
              {collection.description || `${collection.productCount} products`}
            </p>
          </div>
        </div>
      </Card>
    </Link>
  );
}
