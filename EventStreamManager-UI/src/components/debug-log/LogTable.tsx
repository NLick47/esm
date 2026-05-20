import { EventHandle } from '@/types/event-log';
import {
  getScriptStatusBadge,
  getSendStatusBadge,
  getSendStatusLabel,
  getRowClassName,
  formatDateTime
} from './utils';

interface Props {
  handles: EventHandle[];
  retryingId: number | null;
  onViewDetail: (id: number) => void;
  onRetry: (id: number) => void;
}

export default function LogTable({ handles, retryingId, onViewDetail, onRetry }: Props) {
  return (
    <div className="rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">事件ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">事件码</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">处理器</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">脚本状态</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">发送状态</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">处理次数</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">耗时(ms)</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">消息</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">最后处理时间</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">事件创建时间</th>
              <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">操作</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {handles.map(handle => (
              <tr
                key={handle.id}
                onClick={() => onViewDetail(handle.id)}
                className={`hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors ${getRowClassName(handle)}`}
              >
                <td className="px-4 py-3 text-sm whitespace-nowrap">{handle.id}</td>
                <td className="px-4 py-3 text-sm whitespace-nowrap">{handle.eventId}</td>
                <td className="px-4 py-3 text-sm whitespace-nowrap">{handle.eventCode || '-'}</td>
                <td className="px-4 py-3 text-sm whitespace-nowrap">{handle.processorName}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getScriptStatusBadge(handle.scriptSuccess, handle.isDeadLetter)}`}>
                    {handle.isDeadLetter ? '死信' : handle.scriptSuccess === true ? '成功' : handle.scriptSuccess === false ? '失败' : '-'}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getSendStatusBadge(handle.needToSend, handle.sendSuccess)}`}>
                    {getSendStatusLabel(handle.needToSend, handle.sendSuccess)}
                  </span>
                </td>
                <td className="px-4 py-3 text-sm">{handle.handleTimes}</td>
                <td className="px-4 py-3 text-sm">{handle.lastHandleElapsedMs || '-'}</td>
                <td className="px-4 py-3 text-sm max-w-xs">
                  <div className="truncate text-xs text-gray-600 dark:text-gray-400" title={handle.lastHandleMessage || ''}>
                    {handle.lastHandleMessage || '-'}
                  </div>
                </td>
                <td className="px-4 py-3 text-sm whitespace-nowrap">{formatDateTime(handle.lastHandleDatetime)}</td>
                <td className="px-4 py-3 text-sm whitespace-nowrap">{formatDateTime(handle.createDatetime)}</td>
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onViewDetail(handle.id);
                      }}
                      className="text-blue-600 hover:text-blue-700 text-sm whitespace-nowrap"
                    >
                      详情
                    </button>
                    {handle.isDeadLetter && (
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          onRetry(handle.id);
                        }}
                        disabled={retryingId === handle.id}
                        className="text-purple-600 hover:text-purple-700 text-sm whitespace-nowrap disabled:opacity-50"
                      >
                        {retryingId === handle.id ? (
                          <i className="fa-solid fa-spinner fa-spin mr-1"></i>
                        ) : (
                          <i className="fa-solid fa-rotate-right mr-1"></i>
                        )}
                        重试
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {handles.length === 0 && (
        <div className="text-center py-8 text-gray-500">暂无处理记录</div>
      )}
    </div>
  );
}
