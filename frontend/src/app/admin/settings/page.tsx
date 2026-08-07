"use client";

import { useEffect, useState } from "react";
import { adminApi } from "@/services/admin";
import type { StoreSettings } from "@/types/admin";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";
import { useAuth } from "@/hooks/use-auth";

export default function AdminSettingsPage() {
  const { user } = useAuth();
  const [settings, setSettings] = useState<StoreSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<Partial<StoreSettings>>({});

  useEffect(() => {
    adminApi.settings.get().then(s => { setSettings(s); setForm(s); }).finally(() => setLoading(false));
  }, []);

  const handleSave = async () => {
    setSaving(true);
    try { await adminApi.settings.update(form); toast.success("Settings saved"); } catch { toast.error("Failed to save"); } finally { setSaving(false); }
  };

  const update = (field: string, value: string | undefined) => setForm(prev => ({ ...prev, [field]: value }));

  if (loading) return <div className="space-y-4">{[1, 2, 3].map(i => <Skeleton key={i} className="h-20 w-full" />)}</div>;

  if (user && !user.isAdmin) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-bold">Settings</h1>
        <Card>
          <CardContent className="p-6">
            <p className="text-muted-foreground">Only administrators can access store settings.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Settings</h1>
        <Button onClick={handleSave} disabled={saving}>{saving ? "Saving..." : "Save Changes"}</Button>
      </div>

      <Tabs defaultValue="store">
        <TabsList>
          <TabsTrigger value="store">Store</TabsTrigger>
          <TabsTrigger value="contact">Contact</TabsTrigger>
          <TabsTrigger value="social">Social</TabsTrigger>
          <TabsTrigger value="payments">Payments</TabsTrigger>
          <TabsTrigger value="media">Media</TabsTrigger>
        </TabsList>

        <TabsContent value="store" className="space-y-4 mt-4">
          <Card>
            <CardHeader><CardTitle>Store Information</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">Store Name *</label>
                <Input value={form.storeName || ""} onChange={e => update("storeName", e.target.value)} />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Description</label>
                <Textarea value={form.storeDescription || ""} onChange={e => update("storeDescription", e.target.value)} rows={3} />
              </div>
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Currency</label>
                  <Input value={form.currency || "INR"} onChange={e => update("currency", e.target.value)} />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">Currency Symbol</label>
                  <Input value={form.currencySymbol || "₹"} onChange={e => update("currencySymbol", e.target.value)} />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="contact" className="space-y-4 mt-4">
          <Card>
            <CardHeader><CardTitle>Contact Information</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Email</label>
                  <Input type="email" value={form.contactEmail || ""} onChange={e => update("contactEmail", e.target.value)} />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">Phone</label>
                  <Input value={form.contactPhone || ""} onChange={e => update("contactPhone", e.target.value)} />
                </div>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Address</label>
                <Textarea value={form.address || ""} onChange={e => update("address", e.target.value)} rows={2} />
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="social" className="space-y-4 mt-4">
          <Card>
            <CardHeader><CardTitle>Social Links</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2"><label className="text-sm font-medium">Facebook</label><Input value={form.socialFacebook || ""} onChange={e => update("socialFacebook", e.target.value)} placeholder="https://facebook.com/..." /></div>
              <div className="space-y-2"><label className="text-sm font-medium">Instagram</label><Input value={form.socialInstagram || ""} onChange={e => update("socialInstagram", e.target.value)} placeholder="https://instagram.com/..." /></div>
              <div className="space-y-2"><label className="text-sm font-medium">Twitter</label><Input value={form.socialTwitter || ""} onChange={e => update("socialTwitter", e.target.value)} placeholder="https://twitter.com/..." /></div>
              <div className="space-y-2"><label className="text-sm font-medium">YouTube</label><Input value={form.socialYoutube || ""} onChange={e => update("socialYoutube", e.target.value)} placeholder="https://youtube.com/..." /></div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="payments" className="space-y-4 mt-4">
          <Card>
            <CardHeader><CardTitle>Razorpay</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2"><label className="text-sm font-medium">Key ID</label><Input value={form.razorpayKeyId || ""} onChange={e => update("razorpayKeyId", e.target.value)} /></div>
              <div className="space-y-2"><label className="text-sm font-medium">Key Secret</label><Input type="password" value={form.razorpayKeySecret || ""} onChange={e => update("razorpayKeySecret", e.target.value)} /></div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="media" className="space-y-4 mt-4">
          <Card>
            <CardHeader><CardTitle>Cloudinary</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2"><label className="text-sm font-medium">Cloud Name</label><Input value={form.cloudinaryCloudName || ""} onChange={e => update("cloudinaryCloudName", e.target.value)} /></div>
              <div className="space-y-2"><label className="text-sm font-medium">API Key</label><Input value={form.cloudinaryApiKey || ""} onChange={e => update("cloudinaryApiKey", e.target.value)} /></div>
              <div className="space-y-2"><label className="text-sm font-medium">API Secret</label><Input type="password" value={form.cloudinaryApiSecret || ""} onChange={e => update("cloudinaryApiSecret", e.target.value)} /></div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
