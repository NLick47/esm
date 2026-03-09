export interface ServiceStatus {
  /** 系统是否启用 */
  isEnabled: boolean;
  /** 系统启动时间 */
  startTime: string;
  /** 已运行时长（格式化字符串） */
  runningDuration: string;
  /** 总处理器数量 */
  totalProcessorCount: number;
  /** 活跃处理器数量 */
  activeProcessorCount: number;
}

export interface ProcessorStatus {
  databaseType: string;
  isRunning: boolean;
  isEnabled: boolean;
  lastScanTime: string | null;
  lastProcessedEventId: number | null;
  totalProcessedCount: number;
  successCount: number;
  failedCount: number;
  currentBatchCount: number;
  lastError: string | null;
  lastErrorTime: string | null;
}