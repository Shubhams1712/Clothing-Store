"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { addressService, type Address, type CreateAddressPayload } from "@/services/shopping";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { toast } from "sonner";

interface AddressFormProps {
  initialData?: Address | null;
  onSuccess: (address: Address) => void;
  onCancel: () => void;
}

const INDIAN_STATES = [
  "Andhra Pradesh", "Arunachal Pradesh", "Assam", "Bihar", "Chhattisgarh",
  "Goa", "Gujarat", "Haryana", "Himachal Pradesh", "Jharkhand", "Karnataka",
  "Kerala", "Madhya Pradesh", "Maharashtra", "Manipur", "Meghalaya", "Mizoram",
  "Nagaland", "Odisha", "Punjab", "Rajasthan", "Sikkim", "Tamil Nadu",
  "Telangana", "Tripura", "Uttar Pradesh", "Uttarakhand", "West Bengal",
  "Delhi", "Chandigarh", "Puducherry",
];

export function AddressForm({ initialData, onSuccess, onCancel }: AddressFormProps) {
  const queryClient = useQueryClient();
  const [form, setForm] = useState<CreateAddressPayload>({
    fullName: initialData?.fullName || "",
    phone: initialData?.phone || "",
    email: initialData?.email || "",
    addressLine1: initialData?.addressLine1 || "",
    addressLine2: initialData?.addressLine2 || "",
    landmark: initialData?.landmark || "",
    city: initialData?.city || "",
    state: initialData?.state || "",
    country: initialData?.country || "India",
    postalCode: initialData?.postalCode || "",
    isDefault: initialData?.isDefault || false,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const saveMutation = useMutation({
    mutationFn: (payload: CreateAddressPayload) =>
      initialData ? addressService.updateAddress(initialData.id, payload) : addressService.createAddress(payload),
    onSuccess: (address) => {
      toast.success(initialData ? "Address updated" : "Address added");
      queryClient.invalidateQueries({ queryKey: ["addresses"] });
      onSuccess(address);
    },
    onError: () => {
      toast.error("Failed to save address");
    },
  });

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};
    if (!form.fullName.trim()) newErrors.fullName = "Name is required";
    if (!form.phone.trim()) newErrors.phone = "Phone is required";
    else if (!/^[6-9]\d{9}$/.test(form.phone.trim())) newErrors.phone = "Enter a valid 10-digit Indian mobile number";
    if (!form.addressLine1.trim()) newErrors.addressLine1 = "Address is required";
    if (!form.city.trim()) newErrors.city = "City is required";
    if (!form.state.trim()) newErrors.state = "State is required";
    if (!form.postalCode.trim()) newErrors.postalCode = "PIN code is required";
    else if (!/^\d{6}$/.test(form.postalCode.trim())) newErrors.postalCode = "Enter a valid 6-digit PIN code";
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    saveMutation.mutate(form);
  };

  const updateField = (field: keyof CreateAddressPayload, value: string | boolean) => {
    setForm(prev => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors(prev => ({ ...prev, [field]: "" }));
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="addr-name">Full Name *</Label>
          <Input id="addr-name" value={form.fullName} onChange={e => updateField("fullName", e.target.value)} placeholder="John Doe" />
          {errors.fullName && <p className="text-xs text-destructive">{errors.fullName}</p>}
        </div>
        <div className="space-y-2">
          <Label htmlFor="addr-phone">Phone *</Label>
          <Input id="addr-phone" value={form.phone} onChange={e => updateField("phone", e.target.value)} placeholder="9876543210" maxLength={10} />
          {errors.phone && <p className="text-xs text-destructive">{errors.phone}</p>}
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="addr-email">Email</Label>
        <Input id="addr-email" type="email" value={form.email || ""} onChange={e => updateField("email", e.target.value)} placeholder="john@example.com" />
      </div>

      <div className="space-y-2">
        <Label htmlFor="addr-line1">Address Line 1 *</Label>
        <Input id="addr-line1" value={form.addressLine1} onChange={e => updateField("addressLine1", e.target.value)} placeholder="House/Flat no., Building, Street" />
        {errors.addressLine1 && <p className="text-xs text-destructive">{errors.addressLine1}</p>}
      </div>

      <div className="space-y-2">
        <Label htmlFor="addr-line2">Address Line 2</Label>
        <Input id="addr-line2" value={form.addressLine2 || ""} onChange={e => updateField("addressLine2", e.target.value)} placeholder="Landmark, Area (optional)" />
      </div>

      <div className="space-y-2">
        <Label htmlFor="addr-landmark">Landmark</Label>
        <Input id="addr-landmark" value={form.landmark || ""} onChange={e => updateField("landmark", e.target.value)} placeholder="Near (optional)" />
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        <div className="space-y-2">
          <Label htmlFor="addr-city">City *</Label>
          <Input id="addr-city" value={form.city} onChange={e => updateField("city", e.target.value)} placeholder="Mumbai" />
          {errors.city && <p className="text-xs text-destructive">{errors.city}</p>}
        </div>
        <div className="space-y-2">
          <Label htmlFor="addr-state">State *</Label>
          <Select value={form.state} onValueChange={(value) => value && updateField("state", value)}>
            <SelectTrigger id="addr-state" aria-label="Select state">
              <SelectValue placeholder="Select state" />
            </SelectTrigger>
            <SelectContent>
              {INDIAN_STATES.map(s => (
                <SelectItem key={s} value={s}>{s}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          {errors.state && <p className="text-xs text-destructive">{errors.state}</p>}
        </div>
        <div className="space-y-2">
          <Label htmlFor="addr-pin">PIN Code *</Label>
          <Input id="addr-pin" value={form.postalCode} onChange={e => updateField("postalCode", e.target.value)} placeholder="400001" maxLength={6} />
          {errors.postalCode && <p className="text-xs text-destructive">{errors.postalCode}</p>}
        </div>
      </div>

      <div className="flex items-center gap-2">
        <Checkbox
          id="addr-default"
          checked={form.isDefault}
          onCheckedChange={(checked) => updateField("isDefault", checked === true)}
        />
        <Label htmlFor="addr-default" className="text-sm font-normal">Set as default address</Label>
      </div>

      <div className="flex justify-end gap-3 pt-2">
        <Button type="button" variant="outline" onClick={onCancel}>Cancel</Button>
        <Button type="submit" disabled={saveMutation.isPending}>
          {saveMutation.isPending ? "Saving..." : initialData ? "Update Address" : "Save Address"}
        </Button>
      </div>
    </form>
  );
}
