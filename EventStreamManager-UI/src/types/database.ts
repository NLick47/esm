export type DatabaseType = string;

export enum DriverType {
  SqlServer = 'SqlServer',
  MySql = 'MySql',
  PostgreSQL = 'PostgreSql',
  Oracle = 'Oracle',
  SqLite = 'SqLite'
}

export interface DatabaseConfig {
  id: string;
  name: string;
  connectionString: string;
  driver: DriverType;
  isActive: boolean;
  timeout: number;
}

export interface DatabaseDriver {
  value: string;
  label: string;
}

export interface DatabaseTypeInfo {
  value: string;
  label: string;
  description?: string;
  icon?: string;
  isActive?: boolean;
}


export interface DatabaseTypeWithActiveConfig {
  value: string;
  label: string;
  activeConfig: DatabaseConfig | null;
}