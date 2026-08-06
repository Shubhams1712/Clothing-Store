"use client";

import { useAuth } from "@/hooks/use-auth";

interface PermissionGuardProps {
  children: React.ReactNode;
  roles: string[];
  fallback?: React.ReactNode;
}

export function PermissionGuard({ children, roles, fallback }: PermissionGuardProps) {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated || !user) {
    return fallback ? <>{fallback}</> : null;
  }

  const hasPermission = roles.some((role) => user.roles?.includes(role));

  if (!hasPermission) {
    return fallback ? <>{fallback}</> : null;
  }

  return <>{children}</>;
}
