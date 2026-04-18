import { get, post, del } from '@/utils/request';
import { SystemVariable, SystemVariableRequest } from '@/types/system-variable';

const BASE_PATH = '/api/SystemVariables';

export function getAllVariables(): Promise<SystemVariable[]> {
  return get<SystemVariable[]>(BASE_PATH);
}

export function getVariableById(id: string): Promise<SystemVariable> {
  return get<SystemVariable>(`${BASE_PATH}/${id}`);
}

export function getVariableByKey(key: string): Promise<SystemVariable> {
  return get<SystemVariable>(`${BASE_PATH}/by-key/${key}`);
}

export function saveVariable(request: SystemVariableRequest): Promise<SystemVariable> {
  return post<SystemVariable>(BASE_PATH, request);
}

export function deleteVariable(id: string): Promise<void> {
  return del(`${BASE_PATH}/${id}`);
}

export function deleteVariableByKey(key: string): Promise<void> {
  return del(`${BASE_PATH}/by-key/${key}`);
}
