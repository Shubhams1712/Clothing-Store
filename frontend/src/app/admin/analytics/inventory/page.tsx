"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import { formatPrice } from "@/lib/utils";
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

export default function InventoryAnalyticsPage() {
  const [datePreset, setDatePreset] = useState("last30days");

  const { data: inventory, isLoading } = useQuery({
    queryKey: ["analytics-inventory", datePreset],
    queryFn: () => adminApi.analytics.getInventory({ preset: datePreset }),
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">Inventory Analytics</h1>
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
        <h1 className="text-2xl font-bold">Inventory Analytics</h1>
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
            <p className="text-sm text-muted-foreground">Inventory Value</p>
            <p className="text-2xl font-bold">{formatPrice(inventory?.totalInventoryValue || 0)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Total Products</p>
            <p className="text-2xl font-bold">{inventory?.totalProducts || 0}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Total Variants</p>
            <p className="text-2xl font-bold">{inventory?.totalVariants || 0}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Low Stock</p>
            <p className="text-2xl font-bold text-yellow-600">{inventory?.lowStockCount || 0}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Out of Stock</p>
            <p className="text-2xl font-bold text-red-600">{inventory?.outOfStockCount || 0}</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Fast Moving Products</CardTitle>
          </CardHeader>
          <CardContent>
            {inventory?.fastMoving && inventory.fastMoving.length > 0 ? (
              <div className="space-y-3">
                {inventory.fastMoving.map((item) => (
                  <div key={item.productId} className="flex items-center justify-between">
                    <span className="text-sm">{item.productName}</span>
                    <div className="text-right">
                      <span className="text-sm font-medium">{item.totalSold} sold</span>
                      <span className="ml-2 text-xs text-muted-foreground">({item.currentStock} left)</span>
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
            <CardTitle>Slow Moving Products</CardTitle>
          </CardHeader>
          <CardContent>
            {inventory?.slowMoving && inventory.slowMoving.length > 0 ? (
              <div className="space-y-3">
                {inventory.slowMoving.map((item) => (
                  <div key={item.productId} className="flex items-center justify-between">
                    <span className="text-sm">{item.productName}</span>
                    <div className="text-right">
                      <span className="text-sm font-medium">{item.totalSold} sold</span>
                      <span className="ml-2 text-xs text-muted-foreground">({item.currentStock} in stock)</span>
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
            <CardTitle>Stock Alerts</CardTitle>
          </CardHeader>
          <CardContent>
            {inventory?.stockAlerts && inventory.stockAlerts.length > 0 ? (
              <div className="space-y-3">
                {inventory.stockAlerts.map((item) => (
                  <div key={item.variantId} className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium">{item.productName}</p>
                      <p className="text-xs text-muted-foreground">
                        {item.sku} {item.size && `\u00B7 ${item.size}`} {item.color && `\u00B7 ${item.color}`}
                      </p>
                    </div>
                    <Badge variant={item.stock === 0 ? "destructive" : "outline"}>
                      {item.stock} left
                    </Badge>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No stock alerts</p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
