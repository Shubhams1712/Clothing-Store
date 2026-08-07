"use client";

import { useEffect, useState, useCallback } from "react";
import { adminApi } from "@/services/admin";
import type { Product, PaginatedResponse, ProductVariant } from "@/types/admin";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { Search, Edit } from "lucide-react";
import { toast } from "sonner";

export default function AdminInventoryPage() {
  const [data, setData] = useState<PaginatedResponse<Product> | null>(null);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [editVariant, setEditVariant] = useState<ProductVariant & { productName: string } | null>(null);
  const [stock, setStock] = useState(0);
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try { setData(await adminApi.inventory.list({ page, pageSize: 10, search })); } finally { setLoading(false); }
  }, [page, search]);

  useEffect(() => { load(); }, [load]);

  const handleUpdateStock = async () => {
    if (!editVariant) return;
    setSaving(true);
    try { await adminApi.inventory.updateStock(editVariant.id, stock); toast.success("Stock updated"); setEditVariant(null); load(); } catch { toast.error("Failed"); } finally { setSaving(false); }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Inventory</h1>
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center gap-4 mb-4">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input placeholder="Search products..." value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} className="pl-9" />
            </div>
          </div>
          {loading ? <div className="space-y-2">{[1, 2, 3].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div> : (
            <>
              <Table>
                <TableHeader><TableRow><TableHead>Product</TableHead><TableHead>SKU</TableHead><TableHead>Variants</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
                <TableBody>
                  {data?.items.map(p => (
                    <TableRow key={p.id}>
                      <TableCell className="font-medium">{p.name}</TableCell>
                      <TableCell>{p.sku}</TableCell>
                      <TableCell>—</TableCell>
                      <TableCell className="text-right">
                        <Button variant="ghost" size="sm" onClick={() => { /* TODO: expand variants */ }}>View Variants</Button>
                      </TableCell>
                    </TableRow>
                  ))}
                  {data?.items.length === 0 && <TableRow><TableCell colSpan={4} className="text-center text-muted-foreground py-8">No products found</TableCell></TableRow>}
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

      <Dialog open={!!editVariant} onOpenChange={() => setEditVariant(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Update Stock - {editVariant?.productName}</DialogTitle></DialogHeader>
          <div className="space-y-4">
            <div><p className="text-sm text-muted-foreground">SKU: {editVariant?.sku}</p></div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Stock Quantity *</label>
              <Input type="number" min="0" value={stock} onChange={e => setStock(parseInt(e.target.value) || 0)} />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setEditVariant(null)}>Cancel</Button>
            <Button onClick={handleUpdateStock} disabled={saving}>{saving ? "Saving..." : "Update"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
