import { HTMLAttributes, forwardRef } from "react";
import { cn } from "@/_lib/utils";

export interface BadgeProps extends HTMLAttributes<HTMLDivElement> {
  variant?: "default" | "success" | "warning" | "danger" | "info";
}

const Badge = forwardRef<HTMLDivElement, BadgeProps>(
  ({ className, variant = "default", ...props }, ref) => {
    const variants = {
      default: "bg-dark-100 text-dark-700 border border-dark-200",
      success: "bg-emerald-50 text-emerald-700 border border-emerald-200",
      warning: "bg-amber-50 text-amber-700 border border-amber-200",
      danger: "bg-primary-50 text-primary-700 border border-primary-200",
      info: "bg-dark-900 text-white border border-dark-800",
    };

    return (
      <div
        ref={ref}
        className={cn(
          "inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold",
          variants[variant],
          className
        )}
        {...props}
      />
    );
  }
);

Badge.displayName = "Badge";

export { Badge };
