
export const API_CONFIG = {
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5236', // Vite
  timeout: 30000,
};

export const getApiUrl = (path: string): string => {
  return `${API_CONFIG.baseURL}${path}`;
};