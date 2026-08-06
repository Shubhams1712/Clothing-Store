import { cn } from "@/lib/utils";

interface SectionWrapperProps {
  children: React.ReactNode;
  className?: string;
  title?: string;
  description?: string;
}

export function SectionWrapper({ children, className, title, description }: SectionWrapperProps) {
  return (
    <section className={cn("py-12 sm:py-16", className)}>
      {(title || description) && (
        <div className="mb-8">
          {title && (
            <h2 className="text-2xl font-semibold tracking-tight sm:text-3xl">{title}</h2>
          )}
          {description && (
            <p className="mt-2 text-muted-foreground">{description}</p>
          )}
        </div>
      )}
      {children}
    </section>
  );
}
