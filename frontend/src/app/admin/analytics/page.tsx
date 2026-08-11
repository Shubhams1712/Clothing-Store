"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import { formatPrice } from "@/lib/utils";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
  IndianRupee,
  ShoppingCart,
  Users,
  TrendingUp,
  AlertTriangle,
  BarChart3,
} from "lucide-react";

const DATE_PRESETS = [
  { label: "Today", value: "today" },
  { label: "Last 7 Days", value: "last7days" },
  { label: "Last 30 Days", value: "last30days" },
  { label: "This Month", value: "thismonth" },
  { label: "Last Month", value: "lastmonth" },
];

export default function AnalyticsPage() {
  const [datePreset, setDatePreset] = useState("last30days");

  const { data: dashboard, isLoading } = useQuery({
    queryKey: ["analytics-dashboard", datePreset],
    queryFn: () => adminApi.analytics.getDashboard({ preset: datePreset }),
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">Analytics</h1>
          <Skeleton className="h-9 w-32" />
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
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

  const statCards = [
    { title: "Total Revenue", value: formatPrice(dashboard?.totalRevenue || 0), icon: IndianRupee, color: "text-green-600" },
    { title: "Total Orders", value: (dashboard?.totalOrders || 0).toString(), icon: ShoppingCart, color: "text-blue-600" },
    { title: "Avg Order Value", value: formatPrice(dashboard?.averageOrderValue || 0), icon: TrendingUp, color: "text-purple-600" },
    { title: "New Customers", value: (dashboard?.newCustomers || 0).toString(), icon: Users, color: "text-orange-600" },
    { title: "Pending Orders", value: (dashboard?.pendingOrders || 0).toString(), icon: AlertTriangle, color: "text-yellow-600" },
    { title: "Low Stock", value: (dashboard?.lowStockProducts || 0).toString(), icon: AlertTriangle, color: "text-red-600" },
    { title: "Conversion Rate", value: `${(dashboard?.conversionRate || 0).toFixed(1)}%`, icon: BarChart3, color: "text-indigo-600" },
    { title: "Refunds", value: (dashboard?.refunds || 0).toString(), icon: IndianRupee, color: "text-gray-600" },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Analytics</h1>
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

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {statCards.map((card) => (
          <Card key={card.title}>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-muted-foreground">{card.title}</p>
                  <p className="text-2xl font-bold">{card.value}</p>
                </div>
                <card.icon className={`h-8 w-8 ${card.color}`} />
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Quick Links</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-3">
              {[
                { label: "Sales Analytics", href: "/admin/analytics/sales" },
                { label: "Product Analytics", href: "/admin/analytics/products" },
                { label: "Customer Analytics", href: "/admin/analytics/customers" },
                { label: "Inventory Analytics", href: "/admin/analytics/inventory" },
                { label: "Order Analytics", href: "/admin/analytics/orders" },
                { label: "Reports", href: "/admin/analytics/reports" },
              ].map((link) => (
                <Link
                  key={link.href}
                  href={link.href}
                  className="rounded-lg border p-3 text-sm font-medium transition-colors hover:bg-muted"
                >
                  {link.label}
                </Link>
              ))}
            </div>
          </CardContent>
        </Card>

        {dashboard?.revenueOverTime && dashboard.revenueOverTime.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle>Revenue Trend</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                {dashboard.revenueOverTime.slice(-7).map((point) => (
                  <div key={point.date} className="flex items-center justify-between text-sm">
                    <span className="text-muted-foreground">
                      {new Date(point.date).toLocaleDateString("en-IN", { day: "numeric", month: "short" })}
                    </span>
                    <span className="font-medium">{formatPrice(point.revenue)}</span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
