import { ButtonHTMLAttributes, forwardRef } from "react";
import { cn } from "@/_lib/utils";

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "outline" | "ghost" | "danger";
  size?: "sm" | "md" | "lg";
  isLoading?: boolean;
}

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant = "primary", size = "md", isLoading, children, disabled, ...props }, ref) => {
    const baseStyles = "inline-flex items-center justify-center rounded-lg font-semibold transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50";

    const variants = {
      primary: "bg-primary-600 text-white hover:bg-primary-700 focus-visible:ring-primary-600 shadow-lg shadow-primary-600/25 hover:shadow-primary-600/40",
      secondary: "bg-dark-900 text-white hover:bg-dark-800 focus-visible:ring-dark-600",
      outline: "border-2 border-dark-300 bg-transparent text-dark-900 hover:bg-dark-100 hover:border-dark-400 focus-visible:ring-dark-600",
      ghost: "text-dark-700 hover:bg-dark-100 focus-visible:ring-dark-600",
      danger: "bg-primary-600 text-white hover:bg-primary-700 focus-visible:ring-primary-600",
    };

    const sizes = {
      sm: "h-9 px-4 text-sm",
      md: "h-11 px-5 text-sm",
      lg: "h-12 px-8 text-base",
    };

    return (
      <button
        ref={ref}
        className={cn(baseStyles, variants[variant], sizes[size], className)}
        disabled={disabled || isLoading}
        {...props}
      >
        {isLoading ? (
          <>
            <svg className="animate-spin -ml-1 mr-2 h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
            Loading...
          </>
        ) : children}
      </button>
    );
  }
);

Button.displayName = "Button";

export { Button };
