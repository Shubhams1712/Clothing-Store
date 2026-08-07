export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface PaginatedRequest {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface Product {
  id: string;
  name: string;
  slug: string;
  description: string;
  shortDescription?: string;
  sku: string;
  price: number;
  comparePrice?: number;
  costPrice?: number;
  brand?: string;
  tags?: string;
  isFeatured: boolean;
  isPublished: boolean;
  isActive: boolean;
  seoTitle?: string;
  seoDescription?: string;
  categoryId?: string;
  categoryName?: string;
  createdAt: string;
  updatedAt: string;
  variants: ProductVariant[];
  images: ProductImage[];
}

export interface ProductVariant {
  id: string;
  size?: string;
  color?: string;
  sku: string;
  price: number;
  stock: number;
  isAvailable: boolean;
}

export interface ProductImage {
  id: string;
  url: string;
  altText?: string;
  sortOrder: number;
  isFeatured: boolean;
}

export interface ProductBrief {
  id: string;
  name: string;
  slug: string;
  price: number;
  imageUrl?: string;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
  parentId?: string;
  parentName?: string;
  isActive: boolean;
  productCount: number;
  createdAt: string;
  children: Category[];
}

export interface Collection {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  isFeatured: boolean;
  isActive: boolean;
  displayOrder: number;
  productCount: number;
  createdAt: string;
  products: ProductBrief[];
}

export interface Order {
  id: string;
  orderNumber: string;
  userId: string;
  customerName: string;
  customerEmail: string;
  status: OrderStatus;
  subTotal: number;
  taxAmount: number;
  shippingAmount: number;
  discountAmount: number;
  totalAmount: number;
  currency?: string;
  shippingName?: string;
  shippingAddress?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingPostalCode?: string;
  shippingCountry?: string;
  shippingPhone?: string;
  paymentMethod?: string;
  paymentStatus?: string;
  paymentId?: string;
  notes?: string;
  internalNotes?: string;
  createdAt: string;
  shippedAt?: string;
  deliveredAt?: string;
  items: OrderItem[];
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  sku?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  size?: string;
  color?: string;
}

export type OrderStatus = "Pending" | "Confirmed" | "Packed" | "Shipped" | "Delivered" | "Cancelled" | "Refunded";

export interface Customer {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isEmailVerified: boolean;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
  orderCount: number;
  totalSpent: number;
  roles: string[];
}

export interface CustomerDetail extends Customer {
  recentOrders: Order[];
  notes?: string;
}

export interface Coupon {
  id: string;
  code: string;
  description?: string;
  type: CouponType;
  value: number;
  minimumOrderAmount?: number;
  maximumDiscountAmount?: number;
  usageLimit?: number;
  usedCount: number;
  expiresAt?: string;
  isActive: boolean;
  createdAt: string;
}

export type CouponType = "Percentage" | "FixedAmount";

export interface Review {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  productId: string;
  productName: string;
  rating: number;
  title: string;
  comment?: string;
  isApproved: boolean;
  isHidden: boolean;
  adminReply?: string;
  createdAt: string;
}

export interface DashboardStats {
  totalRevenue: number;
  todayOrders: number;
  pendingOrders: number;
  lowStockProducts: number;
  totalProducts: number;
  totalCustomers: number;
  totalOrders: number;
  recentOrders: Order[];
  topSellingProducts: ProductBrief[];
  latestCustomers: Customer[];
  salesOverview: SalesOverviewPoint[];
}

export interface SalesOverviewPoint {
  date: string;
  revenue: number;
  orders: number;
}

export interface StoreSettings {
  id: string;
  storeName: string;
  storeDescription?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  currency?: string;
  currencySymbol?: string;
  taxRate?: string;
  shippingPolicy?: string;
  returnPolicy?: string;
  privacyPolicy?: string;
  termsOfService?: string;
  logoUrl?: string;
  faviconUrl?: string;
  primaryColor?: string;
  socialFacebook?: string;
  socialInstagram?: string;
  socialTwitter?: string;
  socialYoutube?: string;
  razorpayKeyId?: string;
  razorpayKeySecret?: string;
  cloudinaryCloudName?: string;
  cloudinaryApiKey?: string;
  cloudinaryApiSecret?: string;
}
