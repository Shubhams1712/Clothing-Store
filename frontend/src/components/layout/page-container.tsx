import { cn } from "@/lib/utils";

interface PageContainerProps {
  children: React.ReactNode;
  className?: string;
}

export function PageContainer({ children, className }: PageContainerProps) {
  return (
    <main className={cn("mx-auto w-full max-w-7xl flex-1 px-4 sm:px-6 lg:px-8", className)}>
      {children}
    </main>
  );
}
