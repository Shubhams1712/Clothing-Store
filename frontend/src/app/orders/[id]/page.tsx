"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Package, Truck, CheckCircle, Clock, XCircle, RotateCcw } from "lucide-react";
import { useAuth } from "@/hooks/use-auth";
import { orderService, type CustomerOrder, type OrderTracking } from "@/services/payment";
import { Button, buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { formatPrice, getSafeImageUrl } from "@/lib/utils";
import { toast } from "sonner";

const STATUS_STYLES: Record<string, string> = {
  PendingPayment: "bg-yellow-100 text-yellow-800",
  PaymentFailed: "bg-red-100 text-red-800",
  PaymentSuccessful: "bg-blue-100 text-blue-800",
  Confirmed: "bg-blue-100 text-blue-800",
  Packed: "bg-indigo-100 text-indigo-800",
  Shipped: "bg-purple-100 text-purple-800",
  OutForDelivery: "bg-orange-100 text-orange-800",
  Delivered: "bg-green-100 text-green-800",
  Cancelled: "bg-gray-100 text-gray-800",
  RefundRequested: "bg-yellow-100 text-yellow-800",
  Refunded: "bg-green-100 text-green-800",
};

const STATUS_LABELS: Record<string, string> = {
  PendingPayment: "Pending Payment",
  PaymentFailed: "Payment Failed",
  PaymentSuccessful: "Confirmed",
  Confirmed: "Confirmed",
  Packed: "Packed",
  Shipped: "Shipped",
  OutForDelivery: "Out for Delivery",
  Delivered: "Delivered",
  Cancelled: "Cancelled",
  RefundRequested: "Refund Requested",
  Refunded: "Refunded",
};

const TIMELINE_ICONS: Record<string, typeof Clock> = {
  "Order Placed": Package,
  "Payment Confirmed": CheckCircle,
  "Order Confirmed": CheckCircle,
  "Packed": Package,
  "Shipped": Truck,
  "Out for Delivery": Truck,
  "Delivered": CheckCircle,
  "Cancelled": XCircle,
  "Refund Requested": RotateCcw,
  "Refunded": CheckCircle,
};

export default function OrderDetailPage() {
  const params = useParams();
  const router = useRouter();
  const queryClient = useQueryClient();
  const orderId = params.id as string;
  const { isAuthenticated } = useAuth();
  const [showCancelConfirm, setShowCancelConfirm] = useState(false);

  const { data: order, isLoading } = useQuery({
    queryKey: ["order", orderId],
    queryFn: () => orderService.getOrderById(orderId),
    enabled: isAuthenticated && !!orderId,
  });

  const { data: tracking } = useQuery({
    queryKey: ["order-tracking", orderId],
    queryFn: () => orderService.getOrderTracking(orderId),
    enabled: isAuthenticated && !!orderId,
  });

  const cancelMutation = useMutation({
    mutationFn: () => orderService.cancelOrder(orderId),
    onSuccess: () => {
      toast.success("Order cancelled");
      queryClient.invalidateQueries({ queryKey: ["order", orderId] });
      queryClient.invalidateQueries({ queryKey: ["orders"] });
      setShowCancelConfirm(false);
    },
    onError: () => {
      toast.error("Failed to cancel order");
    },
  });

  const refundMutation = useMutation({
    mutationFn: () => orderService.requestRefund(orderId),
    onSuccess: () => {
      toast.success("Refund request submitted");
      queryClient.invalidateQueries({ queryKey: ["order", orderId] });
    },
    onError: () => {
      toast.error("Failed to request refund");
    },
  });

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center space-y-6 text-center">
          <h1 className="text-2xl font-bold">Sign in to view order</h1>
          <Link href="/login" className={buttonVariants({ size: "lg" })}>
            Sign In
          </Link>
        </div>
      </div>
    );
  }

  if (isLoading) return <LoadingOverlay text="Loading order details..." />;
  if (!order) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center space-y-6 text-center">
          <h1 className="text-2xl font-bold">Order not found</h1>
          <Link href="/orders" className={buttonVariants({ size: "lg" })}>
            View All Orders
          </Link>
        </div>
      </div>
    );
  }

  const canCancel = ["PendingPayment", "PaymentSuccessful", "Confirmed"].includes(order.status);
  const canRefund = order.status === "Delivered";

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
      <button
        onClick={() => router.back()}
        className="mb-6 flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="h-4 w-4" />
        Back
      </button>

      <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Order {order.orderNumber}</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Placed on {new Date(order.createdAt).toLocaleDateString("en-IN", {
              day: "numeric",
              month: "long",
              year: "numeric",
              hour: "2-digit",
              minute: "2-digit",
            })}
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Badge className={STATUS_STYLES[order.status] || "bg-gray-100 text-gray-800"}>
            {STATUS_LABELS[order.status] || order.status}
          </Badge>
          {canCancel && (
            <Button variant="outline" size="sm" onClick={() => setShowCancelConfirm(true)}>
              Cancel Order
            </Button>
          )}
          {canRefund && (
            <Button variant="outline" size="sm" onClick={() => refundMutation.mutate()}>
              Request Refund
            </Button>
          )}
        </div>
      </div>

      {showCancelConfirm && (
        <div className="mb-6 rounded-lg border border-destructive/50 bg-destructive/5 p-4">
          <p className="mb-3 text-sm font-medium">Cancel this order?</p>
          <p className="mb-4 text-sm text-muted-foreground">
            This action cannot be undone. Your order will be cancelled immediately.
          </p>
          <div className="flex gap-3">
            <Button variant="destructive" size="sm" onClick={() => cancelMutation.mutate()} disabled={cancelMutation.isPending}>
              {cancelMutation.isPending ? "Cancelling..." : "Yes, Cancel"}
            </Button>
            <Button variant="outline" size="sm" onClick={() => setShowCancelConfirm(false)}>
              Keep Order
            </Button>
          </div>
        </div>
      )}

      <div className="grid gap-8 lg:grid-cols-[1fr_320px]">
        <div className="space-y-6">
          <div className="rounded-lg border bg-card p-5">
            <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
              Items
            </h2>
            <div className="space-y-4">
              {order.items.map((item, idx) => (
                <div key={idx} className="flex items-center gap-4">
                  <div className="relative h-16 w-16 shrink-0 overflow-hidden rounded-md border bg-muted">
                    <Image
                      src={getSafeImageUrl(item.imageUrl || "")}
                      alt={item.productName}
                      fill
                      className="object-cover"
                      sizes="64px"
                    />
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium">{item.productName}</p>
                    <p className="text-xs text-muted-foreground">
                      {[item.size && `Size ${item.size}`, item.color && item.color]
                        .filter(Boolean)
                        .join(" \u00B7 ") || `Qty: ${item.quantity}`}
                    </p>
                  </div>
                  <span className="text-sm font-medium">{formatPrice(item.totalPrice)}</span>
                </div>
              ))}
            </div>
          </div>

          {tracking && (
            <div className="rounded-lg border bg-card p-5">
              <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
                Tracking
              </h2>
              <div className="relative space-y-4">
                <div className="absolute left-[11px] top-2 bottom-2 w-px bg-border" />
                {tracking.timeline.map((event, idx) => {
                  const Icon = TIMELINE_ICONS[event.status] || Clock;
                  const isLatest = idx === tracking.timeline.length - 1;
                  return (
                    <div key={idx} className="relative flex gap-3">
                      <div className={`relative z-10 flex h-6 w-6 items-center justify-center rounded-full ${
                        isLatest ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground"
                      }`}>
                        <Icon className="h-3 w-3" />
                      </div>
                      <div className="flex-1 pt-0.5">
                        <p className={`text-sm ${isLatest ? "font-medium" : "text-muted-foreground"}`}>
                          {event.status}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {new Date(event.timestamp).toLocaleDateString("en-IN", {
                            day: "numeric",
                            month: "short",
                            hour: "2-digit",
                            minute: "2-digit",
                          })}
                        </p>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>

        <div className="space-y-4 lg:sticky lg:top-24 lg:self-start">
          <div className="rounded-lg border bg-card p-5">
            <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
              Summary
            </h2>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Subtotal</span>
                <span>{formatPrice(order.subTotal)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Tax</span>
                <span>{formatPrice(order.taxAmount)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Shipping</span>
                <span>{order.shippingAmount === 0 ? "Free" : formatPrice(order.shippingAmount)}</span>
              </div>
              {order.discountAmount > 0 && (
                <div className="flex justify-between text-green-600">
                  <span>Discount</span>
                  <span>-{formatPrice(order.discountAmount)}</span>
                </div>
              )}
              <Separator className="my-2" />
              <div className="flex justify-between font-semibold">
                <span>Total</span>
                <span>{formatPrice(order.totalAmount)}</span>
              </div>
            </div>
          </div>

          <div className="rounded-lg border bg-card p-5">
            <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
              Shipping
            </h2>
            <div className="space-y-1 text-sm">
              <p className="font-medium">{order.shippingName}</p>
              <p className="text-muted-foreground">{order.shippingAddress}</p>
              <p className="text-muted-foreground">
                {order.shippingCity}, {order.shippingState} {order.shippingPostalCode}
              </p>
              <p className="text-muted-foreground">{order.shippingCountry}</p>
              {order.shippingPhone && (
                <p className="pt-1 text-muted-foreground">Phone: {order.shippingPhone}</p>
              )}
            </div>
          </div>

          <div className="rounded-lg border bg-card p-5">
            <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
              Payment
            </h2>
            <div className="space-y-1 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Method</span>
                <span>{order.paymentMethod || "N/A"}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Status</span>
                <span>{order.paymentStatus || "N/A"}</span>
              </div>
              {order.paymentId && (
                <div className="flex justify-between">
                  <span className="text-muted-foreground">ID</span>
                  <span className="max-w-[140px] truncate font-mono text-xs">{order.paymentId}</span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
