import { SqlTemplateType } from './processor';

export interface ProcessorVersion {
  id: string;
  processorId: string;
  version: number;
  commitMessage: string;
  name: string;
  databaseTypes: string[];
  eventCodes: string[];
  code: string;
  sqlTemplate: string;
  sqlTemplateId: string;
  sqlTemplateType: SqlTemplateType;
  sqlTemplateName: string;
  enabled: boolean;
  description: string;
  createdAt: string;
}

/**
 * 回滚选项
 */
export interface RollbackOptions {
  /** 恢复脚本代码 */
  restoreCode: boolean;
  /** 恢复 SQL 模板配置 */
  restoreSqlTemplate: boolean;
  /** 恢复订阅事件码 */
  restoreEventCodes: boolean;
  /** 恢复数据库类型 */
  restoreDatabaseTypes: boolean;
  /** 恢复处理器基本信息（名称/描述/启用状态） */
  restoreMetadata: boolean;
}

/**
 * 回滚结果
 */
export interface RollbackResult {
  version: ProcessorVersion;
  /** 本次回滚自动恢复的模板ID列表 */
  recoveredTemplates: string[];
  /** 已不存在于系统配置中的事件码 */
  missingEventCodes: string[];
  /** 是否有需要用户关注的事项 */
  hasWarnings: boolean;
}
