"use client";

import { useState, useRef } from "react";
import Image from "next/image";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import { api } from "@/lib/api";
import { getSafeImageUrl } from "@/lib/utils";
import type { Collection } from "@/types/admin";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { Plus, Search, Trash2, Edit, Upload, X } from "lucide-react";
import { toast } from "sonner";

interface CollectionForm {
  name: string;
  slug: string;
  description: string;
  imageUrl: string;
  isFeatured: boolean;
}

const EMPTY_FORM: CollectionForm = { name: "", slug: "", description: "", imageUrl: "", isFeatured: false };

export default function AdminCollectionsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [editItem, setEditItem] = useState<Collection | null>(null);
  const [form, setForm] = useState<CollectionForm>(EMPTY_FORM);
  const [uploading, setUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["admin-collections", page, search],
    queryFn: () => adminApi.collections.list({ page, pageSize: 10, search }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => adminApi.collections.delete(id),
    onSuccess: () => { toast.success("Collection deleted"); setDeleteId(null); queryClient.invalidateQueries({ queryKey: ["admin-collections"] }); },
    onError: () => { toast.error("Failed"); },
  });

  const saveMutation = useMutation({
    mutationFn: (payload: { id?: string; data: CollectionForm }) => {
      const body = { ...payload.data };
      return payload.id
        ? adminApi.collections.update(payload.id, body as unknown as Record<string, unknown>)
        : adminApi.collections.create(body as unknown as Record<string, unknown>);
    },
    onSuccess: (_, payload) => {
      toast.success(payload.id ? "Updated" : "Created");
      setFormOpen(false); setEditItem(null); setForm(EMPTY_FORM);
      queryClient.invalidateQueries({ queryKey: ["admin-collections"] });
    },
    onError: () => { toast.error("Failed"); },
  });

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (!file.type.startsWith("image/")) { toast.error("Please select an image file"); return; }

    setUploading(true);
    try {
      const formData = new FormData();
      formData.append("file", file);
      const response = await api.post<{ data: { url: string } }>("/api/media/upload", formData);
      setForm(prev => ({ ...prev, imageUrl: response.data.data.url }));
      toast.success("Image uploaded");
    } catch {
      toast.error("Failed to upload image");
    } finally {
      setUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  const handleDelete = () => { if (deleteId) deleteMutation.mutate(deleteId); };
  const handleSave = () => {
    if (!form.name || !form.slug) { toast.error("Name and slug required"); return; }
    saveMutation.mutate({ id: editItem?.id, data: form });
  };

  const openEdit = (c: Collection) => {
    setEditItem(c);
    setForm({ name: c.name, slug: c.slug, description: c.description || "", imageUrl: c.imageUrl || "", isFeatured: c.isFeatured });
    setFormOpen(true);
  };
  const openNew = () => { setEditItem(null); setForm(EMPTY_FORM); setFormOpen(true); };

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
          {isLoading ? <div className="space-y-2">{[1, 2, 3].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div> : (
            <>
              <Table>
                <TableHeader><TableRow><TableHead>Image</TableHead><TableHead>Name</TableHead><TableHead>Slug</TableHead><TableHead>Products</TableHead><TableHead>Featured</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
                <TableBody>
                  {data?.items.map(c => (
                    <TableRow key={c.id}>
                      <TableCell>
                        <div className="relative h-10 w-10 overflow-hidden rounded bg-muted">
                          {c.imageUrl ? (
                            <Image src={getSafeImageUrl(c.imageUrl)} alt={c.name} fill className="object-cover" sizes="40px" />
                          ) : (
                            <div className="flex h-full items-center justify-center text-xs text-muted-foreground">No img</div>
                          )}
                        </div>
                      </TableCell>
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
                  {data?.items.length === 0 && <TableRow><TableCell colSpan={6} className="text-center text-muted-foreground py-8">No collections found</TableCell></TableRow>}
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
        <DialogContent className="max-w-lg">
          <DialogHeader><DialogTitle>{editItem ? "Edit Collection" : "New Collection"}</DialogTitle></DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Collection Image</label>
              <input ref={fileInputRef} type="file" accept="image/*" className="hidden" onChange={handleImageUpload} />
              {form.imageUrl ? (
                <div className="relative group">
                  <div className="relative h-40 w-full overflow-hidden rounded-lg border bg-muted">
                    <Image src={getSafeImageUrl(form.imageUrl)} alt="Collection" fill className="object-cover" sizes="160px" />
                  </div>
                  <Button
                    variant="destructive"
                    size="icon"
                    className="absolute top-2 right-2 h-6 w-6 opacity-0 group-hover:opacity-100 transition-opacity"
                    onClick={() => setForm(prev => ({ ...prev, imageUrl: "" }))}
                  >
                    <X className="h-3 w-3" />
                  </Button>
                </div>
              ) : (
                <button
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={uploading}
                  className="flex h-40 w-full flex-col items-center justify-center rounded-lg border-2 border-dashed bg-muted/50 transition-colors hover:bg-muted"
                >
                  <Upload className="mb-2 h-8 w-8 text-muted-foreground" />
                  <span className="text-sm text-muted-foreground">{uploading ? "Uploading..." : "Click to upload image"}</span>
                </button>
              )}
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Name *</label>
              <Input value={form.name} onChange={e => setForm(p => ({ ...p, name: e.target.value }))} placeholder="Collection name" />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Slug *</label>
              <Input value={form.slug} onChange={e => setForm(p => ({ ...p, slug: e.target.value }))} placeholder="collection-slug" />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Description</label>
              <Input value={form.description} onChange={e => setForm(p => ({ ...p, description: e.target.value }))} placeholder="Brief description" />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="col-featured"
                checked={form.isFeatured}
                onChange={e => setForm(p => ({ ...p, isFeatured: e.target.checked }))}
                className="h-4 w-4 rounded"
              />
              <label htmlFor="col-featured" className="text-sm font-medium">Featured on homepage</label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button onClick={handleSave} disabled={saveMutation.isPending}>{saveMutation.isPending ? "Saving..." : "Save"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Delete Collection</DialogTitle></DialogHeader>
          <p>Are you sure? This cannot be undone.</p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteId(null)}>Cancel</Button>
            <Button variant="destructive" onClick={handleDelete} disabled={deleteMutation.isPending}>{deleteMutation.isPending ? "Deleting..." : "Delete"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
