import { Store } from "lucide-react";
import Link from "next/link";

export function NavbarShell() {
  return (
    <header className="sticky top-0 z-50 border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <Link href="/" className="flex items-center gap-2">
          <Store className="h-6 w-6" />
          <span className="text-lg font-semibold tracking-tight">Store</span>
        </Link>
        <nav className="hidden items-center gap-6 text-sm font-medium md:flex">
          <Link href="/shop" className="transition-colors hover:text-foreground/80">
            Shop
          </Link>
          <Link href="/collections" className="transition-colors hover:text-foreground/80">
            Collections
          </Link>
        </nav>
      </div>
    </header>
  );
}
