"use client";

import { useState } from "react";
import { Store, LogOut, User, ShoppingBag, Search, Package, Heart, Settings } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@/hooks/use-auth";
import { useCart } from "@/hooks/use-cart";
import { buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { CartDrawer } from "./cart-drawer";

const NAV_LINKS = [
  { href: "/", label: "Home" },
  { href: "/shop", label: "Shop" },
  { href: "/collections", label: "Collections" },
  { href: "/new-arrivals", label: "New Arrivals" },
  { href: "/best-sellers", label: "Best Sellers" },
];

export function NavbarShell() {
  const { user, isAuthenticated, logout } = useAuth();
  const { totalItems } = useCart();
  const pathname = usePathname();
  const [cartOpen, setCartOpen] = useState(false);

  const getInitials = () => {
    if (!user) return "U";
    return `${user.firstName?.[0] || ""}${user.lastName?.[0] || ""}`.toUpperCase() || "U";
  };

  return (
    <>
      <header className="sticky top-0 z-40 border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
          <Link href="/" className="flex items-center gap-2">
            <Store className="h-6 w-6" />
            <span className="text-lg font-semibold tracking-tight">Store</span>
          </Link>
          <nav className="hidden items-center gap-1 text-sm font-medium md:flex">
            {NAV_LINKS.map(({ href, label }) => {
              const isActive = href === "/" ? pathname === "/" : pathname.startsWith(href);
              return (
                <Link
                  key={href}
                  href={href}
                  className={`px-3 py-2 rounded-md transition-colors ${
                    isActive
                      ? "text-foreground bg-muted"
                      : "text-muted-foreground hover:text-foreground hover:bg-muted/50"
                  }`}
                >
                  {label}
                </Link>
              );
            })}
          </nav>
          <div className="flex items-center gap-1">
            <Link
              href="/search"
              className={buttonVariants({ variant: "ghost", size: "icon" })}
            >
              <Search className="h-5 w-5" />
              <span className="sr-only">Search</span>
            </Link>
            <button
              type="button"
              className={`${buttonVariants({ variant: "ghost", size: "icon" })} relative`}
              onClick={() => setCartOpen(true)}
            >
              <ShoppingBag className="h-5 w-5" />
              {totalItems > 0 && (
                <Badge className="absolute -top-1 -right-1 h-4 min-w-4 flex items-center justify-center rounded-full px-1 text-[10px]">
                  {totalItems > 99 ? "99+" : totalItems}
                </Badge>
              )}
              <span className="sr-only">Cart</span>
            </button>
            {isAuthenticated ? (
              <DropdownMenu>
                <DropdownMenuTrigger render={
                  <button type="button" className="flex items-center gap-2 px-2 py-1.5 rounded-md hover:bg-muted transition-colors" />
                }>
                  <Avatar className="h-8 w-8">
                    <AvatarFallback className="text-xs">{getInitials()}</AvatarFallback>
                  </Avatar>
                  <span className="hidden lg:block text-sm font-medium max-w-[100px] truncate">
                    {user?.firstName}
                  </span>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56">
                  <div className="flex items-center gap-3 px-3 py-2">
                    <Avatar className="h-10 w-10">
                      <AvatarFallback>{getInitials()}</AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium truncate">{user?.firstName} {user?.lastName}</p>
                      <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
                    </div>
                  </div>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem render={<Link href="/account" className="flex items-center gap-2 w-full" />}>
                    <User className="h-4 w-4" />
                    My Profile
                  </DropdownMenuItem>
                  <DropdownMenuItem render={<Link href="/orders" className="flex items-center gap-2 w-full" />}>
                    <Package className="h-4 w-4" />
                    My Orders
                  </DropdownMenuItem>
                  <DropdownMenuItem render={<Link href="/wishlist" className="flex items-center gap-2 w-full" />}>
                    <Heart className="h-4 w-4" />
                    Wishlist
                  </DropdownMenuItem>
                  <DropdownMenuItem render={<Link href="/account/settings" className="flex items-center gap-2 w-full" />}>
                    <Settings className="h-4 w-4" />
                    Settings
                  </DropdownMenuItem>
                  {user?.isAdmin && (
                    <>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem render={<Link href="/admin" className="flex items-center gap-2 w-full" />}>
                        <Store className="h-4 w-4" />
                        Admin Panel
                      </DropdownMenuItem>
                    </>
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
      <CartDrawer open={cartOpen} onOpenChange={setCartOpen} />
    </>
  );
}
