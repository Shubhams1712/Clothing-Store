"use client";

import { useQuery } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import { formatPrice } from "@/lib/utils";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { IndianRupee, ShoppingCart, Package, Users, AlertTriangle } from "lucide-react";

export default function AdminDashboardPage() {
  const { data: stats, isLoading, error } = useQuery({
    queryKey: ["admin-dashboard"],
    queryFn: () => adminApi.dashboard.getStats(),
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3, 4, 5, 6].map((i) => (
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

  if (error) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <Card>
          <CardContent className="flex flex-col items-center justify-center p-12 text-center">
            <AlertTriangle className="h-12 w-12 text-destructive mb-4" />
            <h2 className="text-lg font-semibold">Failed to load dashboard</h2>
            <p className="text-sm text-muted-foreground mt-1">
              {error instanceof Error ? error.message : "An unexpected error occurred"}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!stats) return null;

  const statCards = [
    { title: "Total Revenue", value: formatPrice(stats.totalRevenue), icon: IndianRupee, color: "text-green-600" },
    { title: "Today's Orders", value: stats.todayOrders.toString(), icon: ShoppingCart, color: "text-blue-600" },
    { title: "Pending Orders", value: stats.pendingOrders.toString(), icon: AlertTriangle, color: "text-yellow-600" },
    { title: "Total Customers", value: stats.totalCustomers.toString(), icon: Users, color: "text-purple-600" },
    { title: "Total Products", value: stats.totalProducts.toString(), icon: Package, color: "text-orange-600" },
    { title: "Low Stock Items", value: stats.lowStockProducts.toString(), icon: AlertTriangle, color: "text-red-600" },
  ];

  const statusColors: Record<string, string> = {
    Pending: "bg-yellow-100 text-yellow-800",
    Confirmed: "bg-blue-100 text-blue-800",
    Packed: "bg-indigo-100 text-indigo-800",
    Shipped: "bg-purple-100 text-purple-800",
    Delivered: "bg-green-100 text-green-800",
    Cancelled: "bg-red-100 text-red-800",
    Refunded: "bg-gray-100 text-gray-800",
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Dashboard</h1>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
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
            <CardTitle>Recent Orders</CardTitle>
          </CardHeader>
          <CardContent>
            {stats.recentOrders.length === 0 ? (
              <p className="text-sm text-muted-foreground">No orders yet</p>
            ) : (
              <div className="space-y-3">
                {stats.recentOrders.map((order) => (
                  <div key={order.id} className="flex items-center justify-between border-b pb-2 last:border-0">
                    <div>
                      <p className="font-medium text-sm">{order.orderNumber}</p>
                      <p className="text-xs text-muted-foreground">{order.customerName}</p>
                    </div>
                    <div className="text-right">
                      <p className="font-medium text-sm">{formatPrice(order.totalAmount)}</p>
                      <Badge variant="outline" className={statusColors[order.status] || ""}>
                        {order.status}
                      </Badge>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Latest Customers</CardTitle>
          </CardHeader>
          <CardContent>
            {stats.latestCustomers.length === 0 ? (
              <p className="text-sm text-muted-foreground">No customers yet</p>
            ) : (
              <div className="space-y-3">
                {stats.latestCustomers.map((customer) => (
                  <div key={customer.id} className="flex items-center justify-between border-b pb-2 last:border-0">
                    <div>
                      <p className="font-medium text-sm">{customer.firstName} {customer.lastName}</p>
                      <p className="text-xs text-muted-foreground">{customer.email}</p>
                    </div>
                    <p className="text-xs text-muted-foreground">
                      {new Date(customer.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
