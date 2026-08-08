"use client";

import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from "react";

export interface WishlistItem {
  productId: string;
  name: string;
  slug: string;
  price: number;
  imageUrl: string;
  brand?: string;
}

interface WishlistContextType {
  items: WishlistItem[];
  addItem: (item: WishlistItem) => void;
  removeItem: (productId: string) => void;
  toggleItem: (item: WishlistItem) => void;
  isInWishlist: (productId: string) => boolean;
  totalItems: number;
}

const WishlistContext = createContext<WishlistContextType | null>(null);

const WISHLIST_STORAGE_KEY = "store-wishlist";

function loadWishlist(): WishlistItem[] {
  if (typeof window === "undefined") return [];
  try {
    const stored = localStorage.getItem(WISHLIST_STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
}

function saveWishlist(items: WishlistItem[]) {
  if (typeof window === "undefined") return;
  try {
    localStorage.setItem(WISHLIST_STORAGE_KEY, JSON.stringify(items));
  } catch {
    // silently fail
  }
}

export function WishlistProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<WishlistItem[]>(() => loadWishlist());

  useEffect(() => {
    saveWishlist(items);
  }, [items]);

  const addItem = useCallback((item: WishlistItem) => {
    setItems(prev => {
      if (prev.some(i => i.productId === item.productId)) return prev;
      return [...prev, item];
    });
  }, []);

  const removeItem = useCallback((productId: string) => {
    setItems(prev => prev.filter(i => i.productId !== productId));
  }, []);

  const toggleItem = useCallback((item: WishlistItem) => {
    setItems(prev => {
      if (prev.some(i => i.productId === item.productId)) {
        return prev.filter(i => i.productId !== item.productId);
      }
      return [...prev, item];
    });
  }, []);

  const isInWishlist = useCallback((productId: string) => {
    return items.some(i => i.productId === productId);
  }, [items]);

  const totalItems = items.length;

  return (
    <WishlistContext.Provider value={{ items, addItem, removeItem, toggleItem, isInWishlist, totalItems }}>
      {children}
    </WishlistContext.Provider>
  );
}

export function useWishlist() {
  const ctx = useContext(WishlistContext);
  if (!ctx) throw new Error("useWishlist must be used within WishlistProvider");
  return ctx;
}
