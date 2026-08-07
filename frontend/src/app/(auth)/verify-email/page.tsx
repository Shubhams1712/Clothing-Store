"use client";

import { Suspense, useEffect, useState, startTransition } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { authApi } from "@/services/auth";
import { Button } from "@/components/ui/button";
import { Card, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { CheckCircle, XCircle } from "lucide-react";

function VerifyEmailContent() {
  const [status, setStatus] = useState<"loading" | "success" | "error">("loading");
  const [message, setMessage] = useState("");
  const searchParams = useSearchParams();
  const token = searchParams.get("token") || "";
  const email = searchParams.get("email") || "";

  useEffect(() => {
    if (!token || !email) {
      startTransition(() => {
        setStatus("error");
        setMessage("Invalid verification link");
      });
      return;
    }

    authApi
      .verifyEmail({ token, email })
      .then(() => {
        startTransition(() => {
          setStatus("success");
          setMessage("Email verified successfully!");
        });
      })
      .catch((err) => {
        startTransition(() => {
          setStatus("error");
          setMessage(err instanceof Error ? err.message : "Verification failed");
        });
      });
  }, [token, email]);

  return (
    <div className="flex min-h-[60vh] items-center justify-center py-12">
      <Card className="w-full max-w-md text-center">
        <CardHeader>
          {status === "loading" && (
            <div className="mx-auto h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent" />
          )}
          {status === "success" && <CheckCircle className="mx-auto h-12 w-12 text-green-600" />}
          {status === "error" && <XCircle className="mx-auto h-12 w-12 text-destructive" />}
          <CardTitle className="text-2xl">
            {status === "loading" && "Verifying email..."}
            {status === "success" && "Email verified"}
            {status === "error" && "Verification failed"}
          </CardTitle>
          <CardDescription>{message}</CardDescription>
        </CardHeader>
        <CardFooter className="flex justify-center">
          <Link href={status === "success" ? "/login" : "/register"}>
            <Button>{status === "success" ? "Sign in" : "Back to register"}</Button>
          </Link>
        </CardFooter>
      </Card>
    </div>
  );
}

export default function VerifyEmailPage() {
  return (
    <Suspense fallback={<div className="flex min-h-[60vh] items-center justify-center"><div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" /></div>}>
      <VerifyEmailContent />
    </Suspense>
  );
}
