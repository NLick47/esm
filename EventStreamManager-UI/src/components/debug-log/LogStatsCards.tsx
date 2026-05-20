import { EventHandleStats } from '@/types/event-log';

interface Props {
  stats: EventHandleStats;
}

export default function LogStatsCards({ stats }: Props) {
  return (
    <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
      <div className="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
        <div className="text-xs text-gray-500 dark:text-gray-400">总记录</div>
        <div className="text-xl font-bold text-gray-900 dark:text-gray-100">{stats.total}</div>
      </div>
      <div className="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
        <div className="text-xs text-green-600 dark:text-green-400">成功</div>
        <div className="text-xl font-bold text-green-700 dark:text-green-400">{stats.success}</div>
      </div>
      <div className="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
        <div className="text-xs text-red-600 dark:text-red-400">失败</div>
        <div className="text-xl font-bold text-red-700 dark:text-red-400">{stats.failed}</div>
      </div>
      <div className="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
        <div className="text-xs text-purple-600 dark:text-purple-400">死信</div>
        <div className="text-xl font-bold text-purple-700 dark:text-purple-400">{stats.deadLetter}</div>
      </div>
      <div className="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
        <div className="text-xs text-blue-600 dark:text-blue-400">处理中</div>
        <div className="text-xl font-bold text-blue-700 dark:text-blue-400">{stats.processing}</div>
      </div>
    </div>
  );
}
