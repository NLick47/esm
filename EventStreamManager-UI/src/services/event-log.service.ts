// event-log.service.ts
import { get } from '@/utils/request';
import { 
  EventHandle, 
  EventHandleLog, 
  EventWithHandles, 
  EventLogStats,
  ProcessorStats,
  FailedHandle,
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
}): Promise<PaginatedResult<EventHandle>> {
  const { databaseType, ...queryParams } = params;
  return get(`${BASE_PATH}/handles`, { params: { databaseType, ...queryParams } });
}

/**
 * 获取单条处理记录
 */
export function getEventHandle(databaseType: string, id: number): Promise<EventHandle> {
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
}): Promise<PaginatedResult<EventHandleLog>> {
  const { databaseType, ...queryParams } = params;
  return get(`${BASE_PATH}/logs`, { params: { databaseType, ...queryParams } });
}

/**
 * 获取单条日志
 */
export function getEventLog(databaseType: string, id: number): Promise<EventHandleLog> {
  return get(`${BASE_PATH}/logs/${databaseType}/${id}`);
}

/**
 * 获取事件及处理记录
 */
export function getEventWithHandles(databaseType: string, eventId: number): Promise<EventWithHandles> {
  return get(`${BASE_PATH}/event-with-handles/${databaseType}/${eventId}`);
}

/**
 * 获取统计状态
 */
export function getEventLogStats(databaseType: string): Promise<EventLogStats> {
  return get(`${BASE_PATH}/stats/${databaseType}`);
}

/**
 * 按处理器统计
 */
export function getStatsByProcessor(databaseType: string): Promise<ProcessorStats[]> {
  return get(`${BASE_PATH}/stats/${databaseType}/by-processor`);
}

/**
 * 获取失败的处理记录
 */
export function getFailedHandles(params: {
  databaseType: string;
  page: number;
  pageSize: number;
  processorId?: string;
  maxRetryTimes?: number;
}): Promise<PaginatedResult<FailedHandle>> {
  const { databaseType, ...queryParams } = params;
  return get(`${BASE_PATH}/failed-handles/${databaseType}`, { params: queryParams });
}