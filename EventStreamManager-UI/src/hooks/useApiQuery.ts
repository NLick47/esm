import { useState, useEffect, useCallback, useRef } from 'react';
import { toast } from 'sonner';

interface UseApiQueryOptions<T> {
  initialData?: T;
  errorMessage?: string;
  enabled?: boolean;
  onSuccess?: (data: T) => void;
  onError?: (error: any) => void;
  showErrorToast?: boolean;
}

export function useApiQuery<T, Args extends any[] = []>(
  apiFn: (...args: Args) => Promise<T>,
  options: UseApiQueryOptions<T> = {}
) {
  const [data, setData] = useState<T | undefined>(options.initialData);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<any>(null);
  const mountedRef = useRef(true);
  const apiFnRef = useRef(apiFn);
  const optionsRef = useRef(options);

  apiFnRef.current = apiFn;
  optionsRef.current = options;

  const execute = useCallback(async (...args: Args) => {
    setLoading(true);
    setError(null);
    try {
      const result = await apiFnRef.current(...args);
      if (mountedRef.current) {
        setData(result);
        optionsRef.current.onSuccess?.(result);
      }
      return result;
    } catch (err: any) {
      if (mountedRef.current) {
        setError(err);
        if (optionsRef.current.showErrorToast !== false) {
          toast.error(err.message || optionsRef.current.errorMessage || '加载失败');
        }
        optionsRef.current.onError?.(err);
      }
      throw err;
    } finally {
      if (mountedRef.current) {
        setLoading(false);
      }
    }
  }, []);

  const refresh = useCallback(() => {
    return execute(...([] as unknown as Args));
  }, [execute]);

  useEffect(() => {
    mountedRef.current = true;
    if (options.enabled !== false) {
      execute(...([] as unknown as Args));
    }
    return () => {
      mountedRef.current = false;
    };
  }, [options.enabled, execute]);

  return { data, loading, error, execute, refresh, setData };
}
