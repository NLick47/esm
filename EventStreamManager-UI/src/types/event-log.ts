export interface EventHandleResult {
  id: number;
  eventId: number;
  eventCode: string;
  eventName: string;
  eventType: string;
  hospitalId: number;
  operatorName: string;
  processorId: string;
  processorName: string;
  handleTimes: number;
  lastHandleStatus: 'Success' | 'Fail' | 'Exception' | 'Processing';
  lastHandleMessage?: string;
  lastHandleDatetime: string;
  lastHandleElapsedMs?: number;
  isFinished: boolean;
  createDatetime: string;
}


export interface PaginatedResult<T> {
  list: T[];
  total: number;
  page: number;
  pageSize: number;
}