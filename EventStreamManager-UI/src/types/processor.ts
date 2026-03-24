export enum SqlTemplateType {
  /** 系统模板 */
  System = 'System',
  /** 自定义模板 */
  Custom = 'Custom'
}


export interface JSProcessor {
  id: string;
  name: string;
  databaseTypes: string[];
  eventCodes: string[];
  sqlTemplateType: SqlTemplateType;
  sqlTemplateId: string;
  code: string;
  enabled: boolean;
  description: string;
}


export interface JSProcessorListResponse {
  id: string;
  name: string;
  databaseTypes: string[];
  eventCodes: string[];
  sqlTemplateType: SqlTemplateType;
  sqlTemplateId: string;
  enabled: boolean;
  description: string;
}



export interface JSProcessorDetailResponse extends JSProcessorListResponse {
  sqlTemplate: string;
  code: string;
  sqlTemplateName?: string;
  createdAt?: string;
  updatedAt?: string;
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
  sqlTemplate: string;
  description?: string;
  createdAt?: string;
  updatedAt?: string;
}

/**
 * 创建自定义 SQL 模板请求
 */
export interface CreateCustomSqlTemplateRequest {
  name: string;
  sqlTemplate: string;
  description?: string;
}

/**
 * 更新自定义 SQL 模板请求
 */
export interface UpdateCustomSqlTemplateRequest {
  name?: string;
  sqlTemplate?: string;
  description?: string;
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