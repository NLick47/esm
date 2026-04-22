import { get, post } from '@/utils/request';
import { ProcessorVersion, RollbackResult, RollbackOptions } from '@/types/processor-version';

const BASE_PATH = '/api/processorversions';

/**
 * 获取处理器的版本历史
 */
export function getProcessorVersions(processorId: string): Promise<ProcessorVersion[]> {
  return get<ProcessorVersion[]>(`${BASE_PATH}/${processorId}`);
}

/**
 * 获取单个版本详情
 */
export function getProcessorVersion(versionId: string): Promise<ProcessorVersion> {
  return get<ProcessorVersion>(`${BASE_PATH}/detail/${versionId}`);
}

/**
 * 提交新版本 (commit)
 */
export function commitProcessorVersion(processorId: string, commitMessage: string): Promise<ProcessorVersion> {
  return post<ProcessorVersion>(`${BASE_PATH}/${processorId}/commit`, { commitMessage });
}

/**
 * 回退到指定版本 (rollback)
 */
export function rollbackProcessorVersion(processorId: string, versionId: string, options?: RollbackOptions): Promise<RollbackResult> {
  return post<RollbackResult>(`${BASE_PATH}/${processorId}/rollback/${versionId}`, options ?? {});
}
