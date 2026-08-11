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

export default function CustomerAnalyticsPage() {
  const [datePreset, setDatePreset] = useState("last30days");

  const { data: customers, isLoading } = useQuery({
    queryKey: ["analytics-customers", datePreset],
    queryFn: () => adminApi.analytics.getCustomers({ preset: datePreset }),
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">Customer Analytics</h1>
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
        <h1 className="text-2xl font-bold">Customer Analytics</h1>
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
            <p className="text-sm text-muted-foreground">Total Customers</p>
            <p className="text-2xl font-bold">{customers?.totalCustomers || 0}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">New Customers</p>
            <p className="text-2xl font-bold">{customers?.newCustomers || 0}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Returning Customers</p>
            <p className="text-2xl font-bold">{customers?.returningCustomers || 0}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Repeat Purchase Rate</p>
            <p className="text-2xl font-bold">{(customers?.repeatPurchaseRate || 0).toFixed(1)}%</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">Avg Lifetime Value</p>
            <p className="text-2xl font-bold">{formatPrice(customers?.averageLifetimeValue || 0)}</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Top Customers</CardTitle>
          </CardHeader>
          <CardContent>
            {customers?.topCustomers && customers.topCustomers.length > 0 ? (
              <div className="space-y-3">
                {customers.topCustomers.map((item) => (
                  <div key={item.customerId} className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium">{item.customerName}</p>
                      <p className="text-xs text-muted-foreground">{item.email}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium">{formatPrice(item.totalSpent)}</p>
                      <p className="text-xs text-muted-foreground">{item.orderCount} orders</p>
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
            <CardTitle>Customer Growth</CardTitle>
          </CardHeader>
          <CardContent>
            {customers?.customerGrowth && customers.customerGrowth.length > 0 ? (
              <div className="space-y-2">
                {customers.customerGrowth.slice(-7).map((point) => (
                  <div key={point.date} className="flex items-center justify-between text-sm">
                    <span className="text-muted-foreground">
                      {new Date(point.date).toLocaleDateString("en-IN", { day: "numeric", month: "short" })}
                    </span>
                    <span className="font-medium">+{point.newCustomers} ({point.totalCustomers} total)</span>
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
