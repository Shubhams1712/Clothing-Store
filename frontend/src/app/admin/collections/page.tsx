"use client";

import { useEffect, useState, useCallback } from "react";
import { adminApi } from "@/services/admin";
import type { Collection, PaginatedResponse } from "@/types/admin";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { Plus, Search, Trash2, Edit } from "lucide-react";
import { toast } from "sonner";

export default function AdminCollectionsPage() {
  const [data, setData] = useState<PaginatedResponse<Collection> | null>(null);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [formOpen, setFormOpen] = useState(false);
  const [editItem, setEditItem] = useState<Collection | null>(null);
  const [form, setForm] = useState({ name: "", slug: "", description: "", isFeatured: false });
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try { setData(await adminApi.collections.list({ page, pageSize: 10, search })); } finally { setLoading(false); }
  }, [page, search]);

  useEffect(() => { load(); }, [load]);

  const handleDelete = async () => {
    if (!deleteId) return;
    setDeleting(true);
    try { await adminApi.collections.delete(deleteId); toast.success("Collection deleted"); setDeleteId(null); load(); } catch { toast.error("Failed"); } finally { setDeleting(false); }
  };

  const handleSave = async () => {
    if (!form.name || !form.slug) { toast.error("Name and slug required"); return; }
    setSaving(true);
    try {
      if (editItem) { await adminApi.collections.update(editItem.id, form); toast.success("Updated"); }
      else { await adminApi.collections.create(form); toast.success("Created"); }
      setFormOpen(false); setEditItem(null); setForm({ name: "", slug: "", description: "", isFeatured: false }); load();
    } catch { toast.error("Failed"); } finally { setSaving(false); }
  };

  const openEdit = (c: Collection) => { setEditItem(c); setForm({ name: c.name, slug: c.slug, description: c.description || "", isFeatured: c.isFeatured }); setFormOpen(true); };
  const openNew = () => { setEditItem(null); setForm({ name: "", slug: "", description: "", isFeatured: false }); setFormOpen(true); };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Collections</h1>
        <Button onClick={openNew}><Plus className="mr-2 h-4 w-4" />Add Collection</Button>
      </div>
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center gap-4 mb-4">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input placeholder="Search..." value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} className="pl-9" />
            </div>
          </div>
          {loading ? <div className="space-y-2">{[1, 2, 3].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div> : (
            <>
              <Table>
                <TableHeader><TableRow><TableHead>Name</TableHead><TableHead>Slug</TableHead><TableHead>Products</TableHead><TableHead>Featured</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
                <TableBody>
                  {data?.items.map(c => (
                    <TableRow key={c.id}>
                      <TableCell className="font-medium">{c.name}</TableCell>
                      <TableCell>{c.slug}</TableCell>
                      <TableCell>{c.productCount}</TableCell>
                      <TableCell>{c.isFeatured ? <Badge>Featured</Badge> : "-"}</TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-1">
                          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => openEdit(c)}><Edit className="h-4 w-4" /></Button>
                          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => setDeleteId(c.id)}><Trash2 className="h-4 w-4" /></Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                  {data?.items.length === 0 && <TableRow><TableCell colSpan={5} className="text-center text-muted-foreground py-8">No collections found</TableCell></TableRow>}
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
          <DialogHeader><DialogTitle>{editItem ? "Edit Collection" : "New Collection"}</DialogTitle></DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2"><label className="text-sm font-medium">Name *</label><Input value={form.name} onChange={e => setForm(p => ({ ...p, name: e.target.value }))} /></div>
            <div className="space-y-2"><label className="text-sm font-medium">Slug *</label><Input value={form.slug} onChange={e => setForm(p => ({ ...p, slug: e.target.value }))} /></div>
            <div className="space-y-2"><label className="text-sm font-medium">Description</label><Input value={form.description} onChange={e => setForm(p => ({ ...p, description: e.target.value }))} /></div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button onClick={handleSave} disabled={saving}>{saving ? "Saving..." : "Save"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
      <Dialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Delete Collection</DialogTitle></DialogHeader>
          <p>Are you sure?</p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteId(null)}>Cancel</Button>
            <Button variant="destructive" onClick={handleDelete} disabled={deleting}>{deleting ? "Deleting..." : "Delete"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
