import { get, post, put } from '@/utils/request';
import { 
  EventConfig, 
  StatisticsResponse,
  StartCondition 
} from '@/types/event-listener';

const BASE_PATH = '/api/EventListenerConfig';

/**
 * 获取所有事件监听配置
 */
export function getAllEventConfigs(): Promise<{ databases: Record<string, EventConfig> }> {
  return get<{ databases: Record<string, EventConfig> }>(BASE_PATH);
}

/**
 * 获取指定类型的事件监听配置
 */
export function getEventConfigByType(type: string): Promise<EventConfig> {
  return get<EventConfig>(`${BASE_PATH}/${type}`);
}

/**
 * 更新事件监听配置
 */
export function updateEventConfig(type: string, config: Partial<EventConfig>): Promise<EventConfig> {
  return put<EventConfig>(`${BASE_PATH}/${type}`, config);
}

/**
 * 获取起始条件
 */
export function getStartCondition(type: string): Promise<StartCondition> {
  return get<StartCondition>(`${BASE_PATH}/${type}/start-condition`);
}

/**
 * 设置起始条件
 */
export function setStartCondition(type: string, condition: StartCondition): Promise<void> {
  return put<void>(`${BASE_PATH}/${type}/start-condition`, condition);
}

/**
 * 获取统计数据
 */
export function getStatistics(): Promise<StatisticsResponse> {
  return get<StatisticsResponse>(`${BASE_PATH}/statistics`);
}

/**
 * 启用事件监听
 */
export function enableEventListener(type: string): Promise<void> {
  return post<void>(`${BASE_PATH}/${type}/enable`);
}

/**
 * 停用事件监听
 */
export function disableEventListener(type: string): Promise<void> {
  return post<void>(`${BASE_PATH}/${type}/disable`);
}

/**
 * 重置事件监听
 */
export function resetEventListener(type: string): Promise<void> {
  return post<void>(`${BASE_PATH}/${type}/reset`);
}