import { get } from '@/utils/request';
import { getApiUrl } from '@/config/api.config';
import type { 
  EventHandle, 
  PagedResult, 
  GetEventHandlesParams, 
  ExportEventHandlesParams 
} from '@/types/event-log';

export type { 
  EventHandle, 
  PagedResult, 
  GetEventHandlesParams, 
  ExportEventHandlesParams 
};

const BASE_PATH = '/api/EventLog';

export function getEventHandles(params: GetEventHandlesParams): Promise<PagedResult<EventHandle>> {
  return get(`${BASE_PATH}/handles`, { params });
}

export function exportEventHandles(params: ExportEventHandlesParams): string {
  const queryParams = new URLSearchParams();
  queryParams.append('databaseType', params.databaseType);
  
  if (params.eventId !== undefined) {
    queryParams.append('eventId', params.eventId.toString());
  }
  if (params.strEventReferenceId) {
    queryParams.append('strEventReferenceId', params.strEventReferenceId);
  }
  if (params.processorId) {
    queryParams.append('processorId', params.processorId);
  }
  if (params.processorName) {
    queryParams.append('processorName', params.processorName);
  }
  if (params.status) {
    queryParams.append('status', params.status);
  }
  if (params.isFinished !== undefined) {
    queryParams.append('isFinished', params.isFinished.toString());
  }
  if (params.eventCode) {
    queryParams.append('eventCode', params.eventCode);
  }
  if (params.requestDataKeyword) {
    queryParams.append('requestDataKeyword', params.requestDataKeyword);
  }
  if (params.startDate) {
    queryParams.append('startDate', params.startDate);
  }
  if (params.endDate) {
    queryParams.append('endDate', params.endDate);
  }
  
  return getApiUrl(`${BASE_PATH}/export?${queryParams.toString()}`);
}

export function downloadExportFile(params: ExportEventHandlesParams): void {
  const url = exportEventHandles(params);
  const link = document.createElement('a');
  link.href = url;
  link.download = `${params.databaseType}事件处理记录_${new Date().toISOString().slice(0, 10)}.xlsx`;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}
