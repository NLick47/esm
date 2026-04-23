import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import * as eventLogService from '@/services/event-log.service';
import * as databaseService from '@/services/database.service';
import * as processorService from '@/services/processor.service';
import { EventHandle } from '@/types/event-log';

type StatusType = 'Success' | 'Fail' | 'Exception' | 'Processing' | '';

export default function DebugLogModule() {
  const [databaseType, setDatabaseType] = useState<string>('');
  const [eventId, setEventId] = useState<string>('');
  const [processorId, setProcessorId] = useState<string>('');
  const [status, setStatus] = useState<StatusType>('');
  const [eventCode, setEventCode] = useState<string>('');
  const [strEventReferenceId, setStrEventReferenceId] = useState<string>('');
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');
  
  const [showFilters, setShowFilters] = useState(true);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [total, setTotal] = useState(0);
  
  const [handles, setHandles] = useState<EventHandle[]>([]);
  const [selectedEvent, setSelectedEvent] = useState<EventHandle | null>(null);
  const [selectedHandle, setSelectedHandle] = useState<EventHandle | null>(null);
  
  const [loading, setLoading] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [retryingId, setRetryingId] = useState<number | null>(null);

  const [databaseTypes, setDatabaseTypes] = useState<Array<{value: string; label: string}>>([]);
  const [processors, setProcessors] = useState<Array<{id: string; name: string}>>([]);
  const [eventCodes, setEventCodes] = useState<Array<{value: string; label: string}>>([]);

  useEffect(() => {
    loadDatabaseTypes();
    loadProcessors();
    loadEventCodes();
  }, []);

  useEffect(() => {
    if (databaseType) {
      setPage(1);
      fetchHandles();
    }
  }, [databaseType]);

  useEffect(() => {
    if (databaseType) {
      fetchHandles();
    }
  }, [page]);

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

  const loadEventCodes = async () => {
    try {
      const data = await processorService.getEventCodes();
      setEventCodes(data.map(code => ({ 
        value: code.code, 
        label: code.description || code.code 
      })));
    } catch (error) {
      console.error('加载事件码列表失败:', error);
    }
  };

  const resetFilters = () => {
    setEventId('');
    setProcessorId('');
    setStatus('');
    setEventCode('');
    setStrEventReferenceId('');
    setStartDate('');
    setEndDate('');
  };

  const fetchHandles = async () => {
    if (!databaseType) return;
    
    setLoading(true);
    try {
      const params: eventLogService.GetEventHandlesRequest = {
        databaseType,
        page,
        pageSize,
        eventId: eventId ? parseInt(eventId) : undefined,
        processorId: processorId || undefined,
        status: status || undefined,
        eventCode: eventCode || undefined,
        strEventReferenceId: strEventReferenceId || undefined,
        startDate: startDate || undefined,
        endDate: endDate || undefined
      };
      
      const result = await eventLogService.getEventHandles(params);
      
      setHandles(result.items || []);
      setTotal(result.total || 0);
    } catch (error) {
      toast.error('获取处理记录失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const viewHandleDetails = async (handleId: number) => {
    const handle = handles.find(h => h.id === handleId);
    if (handle) {
      setSelectedHandle(handle);
    } else {
      toast.error('未找到对应的处理记录');
    }
  };

  const handleRetryDeadLetter = async (handleId: number) => {
    if (!databaseType) return;
    setRetryingId(handleId);
    try {
      await eventLogService.retryDeadLetter(databaseType, handleId);
      toast.success('死信已重置，将在下次扫描时重新处理');
      // 刷新当前列表
      await fetchHandles();
    } catch (error) {
      toast.error('重置死信失败');
      console.error(error);
    } finally {
      setRetryingId(null);
    }
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

  const getScriptStatusBadge = (scriptSuccess?: boolean, isDeadLetter?: boolean) => {
    if (isDeadLetter) {
      return 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400';
    }
    if (scriptSuccess === true) {
      return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400';
    }
    if (scriptSuccess === false) {
      return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400';
    }
    return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
  };

  const getSendStatusBadge = (needToSend?: boolean, sendSuccess?: boolean) => {
    if (needToSend === false) {
      return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
    }
    if (sendSuccess === true) {
      return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400';
    }
    if (sendSuccess === false) {
      return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400';
    }
    return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
  };

  const formatJsonData = (data: string): string => {
    try {
      const parsed = JSON.parse(data);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return data;
    }
  };

  const getSendStatusLabel = (needToSend?: boolean, sendSuccess?: boolean) => {
    if (needToSend === false) {
      return '无需发送';
    }
    if (sendSuccess === true) {
      return '发送成功';
    }
    if (sendSuccess === false) {
      return '发送失败';
    }
    return '-';
  };

  const handleExport = async () => {
    if (!databaseType) {
      toast.error('请先选择数据库类型');
      return;
    }
    
    setExporting(true);
    try {
      const params: eventLogService.ExportEventHandlesRequest = {
        databaseType,
        eventId: eventId ? parseInt(eventId) : undefined,
        processorId: processorId || undefined,
        status: status || undefined,
        eventCode: eventCode || undefined,
        strEventReferenceId: strEventReferenceId || undefined,
        startDate: startDate || undefined,
        endDate: endDate || undefined
      };
      
      eventLogService.downloadExportFile(params);
      toast.success('导出成功');
    } catch (error) {
      toast.error('导出失败');
      console.error(error);
    } finally {
      setExporting(false);
    }
  };

  const toggleFilters = () => {
    setShowFilters(!showFilters);
  };

  const renderFilterBar = () => (
    <div className="bg-white dark:bg-gray-800 p-4 rounded-lg shadow-sm space-y-4">
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-medium">筛选条件</h3>
        <button
          onClick={toggleFilters}
          className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300 flex items-center gap-1"
        >
          <i className={`fa-solid fa-chevron-${showFilters ? 'up' : 'down'} text-xs`}></i>
          <span className="text-sm">{showFilters ? '隐藏' : '显示'}筛选</span>
        </button>
      </div>

      {showFilters && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4">
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">事件ID</label>
              <input
                type="number"
                value={eventId}
                onChange={(e) => setEventId(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
                placeholder="输入事件ID"
                min="1"
              />
            </div>

            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">事件引用ID</label>
              <input
                type="text"
                value={strEventReferenceId}
                onChange={(e) => setStrEventReferenceId(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
                placeholder="输入事件引用ID"
              />
            </div>

            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">处理器ID</label>
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

            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">处理状态</label>
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value as StatusType)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              >
                <option value="">全部状态</option>
                <option value="Success">成功</option>
                <option value="Fail">失败</option>
                <option value="Exception">异常</option>
                <option value="Processing">处理中</option>
              </select>
            </div>

            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">事件码</label>
              <select
                value={eventCode}
                onChange={(e) => setEventCode(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              >
                <option value="">全部事件码</option>
                {eventCodes.map(code => (
                  <option key={code.value} value={code.value}>{code.label}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">开始日期</label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              />
            </div>

            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">结束日期</label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              />
            </div>
          </div>

          <div className="flex justify-end gap-2">
            <button
              onClick={resetFilters}
              className="px-4 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-gray-800"
            >
              重置
            </button>
            <button
              onClick={fetchHandles}
              className="px-4 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700"
            >
              查询
            </button>
          </div>
        </>
      )}
    </div>
  );

  const renderHandlesTable = () => (
    <div className="space-y-4">
      {renderFilterBar()}
      
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
                  onClick={() => viewHandleDetails(handle.id)}
                  className="hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer"
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
                  <td className="px-4 py-3 text-sm whitespace-nowrap">
                    {handle.lastHandleDatetime ? new Date(handle.lastHandleDatetime).toLocaleString('zh-CN', { year: '2-digit', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-') : '-'}
                  </td>
                  <td className="px-4 py-3 text-sm whitespace-nowrap">
                    {handle.createDatetime ? new Date(handle.createDatetime).toLocaleString('zh-CN', { year: '2-digit', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-') : '-'}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          viewHandleDetails(handle.id);
                        }}
                        className="text-blue-600 hover:text-blue-700 text-sm whitespace-nowrap"
                      >
                        详情
                      </button>
                      {handle.isDeadLetter && (
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            handleRetryDeadLetter(handle.id);
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
    </div>
  );

  const renderPagination = () => (
    <div className="flex items-center justify-between mt-4">
      <div className="text-sm text-gray-500">共 {total} 条记录</div>
      <div className="flex gap-2">
        <button
          onClick={() => setPage(p => Math.max(1, p - 1))}
          disabled={page === 1 || loading}
          className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50 dark:border-gray-700"
        >
          上一页
        </button>
        <span className="px-3 py-1">{page} / {Math.ceil(total / pageSize) || 1}</span>
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
        <h2 className="text-2xl font-bold">处理记录调试</h2>
        <div className="flex items-center gap-3">
          <button
            onClick={fetchHandles}
            disabled={loading || !databaseType}
            className="px-4 py-2 rounded-lg border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 transition-colors"
          >
            <i className={`fa-solid fa-rotate-right ${loading ? 'fa-spin' : ''}`}></i>
            刷新
          </button>
          <button
            onClick={handleExport}
            disabled={exporting || !databaseType}
            className="px-4 py-2 rounded-lg bg-green-600 text-white hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
          >
            {exporting ? (
              <>
                <i className="fa-solid fa-spinner fa-spin"></i>
                导出中...
              </>
            ) : (
              <>
                <i className="fa-solid fa-download"></i>
                导出日志
              </>
            )}
          </button>
        </div>
      </div>

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

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <i className="fa-solid fa-spinner fa-spin text-3xl text-blue-600"></i>
        </div>
      ) : (
        <>
          {renderHandlesTable()}
          {renderPagination()}
        </>
      )}

      {selectedEvent && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-4xl w-full mx-4 max-h-[90vh] overflow-auto">
            <div className="flex justify-between items-start mb-4">
              <h3 className="text-lg font-semibold">事件详情 - ID: {selectedEvent.eventId}</h3>
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

      {selectedHandle && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={() => setSelectedHandle(null)}>
          <div
            className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-4xl w-full mx-4 max-h-[90vh] overflow-auto"
            onClick={(e) => e.stopPropagation()}
          >
            {/* 弹窗头部 */}
            <div className="flex items-center justify-between mb-5 pb-4 border-b border-gray-200 dark:border-gray-700">
              <div className="flex items-center gap-3">
                <h3 className="text-lg font-semibold">处理记录详情</h3>
                <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${getScriptStatusBadge(selectedHandle.scriptSuccess, selectedHandle.isDeadLetter)}`}>
                  {selectedHandle.isDeadLetter ? '死信' : selectedHandle.scriptSuccess === true ? '脚本成功' : selectedHandle.scriptSuccess === false ? '脚本失败' : '未执行'}
                </span>
                <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${getSendStatusBadge(selectedHandle.needToSend, selectedHandle.sendSuccess)}`}>
                  {getSendStatusLabel(selectedHandle.needToSend, selectedHandle.sendSuccess)}
                </span>
              </div>
              <button
                onClick={() => setSelectedHandle(null)}
                className="text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
              >
                <i className="fa-solid fa-times text-lg"></i>
              </button>
            </div>

            <div className="space-y-6">
              {/* 核心信息 */}
              <div>
                <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-3">基本信息</h4>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-y-4 gap-x-6 text-sm">
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">记录 ID</div>
                    <div className="font-mono mt-0.5">{selectedHandle.id}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">事件 ID</div>
                    <div className="font-mono mt-0.5">{selectedHandle.eventId}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">事件码</div>
                    <div className="mt-0.5">{selectedHandle.eventCode || '-'}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">事件名称</div>
                    <div className="mt-0.5">{selectedHandle.eventName || '-'}</div>
                  </div>
                  <div className="col-span-2">
                    <div className="text-xs text-gray-500 dark:text-gray-400">引用 ID</div>
                    <div className="font-mono mt-0.5">{selectedHandle.strEventReferenceId || '-'}</div>
                  </div>
                  <div className="col-span-2">
                    <div className="text-xs text-gray-500 dark:text-gray-400">处理器</div>
                    <div className="mt-0.5">{selectedHandle.processorName} <span className="text-gray-400">({selectedHandle.processorId})</span></div>
                  </div>
                </div>
              </div>

              {/* 执行结果 */}
              <div>
                <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-3">执行结果</h4>
                <div className="grid grid-cols-2 md:grid-cols-5 gap-y-4 gap-x-6 text-sm">
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">脚本执行</div>
                    <div className="mt-1">
                      <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${getScriptStatusBadge(selectedHandle.scriptSuccess, selectedHandle.isDeadLetter)}`}>
                        {selectedHandle.isDeadLetter ? '死信' : selectedHandle.scriptSuccess === true ? '成功' : selectedHandle.scriptSuccess === false ? '失败' : '-'}
                      </span>
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">接口发送</div>
                    <div className="mt-1">
                      <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${getSendStatusBadge(selectedHandle.needToSend, selectedHandle.sendSuccess)}`}>
                        {getSendStatusLabel(selectedHandle.needToSend, selectedHandle.sendSuccess)}
                      </span>
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">处理次数</div>
                    <div className="mt-0.5">{selectedHandle.handleTimes}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">耗时</div>
                    <div className="mt-0.5">{selectedHandle.lastHandleElapsedMs ? `${selectedHandle.lastHandleElapsedMs} ms` : '-'}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">是否完成</div>
                    <div className="mt-0.5">{selectedHandle.isFinished ? '是' : '否'}</div>
                  </div>
                  {selectedHandle.reason && (
                    <div className="col-span-2 md:col-span-3">
                      <div className="text-xs text-gray-500 dark:text-gray-400">脚本返回信息</div>
                      <div className="mt-0.5 text-gray-700 dark:text-gray-300">{selectedHandle.reason}</div>
                    </div>
                  )}
                </div>
              </div>

              {/* 时间 */}
              <div>
                <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-3">时间</h4>
                <div className="grid grid-cols-2 gap-6 text-sm">
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">事件创建</div>
                    <div className="font-mono mt-0.5">{selectedHandle.createDatetime ? new Date(selectedHandle.createDatetime).toLocaleString() : '-'}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">最后处理</div>
                    <div className="font-mono mt-0.5">{selectedHandle.lastHandleDatetime ? new Date(selectedHandle.lastHandleDatetime).toLocaleString() : '-'}</div>
                  </div>
                </div>
              </div>

              {/* 消息 */}
              {selectedHandle.lastHandleMessage && (
                <div>
                  <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-2">处理消息</h4>
                  <div className="text-sm text-gray-800 dark:text-gray-200 whitespace-pre-wrap break-words font-mono bg-gray-50 dark:bg-gray-900/50 p-3 rounded border border-gray-200 dark:border-gray-700">
                    {selectedHandle.lastHandleMessage}
                  </div>
                </div>
              )}

              {/* 请求与响应 */}
              {(selectedHandle.requestData || selectedHandle.responseData) && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {selectedHandle.requestData && (
                    <div>
                      <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-2">请求数据</h4>
                      <pre className="text-xs text-gray-800 dark:text-gray-200 whitespace-pre-wrap break-words font-mono bg-gray-50 dark:bg-gray-900/50 p-3 rounded border border-gray-200 dark:border-gray-700 max-h-80 overflow-auto">
                        {formatJsonData(selectedHandle.requestData)}
                      </pre>
                    </div>
                  )}
                  {selectedHandle.responseData && (
                    <div>
                      <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-2">响应数据</h4>
                      <pre className="text-xs text-gray-800 dark:text-gray-200 whitespace-pre-wrap break-words font-mono bg-gray-50 dark:bg-gray-900/50 p-3 rounded border border-gray-200 dark:border-gray-700 max-h-80 overflow-auto">
                        {formatJsonData(selectedHandle.responseData)}
                      </pre>
                    </div>
                  )}
                </div>
              )}
            </div>

            {/* 弹窗底部 */}
            <div className="mt-6 pt-4 border-t border-gray-200 dark:border-gray-700 flex justify-end">
              <button
                onClick={() => setSelectedHandle(null)}
                className="px-4 py-2 rounded bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600 text-sm"
              >
                关闭
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
