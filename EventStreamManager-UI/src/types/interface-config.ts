/**
 * 接口发送配置相关类型定义
 */

/**
 * Header 项
 */
export interface HeaderItem {
  key: string;
  value: string;
}

/**
 * 接口配置
 */
export interface InterfaceConfig {
  id: string;
  name: string;
  processorIds: string[];
  processorNames: string[];
  url: string;
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  headers: HeaderItem[];
  timeout: number;
  retryCount: number;
  retryInterval: number;
  enabled: boolean;
  requestTemplate: string;
  description: string;
}

/**
 * 可用处理器
 */
export interface AvailableProcessor {
  id: string;
  name: string;
}

/**
 * 数据库类型信息（带激活配置）
 */
export interface DatabaseTypeWithActiveConfig {
  value: string;
  label: string;
  activeConfig: {
    id: string;
    name: string;
    connectionString: string;
    driver: string;
    isActive: boolean;
    timeout: number;
  } | null;
}

/**
 * 调试日志条目
 */
export interface DebugLogEntry {
  type: 'info' | 'warn' | 'error' | 'success' | 'output';
  message: string;
  timestamp: string;
}

/**
 * 处理器执行结果
 */
export interface ProcessResultDto {
  needToSend: boolean;
  reason?: string;
  data?: any;
  [key: string]: any;
}

/**
 * 请求信息
 */
export interface RequestInfo {
  url: string;
  method: string;
  headers: Record<string, string>;
  body?: string;
}

/**
 * 响应信息
 */
export interface ResponseInfo {
  statusCode: number;
  headers?: Record<string, string>;
  body?: string;
}

/**
 * 接口调试请求参数
 */
export interface InterfaceDebugRequest {
  interfaceConfigId: string;
  processorId: string;
  databaseType: string;
  eventCode?: string;
  eventId?: string;
}

/**
 * 接口调试响应
 */
export interface InterfaceDebugResponse {
  success: boolean;
  errorMessage?: string;
  logs: DebugLogEntry[];
  executionTimeMs: number;
  processorExecutionTime?: number;
  interfaceExecutionTime?: number;
  processorResult?: ProcessResultDto;
  requestInfo?: RequestInfo;
  responseInfo?: ResponseInfo;
}

/**
 * 接口调试结果（兼容旧版）
 */
export interface InterfaceDebugResult {
  success: boolean;
  requestUrl: string;
  requestMethod: string;
  requestHeaders: Record<string, string>;
  requestBody: string;
  responseStatus?: number;
  responseBody?: string;
  executionTimeMs: number;
  errorMessage?: string;
  processorExecutionTime?: number;
  processorResult?: any;
}