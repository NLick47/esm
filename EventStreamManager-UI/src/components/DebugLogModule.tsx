import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import * as eventLogService from '@/services/event-log.service';
import * as databaseService from '@/services/database.service';
import * as processorService from '@/services/processor.service';
import { EventHandle, EventHandleLog, EventWithHandles, EventLogStats, ProcessorStats, FailedHandle } from '@/types';

// 状态类型定义
type StatusType = 'Success' | 'Fail' | 'Exception' | 'Processing' | '';

export default function DebugLogModule() {
  const [databaseType, setDatabaseType] = useState<string>('');
  const [activeTab, setActiveTab] = useState<'handles' | 'logs' | 'stats' | 'failed' | 'processor-stats'>('handles');
  
  // 查询条件
  const [eventId, setEventId] = useState<string>('');
  const [eventHandleId, setEventHandleId] = useState<string>('');
  const [processorId, setProcessorId] = useState<string>('');
  const [processorName, setProcessorName] = useState<string>('');
  const [status, setStatus] = useState<StatusType>('');
  const [isFinished, setIsFinished] = useState<boolean | undefined>(undefined);
  const [maxRetryTimes, setMaxRetryTimes] = useState<number>(3);
  
  // 分页
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [total, setTotal] = useState(0);
  
  // 数据
  const [handles, setHandles] = useState<EventHandle[]>([]);
  const [logs, setLogs] = useState<EventHandleLog[]>([]);
  const [stats, setStats] = useState<EventLogStats | null>(null);
  const [processorStats, setProcessorStats] = useState<ProcessorStats[]>([]);
  const [failedHandles, setFailedHandles] = useState<FailedHandle[]>([]);
  const [selectedEvent, setSelectedEvent] = useState<EventWithHandles | null>(null);
  const [selectedLog, setSelectedLog] = useState<EventHandleLog | null>(null);
  const [selectedHandle, setSelectedHandle] = useState<EventHandle | null>(null);
  
  // 状态
  const [loading, setLoading] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());

  const [databaseTypes, setDatabaseTypes] = useState<Array<{value: string; label: string}>>([]);
  const [processors, setProcessors] = useState<Array<{id: string; name: string}>>([]);

  // 初始化
  useEffect(() => {
    loadDatabaseTypes();
    loadProcessors();
  }, []);

  // 切换数据库时重置
  useEffect(() => {
    if (databaseType) {
      setPage(1);
      resetFilters();
      loadDataByTab();
    }
  }, [databaseType, activeTab]);

  const loadProcessors = async () => {
    try {
      const data = await processorService.getProcessors();
      setProcessors(data.map(p => ({ id: p.id, name: p.name })));
    } catch (error) {
      console.error('加载处理器列表失败:', error);
    }
  };

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

  const resetFilters = () => {
    setEventId('');
    setEventHandleId('');
    setProcessorId('');
    setProcessorName('');
    setStatus('');
    setIsFinished(undefined);
  };

  const loadDataByTab = () => {
    switch (activeTab) {
      case 'handles':
        fetchHandles();
        break;
      case 'logs':
        fetchLogs();
        break;
      case 'stats':
        fetchStats();
        break;
      case 'failed':
        fetchFailedHandles();
        break;
      case 'processor-stats':
        fetchProcessorStats();
        break;
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
        eventId: eventId ? parseInt(eventId) : undefined,
        processorId: processorId || undefined,
        processorName: processorName || undefined,
        status: status || undefined,
        isFinished
      });
      
      setHandles(result.list);
      setTotal(result.total);
    } catch (error) {
      toast.error('获取处理记录失败');
      console.error(error);
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
        eventId: eventId ? parseInt(eventId) : undefined,
        eventHandleId: eventHandleId ? parseInt(eventHandleId) : undefined,
        processorId: processorId || undefined,
        status: status || undefined
      });
      
      setLogs(result.list);
      setTotal(result.total);
    } catch (error) {
      toast.error('获取日志失败');
      console.error(error);
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
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const fetchProcessorStats = async () => {
    if (!databaseType) return;
    
    setLoading(true);
    try {
      const result = await eventLogService.getStatsByProcessor(databaseType);
      setProcessorStats(result);
    } catch (error) {
      toast.error('获取处理器统计失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const fetchFailedHandles = async () => {
    if (!databaseType) return;
    
    setLoading(true);
    try {
      const result = await eventLogService.getFailedHandles({
        databaseType,
        page,
        pageSize,
        processorId: processorId || undefined,
        maxRetryTimes
      });
      
      setFailedHandles(result.list);
      setTotal(result.total);
    } catch (error) {
      toast.error('获取失败记录失败');
      console.error(error);
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
      console.error(error);
    } finally {
      setDetailLoading(false);
    }
  };

  const viewHandleDetails = async (handleId: number) => {
    if (!databaseType) return;
    
    setDetailLoading(true);
    try {
      const result = await eventLogService.getEventHandle(databaseType, handleId);
      setSelectedHandle(result);
    } catch (error) {
      toast.error('获取处理记录详情失败');
      console.error(error);
    } finally {
      setDetailLoading(false);
    }
  };

  const viewLogDetails = async (logId: number) => {
    if (!databaseType) return;
    
    setDetailLoading(true);
    try {
      const result = await eventLogService.getEventLog(databaseType, logId);
      setSelectedLog(result);
    } catch (error) {
      toast.error('获取日志详情失败');
      console.error(error);
    } finally {
      setDetailLoading(false);
    }
  };

  const toggleRowExpand = (id: number) => {
    const newExpanded = new Set(expandedRows);
    if (newExpanded.has(id)) {
      newExpanded.delete(id);
    } else {
      newExpanded.add(id);
    }
    setExpandedRows(newExpanded);
  };

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      'Success': 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
      'Fail': 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
      'Exception': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
      'Processing': 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400'
    };
    return colors[status] || 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
  };

  

  const renderFilterBar = () => {
    const isHandlesTab = activeTab === 'handles';
    const isLogsTab = activeTab === 'logs';
    const isFailedTab = activeTab === 'failed';

    return (
      <div className="bg-white dark:bg-gray-800 p-4 rounded-lg shadow-sm space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* 事件ID */}
          <div>
            <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
              事件ID
            </label>
            <input
              type="number"
              value={eventId}
              onChange={(e) => setEventId(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              placeholder="输入事件ID"
              min="1"
            />
          </div>

          {/* 处理器选择 */}
          <div>
            <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
              处理器
            </label>
            <select
              value={processorId}
              onChange={(e) => setProcessorId(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
            >
              <option value="">全部处理器</option>
              {processors.map(p => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </div>

          {/* 状态选择 */}
          <div>
            <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
              状态
            </label>
            <select
              value={status}
              onChange={(e) => setStatus(e.target.value as StatusType)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
            >
              <option value="">全部</option>
              <option value="Success">成功</option>
              <option value="Fail">失败</option>
              <option value="Exception">异常</option>
              <option value="Processing">处理中</option>
            </select>
          </div>

          {/* 处理器名称模糊搜索 */}
          {(isHandlesTab || isFailedTab) && (
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
                处理器名称
              </label>
              <input
                type="text"
                value={processorName}
                onChange={(e) => setProcessorName(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
                placeholder="模糊搜索"
              />
            </div>
          )}

          {/* 处理记录ID（仅日志标签页） */}
          {isLogsTab && (
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
                处理记录ID
              </label>
              <input
                type="number"
                value={eventHandleId}
                onChange={(e) => setEventHandleId(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
                placeholder="输入处理记录ID"
              />
            </div>
          )}

          {/* 完成状态（仅处理记录标签页） */}
          {isHandlesTab && (
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
                完成状态
              </label>
              <select
                value={isFinished === undefined ? '' : isFinished.toString()}
                onChange={(e) => {
                  const val = e.target.value;
                  setIsFinished(val === '' ? undefined : val === 'true');
                }}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              >
                <option value="">全部</option>
                <option value="true">已完成</option>
                <option value="false">未完成</option>
              </select>
            </div>
          )}

          {/* 最大重试次数（仅失败记录标签页） */}
          {isFailedTab && (
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
                最大重试次数
              </label>
              <input
                type="number"
                value={maxRetryTimes}
                onChange={(e) => setMaxRetryTimes(parseInt(e.target.value) || 3)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
                min="1"
                max="10"
              />
            </div>
          )}
        </div>

        <div className="flex justify-end gap-2">
          <button
            onClick={resetFilters}
            className="px-4 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-gray-800"
          >
            重置
          </button>
          <button
            onClick={loadDataByTab}
            className="px-4 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700"
          >
            查询
          </button>
        </div>
      </div>
    );
  };

  const renderHandlesTab = () => (
    <div className="space-y-4">
      {renderFilterBar()}
      
      <div className="rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium">ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium">事件ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium">处理器</th>
              <th className="px-4 py-3 text-left text-sm font-medium">状态</th>
              <th className="px-4 py-3 text-left text-sm font-medium">处理次数</th>
              <th className="px-4 py-3 text-left text-sm font-medium">耗时(ms)</th>
              <th className="px-4 py-3 text-left text-sm font-medium">最后处理时间</th>
              <th className="px-4 py-3 text-left text-sm font-medium">完成</th>
              <th className="px-4 py-3 text-left text-sm font-medium">操作</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {handles.map(handle => (
              <tr key={handle.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                <td className="px-4 py-3 text-sm">{handle.id}</td>
                <td className="px-4 py-3 text-sm">{handle.eventId}</td>
                <td className="px-4 py-3">
                  <div className="text-sm font-medium">{handle.processorName}</div>
                  <div className="text-xs text-gray-500">{handle.processorId}</div>
                </td>
                <td className="px-4 py-3">
                  <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getStatusBadge(handle.lastHandleStatus)}`}>
                    {handle.lastHandleStatus}
                  </span>
                </td>
                <td className="px-4 py-3 text-sm">{handle.handleTimes}</td>
                <td className="px-4 py-3 text-sm">{handle.lastHandleElapsedMs || '-'}</td>
                <td className="px-4 py-3 text-sm">{new Date(handle.lastHandleDatetime).toLocaleString()}</td>
                <td className="px-4 py-3">
                  {handle.isFinished ? (
                    <span className="text-green-600">✓</span>
                  ) : (
                    <span className="text-yellow-600">⏳</span>
                  )}
                </td>
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    <button
                      onClick={() => viewHandleDetails(handle.id)}
                      className="text-blue-600 hover:text-blue-700 text-sm"
                      title="查看详情"
                    >
                      详情
                    </button>
                    <button
                      onClick={() => viewEventDetails(handle.eventId)}
                      className="text-green-600 hover:text-green-700 text-sm"
                      title="查看事件"
                    >
                      事件
                    </button>
                    <button
                      onClick={() => toggleRowExpand(handle.id)}
                      className="text-gray-600 hover:text-gray-700 text-sm"
                    >
                      {expandedRows.has(handle.id) ? '收起' : '消息'}
                    </button>
                  </div>
                  {expandedRows.has(handle.id) && handle.lastHandleMessage && (
                    <div className="mt-2 p-2 bg-gray-50 dark:bg-gray-800 rounded text-xs">
                      {handle.lastHandleMessage}
                    </div>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {handles.length === 0 && (
          <div className="text-center py-8 text-gray-500">暂无处理记录</div>
        )}
      </div>
    </div>
  );

  const renderLogsTab = () => (
    <div className="space-y-4">
      {renderFilterBar()}
      
      <div className="rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium">ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium">事件ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium">处理记录ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium">处理器</th>
              <th className="px-4 py-3 text-left text-sm font-medium">状态</th>
              <th className="px-4 py-3 text-left text-sm font-medium">处理时间</th>
              <th className="px-4 py-3 text-left text-sm font-medium">耗时(ms)</th>
              <th className="px-4 py-3 text-left text-sm font-medium">操作</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {logs.map(log => (
              <tr key={log.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                <td className="px-4 py-3 text-sm">{log.id}</td>
                <td className="px-4 py-3 text-sm">{log.eventId}</td>
                <td className="px-4 py-3 text-sm">{log.eventHandleId}</td>
                <td className="px-4 py-3 text-sm">{log.processorName}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getStatusBadge(log.status)}`}>
                    {log.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-sm">{new Date(log.handleDatetime).toLocaleString()}</td>
                <td className="px-4 py-3 text-sm">{log.elapsedMs || '-'}</td>
                <td className="px-4 py-3">
                  <button
                    onClick={() => viewLogDetails(log.id)}
                    className="text-blue-600 hover:text-blue-700 text-sm"
                  >
                    详情
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {logs.length === 0 && (
          <div className="text-center py-8 text-gray-500">暂无日志</div>
        )}
      </div>
    </div>
  );

  const renderStatsTab = () => (
    <div className="space-y-6">
      {/* 总体统计卡片 */}
      {stats && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
            <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
              <h4 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">总数</h4>
              <p className="text-3xl font-bold">{stats.total}</p>
            </div>
            <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
              <h4 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">已完成</h4>
              <p className="text-3xl font-bold text-green-600">{stats.finished}</p>
              <p className="text-xs text-gray-500 mt-1">{((stats.finished / stats.total) * 100).toFixed(1)}%</p>
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
          </div>

          {/* 成功率图表 */}
          <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
            <h3 className="text-lg font-medium mb-4">处理成功率</h3>
            <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
              <div 
                className="h-full bg-green-500"
                style={{ width: `${(stats.success / stats.total) * 100}%` }}
              />
            </div>
            <div className="flex justify-between mt-2 text-sm text-gray-600 dark:text-gray-400">
              <span>成功: {stats.success}</span>
              <span>失败: {stats.failed}</span>
              <span>成功率: {((stats.success / stats.total) * 100).toFixed(1)}%</span>
            </div>
          </div>
        </>
      )}
    </div>
  );

  const renderProcessorStatsTab = () => (
    <div className="space-y-4">
      <div className="rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium">处理器ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium">处理器名称</th>
              <th className="px-4 py-3 text-left text-sm font-medium">总数</th>
              <th className="px-4 py-3 text-left text-sm font-medium">成功</th>
              <th className="px-4 py-3 text-left text-sm font-medium">失败</th>
              <th className="px-4 py-3 text-left text-sm font-medium">待处理</th>
              <th className="px-4 py-3 text-left text-sm font-medium">成功率</th>
              <th className="px-4 py-3 text-left text-sm font-medium">平均耗时(ms)</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {processorStats.map(stat => {
              const successRate = stat.totalCount > 0 
                ? ((stat.successCount / stat.totalCount) * 100).toFixed(1)
                : '0';
              
              return (
                <tr key={stat.processorId} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                  <td className="px-4 py-3 text-sm font-mono">{stat.processorId}</td>
                  <td className="px-4 py-3 text-sm">{stat.processorName}</td>
                  <td className="px-4 py-3 text-sm">{stat.totalCount}</td>
                  <td className="px-4 py-3 text-sm text-green-600">{stat.successCount}</td>
                  <td className="px-4 py-3 text-sm text-red-600">{stat.failedCount}</td>
                  <td className="px-4 py-3 text-sm text-yellow-600">{stat.pendingCount}</td>
                  <td className="px-4 py-3 text-sm">
                    <span className={`${parseFloat(successRate) > 80 ? 'text-green-600' : 'text-yellow-600'}`}>
                      {successRate}%
                    </span>
                  </td>
                  <td className="px-4 py-3 text-sm">{stat.avgHandleTimes?.toFixed(0) || '-'}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
        {processorStats.length === 0 && (
          <div className="text-center py-8 text-gray-500">暂无处理器统计数据</div>
        )}
      </div>
    </div>
  );

  const renderFailedTab = () => (
    <div className="space-y-4">
      {renderFilterBar()}
      
      <div className="rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium">ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium">事件ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium">处理器</th>
              <th className="px-4 py-3 text-left text-sm font-medium">最后状态</th>
              <th className="px-4 py-3 text-left text-sm font-medium">已处理次数</th>
              <th className="px-4 py-3 text-left text-sm font-medium">最后处理时间</th>
              <th className="px-4 py-3 text-left text-sm font-medium">消息</th>
              <th className="px-4 py-3 text-left text-sm font-medium">操作</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {failedHandles.map(handle => (
              <tr key={handle.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                <td className="px-4 py-3 text-sm">{handle.id}</td>
                <td className="px-4 py-3 text-sm">{handle.eventId}</td>
                <td className="px-4 py-3">
                  <div className="text-sm">{handle.processorName}</div>
                </td>
                <td className="px-4 py-3">
                  <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getStatusBadge(handle.lastHandleStatus)}`}>
                    {handle.lastHandleStatus}
                  </span>
                </td>
                <td className="px-4 py-3 text-sm">{handle.handleTimes}</td>
                <td className="px-4 py-3 text-sm">{new Date(handle.lastHandleDatetime).toLocaleString()}</td>
                <td className="px-4 py-3 text-sm text-gray-500 max-w-xs truncate">{handle.lastHandleMessage || '-'}</td>
                <td className="px-4 py-3">
                  <button
                    onClick={() => viewHandleDetails(handle.id)}
                    className="text-blue-600 hover:text-blue-700 text-sm"
                  >
                    详情
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {failedHandles.length === 0 && (
          <div className="text-center py-8 text-gray-500">暂无失败记录</div>
        )}
      </div>
    </div>
  );

  const renderPagination = () => (
    <div className="flex items-center justify-between mt-4">
      <div className="text-sm text-gray-500">
        共 {total} 条记录
      </div>
      <div className="flex gap-2">
        <button
          onClick={() => setPage(p => Math.max(1, p - 1))}
          disabled={page === 1 || loading}
          className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50 dark:border-gray-700"
        >
          上一页
        </button>
        <span className="px-3 py-1">
          {page} / {Math.ceil(total / pageSize) || 1}
        </span>
        <button
          onClick={() => setPage(p => p + 1)}
          disabled={page >= Math.ceil(total / pageSize) || loading}
          className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50 dark:border-gray-700"
        >
          下一页
        </button>
      </div>
    </div>
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">调试与日志</h2>
        {detailLoading && (
          <div className="text-sm text-gray-500">
            <i className="fa-solid fa-spinner fa-spin mr-1"></i>
            加载详情中...
          </div>
        )}
      </div>

      {/* 数据库类型选择 */}
      <div className="flex flex-wrap gap-2">
        {databaseTypes.map(type => (
          <button
            key={type.value}
            onClick={() => setDatabaseType(type.value)}
            className={`rounded-lg px-4 py-2 font-medium transition-all ${
              databaseType === type.value
                ? 'bg-blue-600 text-white shadow-lg'
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
          className={`px-4 py-2 font-medium transition-colors relative ${
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
          总体统计
        </button>
        <button
          onClick={() => setActiveTab('processor-stats')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'processor-stats'
              ? 'border-b-2 border-blue-600 text-blue-600'
              : 'text-gray-600 hover:text-gray-900 dark:text-gray-400'
          }`}
        >
          处理器统计
        </button>
        <button
          onClick={() => setActiveTab('failed')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'failed'
              ? 'border-b-2 border-blue-600 text-blue-600'
              : 'text-gray-600 hover:text-gray-900 dark:text-gray-400'
          }`}
        >
          失败记录
        </button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <i className="fa-solid fa-spinner fa-spin text-3xl text-blue-600"></i>
        </div>
      ) : (
        <>
          {activeTab === 'handles' && renderHandlesTab()}
          {activeTab === 'logs' && renderLogsTab()}
          {activeTab === 'stats' && renderStatsTab()}
          {activeTab === 'processor-stats' && renderProcessorStatsTab()}
          {activeTab === 'failed' && renderFailedTab()}

          {/* 分页 */}
          {(activeTab === 'handles' || activeTab === 'logs' || activeTab === 'failed') && renderPagination()}
        </>
      )}

      {/* 事件详情弹窗 */}
      {selectedEvent && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-4xl w-full mx-4 max-h-[90vh] overflow-auto">
            <div className="flex justify-between items-start mb-4">
              <h3 className="text-lg font-semibold">事件详情 - ID: </h3>
              <button
                onClick={() => setSelectedEvent(null)}
                className="text-gray-500 hover:text-gray-700"
              >
                <i className="fa-solid fa-times"></i>
              </button>
            </div>
            
            <div className="space-y-4">
              {/* 事件信息 */}
              <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded">
                <h4 className="font-medium mb-2">事件信息</h4>
                <pre className="text-xs overflow-auto">
                  {JSON.stringify(selectedEvent.event, null, 2)}
                </pre>
              </div>

              {/* 处理记录 */}
              {selectedEvent.handles && selectedEvent.handles.length > 0 && (
                <div>
                  <h4 className="font-medium mb-2">处理记录 ({selectedEvent.handles.length})</h4>
                  <div className="space-y-2">
                    {selectedEvent.handles.map(handle => (
                      <div key={handle.id} className="bg-gray-50 dark:bg-gray-900 p-3 rounded">
                        <pre className="text-xs overflow-auto">
                          {JSON.stringify(handle, null, 2)}
                        </pre>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* 日志 */}
              {selectedEvent.logs && selectedEvent.logs.length > 0 && (
                <div>
                  <h4 className="font-medium mb-2">日志 ({selectedEvent.logs.length})</h4>
                  <div className="space-y-2 max-h-60 overflow-auto">
                    {selectedEvent.logs.map(log => (
                      <div key={log.id} className="bg-gray-50 dark:bg-gray-900 p-2 rounded text-xs">
                        [{new Date(log.handleDatetime).toLocaleString()}] {log.status}: {log.message || '-'}
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* 处理记录详情弹窗 */}
      {selectedHandle && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-2xl w-full mx-4 max-h-[80vh] overflow-auto">
            <div className="flex justify-between items-start mb-4">
              <h3 className="text-lg font-semibold">处理记录详情</h3>
              <button
                onClick={() => setSelectedHandle(null)}
                className="text-gray-500 hover:text-gray-700"
              >
                <i className="fa-solid fa-times"></i>
              </button>
            </div>
            <pre className="bg-gray-100 dark:bg-gray-900 p-4 rounded overflow-auto text-sm">
              {JSON.stringify(selectedHandle, null, 2)}
            </pre>
          </div>
        </div>
      )}

      {/* 日志详情弹窗 */}
      {selectedLog && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-2xl w-full mx-4 max-h-[80vh] overflow-auto">
            <div className="flex justify-between items-start mb-4">
              <h3 className="text-lg font-semibold">日志详情</h3>
              <button
                onClick={() => setSelectedLog(null)}
                className="text-gray-500 hover:text-gray-700"
              >
                <i className="fa-solid fa-times"></i>
              </button>
            </div>
            <pre className="bg-gray-100 dark:bg-gray-900 p-4 rounded overflow-auto text-sm">
              {JSON.stringify(selectedLog, null, 2)}
            </pre>
          </div>
        </div>
      )}
    </div>
  );
}