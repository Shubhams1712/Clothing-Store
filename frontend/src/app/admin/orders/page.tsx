"use client";

import { useEffect, useState, useCallback } from "react";
import { adminApi } from "@/services/admin";
import type { Order, PaginatedResponse, OrderStatus } from "@/types/admin";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Search, Eye } from "lucide-react";
import { toast } from "sonner";

const ORDER_STATUSES: OrderStatus[] = ["Pending", "Confirmed", "Packed", "Shipped", "Delivered", "Cancelled", "Refunded"];
const STATUS_COLORS: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-800", Confirmed: "bg-blue-100 text-blue-800", Packed: "bg-indigo-100 text-indigo-800",
  Shipped: "bg-purple-100 text-purple-800", Delivered: "bg-green-100 text-green-800", Cancelled: "bg-red-100 text-red-800", Refunded: "bg-gray-100 text-gray-800",
};

export default function AdminOrdersPage() {
  const [data, setData] = useState<PaginatedResponse<Order> | null>(null);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [detailOrder, setDetailOrder] = useState<Order | null>(null);
  const [newStatus, setNewStatus] = useState<OrderStatus>("Pending");
  const [updating, setUpdating] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try { setData(await adminApi.orders.list({ page, pageSize: 10, search })); } finally { setLoading(false); }
  }, [page, search]);

  useEffect(() => { load(); }, [load]);

  const handleStatusUpdate = async () => {
    if (!detailOrder) return;
    setUpdating(true);
    try { await adminApi.orders.updateStatus(detailOrder.id, { status: newStatus }); toast.success("Status updated"); setDetailOrder(null); load(); } catch { toast.error("Failed"); } finally { setUpdating(false); }
  };

  const openDetail = (order: Order) => { setDetailOrder(order); setNewStatus(order.status); };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Orders</h1>
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center gap-4 mb-4">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input placeholder="Search orders..." value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} className="pl-9" />
            </div>
          </div>
          {loading ? <div className="space-y-2">{[1, 2, 3].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div> : (
            <>
              <Table>
                <TableHeader><TableRow><TableHead>Order #</TableHead><TableHead>Customer</TableHead><TableHead>Total</TableHead><TableHead>Status</TableHead><TableHead>Date</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
                <TableBody>
                  {data?.items.map(order => (
                    <TableRow key={order.id}>
                      <TableCell className="font-medium">{order.orderNumber}</TableCell>
                      <TableCell>{order.customerName}</TableCell>
                      <TableCell>₹{order.totalAmount.toLocaleString()}</TableCell>
                      <TableCell><Badge className={STATUS_COLORS[order.status]}>{order.status}</Badge></TableCell>
                      <TableCell>{new Date(order.createdAt).toLocaleDateString()}</TableCell>
                      <TableCell className="text-right">
                        <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => openDetail(order)}><Eye className="h-4 w-4" /></Button>
                      </TableCell>
                    </TableRow>
                  ))}
                  {data?.items.length === 0 && <TableRow><TableCell colSpan={6} className="text-center text-muted-foreground py-8">No orders found</TableCell></TableRow>}
                </TableBody>
              </Table>
              {data && data.totalPages > 1 && (
                <div className="flex justify-between mt-4">
                  <p className="text-sm text-muted-foreground">Page {page} of {data.totalPages}</p>
                  <div className="flex gap-2">
                    <Button variant="outline" size="sm" disabled={page === 1} onClick={() => setPage(page - 1)}>Previous</Button>
                    <Button variant="outline" size="sm" disabled={page >= data.totalPages} onClick={() => setPage(page + 1)}>Next</Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <Dialog open={!!detailOrder} onOpenChange={() => setDetailOrder(null)}>
        <DialogContent className="max-w-2xl">
          <DialogHeader><DialogTitle>Order {detailOrder?.orderNumber}</DialogTitle></DialogHeader>
          {detailOrder && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div><p className="text-sm text-muted-foreground">Customer</p><p className="font-medium">{detailOrder.customerName}</p><p className="text-sm text-muted-foreground">{detailOrder.customerEmail}</p></div>
                <div><p className="text-sm text-muted-foreground">Total</p><p className="font-medium text-lg">₹{detailOrder.totalAmount.toLocaleString()}</p></div>
              </div>
              <div>
                <p className="text-sm text-muted-foreground mb-2">Items</p>
                {detailOrder.items.map(item => (
                  <div key={item.id} className="flex justify-between border-b py-1 text-sm">
                    <span>{item.productName} x {item.quantity}</span>
                    <span>₹{item.totalPrice.toLocaleString()}</span>
                  </div>
                ))}
              </div>
              <div className="flex items-center gap-4">
                <label className="text-sm font-medium">Update Status:</label>
                <Select value={newStatus} onValueChange={v => setNewStatus(v as OrderStatus)}>
                  <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {ORDER_STATUSES.map(s => <SelectItem key={s} value={s}>{s}</SelectItem>)}
                  </SelectContent>
                </Select>
                <Button size="sm" onClick={handleStatusUpdate} disabled={updating || newStatus === detailOrder.status}>
                  {updating ? "Updating..." : "Update"}
                </Button>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
