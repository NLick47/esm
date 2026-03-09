/**
 * 数据库配置服务
 */

import { get, post, put, del } from '@/utils/request';
import { DatabaseConfig } from '@/types/database';

const BASE_PATH = '/api/DatabaseConfig';

/**
 * 获取所有配置
 */
export function getAllConfigs(): Promise<Record<string, DatabaseConfig[]>> {
  return get<Record<string, DatabaseConfig[]>>(BASE_PATH);
}

/**
 * 获取指定类型的配置
 */
export function getConfigsByType(type: string): Promise<DatabaseConfig[]> {
  return get<DatabaseConfig[]>(`${BASE_PATH}/${type}`);
}

/**
 * 获取数据库类型列表
 */
export function getDatabaseTypes(): Promise<{ value: string; label: string }[]> {
  return get(`${BASE_PATH}/types`);
}

/**
 * 获取数据库类型列表（包含激活配置）
 */
export function getDatabaseTypesWithActiveConfig(): Promise<Array<{
  value: string;
  label: string;
  activeConfig: DatabaseConfig | null;
}>> {
  return get(`${BASE_PATH}/types-with-active-config`);
}

/**
 * 获取指定类型的激活配置
 */
export function getActiveConfig(type: string): Promise<DatabaseConfig | null> {
  return get<DatabaseConfig | null>(`${BASE_PATH}/${type}/active-config`);
}

/**
 * 添加数据库类型
 */
export function addDatabaseType(value: string, label: string): Promise<{ value: string; label: string }> {
  return post(`${BASE_PATH}/types`, { value, label });
}

/**
 * 删除数据库类型
 */
export function deleteDatabaseType(type: string): Promise<void> {
  return del(`${BASE_PATH}/types/${type}`);
}

/**
 * 创建配置
 */
export function createConfig(type: string, config: Partial<DatabaseConfig>): Promise<DatabaseConfig> {
  return post(`${BASE_PATH}/${type}`, config);
}

/**
 * 更新配置
 */
export function updateConfig(type: string, id: string, config: Partial<DatabaseConfig>): Promise<DatabaseConfig> {
  return put(`${BASE_PATH}/${type}/${id}`, config);
}

/**
 * 删除配置
 */
export function deleteConfig(type: string, id: string): Promise<void> {
  return del(`${BASE_PATH}/${type}/${id}`);
}

/**
 * 测试连接
 */
export function testConnection(type: string, config: Partial<DatabaseConfig>): Promise<{ success: boolean; message: string }> {
  return post(`${BASE_PATH}/${type}/test`, config);
}

/**
 * 设置激活配置
 */
export function setActiveConfig(type: string, id: string): Promise<void> {
  return post(`${BASE_PATH}/${type}/${id}/activate`);
}
