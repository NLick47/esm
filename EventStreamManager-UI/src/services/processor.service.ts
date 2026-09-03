import { get, post, put, del, patch } from '@/utils/request';
import { 
  JSProcessor, 
  EventCode, 
  SystemSqlTemplate, 
  CustomSqlTemplate,
  CreateCustomSqlTemplateRequest,
  UpdateCustomSqlTemplateRequest,
  DefaultTemplateResponse,
  ValidationResult,
  JSProcessorDetailResponse
} from '@/types/processor';

import { executeDebug, executeExamineDebug } from './debug.service';

const BASE_PATH = '/api';

/**
 * 获取所有处理器
 */
export function getProcessors(): Promise<JSProcessor[]> {
  return get<JSProcessor[]>(`${BASE_PATH}/processors`);
}

/**
 * 获取单个处理器
 */
export function getProcessor(id: string): Promise<JSProcessorDetailResponse> {
  return get<JSProcessorDetailResponse>(`${BASE_PATH}/processors/${id}`);
}

/**
 * 创建处理器
 */
export function createProcessor(processor: Partial<JSProcessor>): Promise<JSProcessor> {
  return post<JSProcessor>(`${BASE_PATH}/processors`, processor);
}

/**
 * 更新处理器
 */
export function updateProcessor(id: string, processor: Partial<JSProcessor>): Promise<JSProcessor> {
  return put<JSProcessor>(`${BASE_PATH}/processors/${id}`, processor);
}

/**
 * 删除处理器
 */
export function deleteProcessor(id: string): Promise<void> {
  return del(`${BASE_PATH}/processors/${id}`);
}

/**
 * 切换处理器状态
 */
export function toggleProcessor(id: string): Promise<JSProcessor> {
  return patch<JSProcessor>(`${BASE_PATH}/processors/${id}/toggle`);
}

/**
 * 获取事件码列表
 */
export function getEventCodes(): Promise<EventCode[]> {
  return get<EventCode[]>(`${BASE_PATH}/eventcodes`);
}

/**
 * 获取系统 SQL 模板
 */
export function getSystemTemplates(): Promise<SystemSqlTemplate[]> {
  return get<SystemSqlTemplate[]>(`${BASE_PATH}/sqltemplates/system`);
}

/**
 * 获取自定义 SQL 模板
 */
export function getCustomTemplates(): Promise<CustomSqlTemplate[]> {
  return get<CustomSqlTemplate[]>(`${BASE_PATH}/sqltemplates/custom`);
}

/**
 * 获取默认模板
 */
export function getDefaultTemplate(): Promise<DefaultTemplateResponse> {
  return get<DefaultTemplateResponse>(`${BASE_PATH}/processors/default-template`);
}

/**
 * 验证 JS 代码
 */
export function validateCode(code: string): Promise<ValidationResult> {
  return post<ValidationResult>(`${BASE_PATH}/script/validate`, { script: code });
}

/**
 * 创建自定义模板
 */
export function createCustomTemplate(template: CreateCustomSqlTemplateRequest): Promise<CustomSqlTemplate> {
  return post<CustomSqlTemplate>(`${BASE_PATH}/sqltemplates/custom`, template);
}

/**
 * 更新自定义模板
 */
export function updateCustomTemplate(id: string, template: UpdateCustomSqlTemplateRequest): Promise<boolean> {
  return put<boolean>(`${BASE_PATH}/sqltemplates/custom/${id}`, template);
}

/**
 * 删除自定义模板
 */
export function deleteCustomTemplate(id: string): Promise<boolean> {
  return del(`${BASE_PATH}/sqltemplates/custom/${id}`);
}


export { executeDebug, executeExamineDebug };

/**
 * 批量更新处理器排序
 */
export function updateSortOrder(sortOrders: Record<string, number>): Promise<void> {
  return put(`${BASE_PATH}/processors/sort-order`, sortOrders);
}