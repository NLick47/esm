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
 * 接口调试结果
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
