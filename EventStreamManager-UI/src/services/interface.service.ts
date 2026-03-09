/**
 * 接口配置服务
 */

import { get, post, put, del, patch } from '@/utils/request';
import { InterfaceConfig, AvailableProcessor } from '@/types/interface-config';
// 导入调试方法从 debug.service
import { debugInterfaceConfig, executeDebug } from './debug.service';

const BASE_PATH = '/api/InterfaceConfig';

/**
 * 获取所有接口配置
 */
export function getInterfaceConfigs(): Promise<InterfaceConfig[]> {
  return get<InterfaceConfig[]>(BASE_PATH);
}

/**
 * 获取单个接口配置
 */
export function getInterfaceConfig(id: string): Promise<InterfaceConfig> {
  return get<InterfaceConfig>(`${BASE_PATH}/${id}`);
}

/**
 * 创建接口配置
 */
export function createInterfaceConfig(config: Partial<InterfaceConfig>): Promise<InterfaceConfig> {
  return post<InterfaceConfig>(BASE_PATH, config);
}

/**
 * 更新接口配置
 */
export function updateInterfaceConfig(id: string, config: Partial<InterfaceConfig>): Promise<InterfaceConfig> {
  return put<InterfaceConfig>(`${BASE_PATH}/${id}`, config);
}

/**
 * 删除接口配置
 */
export function deleteInterfaceConfig(id: string): Promise<void> {
  return del(`${BASE_PATH}/${id}`);
}

/**
 * 复制接口配置
 */
export function duplicateInterfaceConfig(id: string): Promise<InterfaceConfig> {
  return post<InterfaceConfig>(`${BASE_PATH}/${id}/duplicate`);
}

/**
 * 获取可用处理器列表（用于创建时的多选框）
 * 只返回未被引用的处理器
 */
export function getUnreferencedProcessors(): Promise<AvailableProcessor[]> {
  return get<AvailableProcessor[]>(`${BASE_PATH}/processors/unreferenced`);
}

/**
 * 获取所有处理器列表（用于编辑时的多选框）
 * 返回所有处理器，并包含引用状态
 */
export function getAllProcessors(): Promise<(AvailableProcessor & { isReferenced?: boolean, referencedBy?: string })[]> {
  return get<AvailableProcessor[]>(`${BASE_PATH}/processors/available`);
}

/**
 * 获取指定配置的处理器列表
 */
export function getConfigProcessors(id: string): Promise<AvailableProcessor[]> {
  return get<AvailableProcessor[]>(`${BASE_PATH}/${id}/processors`);
}

/**
 * 测试接口配置
 */
export function testInterfaceConfig(id: string): Promise<any> {
  return post<any>(`${BASE_PATH}/${id}/test`);
}

/**
 * 获取事件码列表
 */
export function getInterfaceEventCodes(): Promise<any[]> {
  return get<any[]>('/api/eventcodes');
}

/**
 * 获取处理器列表
 */
export function getProcessorsList(): Promise<any[]> {
  return get<any[]>('/api/processors');
}

/**
 * 切换接口配置状态
 */
export function toggleInterfaceConfig(id: string): Promise<InterfaceConfig> {
  return patch<InterfaceConfig>(`${BASE_PATH}/${id}/toggle`);
}


export { debugInterfaceConfig, executeDebug };