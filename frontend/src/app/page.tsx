"use client";

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { HealthResponse } from "@/types/api";
import { API_CONFIG } from "@/config/api";
import { LoadingOverlay } from "@/components/feedback/loading-overlay";
import { ErrorBoundary } from "@/components/feedback/error-boundary";
import { SectionWrapper } from "@/components/layout/section-wrapper";
import { Badge } from "@/components/ui/badge";
import { CheckCircle, XCircle } from "lucide-react";

export default function Home() {
  const { data, isLoading, error, refetch } = useQuery<HealthResponse>({
    queryKey: ["health"],
    queryFn: async () => {
      const response = await api.get<HealthResponse>(API_CONFIG.ENDPOINTS.HEALTH);
      return response.data;
    },
  });

  return (
    <SectionWrapper
      title="Platform Status"
      description="Verifying connection between frontend and backend."
      className="py-20"
    >
      <div className="flex flex-col items-center gap-8">
        {isLoading && <LoadingOverlay text="Connecting to backend..." />}

        {error && (
          <div className="flex flex-col items-center gap-4">
            <ErrorBoundary
              message="Unable to connect to the backend API"
              onRetry={() => refetch()}
            />
            <p className="text-sm text-muted-foreground">
              Ensure the backend is running at {API_CONFIG.BASE_URL}
            </p>
          </div>
        )}

        {data && (
          <div className="flex flex-col items-center gap-4 rounded-lg border border-border p-8 shadow-sm">
            <div className="flex items-center gap-2">
              {data.status === "Healthy" ? (
                <CheckCircle className="h-6 w-6 text-green-600" />
              ) : (
                <XCircle className="h-6 w-6 text-destructive" />
              )}
              <h3 className="text-xl font-semibold">{data.status}</h3>
            </div>
            <div className="text-center text-sm text-muted-foreground">
              <p>Service: {data.service}</p>
              <p>Timestamp: {data.timestamp}</p>
            </div>
            <Badge variant="outline">Backend Connected</Badge>
          </div>
        )}
      </div>
    </SectionWrapper>
  );
}
