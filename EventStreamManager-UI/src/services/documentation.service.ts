import { get } from '@/utils/request';
import type {
  LibraryDefinition,
  FunctionDefinition,
  LibraryStructure
} from '@/types/documentation';

const BASE_PATH = '/api/documentation';

/**
 * 获取所有可用的库
 */
export function getLibraries(): Promise<LibraryDefinition[]> {
  return get<LibraryDefinition[]>(`${BASE_PATH}/libraries`);
}

/**
 * 获取所有分类
 */
export function getCategories(): Promise<string[]> {
  return get<string[]>(`${BASE_PATH}/categories`);
}

/**
 * 获取函数列表（可过滤）
 */
export function getFunctions(params?: {
  library?: string;
  category?: string;
}): Promise<FunctionDefinition[]> {
  const queryParams: Record<string, any> = {};
  if (params?.library) queryParams.library = params.library;
  if (params?.category) queryParams.category = params.category;
  
  return get<FunctionDefinition[]>(`${BASE_PATH}/functions`, { params: queryParams });
}

/**
 * 获取完整的文档结构
 */
export function getStructure(): Promise<LibraryStructure[]> {
  return get<LibraryStructure[]>(`${BASE_PATH}/structure`);
}
