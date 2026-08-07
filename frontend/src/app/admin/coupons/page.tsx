"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import type { Coupon } from "@/types/admin";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Plus, Search, Trash2, Edit } from "lucide-react";
import { toast } from "sonner";

export default function AdminCouponsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [editItem, setEditItem] = useState<Coupon | null>(null);
  const [form, setForm] = useState({ code: "", description: "", type: "Percentage" as "Percentage" | "FixedAmount", value: 0, minimumOrderAmount: 0, usageLimit: 0 });

  const { data, isLoading } = useQuery({
    queryKey: ["admin-coupons", page, search],
    queryFn: () => adminApi.coupons.list({ page, pageSize: 10, search }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => adminApi.coupons.delete(id),
    onSuccess: () => { toast.success("Coupon deleted"); setDeleteId(null); queryClient.invalidateQueries({ queryKey: ["admin-coupons"] }); },
    onError: () => { toast.error("Failed"); },
  });

  const saveMutation = useMutation({
    mutationFn: (payload: { id?: string; data: typeof form }) => {
      const processed = { ...payload.data, minimumOrderAmount: payload.data.minimumOrderAmount || undefined, usageLimit: payload.data.usageLimit || undefined };
      return payload.id ? adminApi.coupons.update(payload.id, processed) : adminApi.coupons.create(processed);
    },
    onSuccess: (_, payload) => {
      toast.success(payload.id ? "Updated" : "Created");
      setFormOpen(false); setEditItem(null); setForm({ code: "", description: "", type: "Percentage", value: 0, minimumOrderAmount: 0, usageLimit: 0 });
      queryClient.invalidateQueries({ queryKey: ["admin-coupons"] });
    },
    onError: () => { toast.error("Failed"); },
  });

  const handleDelete = () => { if (deleteId) deleteMutation.mutate(deleteId); };
  const handleSave = () => {
    if (!form.code) { toast.error("Code required"); return; }
    saveMutation.mutate({ id: editItem?.id, data: form });
  };

  const openEdit = (c: Coupon) => { setEditItem(c); setForm({ code: c.code, description: c.description || "", type: c.type, value: c.value, minimumOrderAmount: c.minimumOrderAmount || 0, usageLimit: c.usageLimit || 0 }); setFormOpen(true); };
  const openNew = () => { setEditItem(null); setForm({ code: "", description: "", type: "Percentage", value: 0, minimumOrderAmount: 0, usageLimit: 0 }); setFormOpen(true); };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Coupons</h1>
        <Button onClick={openNew}><Plus className="mr-2 h-4 w-4" />Add Coupon</Button>
      </div>
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center gap-4 mb-4">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input placeholder="Search..." value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} className="pl-9" />
            </div>
          </div>
          {isLoading ? <div className="space-y-2">{[1, 2, 3].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div> : (
            <>
              <Table>
                <TableHeader><TableRow><TableHead>Code</TableHead><TableHead>Type</TableHead><TableHead>Value</TableHead><TableHead>Used</TableHead><TableHead>Status</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
                <TableBody>
                  {data?.items.map(c => (
                    <TableRow key={c.id}>
                      <TableCell className="font-medium font-mono">{c.code}</TableCell>
                      <TableCell>{c.type === "Percentage" ? "Percentage" : "Fixed"}</TableCell>
                      <TableCell>{c.type === "Percentage" ? `${c.value}%` : `₹${c.value}`}</TableCell>
                      <TableCell>{c.usedCount}{c.usageLimit ? ` / ${c.usageLimit}` : ""}</TableCell>
                      <TableCell><Badge variant={c.isActive ? "default" : "secondary"}>{c.isActive ? "Active" : "Inactive"}</Badge></TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-1">
                          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => openEdit(c)}><Edit className="h-4 w-4" /></Button>
                          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => setDeleteId(c.id)}><Trash2 className="h-4 w-4" /></Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                  {data?.items.length === 0 && <TableRow><TableCell colSpan={6} className="text-center text-muted-foreground py-8">No coupons found</TableCell></TableRow>}
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
      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent>
          <DialogHeader><DialogTitle>{editItem ? "Edit Coupon" : "New Coupon"}</DialogTitle></DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2"><label className="text-sm font-medium">Code *</label><Input value={form.code} onChange={e => setForm(p => ({ ...p, code: e.target.value.toUpperCase() }))} /></div>
            <div className="space-y-2"><label className="text-sm font-medium">Description</label><Input value={form.description} onChange={e => setForm(p => ({ ...p, description: e.target.value }))} /></div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Type *</label>
              <Select value={form.type} onValueChange={v => setForm(p => ({ ...p, type: v as "Percentage" | "FixedAmount" }))}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent><SelectItem value="Percentage">Percentage</SelectItem><SelectItem value="FixedAmount">Fixed Amount</SelectItem></SelectContent>
              </Select>
            </div>
            <div className="space-y-2"><label className="text-sm font-medium">Value *</label><Input type="number" step="0.01" value={form.value} onChange={e => setForm(p => ({ ...p, value: parseFloat(e.target.value) || 0 }))} /></div>
            <div className="space-y-2"><label className="text-sm font-medium">Min Order Amount</label><Input type="number" value={form.minimumOrderAmount} onChange={e => setForm(p => ({ ...p, minimumOrderAmount: parseFloat(e.target.value) || 0 }))} /></div>
            <div className="space-y-2"><label className="text-sm font-medium">Usage Limit</label><Input type="number" value={form.usageLimit} onChange={e => setForm(p => ({ ...p, usageLimit: parseInt(e.target.value) || 0 }))} /></div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button onClick={handleSave} disabled={saveMutation.isPending}>{saveMutation.isPending ? "Saving..." : "Save"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
      <Dialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Delete Coupon</DialogTitle></DialogHeader>
          <p>Are you sure?</p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteId(null)}>Cancel</Button>
            <Button variant="destructive" onClick={handleDelete} disabled={deleteMutation.isPending}>{deleteMutation.isPending ? "Deleting..." : "Delete"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
