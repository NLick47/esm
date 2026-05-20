import { useState, useEffect, useCallback, useRef } from 'react';
import { toast } from 'sonner';
import * as eventLogService from '@/services/event-log.service';
import * as databaseService from '@/services/database.service';
import * as processorService from '@/services/processor.service';
import { EventHandle, EventHandleDetail, EventHandleStats } from '@/types/event-log';
import { PageLoading } from '@/components/ui/PageLoading';
import LogFilterBar from './debug-log/LogFilterBar';
import LogStatsCards from './debug-log/LogStatsCards';
import LogTable from './debug-log/LogTable';
import LogPagination from './debug-log/LogPagination';
import LogDetailModal from './debug-log/LogDetailModal';
import { StatusType } from './debug-log/utils';

export default function DebugLogModule() {
  // 基础数据
  const [databaseTypes, setDatabaseTypes] = useState<Array<{ value: string; label: string }>>([]);
  const [processors, setProcessors] = useState<Array<{ id: string; name: string }>>([]);
  const [eventCodes, setEventCodes] = useState<Array<{ value: string; label: string }>>([]);

  // 查询条件
  const [databaseType, setDatabaseType] = useState<string>('');
  const [eventId, setEventId] = useState<string>('');
  const [processorId, setProcessorId] = useState<string>('');
  const [status, setStatus] = useState<StatusType>('');
  const [eventCode, setEventCode] = useState<string>('');
  const [strEventReferenceId, setStrEventReferenceId] = useState<string>('');
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');

  // UI 状态
  const [showFilters, setShowFilters] = useState(true);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [retryingId, setRetryingId] = useState<number | null>(null);

  // 数据
  const [handles, setHandles] = useState<EventHandle[]>([]);
  const [stats, setStats] = useState<EventHandleStats>({
    total: 0, success: 0, failed: 0, deadLetter: 0, processing: 0
  });
  const [selectedHandle, setSelectedHandle] = useState<EventHandleDetail | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);

  // 用 ref 存储最新筛选条件，避免 fetchHandles 因筛选条件变化而频繁重建
  const filtersRef = useRef({
    eventId, processorId, status, eventCode, strEventReferenceId, startDate, endDate
  });
  filtersRef.current = { eventId, processorId, status, eventCode, strEventReferenceId, startDate, endDate };

  // 加载基础数据
  useEffect(() => {
    loadDatabaseTypes();
    loadProcessors();
    loadEventCodes();
  }, []);

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

  // databaseType 变化时重置页码，由 page 变化触发数据加载
  useEffect(() => {
    if (databaseType) {
      setPage(1);
    }
  }, [databaseType]);

  // 获取列表数据
  const fetchHandles = useCallback(async () => {
    if (!databaseType) return;
    const filters = filtersRef.current;

    setLoading(true);
    try {
      const params: eventLogService.GetEventHandlesRequest = {
        databaseType,
        page,
        pageSize,
        eventId: filters.eventId ? parseInt(filters.eventId) : undefined,
        processorId: filters.processorId || undefined,
        status: filters.status || undefined,
        eventCode: filters.eventCode || undefined,
        strEventReferenceId: filters.strEventReferenceId || undefined,
        startDate: filters.startDate || undefined,
        endDate: filters.endDate || undefined
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
  }, [databaseType, page, pageSize]);

  // 获取统计数据
  const fetchStats = useCallback(async () => {
    if (!databaseType) return;
    const filters = filtersRef.current;

    try {
      const params = {
        databaseType,
        eventId: filters.eventId ? parseInt(filters.eventId) : undefined,
        processorId: filters.processorId || undefined,
        status: filters.status || undefined,
        eventCode: filters.eventCode || undefined,
        strEventReferenceId: filters.strEventReferenceId || undefined,
        startDate: filters.startDate || undefined,
        endDate: filters.endDate || undefined
      };

      const result = await eventLogService.getEventHandleStats(params);
      setStats(result);
    } catch (error) {
      console.error('获取统计信息失败:', error);
    }
  }, [databaseType]);

  // 统一自动加载入口（防抖 300ms）
  useEffect(() => {
    if (!databaseType) return;
    const timer = setTimeout(async () => {
      await fetchHandles();
      await fetchStats();
    }, 300);
    return () => clearTimeout(timer);
  }, [fetchHandles, fetchStats]);

  const resetFilters = () => {
    setEventId('');
    setProcessorId('');
    setStatus('');
    setEventCode('');
    setStrEventReferenceId('');
    setStartDate('');
    setEndDate('');
  };

  // 手动查询
  const handleQuery = () => {
    if (page !== 1) {
      setPage(1);
    } else {
      fetchHandles();
      fetchStats();
    }
  };

  // 刷新按钮
  const handleRefresh = async () => {
    await fetchHandles();
    await fetchStats();
  };

  const viewHandleDetails = async (handleId: number) => {
    const handle = handles.find(h => h.id === handleId);
    if (!handle) {
      toast.error('未找到对应的处理记录');
      return;
    }

    setDetailLoading(true);
    try {
      const detail = await eventLogService.getHandleDetail(databaseType, handleId);
      setSelectedHandle(detail);
    } catch (error) {
      toast.error('获取详情失败');
      console.error(error);
    } finally {
      setDetailLoading(false);
    }
  };

  const handleRetryDeadLetter = async (handleId: number) => {
    if (!databaseType) return;
    setRetryingId(handleId);
    try {
      await eventLogService.retryDeadLetter(databaseType, handleId);
      toast.success('死信已重置，将在下次扫描时重新处理');
      await fetchHandles();
      await fetchStats();
    } catch (error) {
      toast.error('重置死信失败');
      console.error(error);
    } finally {
      setRetryingId(null);
    }
  };

  const handleExport = async () => {
    if (!databaseType) {
      toast.error('请先选择数据库类型');
      return;
    }

    setExporting(true);
    try {
      const filters = filtersRef.current;
      const params: eventLogService.ExportEventHandlesRequest = {
        databaseType,
        eventId: filters.eventId ? parseInt(filters.eventId) : undefined,
        processorId: filters.processorId || undefined,
        status: filters.status || undefined,
        eventCode: filters.eventCode || undefined,
        strEventReferenceId: filters.strEventReferenceId || undefined,
        startDate: filters.startDate || undefined,
        endDate: filters.endDate || undefined
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

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">处理记录调试</h2>
        <div className="flex items-center gap-3">
          <button
            onClick={handleRefresh}
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

      {handles.length === 0 && loading ? (
        <PageLoading className="h-96" />
      ) : (
        <>
          <div className="space-y-4">
            <LogFilterBar
              showFilters={showFilters}
              onToggleFilters={() => setShowFilters(!showFilters)}
              eventId={eventId}
              onEventIdChange={setEventId}
              strEventReferenceId={strEventReferenceId}
              onStrEventReferenceIdChange={setStrEventReferenceId}
              processorId={processorId}
              onProcessorIdChange={setProcessorId}
              status={status}
              onStatusChange={setStatus}
              eventCode={eventCode}
              onEventCodeChange={setEventCode}
              startDate={startDate}
              onStartDateChange={setStartDate}
              endDate={endDate}
              onEndDateChange={setEndDate}
              processors={processors}
              eventCodes={eventCodes}
              onReset={resetFilters}
              onQuery={handleQuery}
            />
            <LogStatsCards stats={stats} />
            <LogTable
              handles={handles}
              retryingId={retryingId}
              onViewDetail={viewHandleDetails}
              onRetry={handleRetryDeadLetter}
            />
          </div>
          <LogPagination
            page={page}
            pageSize={pageSize}
            total={total}
            loading={loading}
            onPageChange={setPage}
            onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
          />
        </>
      )}

      <LogDetailModal
        handle={selectedHandle}
        loading={detailLoading}
        onClose={() => setSelectedHandle(null)}
      />
    </div>
  );
}
