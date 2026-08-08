"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import type { Customer } from "@/types/admin";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { Search, Eye, UserX, UserCheck } from "lucide-react";
import { toast } from "sonner";
import { formatPrice } from "@/lib/utils";

export default function AdminCustomersPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [detailCustomer, setDetailCustomer] = useState<Customer | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["admin-customers", page, search],
    queryFn: () => adminApi.customers.list({ page, pageSize: 10, search }),
  });

  const toggleMutation = useMutation({
    mutationFn: (id: string) => adminApi.customers.toggleActive(id),
    onSuccess: () => { toast.success("Customer status updated"); queryClient.invalidateQueries({ queryKey: ["admin-customers"] }); },
    onError: () => { toast.error("Failed"); },
  });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Customers</h1>
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center gap-4 mb-4">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input placeholder="Search customers..." value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} className="pl-9" />
            </div>
          </div>
          {isLoading ? <div className="space-y-2">{[1, 2, 3].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div> : (
            <>
              <Table>
                <TableHeader><TableRow><TableHead>Name</TableHead><TableHead>Email</TableHead><TableHead>Orders</TableHead><TableHead>Spent</TableHead><TableHead>Status</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
                <TableBody>
                  {data?.items.map(c => (
                    <TableRow key={c.id}>
                      <TableCell className="font-medium">{c.firstName} {c.lastName}</TableCell>
                      <TableCell>{c.email}</TableCell>
                      <TableCell>{c.orderCount}</TableCell>
                      <TableCell>{formatPrice(c.totalSpent)}</TableCell>
                      <TableCell><Badge variant={c.isActive ? "default" : "destructive"}>{c.isActive ? "Active" : "Disabled"}</Badge></TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-1">
                          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => setDetailCustomer(c)}><Eye className="h-4 w-4" /></Button>
                          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => toggleMutation.mutate(c.id)}>
                            {c.isActive ? <UserX className="h-4 w-4" /> : <UserCheck className="h-4 w-4" />}
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                  {data?.items.length === 0 && <TableRow><TableCell colSpan={6} className="text-center text-muted-foreground py-8">No customers found</TableCell></TableRow>}
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

      <Dialog open={!!detailCustomer} onOpenChange={() => setDetailCustomer(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Customer Details</DialogTitle></DialogHeader>
          {detailCustomer && (
            <div className="space-y-3">
              <div><p className="text-sm text-muted-foreground">Name</p><p className="font-medium">{detailCustomer.firstName} {detailCustomer.lastName}</p></div>
              <div><p className="text-sm text-muted-foreground">Email</p><p>{detailCustomer.email}</p></div>
              <div><p className="text-sm text-muted-foreground">Orders</p><p>{detailCustomer.orderCount}</p></div>
              <div><p className="text-sm text-muted-foreground">Total Spent</p><p>{formatPrice(detailCustomer.totalSpent)}</p></div>
              <div><p className="text-sm text-muted-foreground">Joined</p><p>{new Date(detailCustomer.createdAt).toLocaleDateString()}</p></div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
