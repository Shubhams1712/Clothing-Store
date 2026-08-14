"use client";

import { useState } from "react";
import { Store, LogOut, User, ShoppingBag, Search, Package, Heart, Settings, Menu, X } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@/hooks/use-auth";
import { useCart } from "@/hooks/use-cart";
import { useStoreSettings } from "@/hooks/use-store-settings";
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
  { href: "/shop", label: "Shop" },
  { href: "/collections", label: "Collections" },
  { href: "/new-arrivals", label: "New Arrivals" },
  { href: "/best-sellers", label: "Best Sellers" },
];

export function NavbarShell() {
  const { user, isAuthenticated, logout } = useAuth();
  const { totalItems } = useCart();
  const { storeName } = useStoreSettings();
  const pathname = usePathname();
  const [cartOpen, setCartOpen] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const getInitials = () => {
    if (!user) return "U";
    return `${user.firstName?.[0] || ""}${user.lastName?.[0] || ""}`.toUpperCase() || "U";
  };

  return (
    <>
      <header className="sticky top-0 z-40 border-b border-black/10 bg-white">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
          {/* Logo */}
          <Link href="/" className="flex items-center gap-2">
            <span className="text-lg font-bold tracking-tight uppercase">{storeName}</span>
          </Link>

          {/* Desktop Navigation */}
          <nav className="hidden items-center gap-1 text-[11px] font-semibold uppercase tracking-widest md:flex">
            {NAV_LINKS.map(({ href, label }) => {
              const isActive = pathname.startsWith(href);
              return (
                <Link
                  key={href}
                  href={href}
                  className={`relative px-4 py-2 transition-colors ${
                    isActive
                      ? "text-black"
                      : "text-neutral-500 hover:text-black"
                  }`}
                >
                  {label}
                  {isActive && (
                    <span className="absolute bottom-0 left-4 right-4 h-0.5 bg-black" />
                  )}
                </Link>
              );
            })}
          </nav>

          {/* Right Actions */}
          <div className="flex items-center gap-1">
            <Link
              href="/search"
              className="flex h-10 w-10 items-center justify-center rounded-md transition-colors hover:bg-neutral-100"
            >
              <Search className="h-5 w-5" />
              <span className="sr-only">Search</span>
            </Link>
            <button
              type="button"
              className="relative flex h-10 w-10 items-center justify-center rounded-md transition-colors hover:bg-neutral-100"
              onClick={() => setCartOpen(true)}
            >
              <ShoppingBag className="h-5 w-5" />
              {totalItems > 0 && (
                <Badge className="absolute -top-0.5 -right-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-black px-1 text-[10px] font-bold text-white">
                  {totalItems > 99 ? "99+" : totalItems}
                </Badge>
              )}
              <span className="sr-only">Cart</span>
            </button>
            {isAuthenticated ? (
              <DropdownMenu>
                <DropdownMenuTrigger render={
                  <button type="button" className="flex h-10 items-center gap-2 rounded-md px-2 transition-colors hover:bg-neutral-100" />
                }>
                  <Avatar className="h-7 w-7">
                    <AvatarFallback className="bg-black text-[10px] font-bold text-white">{getInitials()}</AvatarFallback>
                  </Avatar>
                  <span className="hidden text-sm font-medium lg:block">
                    {user?.firstName}
                  </span>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56 rounded-lg border-0 shadow-lg">
                  <div className="border-b px-3 py-3">
                    <p className="text-sm font-semibold">{user?.firstName} {user?.lastName}</p>
                    <p className="text-xs text-neutral-500">{user?.email}</p>
                  </div>
                  <div className="py-1">
                    <DropdownMenuItem render={<Link href="/account" className="flex items-center gap-3 w-full px-3 py-2" />}>
                      <User className="h-4 w-4 text-neutral-500" />
                      <span className="text-sm">My Profile</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem render={<Link href="/orders" className="flex items-center gap-3 w-full px-3 py-2" />}>
                      <Package className="h-4 w-4 text-neutral-500" />
                      <span className="text-sm">My Orders</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem render={<Link href="/wishlist" className="flex items-center gap-3 w-full px-3 py-2" />}>
                      <Heart className="h-4 w-4 text-neutral-500" />
                      <span className="text-sm">Wishlist</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem render={<Link href="/account/settings" className="flex items-center gap-3 w-full px-3 py-2" />}>
                      <Settings className="h-4 w-4 text-neutral-500" />
                      <span className="text-sm">Settings</span>
                    </DropdownMenuItem>
                  </div>
                  {user?.isAdmin && (
                    <>
                      <div className="border-t py-1">
                        <DropdownMenuItem render={<Link href="/admin" className="flex items-center gap-3 w-full px-3 py-2" />}>
                          <Store className="h-4 w-4 text-neutral-500" />
                          <span className="text-sm">Admin Panel</span>
                        </DropdownMenuItem>
                      </div>
                    </>
                  )}
                  <div className="border-t py-1">
                    <DropdownMenuItem onClick={() => logout()} className="flex items-center gap-3 px-3 py-2">
                      <LogOut className="h-4 w-4 text-neutral-500" />
                      <span className="text-sm">Sign out</span>
                    </DropdownMenuItem>
                  </div>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <Link
                href="/login"
                className="flex h-10 items-center rounded-md px-3 text-sm font-medium transition-colors hover:bg-neutral-100"
              >
                Sign in
              </Link>
            )}

            {/* Mobile Menu Toggle */}
            <button
              type="button"
              className="flex h-10 w-10 items-center justify-center rounded-md transition-colors hover:bg-neutral-100 md:hidden"
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            >
              {mobileMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </button>
          </div>
        </div>

        {/* Mobile Navigation */}
        {mobileMenuOpen && (
          <div className="border-t border-black/10 bg-white md:hidden">
            <nav className="mx-auto max-w-7xl px-4 py-4">
              {NAV_LINKS.map(({ href, label }) => {
                const isActive = pathname.startsWith(href);
                return (
                  <Link
                    key={href}
                    href={href}
                    onClick={() => setMobileMenuOpen(false)}
                    className={`block py-3 text-sm font-semibold uppercase tracking-wider ${
                      isActive ? "text-black" : "text-neutral-500"
                    }`}
                  >
                    {label}
                  </Link>
                );
              })}
            </nav>
          </div>
        )}
      </header>
      <CartDrawer open={cartOpen} onOpenChange={setCartOpen} />
    </>
  );
}
