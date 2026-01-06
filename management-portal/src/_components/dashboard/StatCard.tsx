import { LucideIcon } from "lucide-react";
import { Card, CardContent } from "@/_components/ui/Card";
import { cn } from "@/_lib/utils";

interface StatCardProps {
  title: string;
  value: string | number;
  icon: LucideIcon;
  trend?: {
    value: number;
    isPositive: boolean;
  };
  className?: string;
}

export function StatCard({ title, value, icon: Icon, trend, className }: StatCardProps) {
  return (
    <Card className={className}>
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm font-medium text-dark-500">{title}</p>
            <p className="mt-2 text-3xl font-bold text-dark-900">{value}</p>
            {trend && (
              <p className={cn(
                "mt-2 text-sm font-semibold",
                trend.isPositive ? "text-emerald-600" : "text-primary-600"
              )}>
                {trend.isPositive ? "+" : ""}{trend.value}%
                <span className="text-dark-400 font-normal ml-1">from last month</span>
              </p>
            )}
          </div>
          <div className="flex h-14 w-14 items-center justify-center rounded-xl bg-dark-900">
            <Icon className="h-7 w-7 text-white" />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
