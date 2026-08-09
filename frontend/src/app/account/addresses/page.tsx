"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, MapPin, Plus, Pencil, Trash2 } from "lucide-react";
import { useAuth } from "@/hooks/use-auth";
import { addressService, type Address } from "@/services/shopping";
import { AddressForm } from "@/components/storefront/address-form";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { toast } from "sonner";

export default function AddressesPage() {
  const queryClient = useQueryClient();
  const { isAuthenticated } = useAuth();
  const [formOpen, setFormOpen] = useState(false);
  const [editAddress, setEditAddress] = useState<Address | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const { data: addresses = [], isLoading } = useQuery({
    queryKey: ["addresses"],
    queryFn: addressService.getAddresses,
    enabled: isAuthenticated,
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => addressService.deleteAddress(id),
    onSuccess: () => {
      toast.success("Address deleted");
      setDeleteId(null);
      queryClient.invalidateQueries({ queryKey: ["addresses"] });
    },
    onError: () => {
      toast.error("Failed to delete address");
    },
  });

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center space-y-6 text-center">
          <h1 className="text-3xl font-bold">Sign in to manage addresses</h1>
          <Link href="/login" className={buttonVariants({ size: "lg" })}>
            Sign In
          </Link>
        </div>
      </div>
    );
  }

  if (isLoading) return <LoadingOverlay text="Loading addresses..." />;

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
      <Link href="/checkout" className="mb-6 flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="h-4 w-4" />
        Back to Checkout
      </Link>

      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">My Addresses</h1>
          <p className="mt-1 text-muted-foreground">Manage your shipping addresses</p>
        </div>
        <Button onClick={() => { setEditAddress(null); setFormOpen(true); }}>
          <Plus className="mr-2 h-4 w-4" />
          Add Address
        </Button>
      </div>

      {addresses.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center p-12 text-center">
            <MapPin className="mb-4 h-16 w-16 text-muted-foreground" />
            <h2 className="text-xl font-bold">No addresses saved</h2>
            <p className="mt-2 text-muted-foreground">Add your first shipping address to get started.</p>
            <Button className="mt-4" onClick={() => { setEditAddress(null); setFormOpen(true); }}>
              <Plus className="mr-2 h-4 w-4" />
              Add Address
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {addresses.map(addr => (
            <Card key={addr.id}>
              <CardContent className="flex items-start justify-between p-6">
                <div className="space-y-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{addr.fullName}</span>
                    {addr.isDefault && <Badge variant="secondary">Default</Badge>}
                  </div>
                  <p className="text-sm text-muted-foreground">
                    {addr.addressLine1}
                    {addr.addressLine2 && `, ${addr.addressLine2}`}
                  </p>
                  {addr.landmark && (
                    <p className="text-sm text-muted-foreground">Landmark: {addr.landmark}</p>
                  )}
                  <p className="text-sm text-muted-foreground">
                    {addr.city}, {addr.state} {addr.postalCode}
                  </p>
                  <p className="text-sm text-muted-foreground">{addr.country}</p>
                  <p className="text-sm text-muted-foreground">Phone: {addr.phone}</p>
                  {addr.email && <p className="text-sm text-muted-foreground">Email: {addr.email}</p>}
                </div>
                <div className="flex gap-2">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => { setEditAddress(addr); setFormOpen(true); }}
                  >
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-destructive hover:text-destructive"
                    onClick={() => setDeleteId(addr.id)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>{editAddress ? "Edit Address" : "Add New Address"}</DialogTitle>
          </DialogHeader>
          <AddressForm
            initialData={editAddress}
            onSuccess={() => {
              setFormOpen(false);
              setEditAddress(null);
            }}
            onCancel={() => {
              setFormOpen(false);
              setEditAddress(null);
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Address</DialogTitle>
          </DialogHeader>
          <p>Are you sure you want to delete this address? This action cannot be undone.</p>
          <div className="flex justify-end gap-3">
            <Button variant="outline" onClick={() => setDeleteId(null)}>Cancel</Button>
            <Button
              variant="destructive"
              onClick={() => deleteId && deleteMutation.mutate(deleteId)}
              disabled={deleteMutation.isPending}
            >
              {deleteMutation.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
