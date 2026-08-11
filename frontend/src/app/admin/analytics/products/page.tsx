"use client";

import { useState } from "react";
import Image from "next/image";
import { useQuery } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import { formatPrice, getSafeImageUrl } from "@/lib/utils";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";

const DATE_PRESETS = [
  { label: "Today", value: "today" },
  { label: "Last 7 Days", value: "last7days" },
  { label: "Last 30 Days", value: "last30days" },
  { label: "This Month", value: "thismonth" },
  { label: "Last Month", value: "lastmonth" },
];

export default function ProductAnalyticsPage() {
  const [datePreset, setDatePreset] = useState("last30days");

  const { data: products, isLoading } = useQuery({
    queryKey: ["analytics-products", datePreset],
    queryFn: () => adminApi.analytics.getProducts({ preset: datePreset }),
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">Product Analytics</h1>
          <Skeleton className="h-9 w-32" />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <Card key={i}>
              <CardContent className="p-6">
                <Skeleton className="h-4 w-24 mb-2" />
                <Skeleton className="h-8 w-32" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Product Analytics</h1>
        <div className="flex gap-2">
          {DATE_PRESETS.map((preset) => (
            <button
              key={preset.value}
              onClick={() => setDatePreset(preset.value)}
              className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                datePreset === preset.value
                  ? "bg-primary text-primary-foreground"
                  : "bg-muted text-muted-foreground hover:text-foreground"
              }`}
            >
              {preset.label}
            </button>
          ))}
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Best Selling Products</CardTitle>
          </CardHeader>
          <CardContent>
            {products?.bestSelling && products.bestSelling.length > 0 ? (
              <div className="space-y-3">
                {products.bestSelling.map((item) => (
                  <div key={item.productId} className="flex items-center gap-3">
                    <div className="relative h-10 w-10 shrink-0 overflow-hidden rounded-md border bg-muted">
                      <Image
                        src={getSafeImageUrl(item.imageUrl || "")}
                        alt={item.productName}
                        fill
                        className="object-cover"
                        sizes="40px"
                      />
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium">{item.productName}</p>
                      <p className="text-xs text-muted-foreground">{item.totalSold} sold</p>
                    </div>
                    <span className="text-sm font-medium">{formatPrice(item.revenue)}</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No data available</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Worst Performing Products</CardTitle>
          </CardHeader>
          <CardContent>
            {products?.worstPerforming && products.worstPerforming.length > 0 ? (
              <div className="space-y-3">
                {products.worstPerforming.map((item) => (
                  <div key={item.productId} className="flex items-center gap-3">
                    <div className="relative h-10 w-10 shrink-0 overflow-hidden rounded-md border bg-muted">
                      <Image
                        src={getSafeImageUrl(item.imageUrl || "")}
                        alt={item.productName}
                        fill
                        className="object-cover"
                        sizes="40px"
                      />
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium">{item.productName}</p>
                      <p className="text-xs text-muted-foreground">{item.totalSold} sold</p>
                    </div>
                    <span className="text-sm font-medium">{formatPrice(item.revenue)}</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No data available</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Low Stock Products</CardTitle>
          </CardHeader>
          <CardContent>
            {products?.lowStock && products.lowStock.length > 0 ? (
              <div className="space-y-3">
                {products.lowStock.map((item) => (
                  <div key={item.productId} className="flex items-center justify-between">
                    <span className="text-sm">{item.productName}</span>
                    <Badge variant="outline" className="bg-yellow-50 text-yellow-700">
                      {item.totalStock} in stock
                    </Badge>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No low stock products</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Out of Stock Products</CardTitle>
          </CardHeader>
          <CardContent>
            {products?.outOfStock && products.outOfStock.length > 0 ? (
              <div className="space-y-3">
                {products.outOfStock.map((item) => (
                  <div key={item.productId} className="flex items-center justify-between">
                    <span className="text-sm">{item.productName}</span>
                    <Badge variant="destructive">Out of stock</Badge>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No out of stock products</p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
