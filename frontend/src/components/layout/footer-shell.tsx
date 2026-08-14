"use client";

import Link from "next/link";
import { useStoreSettings } from "@/hooks/use-store-settings";

const FOOTER_SECTIONS = [
  {
    title: "Shop",
    links: [
      { href: "/shop", label: "All Products" },
      { href: "/new-arrivals", label: "New Arrivals" },
      { href: "/best-sellers", label: "Best Sellers" },
      { href: "/collections", label: "Collections" },
    ],
  },
  {
    title: "Help",
    links: [
      { href: "/orders", label: "Track Order" },
      { href: "/account/addresses", label: "Shipping Info" },
      { href: "/account", label: "Returns" },
      { href: "/login", label: "Contact Us" },
    ],
  },
  {
    title: "Company",
    links: [
      { href: "/collections", label: "About" },
      { href: "/collections", label: "Careers" },
      { href: "/collections", label: "Terms" },
      { href: "/collections", label: "Privacy" },
    ],
  },
];

export function FooterShell() {
  const { storeName } = useStoreSettings();

  return (
    <footer className="border-t border-black/10 bg-white">
      <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid grid-cols-2 gap-8 md:grid-cols-4">
          {/* Brand */}
          <div className="col-span-2 md:col-span-1">
            <Link href="/" className="text-lg font-bold uppercase tracking-tight">
              {storeName}
            </Link>
            <p className="mt-3 text-sm text-neutral-500">
              Independent streetwear for the bold.
            </p>
          </div>

          {/* Links */}
          {FOOTER_SECTIONS.map((section) => (
            <div key={section.title}>
              <h3 className="text-[11px] font-semibold uppercase tracking-widest text-neutral-400">
                {section.title}
              </h3>
              <ul className="mt-4 space-y-3">
                {section.links.map((link) => (
                  <li key={link.href}>
                    <Link
                      href={link.href}
                      className="text-sm text-neutral-600 transition-colors hover:text-black"
                    >
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        <div className="mt-12 border-t border-black/10 pt-8">
          <div className="flex flex-col items-center justify-between gap-4 sm:flex-row">
            <p className="text-xs text-neutral-400">
              &copy; {new Date().getFullYear()} {storeName}. All rights reserved.
            </p>
            <div className="flex gap-4 text-xs text-neutral-400">
              <Link href="/collections" className="hover:text-black transition-colors">Terms</Link>
              <Link href="/collections" className="hover:text-black transition-colors">Privacy</Link>
              <Link href="/collections" className="hover:text-black transition-colors">Cookies</Link>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}
