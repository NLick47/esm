/**
 * 函数参数定义
 */
export interface FunctionParameter {
  name: string;
  type: string;
  description: string;
  isOptional: boolean;
  defaultValue?: any;
}

/**
 * 函数定义
 */
export interface FunctionDefinition {
  name: string;
  description: string;
  category: string;
  example: string;
  providerName: string;
  providerVersion: string;
  returnType: string;
  parameters: FunctionParameter[];
}

/**
 * 库定义
 */
export interface LibraryDefinition {
  name: string;
  description: string;
  version: string;
  functionCount: number;
}

/**
 * 分类中的函数组
 */
export interface CategoryFunctions {
  name: string;
  functions: Omit<FunctionDefinition, 'category' | 'providerName' | 'providerVersion'>[];
}

/**
 * 完整的库结构（带分类）
 */
export interface LibraryStructure {
  name: string;
  description: string;
  version: string;
  categories: CategoryFunctions[];
}
