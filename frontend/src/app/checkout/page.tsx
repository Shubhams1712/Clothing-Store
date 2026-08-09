"use client";

import { useState, useEffect, useCallback } from "react";
import Link from "next/link";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { Check, ChevronRight, CreditCard, MapPin, Package, Plus, Truck, ShoppingBag, Wallet } from "lucide-react";
import { useCart } from "@/hooks/use-cart";
import { useAuth } from "@/hooks/use-auth";
import { addressService, checkoutService, type Address, type CheckoutReviewResult } from "@/services/shopping";
import { paymentService } from "@/services/payment";
import { Button, buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Label } from "@/components/ui/label";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { AddressForm } from "@/components/storefront/address-form";
import { formatPrice, getSafeImageUrl } from "@/lib/utils";
import { toast } from "sonner";

declare global {
  interface Window {
    Razorpay: new (options: RazorpayOptions) => RazorpayInstance;
  }
}

interface RazorpayOptions {
  key: string;
  amount: number;
  currency: string;
  name: string;
  description?: string;
  order_id: string;
  handler: (response: RazorpayResponse) => void;
  prefill?: { name?: string; email?: string; contact?: string };
  theme?: { color?: string };
  modal?: { ondismiss?: () => void };
}

interface RazorpayInstance {
  open: () => void;
  on: (event: string, handler: (response: { error: { description: string } }) => void) => void;
}

interface RazorpayResponse {
  razorpay_order_id: string;
  razorpay_payment_id: string;
  razorpay_signature: string;
}

const STEPS = [
  { id: 1, label: "Information", icon: Package },
  { id: 2, label: "Shipping", icon: Truck },
  { id: 3, label: "Payment", icon: CreditCard },
];

