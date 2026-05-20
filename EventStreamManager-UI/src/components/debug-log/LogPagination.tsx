interface Props {
  page: number;
  pageSize: number;
  total: number;
  loading: boolean;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

export default function LogPagination({ page, pageSize, total, loading, onPageChange, onPageSizeChange }: Props) {
  const totalPages = Math.ceil(total / pageSize) || 1;

  return (
    <div className="flex items-center justify-between mt-4">
      <div className="flex items-center gap-3">
        <span className="text-sm text-gray-500">共 {total} 条记录</span>
        <select
          value={pageSize}
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
          className="text-sm rounded border border-gray-300 px-2 py-1 dark:border-gray-700 dark:bg-gray-800"
        >
          <option value={10}>10条/页</option>
          <option value={20}>20条/页</option>
          <option value={50}>50条/页</option>
          <option value={100}>100条/页</option>
        </select>
      </div>
      <div className="flex gap-2">
        <button
          onClick={() => onPageChange(Math.max(1, page - 1))}
          disabled={page === 1 || loading}
          className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50 dark:border-gray-700"
        >
          上一页
        </button>
        <span className="px-3 py-1">{page} / {totalPages}</span>
        <button
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages || loading}
          className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50 dark:border-gray-700"
        >
          下一页
        </button>
      </div>
    </div>
  );
}
