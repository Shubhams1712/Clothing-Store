"use client";

import { useEffect } from "react";

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error("Global error:", error);
  }, [error]);

  return (
    <html lang="en">
      <body>
        <div
          style={{
            display: "flex",
            minHeight: "100vh",
            alignItems: "center",
            justifyContent: "center",
            fontFamily:
              'system-ui, -apple-system, "Segoe UI", Roboto, sans-serif',
            backgroundColor: "#fafafa",
            color: "#1a1a1a",
          }}
        >
          <div
            style={{
              maxWidth: 420,
              width: "100%",
              padding: 40,
              textAlign: "center",
            }}
          >
            <h1
              style={{
                fontSize: 24,
                fontWeight: 600,
                marginBottom: 12,
              }}
            >
              Something went wrong
            </h1>
            <p
              style={{
                fontSize: 14,
                color: "#666",
                marginBottom: 24,
                lineHeight: 1.6,
              }}
            >
              An unexpected error occurred. Please try again.
            </p>
            <button
              onClick={() => reset()}
              style={{
                padding: "10px 24px",
                fontSize: 14,
                fontWeight: 500,
                color: "#fff",
                backgroundColor: "#1a1a1a",
                border: "none",
                borderRadius: 6,
                cursor: "pointer",
              }}
            >
              Try again
            </button>
          </div>
        </div>
      </body>
    </html>
  );
}
