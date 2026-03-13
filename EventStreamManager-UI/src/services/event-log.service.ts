// event-log.service.ts
import { get } from '@/utils/request';
import { 
  EventHandleResult,
  PaginatedResult
} from '@/types';

const BASE_PATH = '/api/EventLog';

/**
 * 获取处理记录列表
 */
export function getEventHandles(params: {
  databaseType: string;
  page: number;
  pageSize: number;
  eventId?: number;
  processorId?: string;
  processorName?: string;
  status?: string;
  isFinished?: boolean;
}): Promise<PaginatedResult<EventHandleResult>> {
  const { databaseType, ...queryParams } = params;
  return get(`${BASE_PATH}/handles`, { params: { databaseType, ...queryParams } });
}

/**
 * 获取单条处理记录
 */
export function getEventHandle(databaseType: string, id: number): Promise<EventHandleResult> {
  return get(`${BASE_PATH}/handles/${databaseType}/${id}`);
}

/**
 * 获取日志列表
 */
export function getEventLogs(params: {
  databaseType: string;
  page: number;
  pageSize: number;
  eventId?: number;
  eventHandleId?: number;
  processorId?: string;
  status?: string;
}): Promise<PaginatedResult<EventHandleResult>> {
  const { databaseType, ...queryParams } = params;
  return get(`${BASE_PATH}/logs`, { params: { databaseType, ...queryParams } });
}

/**
 * 获取单条日志
 */
export function getEventLog(databaseType: string, id: number): Promise<EventHandleResult> {
  return get(`${BASE_PATH}/logs/${databaseType}/${id}`);
}




