"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { fulfillmentService } from "@/services/fulfillment";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Truck, ExternalLink, RefreshCw } from "lucide-react";

const STATUS_COLORS: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-800",
  Submitted: "bg-blue-100 text-blue-800",
  Processing: "bg-indigo-100 text-indigo-800",
  Shipped: "bg-purple-100 text-purple-800",
  Delivered: "bg-green-100 text-green-800",
  Failed: "bg-red-100 text-red-800",
  Cancelled: "bg-gray-100 text-gray-800",
};

export default function FulfillmentPage() {
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ["fulfillment-orders", page],
    queryFn: () => fulfillmentService.listOrders({ page, pageSize: 20 }),
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">Fulfillment</h1>
          <Skeleton className="h-9 w-32" />
        </div>
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-20 w-full rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Fulfillment</h1>
        <Link href="/admin/fulfillment/mappings">
          <Button variant="outline">
            <Truck className="mr-2 h-4 w-4" />
            Mappings
          </Button>
        </Link>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Fulfillment Orders</CardTitle>
        </CardHeader>
        <CardContent>
          {data && data.items.length > 0 ? (
            <div className="space-y-3">
              {data.items.map((fo) => (
                <div
                  key={fo.id}
                  className="flex items-center justify-between rounded-lg border p-4"
                >
                  <div className="space-y-1">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-medium">{fo.externalOrderId || fo.id}</span>
                      <Badge className={STATUS_COLORS[fo.status] || "bg-gray-100 text-gray-800"}>
                        {fo.status}
                      </Badge>
                    </div>
                    <p className="text-xs text-muted-foreground">
                      Order: {fo.orderId} {fo.providerName && `\u00B7 ${fo.providerName}`}
                    </p>
                    {fo.failureReason && (
                      <p className="text-xs text-red-600">{fo.failureReason}</p>
                    )}
                  </div>
                  <div className="flex items-center gap-2">
                    {fo.externalOrderId && (
                      <a
                        href={`https://qikink.com/orders/${fo.externalOrderId}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center justify-center rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 hover:bg-accent hover:text-accent-foreground h-9 px-3"
                      >
                        <ExternalLink className="h-4 w-4" />
                      </a>
                    )}
                    {fo.status === "Failed" && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => fulfillmentService.retry(fo.id)}
                      >
                        <RefreshCw className="mr-1 h-3 w-3" />
                        Retry
                      </Button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">No fulfillment orders</p>
          )}
        </CardContent>
      </Card>

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={!data.hasPrevious}
          >
            Previous
          </Button>
          <span className="text-sm text-muted-foreground">
            Page {data.page} of {data.totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((p) => p + 1)}
            disabled={!data.hasNext}
          >
            Next
          </Button>
        </div>
      )}
    </div>
  );
}
