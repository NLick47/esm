import { post } from '@/utils/request';
import { InterfaceDebugResult } from '@/types/interface-config';

/**
 * 执行处理器调试（主调试标签页）
 */
export function executeDebug(params: {
  processorId: string;
  databaseType: string;
  eventCode: string;
  eventId?: string;
}): Promise<any> {
  return post<any>('/api/debug/execute', params);
}

/**
 * 执行处理器调试（不发送请求）- 兼容旧接口
 * @deprecated 请使用 executeDebug
 */
export function executeProcessorDebug(params: {
  processorId: string;
  databaseType: string;
  eventCode: string;
  eventId?: string;
}): Promise<any> {
  return executeDebug(params);
}

/**
 * 执行编辑器调试
 */
export function executeExamineDebug(params: {
  processorId: string | null;
  javascriptCode: string;
  examineId: string;
  databaseType: string;
  sqlTemplate: string;
  validateCode: boolean;
}): Promise<any> {
  return post<any>('/api/Debug/execute-examine', params);
}

/**
 * 调试接口配置
 */
export function debugInterfaceConfig(params: {
  configId: string;
  processorId: string;
  databaseType: string;
  eventType: string;
  eventId: string;
}): Promise<InterfaceDebugResult> {
  return post<InterfaceDebugResult>('/api/InterfaceConfig/debug', params);
}