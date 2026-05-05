/**
 * Pipeline 配置相关类型定义
 */

/**
 * Stage 失败策略
 */
export type StageFailureAction = 'Stop' | 'Continue' | 'SkipToSender';

/**
 * Pipeline Stage 定义
 */
export interface PipelineStage {
  processorId: string;
  processorName: string;
  sortOrder: number;
  isSender: boolean;
  onFailure: StageFailureAction;
  condition?: string;
}

/**
 * Processor Pipeline 配置
 */
export interface ProcessorPipeline {
  id: string;
  name: string;
  eventCodes: string[];
  databaseTypes: string[];
  stages: PipelineStage[];
  enabled: boolean;
  maxRetryCount: number;
}

/**
 * 可用处理器（Pipeline 阶段选择用）
 */
export interface AvailablePipelineProcessor {
  id: string;
  name: string;
  eventCodes: string[];
  databaseTypes: string[];
  enabled: boolean;
}
