import { get, post, put, del, patch } from '@/utils/request';
import type { ProcessorPipeline } from '@/types/pipeline';

const BASE_PATH = '/api/Pipeline';

/**
 * 获取所有 Pipeline 配置
 */
export function getPipelines(): Promise<ProcessorPipeline[]> {
  return get<ProcessorPipeline[]>(BASE_PATH);
}

/**
 * 获取单个 Pipeline 配置
 */
export function getPipeline(id: string): Promise<ProcessorPipeline> {
  return get<ProcessorPipeline>(`${BASE_PATH}/${id}`);
}

/**
 * 创建 Pipeline 配置
 */
export function createPipeline(pipeline: Partial<ProcessorPipeline>): Promise<ProcessorPipeline> {
  return post<ProcessorPipeline>(BASE_PATH, pipeline);
}

/**
 * 更新 Pipeline 配置
 */
export function updatePipeline(id: string, pipeline: Partial<ProcessorPipeline>): Promise<void> {
  return put<void>(`${BASE_PATH}/${id}`, pipeline);
}

/**
 * 删除 Pipeline 配置
 */
export function deletePipeline(id: string): Promise<void> {
  return del(`${BASE_PATH}/${id}`);
}

/**
 * 切换 Pipeline 启用状态
 */
export function togglePipeline(id: string): Promise<void> {
  return patch<void>(`${BASE_PATH}/${id}/toggle`);
}
