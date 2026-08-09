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
          <ShoppingBag className="h-16 w-16 text-muted-foreground" />
          <h1 className="text-3xl font-bold">Your cart is empty</h1>
          <p className="text-muted-foreground">Looks like you haven&apos;t added anything to your cart yet.</p>
          <Link href="/shop" className={buttonVariants({ size: "lg" })}>
            Continue Shopping
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold">Shopping Cart</h1>
        <p className="mt-2 text-muted-foreground">{totalItems} item{totalItems !== 1 ? "s" : ""} in your cart</p>
      </div>

      <div className="grid gap-8 lg:grid-cols-[1fr_380px]">
        {/* Cart Items */}
        <div className="space-y-4">
          {items.map(item => (
            <Card key={item.variantId}>
              <CardContent className="flex gap-4 p-4">
                <div className="relative h-24 w-24 shrink-0 overflow-hidden rounded-md bg-muted">
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
                        className="font-medium hover:underline"
                      >
                        {item.name}
                      </Link>
                      <div className="mt-1 flex gap-2 text-sm text-muted-foreground">
                        {item.size && <span>Size: {item.size}</span>}
                        {item.color && <span>Color: {item.color}</span>}
                      </div>
                      {item.sku && (
                        <p className="mt-0.5 text-xs text-muted-foreground">SKU: {item.sku}</p>
                      )}
                    </div>
                    <span className="font-semibold">{formatPrice(item.price * item.quantity)}</span>
                  </div>

                  <div className="mt-2 flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <div className="flex items-center border rounded-md">
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-8 w-8"
                          onClick={() => updateQuantity(item.variantId, item.quantity - 1)}
                          disabled={item.quantity <= 1}
                          aria-label="Decrease quantity"
                        >
                          <Minus className="h-3 w-3" />
                        </Button>
                        <span className="w-8 text-center text-sm">{item.quantity}</span>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-8 w-8"
                          onClick={() => updateQuantity(item.variantId, item.quantity + 1)}
                          disabled={item.quantity >= item.stock}
                          aria-label="Increase quantity"
                        >
                          <Plus className="h-3 w-3" />
                        </Button>
                      </div>
                      {item.quantity >= item.stock && (
                        <Badge variant="outline" className="text-xs">Max stock</Badge>
                      )}
                    </div>

                    <div className="flex items-center gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => saveForLater(item.variantId)}
                        className="text-muted-foreground"
                      >
                        <Bookmark className="mr-1 h-3 w-3" />
                        Save for later
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8 text-muted-foreground hover:text-destructive"
                        onClick={() => removeItem(item.variantId)}
                        aria-label="Remove item"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}

          {/* Saved for Later */}
          {savedItems.length > 0 && (
            <>
              <Separator className="my-6" />
              <h2 className="text-lg font-semibold">Saved for Later ({savedItems.length})</h2>
              <div className="space-y-4">
                {savedItems.map(item => (
                  <Card key={item.productId}>
                    <CardContent className="flex gap-4 p-4">
                      <div className="relative h-20 w-20 shrink-0 overflow-hidden rounded-md bg-muted">
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
                          <Link href={`/shop/${item.slug}`} className="font-medium hover:underline">
                            {item.name}
                          </Link>
                          <p className="text-sm text-muted-foreground">{formatPrice(item.price)}</p>
                        </div>
                        <div className="flex items-center gap-2">
                          <Button variant="outline" size="sm" onClick={() => moveToCart(item.productId)}>
                            Move to Cart
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8 text-muted-foreground hover:text-destructive"
                            onClick={() => removeSavedItem(item.productId)}
                            aria-label="Remove saved item"
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </>
          )}

          <div className="flex items-center justify-between pt-4">
            <Link href="/shop" className={buttonVariants({ variant: "ghost" })}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              Continue Shopping
            </Link>
            <Button variant="outline" onClick={() => { if (window.confirm("Are you sure you want to clear your cart?")) clearCart(); }} className="text-destructive hover:text-destructive">
              Clear Cart
            </Button>
          </div>
        </div>

        {/* Order Summary */}
        <div className="lg:sticky lg:top-24 lg:self-start">
          <Card>
            <CardHeader>
              <CardTitle>Order Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Coupon */}
              <div className="space-y-2">
                <label htmlFor="coupon-input" className="text-sm font-medium">Coupon Code</label>
                {appliedCoupon ? (
                  <div className="flex items-center justify-between rounded-md border border-green-500 bg-green-50 p-2">
                    <div className="flex items-center gap-2">
                      <Tag className="h-4 w-4 text-green-600" />
                      <span className="text-sm font-medium text-green-700">{appliedCoupon.code}</span>
                    </div>
                    <Button variant="ghost" size="sm" onClick={handleRemoveCoupon} className="text-destructive">
                      Remove
                    </Button>
                  </div>
                ) : (
                  <div className="flex gap-2">
                    <Input
                      id="coupon-input"
                      value={couponCode}
                      onChange={e => setCouponCode(e.target.value)}
                      placeholder="Enter code"
                      onKeyDown={e => e.key === "Enter" && handleApplyCoupon()}
                    />
                    <Button
                      variant="outline"
                      onClick={handleApplyCoupon}
                      disabled={couponLoading || !couponCode.trim()}
                    >
                      {couponLoading ? "..." : "Apply"}
                    </Button>
                  </div>
                )}
                {couponResult && !couponResult.isValid && (
                  <p className="text-xs text-destructive">{couponResult.message}</p>
                )}
              </div>

              <Separator />

              {/* Price Breakdown */}
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Subtotal</span>
                  <span>{formatPrice(totalPrice)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Estimated Tax (18% GST)</span>
                  <span>{formatPrice(taxAmount)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Shipping</span>
                  <span>{shippingAmount === 0 ? <span className="text-green-600">Free</span> : formatPrice(shippingAmount)}</span>
                </div>
                {discountAmount > 0 && (
                  <div className="flex justify-between text-green-600">
                    <span>Discount ({appliedCoupon?.code})</span>
                    <span>-{formatPrice(discountAmount)}</span>
                  </div>
                )}
              </div>

              <Separator />

              <div className="flex justify-between text-lg font-bold">
                <span>Total</span>
                <span>{formatPrice(grandTotal)}</span>
              </div>

              {shippingAmount > 0 && (
                <p className="text-xs text-muted-foreground">
                  Free shipping on orders over {formatPrice(2000)}
                </p>
              )}

              <Link href="/checkout" className={buttonVariants({ size: "lg", className: "w-full" })}>
                Proceed to Checkout
                <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
