import { cn } from '@/lib/utils';

interface ColumnDef<T> {
  key: string;
  header: string;
  width?: string;
  className?: string;
  render?: (row: T, index: number) => React.ReactNode;
}

interface DataTableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
  emptyText?: string;
  emptyIcon?: string;
  loading?: boolean;
  keyExtractor: (row: T, index: number) => string | number;
  rowActions?: (row: T, index: number) => React.ReactNode;
  onRowClick?: (row: T, index: number) => void;
  className?: string;
}

export function DataTable<T>({
  data,
  columns,
  emptyText = '暂无数据',
  emptyIcon = 'fa-inbox',
  loading,
  keyExtractor,
  rowActions,
  onRowClick,
  className,
}: DataTableProps<T>) {
  return (
    <div className={cn('rounded-xl bg-white shadow-md dark:bg-gray-800 dark:shadow-lg overflow-hidden', className)}>
      <div className="overflow-x-auto">
        <table className="min-w-full">
          <thead>
            <tr className="border-b border-gray-200 bg-gray-50 dark:border-gray-800 dark:bg-gray-900">
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={cn(
                    'px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400',
                    col.className
                  )}
                  style={col.width ? { width: col.width } : undefined}
                >
                  {col.header}
                </th>
              ))}
              {rowActions && <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">操作</th>}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading ? (
              <tr>
                <td colSpan={columns.length + (rowActions ? 1 : 0)} className="px-6 py-10 text-center text-gray-500 dark:text-gray-400">
                  <div className="flex items-center justify-center gap-2">
                    <i className="fa-solid fa-spinner fa-spin"></i>
                    <span>加载中...</span>
                  </div>
                </td>
              </tr>
            ) : data.length === 0 ? (
              <tr>
                <td colSpan={columns.length + (rowActions ? 1 : 0)} className="px-6 py-10 text-center text-gray-500 dark:text-gray-400">
                  <div className="flex flex-col items-center justify-center">
                    <i className={cn('fa-solid text-4xl text-gray-300 dark:text-gray-600 mb-2', emptyIcon)}></i>
                    <p>{emptyText}</p>
                  </div>
                </td>
              </tr>
            ) : (
              data.map((row, index) => (
                <tr
                  key={keyExtractor(row, index)}
                  className={cn(
                    'transition-colors',
                    onRowClick ? 'cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700/50' : 'hover:bg-gray-50 dark:hover:bg-gray-700/50'
                  )}
                  onClick={() => onRowClick?.(row, index)}
                >
                  {columns.map((col) => (
                    <td key={col.key} className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-gray-100">
                      {col.render ? col.render(row, index) : (row as Record<string, React.ReactNode>)[col.key]}
                    </td>
                  ))}
                  {rowActions && (
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex items-center justify-end gap-2" onClick={(e) => e.stopPropagation()}>
                        {rowActions(row, index)}
                      </div>
                    </td>
                  )}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
