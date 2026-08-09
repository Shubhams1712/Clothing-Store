"use client";

import { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { useQuery } from "@tanstack/react-query";
import { Package, ArrowRight } from "lucide-react";
import { useAuth } from "@/hooks/use-auth";
import { orderService, type CustomerOrder } from "@/services/payment";
import { Button, buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { formatPrice, getSafeImageUrl } from "@/lib/utils";

const STATUS_STYLES: Record<string, string> = {
  PendingPayment: "border-l-yellow-400 bg-yellow-50/50",
  PaymentFailed: "border-l-red-400 bg-red-50/50",
  PaymentSuccessful: "border-l-blue-400 bg-blue-50/50",
  Confirmed: "border-l-blue-400 bg-blue-50/50",
  Packed: "border-l-indigo-400 bg-indigo-50/50",
  Shipped: "border-l-purple-400 bg-purple-50/50",
  OutForDelivery: "border-l-orange-400 bg-orange-50/50",
  Delivered: "border-l-green-400 bg-green-50/50",
  Cancelled: "border-l-gray-400 bg-gray-50/50",
  RefundRequested: "border-l-yellow-400 bg-yellow-50/50",
  Refunded: "border-l-green-400 bg-green-50/50",
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

export default function OrdersPage() {
  const { isAuthenticated } = useAuth();
  const [page, setPage] = useState(1);

  const { data: orders = [], isLoading } = useQuery({
    queryKey: ["orders", page],
    queryFn: () => orderService.getOrders(page),
    enabled: isAuthenticated,
  });

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center space-y-6 text-center">
          <Package className="h-12 w-12 text-muted-foreground" />
          <h1 className="text-2xl font-bold">Sign in to view orders</h1>
          <p className="text-muted-foreground">Please sign in to view your order history.</p>
          <Link href="/login" className={buttonVariants({ size: "lg" })}>
            Sign In
          </Link>
        </div>
      </div>
    );
  }

  if (isLoading) return <LoadingOverlay text="Loading orders..." />;

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold tracking-tight">My Orders</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Track and manage your recent purchases
        </p>
      </div>

      {orders.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed py-20 text-center">
          <Package className="mb-4 h-10 w-10 text-muted-foreground" />
          <h2 className="text-lg font-semibold">No orders yet</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            When you place an order, it will appear here.
          </p>
          <Link href="/shop" className={buttonVariants({ size: "sm", className: "mt-4" })}>
            Start Shopping
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <OrderCard key={order.id} order={order} />
          ))}
        </div>
      )}
    </div>
  );
}

function OrderCard({ order }: { order: CustomerOrder }) {
  const statusStyle = STATUS_STYLES[order.status] || "border-l-gray-300 bg-gray-50/50";
  const statusLabel = STATUS_LABELS[order.status] || order.status;

  return (
    <Link href={`/orders/${order.id}`} className="group block">
      <div className={`rounded-lg border border-l-4 p-5 transition-all hover:shadow-sm ${statusStyle}`}>
        <div className="flex items-start justify-between gap-4">
          <div className="min-w-0 flex-1">
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-sm font-semibold">{order.orderNumber}</span>
              <Badge variant="secondary" className="text-xs font-normal">
                {statusLabel}
              </Badge>
            </div>
            <p className="mt-1 text-xs text-muted-foreground">
              {new Date(order.createdAt).toLocaleDateString("en-IN", {
                day: "numeric",
                month: "short",
                year: "numeric",
              })}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <div className="text-right">
              <p className="text-sm font-semibold">{formatPrice(order.totalAmount)}</p>
              <p className="text-xs text-muted-foreground">
                {order.items.length} item{order.items.length !== 1 ? "s" : ""}
              </p>
            </div>
            <ArrowRight className="h-4 w-4 text-muted-foreground transition-transform group-hover:translate-x-0.5" />
          </div>
        </div>

        <div className="mt-4 flex items-center gap-2">
          {order.items.slice(0, 4).map((item, idx) => (
            <div
              key={idx}
              className="relative h-12 w-12 shrink-0 overflow-hidden rounded-md border bg-muted"
            >
              <Image
                src={getSafeImageUrl(item.imageUrl || "")}
                alt={item.productName}
                fill
                className="object-cover"
                sizes="48px"
              />
            </div>
          ))}
          {order.items.length > 4 && (
            <span className="text-xs text-muted-foreground">
              +{order.items.length - 4} more
            </span>
          )}
        </div>
      </div>
    </Link>
  );
}
