import { cn } from '@/lib/utils';

interface StatusBadgeProps {
  variant: 'success' | 'danger' | 'warning' | 'info' | 'default' | 'purple';
  children: React.ReactNode;
  size?: 'sm' | 'md';
  className?: string;
}

const variantClasses = {
  success: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  danger: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
  warning: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
  info: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
  default: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300',
  purple: 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400',
};

const sizeClasses = {
  sm: 'px-2 py-0.5 text-xs',
  md: 'px-2.5 py-1 text-sm',
};

export function StatusBadge({ variant, children, size = 'sm', className }: StatusBadgeProps) {
  return (
    <span className={cn('inline-flex items-center rounded-full font-medium', variantClasses[variant], sizeClasses[size], className)}>
      {children}
    </span>
  );
}
