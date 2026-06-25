import { Routes, Route, Navigate } from "react-router-dom";
import Home from "@/pages/Home";
import { useState, useEffect } from "react";
import { AuthContext } from '@/contexts/authContext';
import DocumentationPage from "@/pages/DocumentationPage";
import LoginPage from "@/pages/LoginPage";
import LogQueryPage from "@/pages/LogQueryPage";
import * as authService from '@/services/auth.service';

type AppMode = 'loading' | 'login' | 'full';

export default function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [appMode, setAppMode] = useState<AppMode>('loading');

  useEffect(() => {
    checkAuthStatus();
  }, []);

  const checkAuthStatus = async () => {
    try {
      const status = await authService.getAuthStatus();
      if (status.enabled) {
       
        const token = localStorage.getItem('esm_token');
        setIsAuthenticated(!!token);
        setAppMode('login');
      } else {
        setAppMode('full');
      }
    } catch {
      // 获取状态失败，降级为完整模式（不阻断使用）
      setAppMode('full');
    }
  };

  const logout = () => {
    setIsAuthenticated(false);
    localStorage.removeItem('esm_token');
  };

  const handleLoginSuccess = () => {
    setIsAuthenticated(true);
  };

  if (appMode === 'loading') {
    return (
      <div className="flex h-screen items-center justify-center bg-gray-50 dark:bg-gray-900">
        <div className="text-gray-500 dark:text-gray-400">
          <i className="fa-solid fa-spinner fa-spin mr-2"></i>
          加载中...
        </div>
      </div>
    );
  }

  // 认证模式：只有登录页和日志查询页
  if (appMode === 'login') {
    return (
      <AuthContext.Provider
        value={{ isAuthenticated, setIsAuthenticated, logout }}
      >
        <Routes>
          {isAuthenticated ? (
            <>
              <Route path="/logs" element={<LogQueryPage onLogout={logout} isLoginMode={true} />} />
              <Route path="*" element={<Navigate to="/logs" replace />} />
            </>
          ) : (
            <>
              <Route path="/login" element={<LoginPage onLoginSuccess={handleLoginSuccess} />} />
              <Route path="*" element={<Navigate to="/login" replace />} />
            </>
          )}
        </Routes>
      </AuthContext.Provider>
    );
  }

  // 完整模式：所有功能
  return (
    <AuthContext.Provider
      value={{ isAuthenticated, setIsAuthenticated, logout }}
    >
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/documentation" element={<DocumentationPage />} />
        <Route path="/other" element={<div className="text-center text-xl">Other Page - Coming Soon</div>} />
      </Routes>
    </AuthContext.Provider>
  );
}
