import { StatusType } from './utils';

interface ProcessorOption {
  id: string;
  name: string;
}

interface EventCodeOption {
  value: string;
  label: string;
}

interface Props {
  showFilters: boolean;
  onToggleFilters: () => void;
  eventId: string;
  onEventIdChange: (v: string) => void;
  strEventReferenceId: string;
  onStrEventReferenceIdChange: (v: string) => void;
  processorId: string;
  onProcessorIdChange: (v: string) => void;
  status: StatusType;
  onStatusChange: (v: StatusType) => void;
  eventCode: string;
  onEventCodeChange: (v: string) => void;
  startDate: string;
  onStartDateChange: (v: string) => void;
  endDate: string;
  onEndDateChange: (v: string) => void;
  processors: ProcessorOption[];
  eventCodes: EventCodeOption[];
  onReset: () => void;
  onQuery: () => void;
}

export default function LogFilterBar({
  showFilters, onToggleFilters,
  eventId, onEventIdChange,
  strEventReferenceId, onStrEventReferenceIdChange,
  processorId, onProcessorIdChange,
  status, onStatusChange,
  eventCode, onEventCodeChange,
  startDate, onStartDateChange,
  endDate, onEndDateChange,
  processors, eventCodes,
  onReset, onQuery
}: Props) {
  return (
    <div className="bg-white dark:bg-gray-800 p-4 rounded-lg shadow-sm space-y-4">
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-medium">筛选条件</h3>
        <button
          onClick={onToggleFilters}
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
                onChange={(e) => onEventIdChange(e.target.value)}
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
                onChange={(e) => onStrEventReferenceIdChange(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
                placeholder="输入事件引用ID"
              />
            </div>

            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">处理器ID</label>
              <select
                value={processorId}
                onChange={(e) => onProcessorIdChange(e.target.value)}
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
                onChange={(e) => onStatusChange(e.target.value as StatusType)}
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
                onChange={(e) => onEventCodeChange(e.target.value)}
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
                onChange={(e) => onStartDateChange(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              />
            </div>

            <div>
              <label className="block mb-1 text-sm font-medium text-gray-700 dark:text-gray-300">结束日期</label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => onEndDateChange(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 dark:border-gray-700 dark:bg-gray-800"
              />
            </div>
          </div>

          <div className="flex justify-end gap-2">
            <button
              onClick={onReset}
              className="px-4 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-gray-800"
            >
              重置
            </button>
            <button
              onClick={onQuery}
              className="px-4 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700"
            >
              查询
            </button>
          </div>
        </>
      )}
    </div>
  );
}
