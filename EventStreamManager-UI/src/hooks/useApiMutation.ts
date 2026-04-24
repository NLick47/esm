import { useState, useCallback, useRef } from 'react';
import { toast } from 'sonner';

interface UseApiMutationOptions<T> {
  successMessage?: string;
  errorMessage?: string;
  showSuccessToast?: boolean;
  showErrorToast?: boolean;
  onSuccess?: (data: T, variables: any) => void;
  onError?: (error: any, variables: any) => void;
}

export function useApiMutation<T, Args extends any[] = any[]>(
  apiFn: (...args: Args) => Promise<T>,
  options: UseApiMutationOptions<T> = {}
) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<any>(null);
  const mountedRef = useRef(true);
  const apiFnRef = useRef(apiFn);
  const optionsRef = useRef(options);

  apiFnRef.current = apiFn;
  optionsRef.current = options;

  const mutate = useCallback(async (...args: Args) => {
    setLoading(true);
    setError(null);
    try {
      const result = await apiFnRef.current(...args);
      if (mountedRef.current) {
        if (optionsRef.current.showSuccessToast && optionsRef.current.successMessage) {
          toast.success(optionsRef.current.successMessage);
        }
        optionsRef.current.onSuccess?.(result, args);
      }
      return result;
    } catch (err: any) {
      if (mountedRef.current) {
        setError(err);
        if (optionsRef.current.showErrorToast !== false) {
          toast.error(err.message || optionsRef.current.errorMessage || '操作失败');
        }
        optionsRef.current.onError?.(err, args);
      }
      throw err;
    } finally {
      if (mountedRef.current) {
        setLoading(false);
      }
    }
  }, []);

  return { mutate, loading, error };
}