export default function CheckoutPage() {
  const router = useRouter();
  const { items, appliedCoupon, totalPrice, clearCart } = useCart();
  const { user, isAuthenticated } = useAuth();
  const [currentStep, setCurrentStep] = useState(1);
  const [selectedAddressId, setSelectedAddressId] = useState<string>("");
  const [shippingMethod, setShippingMethod] = useState("standard");
  const [paymentMethod, setPaymentMethod] = useState("razorpay");
  const [reviewResult, setReviewResult] = useState<CheckoutReviewResult | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [addressDialogOpen, setAddressDialogOpen] = useState(false);

  const { data: addresses = [], isLoading: addressesLoading } = useQuery({
    queryKey: ["addresses"],
    queryFn: addressService.getAddresses,
    enabled: isAuthenticated,
  });

  const { data: checkoutReview, isLoading: reviewLoading } = useQuery({
    queryKey: ["checkout-review", items, appliedCoupon],
    queryFn: () =>
      checkoutService.reviewCheckout(
        items.map(i => ({
          productId: i.productId,
          variantId: i.variantId !== i.productId ? i.variantId : undefined,
          quantity: i.quantity,
        })),
        appliedCoupon?.code
      ),
    enabled: items.length > 0,
  });

  useEffect(() => {
    if (checkoutReview) setReviewResult(checkoutReview);
  }, [checkoutReview]);

  useEffect(() => {
    if (addresses.length > 0 && !selectedAddressId) {
      const defaultAddr = addresses.find(a => a.isDefault);
      setSelectedAddressId(defaultAddr?.id || addresses[0].id);
    }
  }, [addresses, selectedAddressId]);

  const loadRazorpayScript = useCallback(() => {
    return new Promise<boolean>((resolve) => {
      if (document.querySelector('script[src="https://checkout.razorpay.com/v1/checkout.js"]')) {
        resolve(true);
        return;
      }
      const script = document.createElement("script");
      script.src = "https://checkout.razorpay.com/v1/checkout.js";
      script.onload = () => resolve(true);
      script.onerror = () => resolve(false);
      document.body.appendChild(script);
    });
  }, []);

  const handleRazorpayPayment = async () => {
    const loaded = await loadRazorpayScript();
    if (!loaded) {
      toast.error("Failed to load payment gateway");
      return;
    }

    setIsProcessing(true);
    try {
      const paymentOrder = await paymentService.createRazorpayOrder(grandTotal, "INR", `order-${Date.now()}`);

      const options: RazorpayOptions = {
        key: paymentOrder.keyId,
        amount: paymentOrder.amount * 100,
        currency: paymentOrder.currency,
        name: "LUXE Store",
        description: `Order #${paymentOrder.orderId.slice(-8).toUpperCase()}`,
        order_id: paymentOrder.orderId,
        handler: async (response: RazorpayResponse) => {
          try {
            const orderItems = items.map(i => ({
              productId: i.productId,
              variantId: i.variantId !== i.productId ? i.variantId : undefined,
              quantity: i.quantity,
            }));

            const order = await paymentService.createOrderAfterPayment({
              razorpayOrderId: response.razorpay_order_id,
              razorpayPaymentId: response.razorpay_payment_id,
              razorpaySignature: response.razorpay_signature,
              items: orderItems,
              couponCode: appliedCoupon?.code,
              shippingAddressId: selectedAddressId,
              shippingMethod,
            });

            toast.success("Payment successful! Order placed.");
            clearCart();
            router.push(`/orders/${order.id}`);
          } catch {
            toast.error("Payment verified but order creation failed. Contact support.");
          }
        },
        prefill: {
          name: user ? `${user.firstName} ${user.lastName}` : "",
          email: user?.email || "",
          contact: selectedAddress?.phone || "",
        },
        theme: { color: "#000000" },
        modal: {
          ondismiss: () => {
            setIsProcessing(false);
            toast.info("Payment cancelled");
          },
        },
      };

      const razorpay = new window.Razorpay(options);
      razorpay.on("payment.failed", (response: { error: { description: string } }) => {
        toast.error(`Payment failed: ${response.error.description}`);
        setIsProcessing(false);
      });
      razorpay.open();
    } catch {
      toast.error("Failed to initiate payment");
      setIsProcessing(false);
    }
  };

  const handleCodPayment = async () => {
    setIsProcessing(true);
    try {
      const orderItems = items.map(i => ({
        productId: i.productId,
        variantId: i.variantId !== i.productId ? i.variantId : undefined,
        quantity: i.quantity,
      }));

      const order = await paymentService.createCodOrder({
        items: orderItems,
        couponCode: appliedCoupon?.code,
        shippingAddressId: selectedAddressId,
      });

      toast.success("Order placed successfully! Pay on delivery.");
      clearCart();
      router.push(`/orders/${order.id}`);
    } catch {
      toast.error("Failed to place COD order");
    } finally {
      setIsProcessing(false);
    }
  };

  if (items.length === 0) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center space-y-6 text-center">
          <ShoppingBag className="h-16 w-16 text-muted-foreground" />
          <h1 className="text-3xl font-bold">Your cart is empty</h1>
          <p className="text-muted-foreground">Add some items before checking out.</p>
          <Link href="/shop" className={buttonVariants({ size: "lg" })}>
            Continue Shopping
          </Link>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center space-y-6 text-center">
          <h1 className="text-3xl font-bold">Sign in to checkout</h1>
          <p className="text-muted-foreground">Please sign in to continue with your order.</p>
          <div className="flex gap-3">
            <Link href="/login" className={buttonVariants({ size: "lg" })}>
              Sign In
            </Link>
            <Link href="/register" className={buttonVariants({ variant: "outline", size: "lg" })}>
              Create Account
            </Link>
          </div>
        </div>
      </div>
    );
  }

  if (reviewLoading) return <LoadingOverlay text="Loading checkout..." />;

  const selectedAddress = addresses.find(a => a.id === selectedAddressId);
  const shippingCost = shippingMethod === "express" ? 300 : (totalPrice >= 2000 ? 0 : 150);
  const taxAmount = reviewResult?.taxAmount ?? Math.round(totalPrice * 0.18 * 100) / 100;
  const discountAmount = reviewResult?.discountAmount ?? 0;
  const grandTotal = reviewResult?.totalAmount ?? (totalPrice + taxAmount + shippingCost - discountAmount);

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      {isProcessing && <LoadingOverlay text="Processing payment..." />}

      <nav className="mb-8" aria-label="Checkout progress">
        <ol className="flex items-center justify-center gap-4">
          {STEPS.map((step, index) => (
            <li key={step.id} className="flex items-center gap-2">
              <button
                onClick={() => step.id < currentStep && setCurrentStep(step.id)}
                disabled={step.id > currentStep}
                className={`flex items-center gap-2 rounded-full px-3 py-1.5 text-sm font-medium transition-colors ${
                  step.id === currentStep
                    ? "bg-primary text-primary-foreground"
                    : step.id < currentStep
                    ? "bg-primary/10 text-primary"
                    : "bg-muted text-muted-foreground"
                }`}
              >
                {step.id < currentStep ? (
                  <Check className="h-4 w-4" />
                ) : (
                  <step.icon className="h-4 w-4" />
                )}
                <span className="hidden sm:inline">{step.label}</span>
              </button>
              {index < STEPS.length - 1 && (
                <ChevronRight className="h-4 w-4 text-muted-foreground" />
              )}
            </li>
          ))}
        </ol>
      </nav>

      <div className="grid gap-8 lg:grid-cols-[1fr_400px]">
        <div>
          {currentStep === 1 && (
            <div className="space-y-6">
              <h2 className="text-xl font-bold">Customer Information</h2>
              <Card>
                <CardContent className="p-6">
                  <div className="grid gap-4 sm:grid-cols-2">
                    <div>
                      <Label htmlFor="checkout-name">Full Name</Label>
                      <Input id="checkout-name" value={`${user?.firstName || ""} ${user?.lastName || ""}`} disabled />
                    </div>
                    <div>
                      <Label htmlFor="checkout-email">Email</Label>
                      <Input id="checkout-email" value={user?.email || ""} disabled />
                    </div>
                  </div>
                </CardContent>
              </Card>

              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <h3 className="font-semibold">Shipping Address</h3>
                  {addresses.length > 0 && (
                    <button type="button" onClick={() => setAddressDialogOpen(true)} className="text-sm text-primary hover:underline">
                      Add new
                    </button>
                  )}
                </div>

                {addressesLoading ? (
                  <div className="space-y-3">
                    {[...Array(2)].map((_, i) => (
                      <div key={i} className="h-24 animate-pulse rounded-lg bg-muted" />
                    ))}
                  </div>
                ) : addresses.length === 0 ? (
                  <Card>
                    <CardContent className="flex flex-col items-center justify-center p-8 text-center">
                      <MapPin className="mb-4 h-12 w-12 text-muted-foreground" />
                      <p className="font-medium">No addresses saved yet</p>
                      <p className="mt-1 text-sm text-muted-foreground">
                        Add a shipping address to continue with checkout.
                      </p>
                      <Button className="mt-4" onClick={() => setAddressDialogOpen(true)}>
                        <Plus className="mr-2 h-4 w-4" />
                        Add Address
                      </Button>
                    </CardContent>
                  </Card>
                ) : (
                  <RadioGroup
                    value={selectedAddressId}
                    onValueChange={setSelectedAddressId}
                    className="space-y-3"
                  >
                    {addresses.map(addr => (
                      <label
                        key={addr.id}
                        className={`flex cursor-pointer items-start gap-3 rounded-lg border p-4 transition-colors ${
                          selectedAddressId === addr.id
                            ? "border-primary bg-primary/5"
                            : "hover:border-muted-foreground/50"
                        }`}
                      >
                        <RadioGroupItem value={addr.id} />
                        <div className="flex-1">
                          <div className="flex items-center gap-2">
                            <span className="font-medium">{addr.fullName}</span>
                            {addr.isDefault && <Badge variant="secondary" className="text-xs">Default</Badge>}
                          </div>
                          <p className="mt-1 text-sm text-muted-foreground">
                            {addr.addressLine1}
                            {addr.addressLine2 && `, ${addr.addressLine2}`}
                          </p>
                          <p className="text-sm text-muted-foreground">
                            {addr.city}, {addr.state} {addr.postalCode}
                          </p>
                          <p className="text-sm text-muted-foreground">{addr.country}</p>
                          <p className="text-sm text-muted-foreground">Phone: {addr.phone}</p>
                        </div>
                      </label>
                    ))}
                  </RadioGroup>
                )}
              </div>

              <div className="flex justify-end">
                <Button size="lg" onClick={() => setCurrentStep(2)} disabled={!selectedAddressId}>
                  Continue to Shipping
                  <ChevronRight className="ml-2 h-4 w-4" />
                </Button>
              </div>
            </div>
          )}

          {currentStep === 2 && (
            <div className="space-y-6">
              <h2 className="text-xl font-bold">Shipping Method</h2>

              <RadioGroup value={shippingMethod} onValueChange={setShippingMethod} className="space-y-3">
                <label className={`flex cursor-pointer items-center justify-between rounded-lg border p-4 transition-colors ${
                  shippingMethod === "standard" ? "border-primary bg-primary/5" : "hover:border-muted-foreground/50"
                }`}>
                  <div className="flex items-center gap-3">
                    <RadioGroupItem value="standard" />
                    <div>
                      <p className="font-medium">Standard Shipping</p>
                      <p className="text-sm text-muted-foreground">3-7 business days</p>
                    </div>
                  </div>
                  <span className="font-medium">{totalPrice >= 2000 ? "Free" : formatPrice(150)}</span>
                </label>

                <label className={`flex cursor-pointer items-center justify-between rounded-lg border p-4 transition-colors ${
                  shippingMethod === "express" ? "border-primary bg-primary/5" : "hover:border-muted-foreground/50"
                }`}>
                  <div className="flex items-center gap-3">
                    <RadioGroupItem value="express" />
                    <div>
                      <p className="font-medium">Express Shipping</p>
                      <p className="text-sm text-muted-foreground">1-3 business days</p>
                    </div>
                  </div>
                  <span className="font-medium">{formatPrice(300)}</span>
                </label>
              </RadioGroup>

              <div className="flex justify-between">
                <Button variant="outline" onClick={() => setCurrentStep(1)}>
                  Back
                </Button>
                <Button size="lg" onClick={() => setCurrentStep(3)}>
                  Continue to Payment
                  <ChevronRight className="ml-2 h-4 w-4" />
                </Button>
              </div>
            </div>
          )}

          {currentStep === 3 && (
            <div className="space-y-6">
              <h2 className="text-xl font-bold">Payment Method</h2>

              <RadioGroup value={paymentMethod} onValueChange={setPaymentMethod} className="space-y-3">
                <label className={`flex cursor-pointer items-center gap-3 rounded-lg border p-4 transition-colors ${
                  paymentMethod === "razorpay" ? "border-primary bg-primary/5" : "hover:border-muted-foreground/50"
                }`}>
                  <RadioGroupItem value="razorpay" />
                  <CreditCard className="h-5 w-5" />
                  <div>
                    <p className="font-medium">Pay Online</p>
                    <p className="text-sm text-muted-foreground">UPI, Cards, Net Banking, Wallets</p>
                  </div>
                </label>

                <label className={`flex cursor-pointer items-center gap-3 rounded-lg border p-4 transition-colors ${
                  paymentMethod === "cod" ? "border-primary bg-primary/5" : "hover:border-muted-foreground/50"
                }`}>
                  <RadioGroupItem value="cod" />
                  <Wallet className="h-5 w-5" />
                  <div>
                    <p className="font-medium">Cash on Delivery</p>
                    <p className="text-sm text-muted-foreground">Pay when your order arrives</p>
                  </div>
                </label>
              </RadioGroup>

              <Card>
                <CardContent className="p-4">
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="text-sm font-medium text-muted-foreground">Shipping Address</p>
                      {selectedAddress && (
                        <p className="mt-1 text-sm">
                          {selectedAddress.fullName}, {selectedAddress.addressLine1},
                          {selectedAddress.city}, {selectedAddress.state} {selectedAddress.postalCode}
                        </p>
                      )}
                    </div>
                    <Button variant="ghost" size="sm" onClick={() => setCurrentStep(1)}>Edit</Button>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardContent className="p-4">
                  <p className="mb-3 text-sm font-medium text-muted-foreground">Items ({items.length})</p>
                  <div className="space-y-3">
                    {items.map(item => (
                      <div key={item.variantId} className="flex items-center gap-3">
                        <div className="relative h-14 w-14 shrink-0 overflow-hidden rounded-md bg-muted">
                          <Image
                            src={getSafeImageUrl(item.imageUrl)}
                            alt={item.name}
                            fill
                            className="object-cover"
                            sizes="56px"
                          />
                        </div>
                        <div className="flex-1">
                          <p className="text-sm font-medium">{item.name}</p>
                          <p className="text-xs text-muted-foreground">
                            {item.size && `Size: ${item.size}`}
                            {item.size && item.color && " | "}
                            {item.color && `Color: ${item.color}`}
                            {` | Qty: ${item.quantity}`}
                          </p>
                        </div>
                        <span className="text-sm font-medium">{formatPrice(item.price * item.quantity)}</span>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>

              <div className="flex justify-between">
                <Button variant="outline" onClick={() => setCurrentStep(2)}>
                  Back
                </Button>
                <Button
                  size="lg"
                  onClick={paymentMethod === "razorpay" ? handleRazorpayPayment : handleCodPayment}
                  disabled={isProcessing}
                >
                  {isProcessing ? (
                    "Processing..."
                  ) : paymentMethod === "razorpay" ? (
                    <>
                      <CreditCard className="mr-2 h-4 w-4" />
                      Pay {formatPrice(grandTotal)}
                    </>
                  ) : (
                    <>
                      <Wallet className="mr-2 h-4 w-4" />
                      Place Order - {formatPrice(grandTotal)}
                    </>
                  )}
                </Button>
              </div>
            </div>
          )}
        </div>

        <div className="lg:sticky lg:top-24 lg:self-start">
          <Card>
            <CardHeader>
              <CardTitle>Order Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Subtotal ({items.length} items)</span>
                  <span>{formatPrice(totalPrice)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Tax (18% GST)</span>
                  <span>{formatPrice(taxAmount)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Shipping</span>
                  <span>{shippingCost === 0 ? <span className="text-green-600">Free</span> : formatPrice(shippingCost)}</span>
                </div>
                {discountAmount > 0 && (
                  <div className="flex justify-between text-green-600">
                    <span>Discount</span>
                    <span>-{formatPrice(discountAmount)}</span>
                  </div>
                )}
              </div>
              <Separator />
              <div className="flex justify-between text-lg font-bold">
                <span>Total</span>
                <span>{formatPrice(grandTotal)}</span>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      <Dialog open={addressDialogOpen} onOpenChange={setAddressDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>{addresses.length === 0 ? "Add Shipping Address" : "Add New Address"}</DialogTitle>
          </DialogHeader>
          <AddressForm
            onSuccess={(address) => {
              setAddressDialogOpen(false);
              setSelectedAddressId(address.id);
            }}
            onCancel={() => setAddressDialogOpen(false)}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}
