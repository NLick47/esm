import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import * as eventLogService from '@/services/event-log.service';
import * as databaseService from '@/services/database.service';
import * as processorService from '@/services/processor.service';
import { EventHandleResult } from '@/types';
 
// 状态类型定义
type StatusType = 'Success' | 'Fail' | 'Exception' | 'Processing' | '';

export default function DebugLogModule() {
  const [databaseType, setDatabaseType] = useState<string>('');
  
  const [eventId, setEventId] = useState<string>('');              
  const [processorId, setProcessorId] = useState<string>('');    
  const [status, setStatus] = useState<StatusType>('');          
  const [eventCode, setEventCode] = useState<string>('');         
  const [startDate, setStartDate] = useState<string>('');        
  const [endDate, setEndDate] = useState<string>('');            
  
  // 隐藏/显示筛选条件功能
  const [showFilters, setShowFilters] = useState(true);
  
  // 分页
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [total, setTotal] = useState(0);
  
  // 数据
  const [handles, setHandles] = useState<EventHandleResult[]>([]);
  const [selectedEvent, setSelectedEvent] = useState<EventHandleResult | null>(null);
  const [selectedHandle, setSelectedHandle] = useState<EventHandleResult | null>(null);
  
  // 状态
  const [loading, setLoading] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());

  // 下拉框数据
  const [databaseTypes, setDatabaseTypes] = useState<Array<{value: string; label: string}>>([]);
  const [processors, setProcessors] = useState<Array<{id: string; name: string}>>([]);
  const [eventCodes, setEventCodes] = useState<Array<{value: string; label: string}>>([]);

  // 初始化
  useEffect(() => {
    loadDatabaseTypes();
    loadProcessors();
    loadEventCodes();
  }, []);

  // 切换数据库时重置并加载数据
  useEffect(() => {
    if (databaseType) {
      setPage(1);
      fetchHandles();
    }
  }, [databaseType]);

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
    setStartDate('');
    setEndDate('');
  };

  const fetchHandles = async () => {
    if (!databaseType) return;
    
    setLoading(true);
    try {
      const params: any = {
        databaseType,
        page,
        pageSize,
        eventId: eventId ? parseInt(eventId) : undefined,
        processorId: processorId || undefined,
        status: status || undefined,
        eventCode: eventCode || undefined,
        startDate: startDate || undefined,
        endDate: endDate || undefined
      };
      
      const result = await eventLogService.getEventHandles(params);
      
      setHandles(result.list);
      setTotal(result.total);
    } catch (error) {
      toast.error('获取处理记录失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };



  const viewHandleDetails = async (handleId: number) => {
    if (!databaseType) return;
    
    setDetailLoading(true);
    try {
      // 直接从handles列表中查找对应的处理记录
      const selectedHandleData = handles.find(h => h.id === handleId);
      
      if (selectedHandleData) {
        setSelectedHandle(selectedHandleData);
        // 显示成功提示
        toast.success('已加载处理记录详情');
      } else {
        toast.error('未找到对应的处理记录');
      }
    } catch (error) {
      toast.error('获取处理记录详情失败');
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


  const handleExport = () => {
   
    // TODO: 实现导出功能
  };

  // 切换筛选条件显示/隐藏
  const toggleFilters = () => {
    setShowFilters(!showFilters);
  };

  const renderFilterBar = () => (
    <div className="bg-white dark:bg-gray-800 p-4 rounded-lg shadow-sm space-y-4">
      {/* 筛选条件标题和切换按钮 */}
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

      {/* 筛选条件区域 - 可隐藏/显示 */}
      {showFilters && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4">
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

            {/* 处理器名称（下拉框） */}
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
                处理器名称
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

            {/* 处理状态（下拉框） */}
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
                处理状态
              </label>
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

            {/* 事件码（下拉框） */}
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
                事件码
              </label>
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

            {/* 创建时间范围 - 开始日期 */}
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
                开始日期
              </label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              />
            </div>

            {/* 创建时间范围 - 结束日期 */}
            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">
                结束日期
              </label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              />
            </div>
          </div>

          {/* 操作按钮 */}
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
                <th className="px-4 py-3 text-left text-sm font-medium whitespace-nowrap">状态</th>
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
                    <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${getStatusBadge(handle.lastHandleStatus)}`}>
                      {handle.lastHandleStatus}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-sm">{handle.handleTimes}</td>
                  <td className="px-4 py-3 text-sm">{handle.lastHandleElapsedMs || '-'}</td>
                  <td className="px-4 py-3 text-sm whitespace-nowrap">
                    {new Date(handle.lastHandleDatetime).toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-sm whitespace-nowrap">
                    {handle.createDatetime ? new Date(handle.createDatetime).toLocaleString() : '-'}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      <button
                        onClick={() => viewHandleDetails(handle.id)}
                        className="text-blue-600 hover:text-blue-700 text-sm whitespace-nowrap"
                        title="查看详情"
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
        <h2 className="text-2xl font-bold">处理记录调试</h2>
        <div className="flex items-center gap-3">
          {detailLoading && (
            <div className="text-sm text-gray-500">
              <i className="fa-solid fa-spinner fa-spin mr-1"></i>
              加载详情中...
            </div>
          )}
          {/* 导出按钮 - 只加入UI，不实现功能 */}
          <button
            onClick={handleExport}
            className="px-4 py-2 rounded-lg bg-green-600 text-white hover:bg-green-700 flex items-center gap-2"
          >
            <i className="fa-solid fa-download"></i>
            导出日志
          </button>
        </div>
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
    </div>
  );
}