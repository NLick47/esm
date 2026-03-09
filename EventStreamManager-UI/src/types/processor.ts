/**
 * JS 处理器相关类型定义
 */

/**
 * JS 处理器配置
 */
export interface JSProcessor {
  id: string;
  name: string;
  databaseTypes: string[];
  eventCodes: string[];
  sqlTemplate: string;
  code: string;
  enabled: boolean;
  description: string;
}

/**
 * 系统 SQL 模板
 */
export interface SystemSqlTemplate {
  id: string;
  name: string;
  eventCodes: string[];
  sqlTemplate: string;
}

/**
 * 自定义 SQL 模板
 */
export interface CustomSqlTemplate {
  id: string;
  name: string;
  eventCodes: string[];
  sqlTemplate: string;
}

/**
 * 事件码配置
 */
export interface EventCode {
  code: string;
  description: string;
  enabled: boolean;
}

/**
 * 默认模板响应
 */
export interface DefaultTemplateResponse {
  code: string;
}

/**
 * JS 验证结果
 */
export interface ValidationResult {
  isValid: boolean;
  message?: string;
  lineNumber?: number;
  column?: number;
  source?: string;
  hasProcessFunction: boolean;
}

/**
 * 调试结果
 */
export interface DebugResult {
  success: boolean;
  result?: any;
  logs: string[];
  executionTime: number;
  error?: string;
}
