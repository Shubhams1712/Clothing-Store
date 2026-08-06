import { Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";

interface LoadingOverlayProps {
  className?: string;
  text?: string;
}

export function LoadingOverlay({ className, text = "Loading..." }: LoadingOverlayProps) {
  return (
    <div className={cn("flex flex-col items-center justify-center gap-4 py-16", className)}>
      <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      <p className="text-sm text-muted-foreground">{text}</p>
    </div>
  );
}
