/**
 * 调试日志模块
 */
import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import * as eventLogService from '@/services/event-log.service';
import * as databaseService from '@/services/database.service';
import { EventHandle, EventHandleLog, EventWithHandles, EventLogStats } from '@/types';

export default function DebugLogModule() {
  const [databaseType, setDatabaseType] = useState<string>('');
  const [activeTab, setActiveTab] = useState<'handles' | 'logs' | 'stats'>('handles');
  
  const [eventId, setEventId] = useState<string>('');
  const [status, setStatus] = useState<string>('');
  const [isFinished, setIsFinished] = useState<boolean | undefined>(undefined);
  const [eventHandleId, setEventHandleId] = useState<string>('');
  
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [total, setTotal] = useState(0);
  
  const [handles, setHandles] = useState<EventHandle[]>([]);
  const [logs, setLogs] = useState<EventHandleLog[]>([]);
  const [stats, setStats] = useState<EventLogStats | null>(null);
  const [selectedEvent, setSelectedEvent] = useState<EventWithHandles | null>(null);
  
  const [loading, setLoading] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);

  const [databaseTypes, setDatabaseTypes] = useState<Array<{value: string; label: string}>>([]);

  useEffect(() => {
    loadDatabaseTypes();
  }, []);

  useEffect(() => {
    if (databaseType) {
      setPage(1);
      if (activeTab === 'handles') {
        fetchHandles();
      } else if (activeTab === 'logs') {
        fetchLogs();
      } else if (activeTab === 'stats') {
        fetchStats();
      }
    }
  }, [activeTab, databaseType]);

  const loadDatabaseTypes = async () => {
    try {
      const data = await databaseService.getDatabaseTypes();
      setDatabaseTypes(data);
      if (data.length > 0) {
        setDatabaseType(data[0].value);
      }
    } catch (error) {
      console.error('加载数据库类型失败:', error);
    }
  };

  const fetchHandles = async () => {
    if (!databaseType) return;
    
    setLoading(true);
    try {
      const result = await eventLogService.getEventHandles({
        databaseType,
        page,
        pageSize,
        eventId: eventId || undefined,
        status: status || undefined,
        isFinished
      });
      
      setHandles(result.items);
      setTotal(result.total);
    } catch (error) {
      toast.error('获取处理记录失败');
    } finally {
      setLoading(false);
    }
  };

  const fetchLogs = async () => {
    if (!databaseType) return;
    
    setLoading(true);
    try {
      const result = await eventLogService.getEventLogs({
        databaseType,
        page,
        pageSize,
        eventHandleId: eventHandleId || undefined,
        status: status || undefined
      });
      
      setLogs(result.items);
      setTotal(result.total);
    } catch (error) {
      toast.error('获取日志失败');
    } finally {
      setLoading(false);
    }
  };

  const fetchStats = async () => {
    if (!databaseType) return;
    
    setLoading(true);
    try {
      const result = await eventLogService.getEventLogStats(databaseType);
      setStats(result);
    } catch (error) {
      toast.error('获取统计数据失败');
    } finally {
      setLoading(false);
    }
  };

  const viewEventDetails = async (eventId: number) => {
    if (!databaseType) return;
    
    setDetailLoading(true);
    try {
      const result = await eventLogService.getEventWithHandles(databaseType, eventId);
      setSelectedEvent(result);
    } catch (error) {
      toast.error('获取事件详情失败');
    } finally {
      setDetailLoading(false);
    }
  };

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      Success: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
      Fail: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
      Exception: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
      Processing: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400'
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">调试与日志</h2>
      </div>

      {/* 数据库类型选择 */}
      <div className="flex gap-2">
        {databaseTypes.map(type => (
          <button
            key={type.value}
            onClick={() => setDatabaseType(type.value)}
            className={`rounded-lg px-4 py-2 font-medium transition-all ${
              databaseType === type.value
                ? 'bg-blue-600 text-white'
                : 'bg-white text-gray-700 border border-gray-200 hover:border-blue-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700'
            }`}
          >
            {type.label}
          </button>
        ))}
      </div>

      {/* Tab 切换 */}
      <div className="flex gap-2 border-b border-gray-200 dark:border-gray-700">
        <button
          onClick={() => setActiveTab('handles')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'handles'
              ? 'border-b-2 border-blue-600 text-blue-600'
              : 'text-gray-600 hover:text-gray-900 dark:text-gray-400'
          }`}
        >
          处理记录
        </button>
        <button
          onClick={() => setActiveTab('logs')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'logs'
              ? 'border-b-2 border-blue-600 text-blue-600'
              : 'text-gray-600 hover:text-gray-900 dark:text-gray-400'
          }`}
        >
          日志列表
        </button>
        <button
          onClick={() => setActiveTab('stats')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'stats'
              ? 'border-b-2 border-blue-600 text-blue-600'
              : 'text-gray-600 hover:text-gray-900 dark:text-gray-400'
          }`}
        >
          统计数据
        </button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <i className="fa-solid fa-spinner fa-spin text-3xl text-blue-600"></i>
        </div>
      ) : activeTab === 'handles' ? (
        <div className="space-y-4">
          {/* 筛选条件 */}
          <div className="flex gap-4 items-end">
            <div>
              <label className="block mb-1 text-sm">事件ID</label>
              <input
                type="text"
                value={eventId}
                onChange={(e) => setEventId(e.target.value)}
                className="rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
                placeholder="输入事件ID"
              />
            </div>
            <div>
              <label className="block mb-1 text-sm">状态</label>
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                className="rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              >
                <option value="">全部</option>
                <option value="Success">成功</option>
                <option value="Fail">失败</option>
                <option value="Exception">异常</option>
                <option value="Processing">处理中</option>
              </select>
            </div>
            <button
              onClick={fetchHandles}
              className="px-4 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700"
            >
              查询
            </button>
          </div>

          {/* 处理记录列表 */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
            <table className="w-full">
              <thead className="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th className="px-4 py-3 text-left text-sm font-medium">事件ID</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">状态</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">处理次数</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">最后处理时间</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">消息</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">操作</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                {handles.map(handle => (
                  <tr key={handle.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                    <td className="px-4 py-3">{handle.eventId}</td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getStatusBadge(handle.lastHandleStatus)}`}>
                        {handle.lastHandleStatus}
                      </span>
                    </td>
                    <td className="px-4 py-3">{handle.handleCount}</td>
                    <td className="px-4 py-3 text-sm">{new Date(handle.lastHandleDatetime).toLocaleString()}</td>
                    <td className="px-4 py-3 text-sm text-gray-500">{handle.lastHandleMessage || '-'}</td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => viewEventDetails(handle.eventId)}
                        className="text-blue-600 hover:text-blue-700"
                      >
                        查看详情
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {handles.length === 0 && (
              <div className="text-center py-8 text-gray-500">暂无数据</div>
            )}
          </div>
        </div>
      ) : activeTab === 'logs' ? (
        <div className="space-y-4">
          <div className="rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
            <table className="w-full">
              <thead className="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th className="px-4 py-3 text-left text-sm font-medium">ID</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">处理记录ID</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">状态</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">处理时间</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">消息</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                {logs.map(log => (
                  <tr key={log.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                    <td className="px-4 py-3">{log.id}</td>
                    <td className="px-4 py-3">{log.eventHandleId}</td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getStatusBadge(log.status)}`}>
                        {log.status}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm">{new Date(log.handleDatetime).toLocaleString()}</td>
                    <td className="px-4 py-3 text-sm text-gray-500">{log.message || '-'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            {logs.length === 0 && (
              <div className="text-center py-8 text-gray-500">暂无数据</div>
            )}
          </div>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
          {stats && (
            <>
              <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
                <h4 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">总数</h4>
                <p className="text-3xl font-bold">{stats.total}</p>
              </div>
              <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
                <h4 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">已完成</h4>
                <p className="text-3xl font-bold text-green-600">{stats.finished}</p>
              </div>
              <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
                <h4 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">待处理</h4>
                <p className="text-3xl font-bold text-yellow-600">{stats.pending}</p>
              </div>
              <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
                <h4 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">成功</h4>
                <p className="text-3xl font-bold text-blue-600">{stats.success}</p>
              </div>
              <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
                <h4 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">失败</h4>
                <p className="text-3xl font-bold text-red-600">{stats.failed}</p>
              </div>
            </>
          )}
        </div>
      )}

      {/* 事件详情弹窗 */}
      {selectedEvent && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-2xl w-full mx-4 max-h-[80vh] overflow-auto">
            <div className="flex justify-between items-start mb-4">
              <h3 className="text-lg font-semibold">事件详情</h3>
              <button
                onClick={() => setSelectedEvent(null)}
                className="text-gray-500 hover:text-gray-700"
              >
                <i className="fa-solid fa-times"></i>
              </button>
            </div>
            <pre className="bg-gray-100 dark:bg-gray-900 p-4 rounded overflow-auto text-sm">
              {JSON.stringify(selectedEvent, null, 2)}
            </pre>
          </div>
        </div>
      )}
    </div>
  );
}
