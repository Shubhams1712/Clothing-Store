"use client";

import { Store, LogOut, User, ShoppingBag, Search } from "lucide-react";
import Link from "next/link";
import { useAuth } from "@/hooks/use-auth";
import { buttonVariants } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

export function NavbarShell() {
  const { user, isAuthenticated, logout } = useAuth();

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
          <Link href="/new-arrivals" className="transition-colors hover:text-foreground/80">
            New Arrivals
          </Link>
          <Link href="/best-sellers" className="transition-colors hover:text-foreground/80">
            Best Sellers
          </Link>
        </nav>
        <div className="flex items-center gap-2">
          <Link
            href="/search"
            className={buttonVariants({ variant: "ghost", size: "icon" })}
          >
            <Search className="h-5 w-5" />
            <span className="sr-only">Search</span>
          </Link>
          <button
            type="button"
            className={buttonVariants({ variant: "ghost", size: "icon" })}
          >
            <ShoppingBag className="h-5 w-5" />
            <span className="sr-only">Cart</span>
          </button>
          {isAuthenticated ? (
            <DropdownMenu>
              <DropdownMenuTrigger
                className={buttonVariants({ variant: "ghost", size: "icon" })}
              >
                <User className="h-5 w-5" />
                <span className="sr-only">Account menu</span>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <div className="px-2 py-1.5 text-sm font-medium">
                  {user?.firstName} {user?.lastName}
                </div>
                <div className="px-2 pb-1.5 text-xs text-muted-foreground">
                  {user?.email}
                </div>
                <DropdownMenuSeparator />
                <DropdownMenuItem>Profile</DropdownMenuItem>
                {user?.isAdmin && (
                  <DropdownMenuItem render={<Link href="/admin" />}>
                    Admin Panel
                  </DropdownMenuItem>
                )}
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => logout()}>
                  <LogOut className="mr-2 h-4 w-4" />
                  Sign out
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          ) : (
            <Link
              href="/login"
              className={buttonVariants({ variant: "ghost", size: "sm" })}
            >
              Sign in
            </Link>
          )}
        </div>
      </div>
    </header>
  );
}
