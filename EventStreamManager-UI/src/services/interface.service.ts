import { get, post, put, del } from '@/utils/request';
import { 
  InterfaceConfig, 
  AvailableProcessor, 
  InterfaceDebugRequest,
  InterfaceDebugResponse 
} from '@/types/interface-config';

/**
 * 获取所有接口配置
 */
export function getInterfaceConfigs(): Promise<InterfaceConfig[]> {
  return get<InterfaceConfig[]>('/api/InterfaceConfig');
}

/**
 * 获取单个接口配置
 */
export function getInterfaceConfig(id: string): Promise<InterfaceConfig> {
  return get<InterfaceConfig>(`/api/InterfaceConfig/${id}`);
}

/**
 * 创建接口配置
 */
export function createInterfaceConfig(config: Partial<InterfaceConfig>): Promise<InterfaceConfig> {
  return post<InterfaceConfig>('/api/InterfaceConfig', config);
}

/**
 * 更新接口配置
 */
export function updateInterfaceConfig(id: string, config: Partial<InterfaceConfig>): Promise<InterfaceConfig> {
  return put<InterfaceConfig>(`/api/InterfaceConfig/${id}`, config);
}

/**
 * 删除接口配置
 */
export function deleteInterfaceConfig(id: string): Promise<void> {
  return del(`/api/InterfaceConfig/${id}`);
}

/**
 * 复制接口配置
 */
export function duplicateInterfaceConfig(id: string): Promise<InterfaceConfig> {
  return post<InterfaceConfig>(`/api/InterfaceConfig/${id}/duplicate`);
}

/**
 * 切换接口配置状态
 */
export function toggleInterfaceConfig(id: string): Promise<InterfaceConfig> {
  return post<InterfaceConfig>(`/api/InterfaceConfig/${id}/toggle`);
}

/**
 * 获取未被引用的处理器
 */
export function getUnreferencedProcessors(): Promise<AvailableProcessor[]> {
  return get<AvailableProcessor[]>('/api/InterfaceConfig/unreferenced-processors');
}

/**
 * 获取所有处理器（包含引用状态）
 */
export function getAllProcessors(): Promise<AvailableProcessor[]> {
  return get<AvailableProcessor[]>('/api/Processor/all');
}



/**
 * 调试接口配置
 */
export function debugInterfaceConfig(params: InterfaceDebugRequest): Promise<InterfaceDebugResponse> {
  return post<InterfaceDebugResponse>('/api/Debug/interface', params);
}