import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { ShieldOff } from "lucide-react";

export default function UnauthorizedPage() {
  return (
    <div className="flex min-h-[60vh] items-center justify-center py-12">
      <Card className="w-full max-w-md text-center">
        <CardHeader>
          <ShieldOff className="mx-auto h-12 w-12 text-destructive" />
          <CardTitle className="text-2xl">Access denied</CardTitle>
          <CardDescription>
            You don&apos;t have permission to access this page.
          </CardDescription>
        </CardHeader>
        <CardFooter className="flex justify-center gap-4">
          <Link href="/">
            <Button>Go home</Button>
          </Link>
        </CardFooter>
      </Card>
    </div>
  );
}
