import { get, post } from '@/utils/request';

const BASE_PATH = '/api/auth';

export function getAuthStatus(): Promise<{ enabled: boolean }> {
  return get(`${BASE_PATH}/status`);
}

export function login(password: string): Promise<{ token: string }> {
  return post(`${BASE_PATH}/login`, { password });
}
