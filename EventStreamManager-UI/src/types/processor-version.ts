import { SqlTemplateType } from './processor';

export interface ProcessorVersion {
  id: string;
  processorId: string;
  version: number;
  commitMessage: string;
  name: string;
  code: string;
  sqlTemplate: string;
  sqlTemplateId: string;
  sqlTemplateType: SqlTemplateType;
  createdAt: string;
}
