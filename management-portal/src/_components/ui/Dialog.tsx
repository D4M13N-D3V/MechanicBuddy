"use client";

import { Fragment, ReactNode } from "react";
import { cn } from "@/_lib/utils";

interface DialogProps {
  open: boolean;
  onClose: () => void;
  children: ReactNode;
}

export function Dialog({ open, onClose, children }: DialogProps) {
  if (!open) return null;

  return (
    <Fragment>
      <div
        className="fixed inset-0 bg-black/50 z-40 transition-opacity"
        onClick={onClose}
        aria-hidden="true"
      />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div
          className="bg-white rounded-xl shadow-2xl w-full max-w-md transform transition-all"
          onClick={(e) => e.stopPropagation()}
        >
          {children}
        </div>
      </div>
    </Fragment>
  );
}

interface DialogHeaderProps {
  children: ReactNode;
  className?: string;
}

export function DialogHeader({ children, className }: DialogHeaderProps) {
  return (
    <div className={cn("px-6 pt-6 pb-4 border-b border-gray-100", className)}>
      {children}
    </div>
  );
}

interface DialogTitleProps {
  children: ReactNode;
  className?: string;
}

export function DialogTitle({ children, className }: DialogTitleProps) {
  return (
    <h2 className={cn("text-xl font-semibold text-dark-900", className)}>
      {children}
    </h2>
  );
}

interface DialogDescriptionProps {
  children: ReactNode;
  className?: string;
}

export function DialogDescription({ children, className }: DialogDescriptionProps) {
  return (
    <p className={cn("text-sm text-dark-500 mt-1", className)}>
      {children}
    </p>
  );
}

interface DialogContentProps {
  children: ReactNode;
  className?: string;
}

export function DialogContent({ children, className }: DialogContentProps) {
  return (
    <div className={cn("px-6 py-4", className)}>
      {children}
    </div>
  );
}

interface DialogFooterProps {
  children: ReactNode;
  className?: string;
}

export function DialogFooter({ children, className }: DialogFooterProps) {
  return (
    <div className={cn("px-6 pb-6 pt-4 flex justify-end gap-3", className)}>
      {children}
    </div>
  );
}
