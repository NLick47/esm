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
  scriptSuccess?: boolean;
  sendSuccess?: boolean;
  needToSend?: boolean;
  isDeadLetter?: boolean;
  requestData?: string;
  responseData?: string;
  reason?: string;
  isFinished: boolean;
  createDatetime: string;
  eventName: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface GetEventHandlesRequest {
  databaseType: string;
  eventId?: number;
  strEventReferenceId?: string;
  processorId?: string;
  status?: string;
  eventCode?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}

export interface ExportEventHandlesRequest {
  databaseType: string;
  eventId?: number;
  strEventReferenceId?: string;
  processorId?: string;
  status?: string;
  eventCode?: string;
  startDate?: string;
  endDate?: string;
}
