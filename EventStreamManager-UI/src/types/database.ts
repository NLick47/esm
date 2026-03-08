// types/database.ts
export type DatabaseType = string;

export enum DriverType {
  SqlServer = 'SqlServer',
  MySql = 'MySql',
  PostgreSQL = 'PostgreSql',
  Oracle = 'Oracle',
  SqLite = 'SQLite'
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