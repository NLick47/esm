import { cn } from '@/lib/utils';

export function PageLoading({ className }: { className?: string }) {
  return (
    <div className={cn('flex flex-col items-center justify-center min-h-[400px]', className)}>
      <i className="fa-solid fa-spinner fa-spin text-3xl text-blue-600 mb-3"></i>
      <span className="text-gray-500 dark:text-gray-400">加载中...</span>
    </div>
  );
}
