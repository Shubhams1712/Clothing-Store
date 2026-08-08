import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"
import { API_CONFIG } from "@/config/api"

const currencyFormatter = new Intl.NumberFormat("en-IN", {
  style: "currency",
  currency: "INR",
  minimumFractionDigits: 0,
  maximumFractionDigits: 2,
})

export function formatPrice(amount: number): string {
  return currencyFormatter.format(amount)
}

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

const PLACEHOLDER_IMAGE = "/placeholder-product.svg"

const BLOCKED_HOSTS = new Set([
  "example.com",
  "example.org",
  "example.net",
])

export function getSafeImageUrl(url: string | null | undefined): string {
  if (!url) return PLACEHOLDER_IMAGE

  let resolvedUrl = url

  if (url.startsWith("/")) {
    resolvedUrl = `${API_CONFIG.BASE_URL}${url}`
  }

  try {
    const parsed = new URL(resolvedUrl)
    if (parsed.protocol !== "http:" && parsed.protocol !== "https:") {
      return PLACEHOLDER_IMAGE
    }
    if (BLOCKED_HOSTS.has(parsed.hostname)) {
      return PLACEHOLDER_IMAGE
    }
    return resolvedUrl
  } catch {
    return PLACEHOLDER_IMAGE
  }
}
