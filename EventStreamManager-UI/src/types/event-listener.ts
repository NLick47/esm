import { DatabaseConfig, DatabaseTypeWithActiveConfig } from './database';

export interface StartCondition {
  type: 'time' | 'id';
  timeValue: string;
  idValue: string;
}

export interface EventConfig {
  scanFrequency: number;
  batchSize: number;
  maxRetryCount: number;
  enabled: boolean;
  tableName: string;
  primaryKey: string;
  timestampField: string;
  lastInitializationTime?: string;
  lastInitializationMethod?: string;
  lastEventId?: number;
  totalEventsProcessed?: number;
}

export interface DatabaseStatistics {
  eventsProcessed: number;
  isEnabled: boolean;
  lastRunTime?: string;
  lastEventId?: number;
}

export interface StatisticsResponse {
  totalEventsProcessed: number;
  enabledCount: number;
  disabledCount: number;
  averageScanFrequency: number;
  lastUpdated: string;
  databaseStats?: Record<string, DatabaseStatistics>;
}


export type { DatabaseConfig, DatabaseTypeWithActiveConfig };