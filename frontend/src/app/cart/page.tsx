"use client";

import { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { Trash2, Plus, Minus, ShoppingBag, Tag, ArrowRight, Bookmark, ArrowLeft } from "lucide-react";
import { useCart } from "@/hooks/use-cart";
import { couponService, type CouponApplyResult } from "@/services/shopping";
import { Button, buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { formatPrice, getSafeImageUrl } from "@/lib/utils";
import { toast } from "sonner";

export default function CartPage() {
  const {
    items, savedItems, appliedCoupon,
    removeItem, updateQuantity, clearCart,
    saveForLater, moveToCart, removeSavedItem,
    applyCoupon, removeCoupon,
    totalItems, totalPrice,
  } = useCart();

  const [couponCode, setCouponCode] = useState("");
  const [couponLoading, setCouponLoading] = useState(false);
  const [couponResult, setCouponResult] = useState<CouponApplyResult | null>(null);

  const handleApplyCoupon = async () => {
    if (!couponCode.trim()) return;
    setCouponLoading(true);
    try {
      const result = await couponService.applyCoupon(couponCode.trim(), totalPrice);
      setCouponResult(result);
      if (result.isValid) {
        applyCoupon({
          code: result.code!,
          description: result.description,
          type: result.type,
          value: result.value,
          discountAmount: result.discountAmount,
        });
        toast.success("Coupon applied!");
        setCouponCode("");
      } else {
        toast.error(result.message || "Invalid coupon");
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : "Failed to apply coupon";
      toast.error(message);
    } finally {
      setCouponLoading(false);
    }
  };

  const handleRemoveCoupon = () => {
    removeCoupon();
    setCouponResult(null);
    toast.success("Coupon removed");
  };

  const taxAmount = Math.round(totalPrice * 0.18 * 100) / 100;
  const shippingAmount = totalPrice >= 2000 ? 0 : 150;
  const discountAmount = appliedCoupon?.discountAmount ?? 0;
  const grandTotal = Math.max(0, totalPrice + taxAmount + shippingAmount - discountAmount);

  if (items.length === 0 && savedItems.length === 0) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center space-y-6 text-center">
          <ShoppingBag className="h-16 w-16 text-neutral-300" />
          <h1 className="text-2xl font-bold uppercase tracking-tight">Your Cart is Empty</h1>
          <p className="text-sm text-neutral-500">Looks like you haven&apos;t added anything yet.</p>
          <Link href="/shop" className={buttonVariants({ size: "lg", className: "bg-black text-white hover:bg-neutral-800" })}>
            Continue Shopping
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold uppercase tracking-tight">Shopping Cart</h1>
        <p className="mt-2 text-sm text-neutral-500">{totalItems} item{totalItems !== 1 ? "s" : ""} in your cart</p>
      </div>

      <div className="grid gap-8 lg:grid-cols-[1fr_380px]">
        <div className="space-y-4">
          {items.map(item => (
            <div key={item.variantId} className="flex gap-4 border-b border-black/10 pb-4">
              <div className="relative h-24 w-24 shrink-0 overflow-hidden bg-neutral-100">
                <Image
                  src={getSafeImageUrl(item.imageUrl)}
                  alt={item.name}
                  fill
                  className="object-cover"
                  sizes="96px"
                />
              </div>

              <div className="flex flex-1 flex-col justify-between">
                <div className="flex items-start justify-between">
                  <div>
                    <Link
                      href={`/shop/${item.slug}`}
                      className="text-sm font-semibold hover:underline"
                    >
                      {item.name}
                    </Link>
                    <div className="mt-1 flex gap-2 text-xs text-neutral-500">
                      {item.size && <span>Size: {item.size}</span>}
                      {item.color && <span>Color: {item.color}</span>}
                    </div>
                  </div>
                  <span className="text-sm font-bold">{formatPrice(item.price * item.quantity)}</span>
                </div>

                <div className="mt-2 flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <div className="flex items-center border border-black/10">
                      <button
                        type="button"
                        className="flex h-8 w-8 items-center justify-center transition-colors hover:bg-neutral-100"
                        onClick={() => updateQuantity(item.variantId, item.quantity - 1)}
                        disabled={item.quantity <= 1}
                        aria-label="Decrease quantity"
                      >
                        <Minus className="h-3 w-3" />
                      </button>
                      <span className="w-8 text-center text-sm">{item.quantity}</span>
                      <button
                        type="button"
                        className="flex h-8 w-8 items-center justify-center transition-colors hover:bg-neutral-100"
                        onClick={() => updateQuantity(item.variantId, item.quantity + 1)}
                        disabled={item.quantity >= item.stock}
                        aria-label="Increase quantity"
                      >
                        <Plus className="h-3 w-3" />
                      </button>
                    </div>
                    {item.quantity >= item.stock && (
                      <Badge variant="outline" className="text-[10px] uppercase tracking-wider">Max stock</Badge>
                    )}
                  </div>

                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={() => saveForLater(item.variantId)}
                      className="flex items-center gap-1 text-xs text-neutral-400 transition-colors hover:text-black"
                    >
                      <Bookmark className="h-3 w-3" />
                      Save
                    </button>
                    <button
                      type="button"
                      className="flex h-8 w-8 items-center justify-center text-neutral-400 transition-colors hover:text-[#E10600]"
                      onClick={() => removeItem(item.variantId)}
                      aria-label="Remove item"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}

          {savedItems.length > 0 && (
            <>
              <div className="pt-4">
                <h2 className="text-sm font-bold uppercase tracking-wider">Saved for Later ({savedItems.length})</h2>
              </div>
              <div className="space-y-4">
                {savedItems.map(item => (
                  <div key={item.productId} className="flex gap-4 border-b border-black/10 pb-4">
                    <div className="relative h-20 w-20 shrink-0 overflow-hidden bg-neutral-100">
                      <Image
                        src={getSafeImageUrl(item.imageUrl)}
                        alt={item.name}
                        fill
                        className="object-cover"
                        sizes="80px"
                      />
                    </div>
                    <div className="flex flex-1 items-center justify-between">
                      <div>
                        <Link href={`/shop/${item.slug}`} className="text-sm font-semibold hover:underline">
                          {item.name}
                        </Link>
                        <p className="text-sm text-neutral-500">{formatPrice(item.price)}</p>
                      </div>
                      <div className="flex items-center gap-2">
                        <Button variant="outline" size="sm" onClick={() => moveToCart(item.productId)} className="text-xs">
                          Move to Cart
                        </Button>
                        <button
                          type="button"
                          className="flex h-8 w-8 items-center justify-center text-neutral-400 transition-colors hover:text-[#E10600]"
                          onClick={() => removeSavedItem(item.productId)}
                          aria-label="Remove saved item"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}

          <div className="flex items-center justify-between pt-4">
            <Link href="/shop" className="flex items-center gap-2 text-sm font-medium text-neutral-500 transition-colors hover:text-black">
              <ArrowLeft className="h-4 w-4" />
              Continue Shopping
            </Link>
            <button
              type="button"
              onClick={() => { if (window.confirm("Are you sure you want to clear your cart?")) clearCart(); }}
              className="text-xs font-medium uppercase tracking-wider text-[#E10600] transition-colors hover:text-[#c40500]"
            >
              Clear Cart
            </button>
          </div>
        </div>

        <div className="lg:sticky lg:top-24 lg:self-start">
          <div className="border border-black/10 p-6">
            <h2 className="text-sm font-bold uppercase tracking-wider">Order Summary</h2>

            <div className="mt-6 space-y-4">
              <div className="space-y-2">
                <label htmlFor="coupon-input" className="text-xs font-semibold uppercase tracking-wider text-neutral-500">Coupon Code</label>
                {appliedCoupon ? (
                  <div className="flex items-center justify-between border border-green-500 bg-green-50 p-2">
                    <div className="flex items-center gap-2">
                      <Tag className="h-4 w-4 text-green-600" />
                      <span className="text-sm font-semibold text-green-700">{appliedCoupon.code}</span>
                    </div>
                    <button
                      type="button"
                      onClick={handleRemoveCoupon}
                      className="text-xs font-medium text-[#E10600] hover:text-[#c40500]"
                    >
                      Remove
                    </button>
                  </div>
                ) : (
                  <div className="flex gap-2">
                    <Input
                      id="coupon-input"
                      value={couponCode}
                      onChange={e => setCouponCode(e.target.value)}
                      placeholder="Enter code"
                      onKeyDown={e => e.key === "Enter" && handleApplyCoupon()}
                      className="border-black/10"
                    />
                    <Button
                      variant="outline"
                      onClick={handleApplyCoupon}
                      disabled={couponLoading || !couponCode.trim()}
                      className="border-black/10"
                    >
                      {couponLoading ? "..." : "Apply"}
                    </Button>
                  </div>
                )}
                {couponResult && !couponResult.isValid && (
                  <p className="text-xs text-[#E10600]">{couponResult.message}</p>
                )}
              </div>

              <Separator className="bg-black/10" />

              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-neutral-500">Subtotal</span>
                  <span>{formatPrice(totalPrice)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-neutral-500">Estimated Tax (18% GST)</span>
                  <span>{formatPrice(taxAmount)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-neutral-500">Shipping</span>
                  <span>{shippingAmount === 0 ? <span className="text-green-600 font-medium">Free</span> : formatPrice(shippingAmount)}</span>
                </div>
                {discountAmount > 0 && (
                  <div className="flex justify-between text-green-600">
                    <span>Discount ({appliedCoupon?.code})</span>
                    <span>-{formatPrice(discountAmount)}</span>
                  </div>
                )}
              </div>

              <Separator className="bg-black/10" />

              <div className="flex justify-between text-lg font-bold">
                <span>Total</span>
                <span>{formatPrice(grandTotal)}</span>
              </div>

              {shippingAmount > 0 && (
                <p className="text-xs text-neutral-400">
                  Free shipping on orders over {formatPrice(2000)}
                </p>
              )}

              <Link href="/checkout" className="block">
                <Button className="w-full bg-black text-white hover:bg-neutral-800" size="lg">
                  Proceed to Checkout
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
