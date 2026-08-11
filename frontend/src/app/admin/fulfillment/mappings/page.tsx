"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { fulfillmentService } from "@/services/fulfillment";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";
import { Plus, Trash2 } from "lucide-react";

export default function FulfillmentMappingsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["fulfillment-mappings", search],
    queryFn: () => fulfillmentService.listMappings({ search, pageSize: 50 }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => fulfillmentService.deleteMapping(id),
    onSuccess: () => {
      toast.success("Mapping deleted");
      queryClient.invalidateQueries({ queryKey: ["fulfillment-mappings"] });
    },
    onError: () => {
      toast.error("Failed to delete mapping");
    },
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">Fulfillment Mappings</h1>
          <Skeleton className="h-9 w-32" />
        </div>
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-16 w-full rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Fulfillment Mappings</h1>
      </div>

      <div className="flex gap-3">
        <Input
          placeholder="Search mappings..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="max-w-sm"
        />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Product Mappings</CardTitle>
        </CardHeader>
        <CardContent>
          {data && data.items.length > 0 ? (
            <div className="space-y-3">
              {data.items.map((mapping) => (
                <div
                  key={mapping.id}
                  className="flex items-center justify-between rounded-lg border p-4"
                >
                  <div className="space-y-1">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-medium">
                        {mapping.productName || mapping.productId}
                      </span>
                      <Badge variant={mapping.isActive ? "default" : "secondary"}>
                        {mapping.isActive ? "Active" : "Inactive"}
                      </Badge>
                    </div>
                    <p className="text-xs text-muted-foreground">
                      SKU: {mapping.externalSku} {mapping.variantSku && `\u00B7 Variant: ${mapping.variantSku}`}
                    </p>
                    {mapping.providerName && (
                      <p className="text-xs text-muted-foreground">
                        Provider: {mapping.providerName}
                      </p>
                    )}
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => deleteMutation.mutate(mapping.id)}
                    disabled={deleteMutation.isPending}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">No mappings found</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
