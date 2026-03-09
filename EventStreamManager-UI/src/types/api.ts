/**
 * API 响应类型定义
 * 与后端 ApiResponse 类对应
 */

/**
 * 基础 API 响应结构
 */
export interface ApiResponse<T = any> {
  /** 是否成功 */
  success: boolean;
  /** 响应码 */
  code: number;
  /** 消息 */
  message: string;
  /** 时间戳 */
  timestamp: number;
  /** 数据 */
  data?: T;
}

/**
 * 分页数据结构
 */
export interface PageData<T> {
  /** 数据列表 */
  items: T[];
  /** 总数 */
  total: number;
  /** 当前页 */
  page: number;
  /** 每页大小 */
  pageSize: number;
  /** 总页数 */
  totalPages: number;
}

/**
 * API 错误类型
 */
export class ApiError extends Error {
  public code: number;
  public data?: any;

  constructor(message: string, code: number, data?: any) {
    super(message);
    this.name = 'ApiError';
    this.code = code;
    this.data = data;
  }
}
