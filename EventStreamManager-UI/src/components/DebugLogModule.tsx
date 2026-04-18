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
  const [processorName, setProcessorName] = useState<string>('');
  const [status, setStatus] = useState<StatusType>('');
  const [eventCode, setEventCode] = useState<string>('');
  const [requestDataKeyword, setRequestDataKeyword] = useState<string>('');
  const [strEventReferenceId, setStrEventReferenceId] = useState<string>('');
  const [isFinished, setIsFinished] = useState<string>('');
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
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());
  const [exporting, setExporting] = useState(false);

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
    setProcessorName('');
    setStatus('');
    setEventCode('');
    setRequestDataKeyword('');
    setStrEventReferenceId('');
    setIsFinished('');
    setStartDate('');
    setEndDate('');
  };

  const fetchHandles = async () => {
    if (!databaseType) return;
    
    setLoading(true);
    try {
      const params: eventLogService.GetEventHandlesParams = {
        databaseType,
        page,
        pageSize,
        eventId: eventId ? parseInt(eventId) : undefined,
        processorId: processorId || undefined,
        processorName: processorName || undefined,
        status: status || undefined,
        eventCode: eventCode || undefined,
        requestDataKeyword: requestDataKeyword || undefined,
        strEventReferenceId: strEventReferenceId || undefined,
        isFinished: isFinished === '' ? undefined : isFinished === 'true',
        startDate: startDate || undefined,
        endDate: endDate || undefined
      };
      
      const result = await eventLogService.getEventHandles(params);
      
      setHandles(result.list || []);
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

  const getScriptStatusBadge = (scriptSuccess?: boolean) => {
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
      const params: eventLogService.ExportEventHandlesParams = {
        databaseType,
        eventId: eventId ? parseInt(eventId) : undefined,
        processorId: processorId || undefined,
        processorName: processorName || undefined,
        status: status || undefined,
        eventCode: eventCode || undefined,
        requestDataKeyword: requestDataKeyword || undefined,
        strEventReferenceId: strEventReferenceId || undefined,
        isFinished: isFinished === '' ? undefined : isFinished === 'true',
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
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">处理器名称</label>
              <input
                type="text"
                value={processorName}
                onChange={(e) => setProcessorName(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
                placeholder="输入处理器名称"
              />
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
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">是否完成</label>
              <select
                value={isFinished}
                onChange={(e) => setIsFinished(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              >
                <option value="">全部</option>
                <option value="true">已完成</option>
                <option value="false">未完成</option>
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
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">请求数据关键字</label>
              <input
                type="text"
                value={requestDataKeyword}
                onChange={(e) => setRequestDataKeyword(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
                placeholder="输入关键字"
              />
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
                <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">最后处理时间</th>
                <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">事件创建时间</th>
                <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">操作</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {handles.map(handle => (
                <tr key={handle.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                  <td className="px-4 py-3 text-sm">{handle.id}</td>
                  <td className="px-4 py-3 text-sm">{handle.eventId}</td>
                  <td className="px-4 py-3 text-sm">{handle.eventCode || '-'}</td>
                  <td className="px-4 py-3">
                    <div className="text-sm font-medium">{handle.processorName}</div>
                    <div className="text-xs text-gray-500">{handle.processorId}</div>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getScriptStatusBadge(handle.scriptSuccess)}`}>
                      {handle.scriptSuccess === true ? '成功' : handle.scriptSuccess === false ? '失败' : '-'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getSendStatusBadge(handle.needToSend, handle.sendSuccess)}`}>
                      {getSendStatusLabel(handle.needToSend, handle.sendSuccess)}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-sm">{handle.handleTimes}</td>
                  <td className="px-4 py-3 text-sm">{handle.lastHandleElapsedMs || '-'}</td>
                  <td className="px-4 py-3 text-sm whitespace-nowrap">
                    {handle.lastHandleDatetime ? new Date(handle.lastHandleDatetime).toLocaleString() : '-'}
                  </td>
                  <td className="px-4 py-3 text-sm whitespace-nowrap">
                    {handle.createDatetime ? new Date(handle.createDatetime).toLocaleString() : '-'}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      <button
                        onClick={() => viewHandleDetails(handle.id)}
                        className="text-blue-600 hover:text-blue-700 text-sm whitespace-nowrap"
                      >
                        详情
                      </button>
                      <button
                        onClick={() => toggleRowExpand(handle.id)}
                        className="text-gray-600 hover:text-gray-700 text-sm whitespace-nowrap"
                      >
                        {expandedRows.has(handle.id) ? '收起' : '消息'}
                      </button>
                    </div>
                    {expandedRows.has(handle.id) && handle.lastHandleMessage && (
                      <div className="mt-2 p-2 bg-gray-50 dark:bg-gray-800 rounded text-xs max-w-md">
                        {handle.lastHandleMessage}
                      </div>
                    )}
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
    </div>
  );
}
