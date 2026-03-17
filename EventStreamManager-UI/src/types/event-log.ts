export interface EventHandle {
  id: number;
  eventId: number;
  eventCode?: string;
  processorId: string;
  processorName: string;
  handleTimes: number;
  lastHandleStatus: string;
  lastHandleMessage?: string;
  lastHandleDatetime?: string;
  lastHandleElapsedMs?: number;
  strEventReferenceId?: string;
  requestData?: string;
  isFinished: boolean;
  createDatetime: string;
  eventName: string;
}

export interface PagedResult<T> {
  list: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface GetEventHandlesParams {
  databaseType: string;
  eventId?: number;
  strEventReferenceId?: string;
  processorId?: string;
  processorName?: string;
  status?: string;
  isFinished?: boolean;
  eventCode?: string;
  requestDataKeyword?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}

export interface ExportEventHandlesParams {
  databaseType: string;
  eventId?: number;
  strEventReferenceId?: string;
  processorId?: string;
  processorName?: string;
  status?: string;
  isFinished?: boolean;
  eventCode?: string;
  requestDataKeyword?: string;
  startDate?: string;
  endDate?: string;
}
