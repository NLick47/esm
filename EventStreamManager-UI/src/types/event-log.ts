// types/event-log.ts

export interface EventHandle {
  id: number;
  eventId: number;
  processorId: string;
  processorName: string;
  handleTimes: number;
  lastHandleStatus: 'Success' | 'Fail' | 'Exception' | 'Processing';
  lastHandleMessage?: string;
  lastHandleDatetime: string;
  lastHandleElapsedMs?: number;
  isFinished: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface EventHandleLog {
  id: number;
  eventId: number;
  eventHandleId: number;
  processorId: string;
  processorName: string;
  status: 'Success' | 'Fail' | 'Exception' | 'Processing';
  message?: string;
  handleDatetime: string;
  elapsedMs?: number;
  createdAt: string;
}

export interface EventWithHandles {
  event: Event;
  handles: EventHandle[];
  logs: EventHandleLog[];
}

export interface EventLogStats {
  total: number;
  finished: number;
  pending: number;
  success: number;
  failed: number;
}

export interface ProcessorStats {
  processorId: string;
  processorName: string;
  totalCount: number;
  successCount: number;
  failedCount: number;
  pendingCount: number;
  avgHandleTimes?: number;
}

export interface FailedHandle extends EventHandle {
  // 复用 EventHandle 类型
}

export interface PaginatedResult<T> {
  list: T[];
  total: number;
  page: number;
  pageSize: number;
}