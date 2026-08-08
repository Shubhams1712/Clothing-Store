"use client";

import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from "react";

export interface CartItem {
  productId: string;
  variantId: string;
  name: string;
  slug: string;
  price: number;
  imageUrl: string;
  size: string;
  color: string;
  quantity: number;
  stock: number;
  sku?: string;
}

export interface SavedItem {
  productId: string;
  name: string;
  slug: string;
  price: number;
  imageUrl: string;
  size: string;
  color: string;
  stock: number;
  sku?: string;
}

export interface AppliedCoupon {
  code: string;
  description?: string;
  type: string;
  value: number;
  discountAmount: number;
}

interface CartContextType {
  items: CartItem[];
  savedItems: SavedItem[];
  appliedCoupon: AppliedCoupon | null;
  addItem: (item: Omit<CartItem, "quantity"> & { quantity?: number }) => void;
  removeItem: (variantId: string) => void;
  updateQuantity: (variantId: string, quantity: number) => void;
  clearCart: () => void;
  saveForLater: (variantId: string) => void;
  moveToCart: (productId: string) => void;
  removeSavedItem: (productId: string) => void;
  applyCoupon: (coupon: AppliedCoupon) => void;
  removeCoupon: () => void;
  totalItems: number;
  totalPrice: number;
}

const CartContext = createContext<CartContextType | null>(null);

const CART_STORAGE_KEY = "store-cart";
const SAVED_STORAGE_KEY = "store-saved-items";
const COUPON_STORAGE_KEY = "store-applied-coupon";

function loadCart(): CartItem[] {
  if (typeof window === "undefined") return [];
  try {
    const stored = localStorage.getItem(CART_STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
}

function loadSaved(): SavedItem[] {
  if (typeof window === "undefined") return [];
  try {
    const stored = localStorage.getItem(SAVED_STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
}

function loadCoupon(): AppliedCoupon | null {
  if (typeof window === "undefined") return null;
  try {
    const stored = localStorage.getItem(COUPON_STORAGE_KEY);
    return stored ? JSON.parse(stored) : null;
  } catch {
    return null;
  }
}

function saveCart(items: CartItem[]) {
  if (typeof window === "undefined") return;
  try {
    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(items));
  } catch {
    // silently fail
  }
}

function saveSaved(items: SavedItem[]) {
  if (typeof window === "undefined") return;
  try {
    localStorage.setItem(SAVED_STORAGE_KEY, JSON.stringify(items));
  } catch {
    // silently fail
  }
}

function saveCoupon(coupon: AppliedCoupon | null) {
  if (typeof window === "undefined") return;
  try {
    if (coupon) {
      localStorage.setItem(COUPON_STORAGE_KEY, JSON.stringify(coupon));
    } else {
      localStorage.removeItem(COUPON_STORAGE_KEY);
    }
  } catch {
    // silently fail
  }
}

export function CartProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<CartItem[]>(() => loadCart());
  const [savedItems, setSavedItems] = useState<SavedItem[]>(() => loadSaved());
  const [appliedCoupon, setAppliedCoupon] = useState<AppliedCoupon | null>(() => loadCoupon());

  useEffect(() => {
    saveCart(items);
  }, [items]);

  useEffect(() => {
    saveSaved(savedItems);
  }, [savedItems]);

  useEffect(() => {
    saveCoupon(appliedCoupon);
  }, [appliedCoupon]);

  const addItem = useCallback((item: Omit<CartItem, "quantity"> & { quantity?: number }) => {
    const qty = item.quantity ?? 1;
    setItems(prev => {
      const existing = prev.find(i => i.variantId === item.variantId);
      if (existing) {
        return prev.map(i =>
          i.variantId === item.variantId
            ? { ...i, quantity: Math.min(i.quantity + qty, i.stock) }
            : i
        );
      }
      return [...prev, { ...item, quantity: qty }];
    });
  }, []);

  const removeItem = useCallback((variantId: string) => {
    setItems(prev => prev.filter(i => i.variantId !== variantId));
  }, []);

  const updateQuantity = useCallback((variantId: string, quantity: number) => {
    setItems(prev =>
      prev.map(i =>
        i.variantId === variantId
          ? { ...i, quantity: Math.max(1, Math.min(quantity, i.stock)) }
          : i
      )
    );
  }, []);

  const clearCart = useCallback(() => {
    setItems([]);
    setAppliedCoupon(null);
  }, []);

  const saveForLater = useCallback((variantId: string) => {
    setItems(prev => {
      const item = prev.find(i => i.variantId === variantId);
      if (item) {
        setSavedItems(saved => {
          if (saved.some(s => s.productId === item.productId)) return saved;
          return [...saved, {
            productId: item.productId,
            name: item.name,
            slug: item.slug,
            price: item.price,
            imageUrl: item.imageUrl,
            size: item.size,
            color: item.color,
            stock: item.stock,
            sku: item.sku,
          }];
        });
      }
      return prev.filter(i => i.variantId !== variantId);
    });
  }, []);

  const moveToCart = useCallback((productId: string) => {
    setSavedItems(saved => {
      const item = saved.find(s => s.productId === productId);
      if (item) {
        setItems(prev => {
          const existing = prev.find(i => i.productId === item.productId);
          if (existing) {
            return prev.map(i =>
              i.productId === item.productId
                ? { ...i, quantity: Math.min(i.quantity + 1, i.stock) }
                : i
            );
          }
          return [...prev, {
            productId: item.productId,
            variantId: item.productId,
            name: item.name,
            slug: item.slug,
            price: item.price,
            imageUrl: item.imageUrl,
            size: item.size,
            color: item.color,
            quantity: 1,
            stock: item.stock,
            sku: item.sku,
          }];
        });
      }
      return saved.filter(s => s.productId !== productId);
    });
  }, []);

  const removeSavedItem = useCallback((productId: string) => {
    setSavedItems(prev => prev.filter(s => s.productId !== productId));
  }, []);

  const applyCoupon = useCallback((coupon: AppliedCoupon) => {
    setAppliedCoupon(coupon);
  }, []);

  const removeCoupon = useCallback(() => {
    setAppliedCoupon(null);
  }, []);

  const totalItems = items.reduce((sum, i) => sum + i.quantity, 0);
  const totalPrice = items.reduce((sum, i) => sum + i.price * i.quantity, 0);

  return (
    <CartContext.Provider value={{
      items, savedItems, appliedCoupon,
      addItem, removeItem, updateQuantity, clearCart,
      saveForLater, moveToCart, removeSavedItem,
      applyCoupon, removeCoupon,
      totalItems, totalPrice,
    }}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error("useCart must be used within CartProvider");
  return ctx;
}
