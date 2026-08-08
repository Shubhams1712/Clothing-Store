"use client";

import { useEffect, useState } from "react";
import { useRouter, useParams } from "next/navigation";
import { adminApi } from "@/services/admin";
import type { Category } from "@/types/admin";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Skeleton } from "@/components/ui/skeleton";
import { ArrowLeft, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import Link from "next/link";
import { ImageUpload } from "@/components/ui/image-upload";

export default function AdminProductFormPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;
  const isNew = id === "new";

  const [loading, setLoading] = useState(!isNew);
  const [saving, setSaving] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [form, setForm] = useState({
    name: "", slug: "", description: "", shortDescription: "", sku: "",
    price: 0, comparePrice: 0, costPrice: 0, brand: "", tags: "",
    isFeatured: false, isPublished: false, categoryId: "",
    seoTitle: "", seoDescription: "",
  });
  const [variants, setVariants] = useState<Array<{ size: string; color: string; sku: string; price: number; stock: number; isAvailable: boolean }>>([]);
  const [images, setImages] = useState<Array<{ url: string; altText: string; sortOrder: number; isFeatured: boolean }>>([]);

  useEffect(() => {
    adminApi.categories.list({ pageSize: 100 }).then((res) => setCategories(res.items));
    if (!isNew) {
      adminApi.products.get(id).then((p) => {
        setForm({
          name: p.name, slug: p.slug, description: p.description, shortDescription: p.shortDescription || "",
          sku: p.sku, price: p.price, comparePrice: p.comparePrice || 0, costPrice: p.costPrice || 0,
          brand: p.brand || "", tags: p.tags || "", isFeatured: p.isFeatured, isPublished: p.isPublished,
          categoryId: p.categoryId || "", seoTitle: p.seoTitle || "", seoDescription: p.seoDescription || "",
        });
        setVariants(p.variants.map(v => ({ size: v.size || "", color: v.color || "", sku: v.sku, price: v.price, stock: v.stock, isAvailable: v.isAvailable })));
        setImages(p.images.map(i => ({ url: i.url, altText: i.altText || "", sortOrder: i.sortOrder, isFeatured: i.isFeatured })));
      }).finally(() => setLoading(false));
    }
  }, [id, isNew]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (variants.length === 0) { toast.error("Add at least one variant"); return; }
    if (variants.some(v => !v.sku || v.price < 0.01)) {
      toast.error("Each variant must have a SKU and price >= ₹0.01");
      return;
    }
    const filteredImages = images.filter(img => img.url.trim() !== "");
    setSaving(true);
    try {
      const payload: Record<string, unknown> = {
        ...form,
        price: form.price < 0.01 ? 0.01 : form.price,
        categoryId: form.categoryId || undefined,
        variants: variants.map(v => ({
          ...v,
          price: v.price < 0.01 ? 0.01 : v.price,
          stock: Math.max(0, v.stock),
        })),
        images: filteredImages,
      };
      if (isNew) {
        await adminApi.products.create(payload);
        toast.success("Product created");
      } else {
        await adminApi.products.update(id, payload);
        toast.success("Product updated");
      }
      router.push("/admin/products");
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Failed to save product";
      toast.error(msg);
    } finally { setSaving(false); }
  };

  const updateForm = (field: string, value: unknown) => setForm(prev => ({ ...prev, [field]: value }));
  const addVariant = () => setVariants(prev => [...prev, { size: "", color: "", sku: "", price: 0.01, stock: 0, isAvailable: true }]);
  const removeVariant = (i: number) => setVariants(prev => prev.filter((_, idx) => idx !== i));
  const updateVariant = (i: number, field: string, value: unknown) => setVariants(prev => prev.map((v, idx) => idx === i ? { ...v, [field]: value } : v));
  const addImage = () => setImages(prev => [...prev, { url: "", altText: "", sortOrder: prev.length, isFeatured: prev.length === 0 }]);
  const removeImage = (i: number) => setImages(prev => prev.filter((_, idx) => idx !== i));
  const updateImage = (i: number, field: string, value: unknown) => setImages(prev => prev.map((img, idx) => idx === i ? { ...img, [field]: value } : img));

  if (loading) return <div className="space-y-4">{[1, 2, 3].map(i => <Skeleton key={i} className="h-20 w-full" />)}</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/admin/products" className={buttonVariants({ variant: "ghost", size: "icon" })}><ArrowLeft className="h-4 w-4" /></Link>
        <h1 className="text-2xl font-bold">{isNew ? "Create Product" : "Edit Product"}</h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader><CardTitle>Basic Info</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <label className="text-sm font-medium">Name *</label>
                <Input value={form.name} onChange={e => updateForm("name", e.target.value)} required />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Slug *</label>
                <Input value={form.slug} onChange={e => updateForm("slug", e.target.value)} required />
              </div>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Description</label>
              <Textarea value={form.description} onChange={e => updateForm("description", e.target.value)} rows={4} />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Short Description</label>
              <Input value={form.shortDescription} onChange={e => updateForm("shortDescription", e.target.value)} />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle>Pricing & Inventory</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-3">
              <div className="space-y-2">
                <label className="text-sm font-medium">SKU *</label>
                <Input value={form.sku} onChange={e => updateForm("sku", e.target.value)} required />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Price *</label>
                <Input type="number" step="0.01" min="0.01" value={form.price} onChange={e => updateForm("price", Math.max(0.01, parseFloat(e.target.value) || 0.01))} required />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Compare Price</label>
                <Input type="number" step="0.01" value={form.comparePrice} onChange={e => updateForm("comparePrice", parseFloat(e.target.value) || 0)} />
              </div>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <label className="text-sm font-medium">Cost Price</label>
                <Input type="number" step="0.01" value={form.costPrice} onChange={e => updateForm("costPrice", parseFloat(e.target.value) || 0)} />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Category</label>
                <Select value={form.categoryId} onValueChange={v => updateForm("categoryId", v)}>
                  <SelectTrigger><SelectValue placeholder="Select category" /></SelectTrigger>
                  <SelectContent>
                    {categories.map(c => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>Variants</CardTitle>
            <Button type="button" variant="outline" size="sm" onClick={addVariant}><Plus className="mr-2 h-4 w-4" />Add</Button>
          </CardHeader>
          <CardContent className="space-y-3">
            {variants.map((v, i) => (
              <div key={i} className="grid gap-3 md:grid-cols-6 items-end">
                <Input placeholder="Size" value={v.size} onChange={e => updateVariant(i, "size", e.target.value)} />
                <Input placeholder="Color" value={v.color} onChange={e => updateVariant(i, "color", e.target.value)} />
                <Input placeholder="SKU" value={v.sku} onChange={e => updateVariant(i, "sku", e.target.value)} required />
                <Input type="number" step="0.01" min="0.01" placeholder="Price" value={v.price} onChange={e => updateVariant(i, "price", Math.max(0.01, parseFloat(e.target.value) || 0.01))} />
                <Input type="number" placeholder="Stock" value={v.stock} onChange={e => updateVariant(i, "stock", parseInt(e.target.value) || 0)} />
                <Button type="button" variant="ghost" size="icon" onClick={() => removeVariant(i)}><Trash2 className="h-4 w-4 text-destructive" /></Button>
              </div>
            ))}
            {variants.length === 0 && <p className="text-sm text-muted-foreground">No variants added</p>}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>Images</CardTitle>
            <Button type="button" variant="outline" size="sm" onClick={addImage}><Plus className="mr-2 h-4 w-4" />Add</Button>
          </CardHeader>
          <CardContent className="space-y-4">
            {images.map((img, i) => (
              <div key={i} className="flex items-start gap-4">
                <ImageUpload
                  value={img.url}
                  onChange={(url) => updateImage(i, "url", url)}
                  onRemove={() => removeImage(i)}
                  className="w-[120px]"
                />
                <div className="flex-1 space-y-2">
                  <Input placeholder="Alt text" value={img.altText} onChange={e => updateImage(i, "altText", e.target.value)} />
                  <div className="flex items-center gap-2">
                    <Checkbox checked={img.isFeatured} onCheckedChange={v => updateImage(i, "isFeatured", !!v)} />
                    <label className="text-sm">Featured</label>
                  </div>
                </div>
              </div>
            ))}
            {images.length === 0 && <p className="text-sm text-muted-foreground">No images added</p>}
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle>Options</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="flex gap-6">
              <div className="flex items-center gap-2">
                <Checkbox checked={form.isPublished} onCheckedChange={v => updateForm("isPublished", !!v)} />
                <label className="text-sm">Published</label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox checked={form.isFeatured} onCheckedChange={v => updateForm("isFeatured", !!v)} />
                <label className="text-sm">Featured</label>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex justify-end gap-4">
          <Link href="/admin/products" className={buttonVariants({ variant: "outline" })}>Cancel</Link>
          <Button type="submit" disabled={saving}>{saving ? "Saving..." : isNew ? "Create Product" : "Update Product"}</Button>
        </div>
      </form>
    </div>
  );
}
