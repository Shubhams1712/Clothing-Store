"use client";

import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { Download } from "lucide-react";

const REPORT_TYPES = [
  { label: "Sales Report", value: "sales" },
  { label: "Product Report", value: "products" },
  { label: "Customer Report", value: "customers" },
  { label: "Inventory Report", value: "inventory" },
  { label: "Order Report", value: "orders" },
];

const EXPORT_FORMATS = [
  { label: "CSV", value: "csv" },
  { label: "Excel", value: "excel" },
];

export default function ReportsPage() {
  const [reportType, setReportType] = useState("sales");
  const [format, setFormat] = useState("csv");

  const exportMutation = useMutation({
    mutationFn: () => adminApi.analytics.exportReport({ reportType, format }),
    onSuccess: (data) => {
      const blob = new Blob([data]);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `${reportType}-report.${format === "excel" ? "xlsx" : "csv"}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      toast.success("Report exported successfully");
    },
    onError: () => {
      toast.error("Failed to export report");
    },
  });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Reports</h1>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Export Report</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Report Type</label>
              <div className="flex flex-wrap gap-2">
                {REPORT_TYPES.map((type) => (
                  <button
                    key={type.value}
                    onClick={() => setReportType(type.value)}
                    className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                      reportType === type.value
                        ? "bg-primary text-primary-foreground"
                        : "bg-muted text-muted-foreground hover:text-foreground"
                    }`}
                  >
                    {type.label}
                  </button>
                ))}
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Format</label>
              <div className="flex gap-2">
                {EXPORT_FORMATS.map((f) => (
                  <button
                    key={f.value}
                    onClick={() => setFormat(f.value)}
                    className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                      format === f.value
                        ? "bg-primary text-primary-foreground"
                        : "bg-muted text-muted-foreground hover:text-foreground"
                    }`}
                  >
                    {f.label}
                  </button>
                ))}
              </div>
            </div>

            <Button
              onClick={() => exportMutation.mutate()}
              disabled={exportMutation.isPending}
              className="w-full"
            >
              <Download className="mr-2 h-4 w-4" />
              {exportMutation.isPending ? "Exporting..." : "Export Report"}
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Available Reports</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {REPORT_TYPES.map((type) => (
                <div key={type.value} className="flex items-center justify-between rounded-lg border p-3">
                  <div>
                    <p className="text-sm font-medium">{type.label}</p>
                    <p className="text-xs text-muted-foreground">
                      Export as CSV or Excel
                    </p>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setReportType(type.value);
                      exportMutation.mutate();
                    }}
                    disabled={exportMutation.isPending}
                  >
                    <Download className="mr-1 h-3 w-3" />
                    Export
                  </Button>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
