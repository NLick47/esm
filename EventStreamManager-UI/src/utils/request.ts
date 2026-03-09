/**
 * 统一请求工具
 * 处理 API 响应格式，统一错误处理
 */

import { ApiResponse, ApiError } from '@/types/api';
import { getApiUrl } from '@/config/api.config';

/**
 * 请求配置选项
 */
interface RequestOptions {
  /** 请求方法 */
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
  /** 请求头 */
  headers?: Record<string, string>;
  /** 请求体 */
  body?: any;

  params?: Record<string, any>;
  /** 是否直接返回 data（默认 true） */
  unwrapData?: boolean;
  
  /** 自定义成功消息 */
  successMessage?: string;
  /** 是否显示错误提示 */
  showError?: boolean;
}

/**
 * 统一请求函数
 * @param path API 路径
 * @param options 请求选项
 * @returns 响应数据
 */
export async function request<T = any>(
  path: string,
  options: RequestOptions = {}
): Promise<T> {
  const {
    method = 'GET',
    headers = {},
    body,
    params,
    unwrapData = true,
    showError = true
  } = options;

  let url = getApiUrl(path);
  if (params && Object.keys(params).length > 0) {
    const searchParams = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        searchParams.append(key, String(value));
      }
    });
    const queryString = searchParams.toString();
    if (queryString) {
      url += (url.includes('?') ? '&' : '?') + queryString;
    }
  }


  const config: RequestInit = {
    method,
    headers: {
      'Content-Type': 'application/json',
      ...headers
    }
  };

  if (body && method !== 'GET') {
    config.body = JSON.stringify(body);
  }

  try {
    const response = await fetch(url, config);
    
    // 尝试解析响应为 JSON
    let data: ApiResponse<T>;
    try {
      data = await response.json();
    } catch (e) {
      // 如果无法解析 JSON，检查 HTTP 状态
      if (!response.ok) {
        throw new ApiError(
          `HTTP Error: ${response.status} ${response.statusText}`,
          response.status
        );
      }
      // 对于没有响应体的成功请求，返回默认结构
      data = {
        success: response.ok,
        code: response.status,
        message: response.ok ? '操作成功' : '操作失败',
        timestamp: Date.now()
      };
    }

    // 检查响应是否成功
    if (!data.success) {
      throw new ApiError(
        data.message || '请求失败',
        data.code,
        data.data
      );
    }

    // 返回数据
    return unwrapData ? (data.data as T) : (data as any);
  } catch (error) {
    if (error instanceof ApiError) {
      if (showError) {
        // 错误提示由调用方处理
        console.error('[API Error]', error.message, error.code);
      }
      throw error;
    }
    
    // 网络错误或其他错误
    const networkError = new ApiError(
      '网络请求失败，请检查网络连接',
      0
    );
    console.error('[Network Error]', error);
    throw networkError;
  }
}

/**
 * GET 请求
 */
export function get<T = any>(path: string, options?: Omit<RequestOptions, 'method' | 'body'>): Promise<T> {
  return request<T>(path, { ...options, method: 'GET' });
}

/**
 * POST 请求
 */
export function post<T = any>(path: string, body?: any, options?: Omit<RequestOptions, 'method' | 'body'>): Promise<T> {
  return request<T>(path, { ...options, method: 'POST', body });
}

/**
 * PUT 请求
 */
export function put<T = any>(path: string, body?: any, options?: Omit<RequestOptions, 'method' | 'body'>): Promise<T> {
  return request<T>(path, { ...options, method: 'PUT', body });
}

/**
 * DELETE 请求
 */
export function del<T = any>(path: string, options?: Omit<RequestOptions, 'method'>): Promise<T> {
  return request<T>(path, { ...options, method: 'DELETE' });
}

/**
 * PATCH 请求
 */
export function patch<T = any>(path: string, body?: any, options?: Omit<RequestOptions, 'method' | 'body'>): Promise<T> {
  return request<T>(path, { ...options, method: 'PATCH', body });
}

/**
 * 获取原始响应（不自动解包 data）
 */
export function getRaw<T = any>(path: string, options?: Omit<RequestOptions, 'method' | 'body' | 'unwrapData'>): Promise<ApiResponse<T>> {
  return request<ApiResponse<T>>(path, { ...options, method: 'GET', unwrapData: false });
}
