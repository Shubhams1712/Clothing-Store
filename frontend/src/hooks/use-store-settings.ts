"use client";

import { useQuery } from "@tanstack/react-query";
import { storefrontService } from "@/services/storefront";
import type { StoreSettings } from "@/types/storefront";

const DEFAULT_SETTINGS: StoreSettings = {
  id: "",
  storeName: "The Freak Store",
};

export function useStoreSettings() {
  const { data, isLoading } = useQuery({
    queryKey: ["store-settings"],
    queryFn: () => storefrontService.getSettings(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
    refetchOnWindowFocus: false,
  });

  const settings = data || DEFAULT_SETTINGS;

  return {
    settings,
    storeName: settings.storeName || "The Freak Store",
    isLoading,
  };
}
