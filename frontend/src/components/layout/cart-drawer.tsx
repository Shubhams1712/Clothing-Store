"use client";

import Link from "next/link";
import Image from "next/image";
import { useCart } from "@/hooks/use-cart";
import { formatPrice, getSafeImageUrl } from "@/lib/utils";
import { Sheet, SheetContent, SheetHeader, SheetTitle } from "@/components/ui/sheet";
import { Button } from "@/components/ui/button";
import { Trash2, Plus, Minus, ShoppingBag } from "lucide-react";

interface CartDrawerProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CartDrawer({ open, onOpenChange }: CartDrawerProps) {
  const { items, removeItem, updateQuantity, totalItems, totalPrice, clearCart } = useCart();

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="flex w-full flex-col sm:max-w-md">
        <SheetHeader className="space-y-1">
          <SheetTitle className="flex items-center justify-between">
            <span className="text-sm font-bold uppercase tracking-wider">Cart ({totalItems})</span>
            {items.length > 0 && (
              <button
                type="button"
                onClick={clearCart}
                className="text-[11px] font-medium uppercase tracking-wider text-neutral-400 hover:text-black"
              >
                Clear all
              </button>
            )}
          </SheetTitle>
        </SheetHeader>

        {items.length === 0 ? (
          <div className="flex flex-1 flex-col items-center justify-center gap-4">
            <ShoppingBag className="h-12 w-12 text-neutral-300" />
            <p className="text-sm text-neutral-500">Your cart is empty</p>
            <Link href="/shop" onClick={() => onOpenChange(false)}>
              <Button className="bg-black text-white hover:bg-neutral-800">Continue Shopping</Button>
            </Link>
          </div>
        ) : (
          <>
            <div className="flex-1 overflow-y-auto py-4">
              <div className="space-y-4">
                {items.map((item) => (
                  <div key={item.variantId} className="flex gap-4">
                    <div className="relative h-20 w-20 shrink-0 overflow-hidden bg-neutral-100">
                      <Image
                        src={getSafeImageUrl(item.imageUrl)}
                        alt={item.name}
                        fill
                        className="object-cover"
                        sizes="80px"
                      />
                    </div>
                    <div className="flex flex-1 flex-col justify-between">
                      <div>
                        <Link
                          href={`/shop/${item.slug}`}
                          onClick={() => onOpenChange(false)}
                          className="text-sm font-semibold line-clamp-1 hover:underline"
                        >
                          {item.name}
                        </Link>
                        <p className="text-xs text-neutral-400">
                          {item.size && `Size: ${item.size}`}
                          {item.size && item.color && " / "}
                          {item.color && `Color: ${item.color}`}
                        </p>
                      </div>
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <button
                            type="button"
                            className="flex h-7 w-7 items-center justify-center border border-black/10 transition-colors hover:bg-neutral-100"
                            onClick={() => updateQuantity(item.variantId, item.quantity - 1)}
                            disabled={item.quantity <= 1}
                          >
                            <Minus className="h-3 w-3" />
                          </button>
                          <span className="w-6 text-center text-sm font-semibold">{item.quantity}</span>
                          <button
                            type="button"
                            className="flex h-7 w-7 items-center justify-center border border-black/10 transition-colors hover:bg-neutral-100"
                            onClick={() => updateQuantity(item.variantId, item.quantity + 1)}
                            disabled={item.quantity >= item.stock}
                          >
                            <Plus className="h-3 w-3" />
                          </button>
                        </div>
                        <div className="flex items-center gap-2">
                          <span className="text-sm font-bold">{formatPrice(item.price * item.quantity)}</span>
                          <button
                            type="button"
                            className="flex h-7 w-7 items-center justify-center text-neutral-400 transition-colors hover:text-[#E10600]"
                            onClick={() => removeItem(item.variantId)}
                          >
                            <Trash2 className="h-3.5 w-3.5" />
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            <div className="border-t border-black/10 pt-4 space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-sm text-neutral-500">Subtotal</span>
                <span className="text-lg font-bold">{formatPrice(totalPrice)}</span>
              </div>
              <p className="text-xs text-neutral-400">Shipping calculated at checkout</p>
              <Link href="/checkout" onClick={() => onOpenChange(false)} className="block">
                <Button className="w-full bg-black text-white hover:bg-neutral-800" size="lg">
                  Checkout
                </Button>
              </Link>
              <Link href="/shop" onClick={() => onOpenChange(false)} className="block">
                <Button variant="outline" className="w-full" size="lg">
                  Continue Shopping
                </Button>
              </Link>
            </div>
          </>
        )}
      </SheetContent>
    </Sheet>
  );
}
