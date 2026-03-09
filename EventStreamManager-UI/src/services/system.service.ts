// services/system.service.ts
import { get, post } from '@/utils/request';
import { ServiceStatus, ProcessorStatus } from '@/types/system';

const BASE_PATH = '/api/EventProcessor';

/**
 * 获取服务状态
 */
export function getServiceStatus(): Promise<ServiceStatus> {
  return get(`${BASE_PATH}/status`);
}

/**
 * 启用系统
 */
export function enableSystem(): Promise<ServiceStatus> {
  return post(`${BASE_PATH}/enable`);
}

/**
 * 停用系统
 */
export function disableSystem(): Promise<ServiceStatus> {
  return post(`${BASE_PATH}/disable`);
}

/**
 * 获取所有处理器状态（可选）
 */
export function getAllProcessorStatus(): Promise<ProcessorStatus[]> {
  return get(`${BASE_PATH}/processors`);
}