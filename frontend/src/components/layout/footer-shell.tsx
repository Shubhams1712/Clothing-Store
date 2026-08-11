import Link from "next/link";

export function FooterShell() {
  return (
    <footer className="border-t border-border bg-muted/40">
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-between gap-4 text-sm text-muted-foreground sm:flex-row">
          <p>&copy; {new Date().getFullYear()} Store. All rights reserved.</p>
          <nav className="flex gap-4" aria-label="Footer navigation">
            <Link href="/collections" className="hover:text-foreground transition-colors">
              Collections
            </Link>
            <Link href="/new-arrivals" className="hover:text-foreground transition-colors">
              New Arrivals
            </Link>
            <Link href="/best-sellers" className="hover:text-foreground transition-colors">
              Best Sellers
            </Link>
          </nav>
        </div>
      </div>
    </footer>
  );
}
