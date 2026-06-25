import { useState } from 'react';
import { toast } from 'sonner';
import * as authService from '@/services/auth.service';

interface LoginPageProps {
  onLoginSuccess: () => void;
}

export default function LoginPage({ onLoginSuccess }: LoginPageProps) {
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!password.trim()) {
      toast.error('请输入密码');
      return;
    }

    setLoading(true);
    try {
      const result = await authService.login(password);
      localStorage.setItem('esm_token', result.token);
      toast.success('登录成功');
      onLoginSuccess();
    } catch (error: any) {
      toast.error(error.message || '登录失败');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex h-screen items-center justify-center bg-gray-50 dark:bg-gray-900">
      <div className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-xl dark:bg-gray-800">
        <div className="mb-6 text-center">
          <div className="mb-3 inline-flex h-14 w-14 items-center justify-center rounded-full bg-blue-100 dark:bg-blue-900/40">
            <i className="fa-solid fa-database text-2xl text-blue-600 dark:text-blue-400"></i>
          </div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">ESM 日志查询</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">请输入密码登录系统</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="请输入登录密码"
              autoFocus
              className="w-full rounded-lg border border-gray-300 px-4 py-3 text-gray-900 placeholder-gray-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 dark:placeholder-gray-500 dark:focus:border-blue-400"
            />
          </div>
          <button
            type="submit"
            disabled={loading}
            className="w-full rounded-lg bg-blue-600 px-4 py-3 text-white font-medium hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500/20 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {loading ? (
              <span>
                <i className="fa-solid fa-spinner fa-spin mr-2"></i>
                登录中...
              </span>
            ) : '登 录'}
          </button>
        </form>
      </div>
    </div>
  );
}
