export interface StartCondition {
  type: 'time' | 'id';
  timeValue: string;
  idValue: string;
}

export interface EventConfig {
  scanFrequency: number;
  batchSize: number;
  enabled: boolean;
  tableName: string;
  primaryKey: string;
  timestampField: string;
  lastInitializationTime?: string;
  lastInitializationMethod?: string;
  lastEventId?: number;
  totalEventsProcessed?: number;
}

export interface DatabaseConfig {
  id: string;
  name: string;
  connectionString: string;
  driver: string;
  timeout: number;
  isActive: boolean;
}

export interface DatabaseTypeWithActiveConfig {
  value: string;
  label: string;
  activeConfig: DatabaseConfig | null;
}

export interface DatabaseTypeInfo {
  value: string;
  label: string;
  description?: string;
  icon?: string;
  isActive?: boolean;
}

export interface StatisticsResponse {
  totalEventsProcessed: number;
  enabledCount: number;
  disabledCount: number;
  averageScanFrequency: number;
  lastUpdated: string;
  databaseStats?: Record<string, DatabaseStatistics>;
}

export interface DatabaseStatistics {
  eventsProcessed: number;
  isEnabled: boolean;
  lastRunTime?: string;
  lastEventId?: number;
}