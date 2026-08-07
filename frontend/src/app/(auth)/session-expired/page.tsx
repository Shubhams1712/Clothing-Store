import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Clock } from "lucide-react";

export default function SessionExpiredPage() {
  return (
    <div className="flex min-h-[60vh] items-center justify-center py-12">
      <Card className="w-full max-w-md text-center">
        <CardHeader>
          <Clock className="mx-auto h-12 w-12 text-muted-foreground" />
          <CardTitle className="text-2xl">Session expired</CardTitle>
          <CardDescription>
            Your session has expired. Please sign in again.
          </CardDescription>
        </CardHeader>
        <CardFooter className="flex justify-center">
          <Link href="/login">
            <Button>Sign in</Button>
          </Link>
        </CardFooter>
      </Card>
    </div>
  );
}
