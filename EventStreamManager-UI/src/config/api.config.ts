export const API_CONFIG = {
  baseURL: (() => {
    if (import.meta.env.DEV) {
      return import.meta.env.VITE_API_BASE_URL || 'http://localhost:5236';
    }
    return `${window.location.protocol}//${window.location.host}`;
  })(),
  timeout: 30000,
};

export const getApiUrl = (path: string): string => {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${API_CONFIG.baseURL}${normalizedPath}`;
};