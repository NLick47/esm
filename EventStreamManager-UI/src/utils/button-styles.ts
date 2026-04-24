import { cn } from '@/lib/utils';

export const buttonVariants = {
  primary: cn(
    'rounded-lg bg-blue-600 text-white font-medium transition-all',
    'hover:bg-blue-700 shadow-sm hover:shadow-md active:scale-95',
    'disabled:bg-gray-400 disabled:cursor-not-allowed disabled:shadow-none disabled:active:scale-100'
  ),
  success: cn(
    'rounded-lg bg-green-600 text-white font-medium transition-all',
    'hover:bg-green-700 shadow-sm hover:shadow-md active:scale-95',
    'disabled:bg-gray-400 disabled:cursor-not-allowed disabled:shadow-none disabled:active:scale-100'
  ),
  danger: cn(
    'rounded-lg bg-red-600 text-white font-medium transition-all',
    'hover:bg-red-700 shadow-sm hover:shadow-md active:scale-95',
    'disabled:bg-gray-400 disabled:cursor-not-allowed disabled:shadow-none disabled:active:scale-100'
  ),
  ghost: cn(
    'rounded-lg border border-gray-300 bg-white text-gray-700 font-medium transition-all',
    'hover:bg-gray-50 dark:border-gray-600 dark:bg-transparent dark:text-gray-300 dark:hover:bg-gray-700',
    'disabled:opacity-50 disabled:cursor-not-allowed'
  ),
  link: cn(
    'text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 transition-colors',
    'disabled:opacity-50 disabled:cursor-not-allowed'
  ),
  iconDanger: cn(
    'text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300 transition-colors'
  ),
  iconDefault: cn(
    'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300 transition-colors'
  ),
};

export const badgeVariants = {
  enabled: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  disabled: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300',
  pending: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
  error: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
};
