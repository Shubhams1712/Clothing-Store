import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

const PLACEHOLDER_IMAGE = "/placeholder-product.svg"

const BLOCKED_HOSTS = new Set([
  "example.com",
  "example.org",
  "example.net",
  "localhost",
])

export function getSafeImageUrl(url: string | null | undefined): string {
  if (!url) return PLACEHOLDER_IMAGE
  try {
    const parsed = new URL(url)
    if (parsed.protocol !== "http:" && parsed.protocol !== "https:") {
      return PLACEHOLDER_IMAGE
    }
    if (BLOCKED_HOSTS.has(parsed.hostname)) {
      return PLACEHOLDER_IMAGE
    }
    return url
  } catch {
    return PLACEHOLDER_IMAGE
  }
}
