"use client";

import { useAuth } from "@/hooks/use-auth";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

export default function AdminProfilePage() {
  const { user } = useAuth();

  if (!user) return null;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Profile</h1>
      <Card>
        <CardHeader><CardTitle>Account Information</CardTitle></CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div><p className="text-sm text-muted-foreground">First Name</p><p className="font-medium">{user.firstName}</p></div>
            <div><p className="text-sm text-muted-foreground">Last Name</p><p className="font-medium">{user.lastName}</p></div>
          </div>
          <div><p className="text-sm text-muted-foreground">Email</p><p className="font-medium">{user.email}</p></div>
          <div>
            <p className="text-sm text-muted-foreground">Roles</p>
            <div className="flex gap-2 mt-1">
              {user.roles?.map(role => <Badge key={role} variant="outline">{role}</Badge>)}
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
