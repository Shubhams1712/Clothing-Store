"use client";

import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from "react";

export interface RecentlyViewedItem {
  productId: string;
  name: string;
  slug: string;
  price: number;
  imageUrl: string;
  brand?: string;
  categoryName?: string;
  viewedAt: number;
}

interface RecentlyViewedContextType {
  items: RecentlyViewedItem[];
  addItem: (item: Omit<RecentlyViewedItem, "viewedAt">) => void;
  clearItems: () => void;
}

const RecentlyViewedContext = createContext<RecentlyViewedContextType | null>(null);

const STORAGE_KEY = "store-recently-viewed";
const MAX_ITEMS = 20;

function loadItems(): RecentlyViewedItem[] {
  if (typeof window === "undefined") return [];
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
}

function saveItems(items: RecentlyViewedItem[]) {
  if (typeof window === "undefined") return;
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  } catch {
    // silently fail
  }
}

export function RecentlyViewedProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<RecentlyViewedItem[]>(() => loadItems());

  useEffect(() => {
    saveItems(items);
  }, [items]);

  const addItem = useCallback((item: Omit<RecentlyViewedItem, "viewedAt">) => {
    setItems(prev => {
      const filtered = prev.filter(i => i.productId !== item.productId);
      const updated = [{ ...item, viewedAt: Date.now() }, ...filtered];
      return updated.slice(0, MAX_ITEMS);
    });
  }, []);

  const clearItems = useCallback(() => setItems([]), []);

  return (
    <RecentlyViewedContext.Provider value={{ items, addItem, clearItems }}>
      {children}
    </RecentlyViewedContext.Provider>
  );
}

export function useRecentlyViewed() {
  const ctx = useContext(RecentlyViewedContext);
  if (!ctx) throw new Error("useRecentlyViewed must be used within RecentlyViewedProvider");
  return ctx;
}
