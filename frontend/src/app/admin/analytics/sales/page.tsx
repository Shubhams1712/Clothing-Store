"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import { formatPrice } from "@/lib/utils";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

const DATE_PRESETS = [
  { label: "Today", value: "today" },
  { label: "Last 7 Days", value: "last7days" },
  { label: "Last 30 Days", value: "last30days" },
  { label: "This Month", value: "thismonth" },
  { label: "Last Month", value: "lastmonth" },
];

export default function SalesAnalyticsPage() {
  const [datePreset, setDatePreset] = useState("last30days");

  const { data: sales, isLoading } = useQuery({
    queryKey: ["analytics-sales", datePreset],
    queryFn: () => adminApi.analytics.getSales({ preset: datePreset }),
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">Sales Analytics</h1>
          <Skeleton className="h-9 w-32" />
        </div>
        <div className="grid gap-4 md:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
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
        <h1 className="text-2xl font-bold">Sales Analytics</h1>
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

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Total Revenue</p>
            <p className="text-2xl font-bold">{formatPrice(sales?.totalRevenue || 0)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Avg Order Value</p>
            <p className="text-2xl font-bold">{formatPrice(sales?.averageOrderValue || 0)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Total Orders</p>
            <p className="text-2xl font-bold">{sales?.totalOrders || 0}</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Revenue by Payment Method</CardTitle>
          </CardHeader>
          <CardContent>
            {sales?.revenueByPaymentMethod && sales.revenueByPaymentMethod.length > 0 ? (
              <div className="space-y-3">
                {sales.revenueByPaymentMethod.map((item) => (
                  <div key={item.paymentMethod} className="flex items-center justify-between">
                    <span className="text-sm">{item.paymentMethod}</span>
                    <div className="text-right">
                      <span className="text-sm font-medium">{formatPrice(item.revenue)}</span>
                      <span className="ml-2 text-xs text-muted-foreground">({item.orderCount} orders)</span>
                    </div>
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
            <CardTitle>Revenue by Category</CardTitle>
          </CardHeader>
          <CardContent>
            {sales?.revenueByCategory && sales.revenueByCategory.length > 0 ? (
              <div className="space-y-3">
                {sales.revenueByCategory.map((item) => (
                  <div key={item.categoryName} className="flex items-center justify-between">
                    <span className="text-sm">{item.categoryName}</span>
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
            <CardTitle>Revenue by Collection</CardTitle>
          </CardHeader>
          <CardContent>
            {sales?.revenueByCollection && sales.revenueByCollection.length > 0 ? (
              <div className="space-y-3">
                {sales.revenueByCollection.map((item) => (
                  <div key={item.collectionName} className="flex items-center justify-between">
                    <span className="text-sm">{item.collectionName}</span>
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
            <CardTitle>Top Coupons</CardTitle>
          </CardHeader>
          <CardContent>
            {sales?.topCoupons && sales.topCoupons.length > 0 ? (
              <div className="space-y-3">
                {sales.topCoupons.map((item) => (
                  <div key={item.code} className="flex items-center justify-between">
                    <span className="font-mono text-sm">{item.code}</span>
                    <div className="text-right">
                      <span className="text-sm font-medium">{item.usedCount} uses</span>
                      <span className="ml-2 text-xs text-muted-foreground">
                        ({formatPrice(item.totalDiscount)})
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No data available</p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
