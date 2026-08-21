"use client";

import { useState, useCallback, useRef } from "react";
import { adminApi } from "@/services/admin";
import type { Category, BulkImportProduct, BulkImportVariant, BulkImportResponse } from "@/types/admin";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  ArrowLeft,
  Upload,
  Download,
  FileSpreadsheet,
  CheckCircle2,
  XCircle,
  AlertTriangle,
  Trash2,
  Plus,
  Loader2,
  ArrowRight,
  RotateCcw,
  Package,
} from "lucide-react";
import { toast } from "sonner";
import Link from "next/link";

const CSV_HEADERS = [
  "name", "slug", "description", "shortDescription", "sku", "price",
  "comparePrice", "costPrice", "brand", "tags", "isFeatured", "isPublished",
  "categoryId", "seoTitle", "seoDescription", "isQikinkProduct",
  "qikinkProductId", "qikinkProductName", "designReference", "designFileUrl", "mockupUrl",
  "variantSize", "variantColor", "variantSku", "variantPrice", "variantStock",
  "variantQikinkSku",
];

const CSV_TEMPLATE = `name,slug,description,shortDescription,sku,price,comparePrice,costPrice,brand,tags,isFeatured,isPublished,categoryId,seoTitle,seoDescription,isQikinkProduct,qikinkProductId,qikinkProductName,designReference,designFileUrl,mockupUrl,variantSize,variantColor,variantSku,variantPrice,variantStock,variantQikinkSku
"Classic T-Shirt","classic-t-shirt","A comfortable cotton t-shirt","Soft cotton tee","TS-BLK-S",499,699,200,"BrandX","tshirt,cotton",false,true,"","Classic T-Shirt SEO","Buy classic t-shirt online",false,"","","","","S","Black","TS-BLK-S",499,50,""
"Classic T-Shirt","classic-t-shirt","A comfortable cotton t-shirt","Soft cotton tee","TS-BLK-M",499,699,200,"BrandX","tshirt,cotton",false,true,"","Classic T-Shirt SEO","Buy classic t-shirt online",false,"","","","","M","Black","TS-BLK-M",499,45,""
"Classic T-Shirt","classic-t-shirt","A comfortable cotton t-shirt","Soft cotton tee","TS-WHT-S",499,699,200,"BrandX","tshirt,cotton",false,true,"","Classic T-Shirt SEO","Buy classic t-shirt online",false,"","","","","S","White","TS-WHT-S",499,30,""
"Premium Hoodie","premium-hoodie","A warm fleece hoodie","Cozy fleece hoodie","HD-GRY-L",1299,1599,500,"BrandX","hoodie,winter",true,true,"","Premium Hoodie SEO","Buy premium hoodie",true,"QK-12345","Premium Hoodie","","","L","Grey","HD-GRY-L",1299,20,"QK-HD-GL"
"Premium Hoodie","premium-hoodie","A warm fleece hoodie","Cozy fleece hoodie","HD-GRY-XL",1299,1599,500,"BrandX","hoodie,winter",true,true,"","Premium Hoodie SEO","Buy premium hoodie",true,"QK-12345","Premium Hoodie","","","XL","Grey","HD-GRY-XL",1299,15,"QK-HD-GXL"`;

interface ParsedRow {
  rowIndex: number;
  data: Record<string, string>;
  errors: string[];
}

interface ValidationIssue {
  field: string;
  message: string;
}

interface PreviewProduct extends BulkImportProduct {
  rowIndex: number;
  validationErrors: string[];
  isValid: boolean;
}

