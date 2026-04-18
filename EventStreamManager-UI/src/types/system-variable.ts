export interface SystemVariable {
  id: string;
  key: string;
  value: string;
  description: string;
  category: string;
  createdAt: string;
  updatedAt: string;
}

export interface SystemVariableRequest {
  key: string;
  value: string;
  description?: string;
  category?: string;
}
