import { get } from '@/utils/request';
import { getApiUrl } from '@/config/api.config';
import type { 
  EventHandle, 
  PagedResult, 
  GetEventHandlesRequest, 
  ExportEventHandlesRequest 
} from '@/types/event-log';

export type { 
  EventHandle, 
  PagedResult, 
  GetEventHandlesRequest, 
  ExportEventHandlesRequest 
};

const BASE_PATH = '/api/EventLog';

export function getEventHandles(params: GetEventHandlesRequest): Promise<PagedResult<EventHandle>> {
  return get(`${BASE_PATH}/handles`, { params });
}

export function exportEventHandles(params: ExportEventHandlesRequest): string {
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
  if (params.status) {
    queryParams.append('status', params.status);
  }
  if (params.eventCode) {
    queryParams.append('eventCode', params.eventCode);
  }
  if (params.startDate) {
    queryParams.append('startDate', params.startDate);
  }
  if (params.endDate) {
    queryParams.append('endDate', params.endDate);
  }
  
  return getApiUrl(`${BASE_PATH}/export?${queryParams.toString()}`);
}

export function downloadExportFile(params: ExportEventHandlesRequest): void {
  const url = exportEventHandles(params);
  const link = document.createElement('a');
  link.href = url;
  link.download = `${params.databaseType}事件处理记录_${new Date().toISOString().slice(0, 10)}.xlsx`;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}