function parseCSV(text: string): { headers: string[]; rows: Record<string, string>[] } {
  const lines: string[] = [];
  let current = "";
  let inQuotes = false;

  for (let i = 0; i < text.length; i++) {
    const char = text[i];
    if (char === '"') {
      if (inQuotes && i + 1 < text.length && text[i + 1] === '"') {
        current += '"';
        i++;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (char === "," && !inQuotes) {
      lines.push(current);
      current = "";
    } else if ((char === "\n" || char === "\r") && !inQuotes) {
      if (char === "\r" && i + 1 < text.length && text[i + 1] === "\n") i++;
      lines.push(current);
      current = "";
      if (lines.length > 0) break;
    } else {
      current += char;
    }
  }
  if (current || lines.length > 0) {
    lines.push(current);
  }

  const allLines: string[][] = [];
  let buffer: string[] = [];
  inQuotes = false;
  current = "";

  for (let i = 0; i < text.length; i++) {
    const char = text[i];
    if (char === '"') {
      if (inQuotes && i + 1 < text.length && text[i + 1] === '"') {
        current += '"';
        i++;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (char === "," && !inQuotes) {
      buffer.push(current);
      current = "";
    } else if ((char === "\n" || char === "\r") && !inQuotes) {
      if (char === "\r" && i + 1 < text.length && text[i + 1] === "\n") i++;
      buffer.push(current);
      current = "";
      if (buffer.length > 0) allLines.push(buffer);
      buffer = [];
    } else {
      current += char;
    }
  }
  buffer.push(current);
  if (buffer.length > 0 && !(buffer.length === 1 && buffer[0] === "")) {
    allLines.push(buffer);
  }

  if (allLines.length === 0) return { headers: [], rows: [] };

  const headers = allLines[0].map((h) => h.trim());
  const rows: Record<string, string>[] = [];

  for (let i = 1; i < allLines.length; i++) {
    const row: Record<string, string> = {};
    headers.forEach((header, idx) => {
      row[header] = (allLines[i][idx] || "").trim();
    });
    rows.push(row);
  }

  return { headers, rows };
}

function toNumber(val: string | undefined, fallback: number = 0): number {
  if (!val || val === "") return fallback;
  const n = parseFloat(val);
  return isNaN(n) ? fallback : n;
}

function toBool(val: string | undefined): boolean {
  if (!val) return false;
  return val.toLowerCase() === "true" || val === "1" || val.toLowerCase() === "yes";
}

function parseVariantsFromRow(row: Record<string, string>): BulkImportVariant[] {
  const variants: BulkImportVariant[] = [];

  const size = row.variantSize || "";
  const color = row.variantColor || "";
  const sku = row.variantSku || "";
  const price = toNumber(row.variantPrice, 0.01);
  const stock = toNumber(row.variantStock, 0);
  const qikinkSku = row.variantQikinkSku || "";

  if (sku) {
    variants.push({
      size: size || undefined,
      color: color || undefined,
      sku,
      price: price < 0.01 ? 0.01 : price,
      stock: Math.max(0, Math.floor(stock)),
      isAvailable: true,
      qikinkSku: qikinkSku || undefined,
    });
  }

  return variants;
}

function validateProduct(product: BulkImportProduct, index: number): string[] {
  const errors: string[] = [];

  if (!product.name.trim()) errors.push("Name is required");
  if (!product.sku.trim()) errors.push("SKU is required");
  if (product.price < 0.01) errors.push("Price must be at least 0.01");
  if (product.slug && /[^a-z0-9-]/.test(product.slug)) {
    errors.push("Slug should only contain lowercase letters, numbers, and hyphens");
  }
  if (product.variants.length === 0) {
    errors.push("At least one variant with a SKU is required");
  }
  const variantSkus = product.variants.map((v) => v.sku);
  const duplicateSkus = variantSkus.filter((sku, i) => variantSkus.indexOf(sku) !== i);
  if (duplicateSkus.length > 0) {
    errors.push(`Duplicate variant SKUs: ${duplicateSkus.join(", ")}`);
  }

  return errors;
}

function csvRowToProduct(row: Record<string, string>, index: number): PreviewProduct {
  const product: BulkImportProduct = {
    name: row.name || "",
    slug: row.slug || "",
    description: row.description || "",
    shortDescription: row.shortDescription || undefined,
    sku: row.sku || "",
    price: toNumber(row.price, 0.01),
    comparePrice: row.comparePrice ? toNumber(row.comparePrice) : undefined,
    costPrice: row.costPrice ? toNumber(row.costPrice) : undefined,
    brand: row.brand || undefined,
    tags: row.tags || undefined,
    isFeatured: toBool(row.isFeatured),
    isPublished: toBool(row.isPublished),
    categoryId: row.categoryId || undefined,
    seoTitle: row.seoTitle || undefined,
    seoDescription: row.seoDescription || undefined,
    isQikinkProduct: toBool(row.isQikinkProduct),
    qikinkProductId: row.qikinkProductId || undefined,
    qikinkProductName: row.qikinkProductName || undefined,
    designReference: row.designReference || undefined,
    designFileUrl: row.designFileUrl || undefined,
    mockupUrl: row.mockupUrl || undefined,
    variants: parseVariantsFromRow(row),
  };

  const validationErrors = validateProduct(product, index);

  return {
    ...product,
    rowIndex: index + 2,
    validationErrors,
    isValid: validationErrors.length === 0,
  };
}

export default function BulkImportPage() {
  const [step, setStep] = useState<"upload" | "preview" | "results">("upload");
  const [categories, setCategories] = useState<Category[]>([]);
  const [csvFile, setCsvFile] = useState<File | null>(null);
  const [csvText, setCsvText] = useState("");
  const [parseError, setParseError] = useState("");
  const [products, setProducts] = useState<PreviewProduct[]>([]);
  const [importing, setImporting] = useState(false);
  const [importResult, setImportResult] = useState<BulkImportResponse | null>(null);
  const [selectAll, setSelectAll] = useState(true);
  const [selectedRows, setSelectedRows] = useState<Set<number>>(new Set());
  const fileInputRef = useRef<HTMLInputElement>(null);

  useState(() => {
    adminApi.categories.list({ pageSize: 100 }).then((res) => setCategories(res.items));
  });

  const handleDownloadTemplate = useCallback(() => {
    const blob = new Blob([CSV_TEMPLATE], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = "product_import_template.csv";
    link.click();
    URL.revokeObjectURL(url);
    toast.success("Template downloaded");
  }, []);

  const processCSV = useCallback(
    (text: string) => {
      setParseError("");
      try {
        const { headers, rows } = parseCSV(text);
        if (rows.length === 0) {
          setParseError("No data rows found in the CSV file");
          return;
        }
        const missingHeaders = CSV_HEADERS.filter((h) => !headers.includes(h));
        if (missingHeaders.length > 0) {
          setParseError(`Missing required columns: ${missingHeaders.join(", ")}`);
          return;
        }

        const grouped = new Map<string, { firstRow: Record<string, string>; rows: Record<string, string>[]; startIndex: number }>();
        for (let i = 0; i < rows.length; i++) {
          const row = rows[i];
          const name = (row.name || "").trim();
          const slug = (row.slug || "").trim();
          const key = `${name}|||${slug}`;
          if (!grouped.has(key)) {
            grouped.set(key, { firstRow: row, rows: [], startIndex: i });
          }
          grouped.get(key)!.rows.push(row);
        }

        const parsed: PreviewProduct[] = [];
        let groupIndex = 0;
        for (const [, group] of grouped) {
          const firstRow = group.firstRow;
          const allVariants: BulkImportVariant[] = [];
          const seenVariantSkus = new Set<string>();

          for (const row of group.rows) {
            const v = parseVariantsFromRow(row);
            for (const variant of v) {
              if (!seenVariantSkus.has(variant.sku)) {
                seenVariantSkus.add(variant.sku);
                allVariants.push(variant);
              }
            }
          }

          const product: BulkImportProduct = {
            name: firstRow.name || "",
            slug: firstRow.slug || "",
            description: firstRow.description || "",
            shortDescription: firstRow.shortDescription || undefined,
            sku: firstRow.sku || "",
            price: toNumber(firstRow.price, 0.01),
            comparePrice: firstRow.comparePrice ? toNumber(firstRow.comparePrice) : undefined,
            costPrice: firstRow.costPrice ? toNumber(firstRow.costPrice) : undefined,
            brand: firstRow.brand || undefined,
            tags: firstRow.tags || undefined,
            isFeatured: toBool(firstRow.isFeatured),
            isPublished: toBool(firstRow.isPublished),
            categoryId: firstRow.categoryId || undefined,
            seoTitle: firstRow.seoTitle || undefined,
            seoDescription: firstRow.seoDescription || undefined,
            isQikinkProduct: toBool(firstRow.isQikinkProduct),
            qikinkProductId: firstRow.qikinkProductId || undefined,
            qikinkProductName: firstRow.qikinkProductName || undefined,
            designReference: firstRow.designReference || undefined,
            designFileUrl: firstRow.designFileUrl || undefined,
            mockupUrl: firstRow.mockupUrl || undefined,
            variants: allVariants,
          };

          const validationErrors = validateProduct(product, groupIndex);
          parsed.push({
            ...product,
            rowIndex: group.startIndex + 2,
            validationErrors,
            isValid: validationErrors.length === 0,
          });
          groupIndex++;
        }

        setProducts(parsed);
        setSelectedRows(new Set(parsed.filter((p) => p.isValid).map((_, i) => i)));
        setStep("preview");
        toast.success(`Parsed ${rows.length} CSV rows into ${parsed.length} products`);
      } catch {
        setParseError("Failed to parse CSV file. Please check the format.");
      }
    },
    []
  );

  const handleFileUpload = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (!file) return;

      if (!file.name.endsWith(".csv")) {
        toast.error("Please upload a CSV file");
        return;
      }

      if (file.size > 10 * 1024 * 1024) {
        toast.error("File size must be less than 10MB");
        return;
      }

      setCsvFile(file);
      const reader = new FileReader();
      reader.onload = (event) => {
        const text = event.target?.result as string;
        setCsvText(text);
        processCSV(text);
      };
      reader.readAsText(file);
    },
    [processCSV]
  );

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      const file = e.dataTransfer.files[0];
      if (file && file.name.endsWith(".csv")) {
        setCsvFile(file);
        const reader = new FileReader();
        reader.onload = (event) => {
          const text = event.target?.result as string;
          setCsvText(text);
          processCSV(text);
        };
        reader.readAsText(file);
      }
    },
    [processCSV]
  );

  const handleToggleRow = useCallback((index: number) => {
    setSelectedRows((prev) => {
      const next = new Set(prev);
      if (next.has(index)) next.delete(index);
      else next.add(index);
      return next;
    });
  }, []);

  const handleToggleSelectAll = useCallback(() => {
    if (selectAll) {
      setSelectedRows(new Set());
    } else {
      setSelectedRows(new Set(products.map((_, i) => i)));
    }
    setSelectAll(!selectAll);
  }, [selectAll, products]);

  const handleRemoveRow = useCallback(
    (index: number) => {
      setProducts((prev) => prev.filter((_, i) => i !== index));
      setSelectedRows((prev) => {
        const next = new Set(prev);
        next.delete(index);
        const updated = new Set<number>();
        let shift = 0;
        for (let i = 0; i < products.length; i++) {
          if (i === index) {
            shift = -1;
            continue;
          }
          if (next.has(i)) updated.add(i + shift);
        }
        return updated;
      });
    },
    [products]
  );

  const handleEditField = useCallback(
    (index: number, field: string, value: unknown) => {
      setProducts((prev) =>
        prev.map((p, i) => {
          if (i !== index) return p;
          const updated = { ...p, [field]: value };
          const errors = validateProduct(updated, i);
          return { ...updated, validationErrors: errors, isValid: errors.length === 0 };
        })
      );
    },
    []
  );

  const handleImport = useCallback(async () => {
    const selected = products.filter((_, i) => selectedRows.has(i));
    const validProducts = selected.filter((p) => p.isValid);

    if (validProducts.length === 0) {
      toast.error("No valid products to import");
      return;
    }

    setImporting(true);
    try {
      const payload = validProducts.map((p) => ({
        name: p.name,
        slug: p.slug,
        description: p.description,
        shortDescription: p.shortDescription,
        sku: p.sku,
        price: p.price,
        comparePrice: p.comparePrice,
        costPrice: p.costPrice,
        brand: p.brand,
        tags: p.tags,
        isFeatured: p.isFeatured,
        isPublished: p.isPublished,
        categoryId: p.categoryId,
        seoTitle: p.seoTitle,
        seoDescription: p.seoDescription,
        isQikinkProduct: p.isQikinkProduct,
        qikinkProductId: p.qikinkProductId,
        qikinkProductName: p.qikinkProductName,
        designReference: p.designReference,
        designFileUrl: p.designFileUrl,
        mockupUrl: p.mockupUrl,
        variants: p.variants,
      }));

      const result = await adminApi.products.bulkImport(payload);
      setImportResult(result);
      setStep("results");

      if (result.failureCount === 0) {
        toast.success(`All ${result.successCount} products imported successfully!`);
      } else {
        toast.warning(`Import complete: ${result.successCount} succeeded, ${result.failureCount} failed`);
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Import failed";
      toast.error(msg);
    } finally {
      setImporting(false);
    }
  }, [products, selectedRows]);

  const handleReset = useCallback(() => {
    setStep("upload");
    setCsvFile(null);
    setCsvText("");
    setParseError("");
    setProducts([]);
    setImportResult(null);
    setSelectedRows(new Set());
    setSelectAll(true);
    if (fileInputRef.current) fileInputRef.current.value = "";
  }, []);

  const validCount = products.filter((p) => p.isValid).length;
  const invalidCount = products.filter((p) => !p.isValid).length;
  const selectedValidCount = products.filter((_, i) => selectedRows.has(i) && products[i].isValid).length;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/admin/products" className={buttonVariants({ variant: "ghost", size: "icon" })}>
          <ArrowLeft className="h-4 w-4" />
        </Link>
        <div>
          <h1 className="text-2xl font-bold">Bulk Import Products</h1>
          <p className="text-sm text-muted-foreground">Import multiple products at once using a CSV file</p>
        </div>
      </div>

      <Tabs value={step} className="space-y-6">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="upload" disabled={step === "results"}>
            <Upload className="mr-2 h-4 w-4" />
            Upload
          </TabsTrigger>
          <TabsTrigger value="preview" disabled={step === "upload" || step === "results"}>
            <FileSpreadsheet className="mr-2 h-4 w-4" />
            Preview & Edit
          </TabsTrigger>
          <TabsTrigger value="results" disabled={step !== "results"}>
            <CheckCircle2 className="mr-2 h-4 w-4" />
            Results
          </TabsTrigger>
        </TabsList>

        <TabsContent value="upload" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Download Template</CardTitle>
              <CardDescription>
                Download the CSV template with all supported columns and sample data
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button variant="outline" onClick={handleDownloadTemplate}>
                <Download className="mr-2 h-4 w-4" />
                Download CSV Template
              </Button>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Upload CSV File</CardTitle>
              <CardDescription>
                Upload a CSV file with your product data. Maximum 500 products per import.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div
                className="flex flex-col items-center justify-center rounded-lg border-2 border-dashed p-8 transition-colors hover:border-primary/50 hover:bg-muted/50 cursor-pointer"
                onDrop={handleDrop}
                onDragOver={(e) => e.preventDefault()}
                onClick={() => fileInputRef.current?.click()}
              >
                <FileSpreadsheet className="h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-lg font-medium">
                  {csvFile ? csvFile.name : "Drop your CSV file here or click to browse"}
                </p>
                <p className="text-sm text-muted-foreground mt-1">
                  Supports .csv files up to 10MB
                </p>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept=".csv"
                  className="hidden"
                  onChange={handleFileUpload}
                />
              </div>

              {parseError && (
                <div className="flex items-center gap-2 rounded-md bg-destructive/10 p-3 text-sm text-destructive">
                  <XCircle className="h-4 w-4 shrink-0" />
                  {parseError}
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>CSV Column Reference</CardTitle>
              <CardDescription>All supported columns and their formats</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="py-2 px-3 text-left font-medium">Column</th>
                      <th className="py-2 px-3 text-left font-medium">Required</th>
                      <th className="py-2 px-3 text-left font-medium">Type</th>
                      <th className="py-2 px-3 text-left font-medium">Description</th>
                    </tr>
                  </thead>
                  <tbody>
                    {[
                      { col: "name", req: true, type: "string", desc: "Product name" },
                      { col: "slug", req: false, type: "string", desc: "URL slug (auto-generated from name if empty)" },
                      { col: "description", req: false, type: "string", desc: "Full product description" },
                      { col: "shortDescription", req: false, type: "string", desc: "Brief description" },
                      { col: "sku", req: true, type: "string", desc: "Unique product SKU" },
                      { col: "price", req: true, type: "number", desc: "Selling price (min 0.01)" },
                      { col: "comparePrice", req: false, type: "number", desc: "Original/strikethrough price" },
                      { col: "costPrice", req: false, type: "number", desc: "Cost price for margin calculation" },
                      { col: "brand", req: false, type: "string", desc: "Brand name" },
                      { col: "tags", req: false, type: "string", desc: "Comma-separated tags" },
                      { col: "isFeatured", req: false, type: "boolean", desc: "true/false" },
                      { col: "isPublished", req: false, type: "boolean", desc: "true/false" },
                      { col: "categoryId", req: false, type: "uuid", desc: "Category ID (UUID)" },
                      { col: "seoTitle", req: false, type: "string", desc: "SEO title" },
                      { col: "seoDescription", req: false, type: "string", desc: "SEO description" },
                      { col: "isQikinkProduct", req: false, type: "boolean", desc: "Enable Qikink fulfillment" },
                      { col: "qikinkProductId", req: false, type: "string", desc: "Qikink product ID (required if isQikinkProduct=true)" },
                      { col: "qikinkProductName", req: false, type: "string", desc: "Qikink product name" },
                      { col: "designReference", req: false, type: "string", desc: "Design reference/SKU" },
                      { col: "designFileUrl", req: false, type: "string", desc: "Design file URL" },
                      { col: "mockupUrl", req: false, type: "string", desc: "Mockup image URL" },
                      { col: "variantSize", req: false, type: "string", desc: "Variant size" },
                      { col: "variantColor", req: false, type: "string", desc: "Variant color" },
                      { col: "variantSku", req: false, type: "string", desc: "Variant SKU (creates default variant if empty)" },
                      { col: "variantPrice", req: false, type: "number", desc: "Variant price (falls back to product price)" },
                      { col: "variantStock", req: false, type: "number", desc: "Variant stock quantity" },
                      { col: "variantQikinkSku", req: false, type: "string", desc: "Qikink SKU for this variant" },
                    ].map((row) => (
                      <tr key={row.col} className="border-b last:border-0">
                        <td className="py-2 px-3 font-mono text-xs">{row.col}</td>
                        <td className="py-2 px-3">
                          {row.req ? (
                            <Badge variant="destructive" className="text-xs">Required</Badge>
                          ) : (
                            <Badge variant="secondary" className="text-xs">Optional</Badge>
                          )}
                        </td>
                        <td className="py-2 px-3 text-muted-foreground">{row.type}</td>
                        <td className="py-2 px-3 text-muted-foreground">{row.desc}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="preview" className="space-y-6">
          <div className="flex flex-wrap items-center justify-between gap-4">
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <Checkbox
                  checked={selectAll}
                  onCheckedChange={handleToggleSelectAll}
                />
                <label className="text-sm font-medium">Select All</label>
              </div>
              <Badge variant="default">{selectedRows.size} selected</Badge>
              <Badge variant="destructive">{invalidCount} errors</Badge>
            </div>
            <div className="flex items-center gap-2">
              <Button variant="outline" onClick={handleReset}>
                <RotateCcw className="mr-2 h-4 w-4" />
                Start Over
              </Button>
              <Button
                onClick={handleImport}
                disabled={importing || selectedRows.size === 0}
              >
                {importing ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                  <Package className="mr-2 h-4 w-4" />
                )}
                Import {selectedValidCount} Products
              </Button>
            </div>
          </div>

          <Card>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b bg-muted/50">
                      <th className="py-3 px-3 w-10"></th>
                      <th className="py-3 px-3 text-left font-medium">Row</th>
                      <th className="py-3 px-3 text-left font-medium">Name</th>
                      <th className="py-3 px-3 text-left font-medium">SKU</th>
                      <th className="py-3 px-3 text-left font-medium">Price</th>
                      <th className="py-3 px-3 text-left font-medium">Variants</th>
                      <th className="py-3 px-3 text-left font-medium">Status</th>
                      <th className="py-3 px-3 text-left font-medium">Issues</th>
                      <th className="py-3 px-3 w-10"></th>
                    </tr>
                  </thead>
                  <tbody>
                    {products.map((product, i) => (
                      <tr
                        key={i}
                        className={`border-b last:border-0 ${
                          !product.isValid ? "bg-destructive/5" : ""
                        }`}
                      >
                        <td className="py-3 px-3">
                          <Checkbox
                            checked={selectedRows.has(i)}
                            onCheckedChange={() => handleToggleRow(i)}
                          />
                        </td>
                        <td className="py-3 px-3 text-muted-foreground">
                          {product.rowIndex}
                        </td>
                        <td className="py-3 px-3">
                          <Input
                            value={product.name}
                            onChange={(e) => handleEditField(i, "name", e.target.value)}
                            className="h-8 text-xs"
                          />
                        </td>
                        <td className="py-3 px-3">
                          <Input
                            value={product.sku}
                            onChange={(e) => handleEditField(i, "sku", e.target.value)}
                            className="h-8 text-xs font-mono"
                          />
                        </td>
                        <td className="py-3 px-3">
                          <Input
                            type="number"
                            value={product.price}
                            onChange={(e) =>
                              handleEditField(i, "price", parseFloat(e.target.value) || 0.01)
                            }
                            className="h-8 text-xs w-24"
                          />
                        </td>
                        <td className="py-3 px-3">
                          <Badge variant="outline">
                            {product.variants.length} variant{product.variants.length !== 1 ? "s" : ""}
                          </Badge>
                          {products.length > 0 && (
                            <span className="ml-2 text-xs text-muted-foreground">
                              {product.variants.length > 0 && (() => {
                                const sizes = [...new Set(product.variants.map(v => v.size).filter(Boolean))];
                                const colors = [...new Set(product.variants.map(v => v.color).filter(Boolean))];
                                const parts: string[] = [];
                                if (sizes.length > 0) parts.push(`${sizes.length} size${sizes.length > 1 ? "s" : ""}`);
                                if (colors.length > 0) parts.push(`${colors.length} color${colors.length > 1 ? "s" : ""}`);
                                return parts.length > 0 ? parts.join(", ") : null;
                              })()}
                            </span>
                          )}
                        </td>
                        <td className="py-3 px-3">
                          {product.isValid ? (
                            <Badge variant="default" className="bg-green-100 text-green-800 border-green-200">
                              <CheckCircle2 className="mr-1 h-3 w-3" />
                              Valid
                            </Badge>
                          ) : (
                            <Badge variant="destructive">
                              <XCircle className="mr-1 h-3 w-3" />
                              Error
                            </Badge>
                          )}
                        </td>
                        <td className="py-3 px-3">
                          {product.validationErrors.length > 0 ? (
                            <div className="space-y-1">
                              {product.validationErrors.map((err, j) => (
                                <div key={j} className="flex items-center gap-1 text-xs text-destructive">
                                  <AlertTriangle className="h-3 w-3 shrink-0" />
                                  {err}
                                </div>
                              ))}
                            </div>
                          ) : (
                            <span className="text-xs text-muted-foreground">No issues</span>
                          )}
                        </td>
                        <td className="py-3 px-3">
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-7 w-7"
                            onClick={() => handleRemoveRow(i)}
                          >
                            <Trash2 className="h-3.5 w-3.5 text-destructive" />
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {products.length === 0 && (
                <div className="flex flex-col items-center justify-center p-12 text-center">
                  <FileSpreadsheet className="h-12 w-12 text-muted-foreground mb-4" />
                  <p className="text-lg font-medium">No products to preview</p>
                  <p className="text-sm text-muted-foreground">
                    Go back and upload a CSV file
                  </p>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Summary</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid gap-4 md:grid-cols-4">
                <div className="text-center">
                  <div className="text-2xl font-bold">{products.length}</div>
                  <div className="text-sm text-muted-foreground">Products</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-green-600">{validCount}</div>
                  <div className="text-sm text-muted-foreground">Valid</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-destructive">{invalidCount}</div>
                  <div className="text-sm text-muted-foreground">Errors</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-primary">{selectedRows.size}</div>
                  <div className="text-sm text-muted-foreground">Selected</div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="results" className="space-y-6">
          {importResult && (
            <>
              <Card>
                <CardHeader>
                  <CardTitle>Import Results</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid gap-4 md:grid-cols-3">
                    <div className="text-center">
                      <div className="text-3xl font-bold">{importResult.totalRows}</div>
                      <div className="text-sm text-muted-foreground">Total Processed</div>
                    </div>
                    <div className="text-center">
                      <div className="text-3xl font-bold text-green-600">{importResult.successCount}</div>
                      <div className="text-sm text-muted-foreground">Imported</div>
                    </div>
                    <div className="text-center">
                      <div className="text-3xl font-bold text-destructive">{importResult.failureCount}</div>
                      <div className="text-sm text-muted-foreground">Failed</div>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {importResult.results.length > 0 && (
                <Card>
                  <CardHeader>
                    <CardTitle>Detailed Results</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className="border-b">
                            <th className="py-2 px-3 text-left font-medium">Row</th>
                            <th className="py-2 px-3 text-left font-medium">Product</th>
                            <th className="py-2 px-3 text-left font-medium">Status</th>
                            <th className="py-2 px-3 text-left font-medium">Details</th>
                          </tr>
                        </thead>
                        <tbody>
                          {importResult.results.map((result, i) => (
                            <tr key={i} className={`border-b last:border-0 ${!result.success ? "bg-destructive/5" : ""}`}>
                              <td className="py-2 px-3">{result.rowNumber}</td>
                              <td className="py-2 px-3 font-medium">{result.productName}</td>
                              <td className="py-2 px-3">
                                {result.success ? (
                                  <Badge variant="default" className="bg-green-100 text-green-800 border-green-200">
                                    <CheckCircle2 className="mr-1 h-3 w-3" />
                                    Success
                                  </Badge>
                                ) : (
                                  <Badge variant="destructive">
                                    <XCircle className="mr-1 h-3 w-3" />
                                    Failed
                                  </Badge>
                                )}
                              </td>
                              <td className="py-2 px-3">
                                {result.errorMessage && (
                                  <div className="text-xs text-destructive">{result.errorMessage}</div>
                                )}
                                {result.warnings.map((w, j) => (
                                  <div key={j} className="flex items-center gap-1 text-xs text-amber-600">
                                    <AlertTriangle className="h-3 w-3 shrink-0" />
                                    {w}
                                  </div>
                                ))}
                                {result.productId && (
                                  <div className="text-xs text-muted-foreground mt-1">
                                    ID: {result.productId.slice(0, 8)}...
                                  </div>
                                )}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </CardContent>
                </Card>
              )}

              <div className="flex justify-end gap-4">
                <Button variant="outline" onClick={handleReset}>
                  <RotateCcw className="mr-2 h-4 w-4" />
                  Import More Products
                </Button>
                <Link href="/admin/products">
                  <Button>
                    View Products
                    <ArrowRight className="ml-2 h-4 w-4" />
                  </Button>
                </Link>
              </div>
            </>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
