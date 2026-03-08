// 导航菜单项组件
interface ModuleNavItemProps {
  icon: string;
  label: string;
  active: boolean;
  onClick: () => void;
}

function ModuleNavItem({ icon, label, active, onClick }: ModuleNavItemProps) {
  return (
    <button
      onClick={onClick}
      className={`flex w-full items-center gap-3 rounded-lg px-4 py-3 text-left transition-colors ${
        active
          ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400'
          : 'text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700'
      }`}
    >
      <i className={`fa-solid ${icon} text-lg`}></i>
      <span className="font-medium">{label}</span>
    </button>
  );
}

import { useState, useEffect, useRef } from 'react';
import { useTheme } from '@/hooks/useTheme';
import DatabaseConnectionManager from '@/components/DatabaseConnectionManager';
import EventListenerConfig from '@/components/EventListenerConfig';
import JSProcessorManager from '@/components/JSProcessorManager';
import InterfaceSendConfig from '@/components/InterfaceSendConfig';
import DebugLogModule from '@/components/DebugLogModule';
import TaskMonitoringModule from '@/components/TaskMonitoringModule';
import { toast } from 'sonner';
import { getApiUrl } from '@/config/api.config';

// API基础URL配置
const API_URL = getApiUrl('/api/EventProcessor');

// 系统状态接口
interface SystemStatus {
  isEnabled: boolean;
  startTime: string;
  runningDuration: string;
  processorCount: number;
  activeProcessorCount: number;
}

// 运行时长接口
interface UptimeResponse {
  startTime: string;
  currentTime: string;
  totalUptime: string;
  totalUptimeStr: string;
  effectiveRunningDuration: string;
  effectiveRunningDurationStr: string;
  isEnabled: boolean;
  processorCount: number;
  activeProcessorCount: number;
}

export default function Home() {
  const { theme, toggleTheme } = useTheme();
  const [activeModule, setActiveModule] = useState<string>('database');
  const [systemStatus, setSystemStatus] = useState<SystemStatus | null>(null);
  const [loading, setLoading] = useState(false);
  const mountedRef = useRef(true);
  const timerRef = useRef<ReturnType<typeof setTimeout>>();


  const formatDuration = (durationStr: string): string => {
    if (!durationStr) return '';
    
    const minuteMatch = durationStr.match(/(\d+)分钟/);
    const secondMatch = durationStr.match(/(\d+)秒/);
    
    const minutes = minuteMatch ? parseInt(minuteMatch[1]) : 0;
    const seconds = secondMatch ? parseInt(secondMatch[1]) : 0;
    
    const totalMinutes = minutes + (seconds > 0 ? 1 : 0);
    
    if (totalMinutes < 1) {
      return '1分钟内';
    }
    
    // 转换为天/小时/分钟
    const days = Math.floor(totalMinutes / (24 * 60));
    const remainingAfterDays = totalMinutes % (24 * 60);
    const hours = Math.floor(remainingAfterDays / 60);
    const mins = remainingAfterDays % 60;
    
    const parts = [];
    if (days > 0) {
      parts.push(`${days}天`);
    }
    if (hours > 0) {
      parts.push(`${hours}小时`);
    }
    if (mins > 0 || (days === 0 && hours === 0)) {
      parts.push(`${mins}分钟`);
    }
    
    return parts.join('');
  };

  // 获取系统状态
  const fetchSystemStatus = async () => {
    try {
      const response = await fetch(`${API_URL}/service/status`);
      if (response.ok && mountedRef.current) {
        const data = await response.json();
        setSystemStatus(data);
        
        // 如果系统已启用，立即获取运行时长
        if (data.isEnabled) {
          await fetchUptime();
        }
      }
    } catch (error) {
      console.error('获取系统状态失败:', error);
    }
  };

  // 获取运行时长
  const fetchUptime = async () => {
    try {
      const response = await fetch(`${API_URL}/service/uptime`);
      if (response.ok && mountedRef.current) {
        const data: UptimeResponse = await response.json();
        
        // 格式化运行时长
        const formattedDuration = formatDuration(data.effectiveRunningDurationStr);
        
        setSystemStatus(prev => {
          if (!prev) {
            return {
              isEnabled: data.isEnabled,
              startTime: data.startTime,
              runningDuration: formattedDuration || '1分钟内',
              processorCount: data.processorCount,
              activeProcessorCount: data.activeProcessorCount
            };
          }
          
          return {
            ...prev,
            runningDuration: formattedDuration,
            processorCount: data.processorCount,
            activeProcessorCount: data.activeProcessorCount
          };
        });
      }
    } catch (error) {
      console.error('获取运行时长失败:', error);
    }
  };

  // 启动状态轮询
  const startStatusPolling = () => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }
    
    timerRef.current = setInterval(() => {
      if (systemStatus?.isEnabled && mountedRef.current) {
        fetchUptime();
      }
    }, 5000);
  };

  // 停止状态轮询
  const stopStatusPolling = () => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = undefined;
    }
  };

  // 启用系统
  const enableSystem = async () => {
    setLoading(true);
    try {
      const response = await fetch(`${API_URL}/service/enable`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (response.ok && mountedRef.current) {
        const data = await response.json();
        
        setSystemStatus(prev => ({
          ...prev!,
          isEnabled: data.isEnabled,
        }));
        
        // 立即获取运行时长
        await fetchUptime();
        
        toast.success('系统已启动');
      } else {
        const error = await response.json();
        toast.error(error.message || '启动失败，请重试');
      }
    } catch (error) {
      console.error('启动系统失败:', error);
      toast.error('网络错误，请检查连接');
    } finally {
      setLoading(false);
    }
  };

  // 禁用系统
  const disableSystem = async () => {
    setLoading(true);
    try {
      const response = await fetch(`${API_URL}/service/disable`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (response.ok && mountedRef.current) {
        const data = await response.json();
        
        setSystemStatus(prev => ({
          ...prev!,
          isEnabled: data.isEnabled,
          runningDuration: '', // 系统停止时清空运行时间
        }));
        
        toast.success('系统已停止');
        
        // 停止轮询
        stopStatusPolling();
      } else {
        const error = await response.json();
        toast.error(error.message || '停止失败，请重试');
      }
    } catch (error) {
      console.error('停止系统失败:', error);
      toast.error('网络错误，请检查连接');
    } finally {
      setLoading(false);
    }
  };

  // 刷新状态
  const handleRefresh = async () => {
    setLoading(true);
    await fetchSystemStatus();
    setLoading(false);
    toast.success('状态已刷新');
  };

  // 初始化状态
  useEffect(() => {
    mountedRef.current = true;
    
    // 初始加载
    fetchSystemStatus();
    
    return () => {
      mountedRef.current = false;
      stopStatusPolling();
    };
  }, []);

  // 根据系统状态控制轮询
  useEffect(() => {
    if (systemStatus?.isEnabled) {
      startStatusPolling();
    } else {
      stopStatusPolling();
    }
    
    return () => {
      stopStatusPolling();
    };
  }, [systemStatus?.isEnabled]);

  // 渲染当前激活的模块
  const renderActiveModule = () => {
    switch (activeModule) {
      case 'database':
        return <DatabaseConnectionManager />;
      case 'event':
        return <EventListenerConfig />;
      case 'processor':
        return <JSProcessorManager />;
      case 'interface':
        return <InterfaceSendConfig />;
      case 'debug':
        return <DebugLogModule />;
      case 'monitor':
        return <TaskMonitoringModule />;
      default:
        return <DatabaseConnectionManager />;
    }
  };

  return (
    <div className={`flex h-screen flex-col bg-gray-50 dark:bg-gray-900 text-gray-900 dark:text-gray-100 overflow-hidden`}>
      {/* 顶部导航栏 */}
      <header className="flex items-center justify-between border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-800 px-4 py-3 shadow-sm">
        <div className="flex items-center gap-2">
          <i className="fa-solid fa-database text-blue-600 dark:text-blue-400 text-xl"></i>
          <h1 className="text-xl font-bold">ESM</h1>
        </div>
        
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <span className={`inline-block h-3 w-3 rounded-full ${systemStatus?.isEnabled ? 'bg-green-500 animate-pulse' : 'bg-red-500'}`}></span>
              <span className="text-sm font-medium">
                {systemStatus?.isEnabled ? '系统运行中' : '系统已停止'}
              </span>
            </div>
            
            {systemStatus?.isEnabled && (
              <>
                <div className="text-sm text-gray-600 dark:text-gray-400">
                  <span className="mr-2">处理器:</span>
                  <span className="font-medium">
                    {systemStatus.activeProcessorCount}/{systemStatus.processorCount} 活跃
                  </span>
                </div>
                
                {systemStatus.runningDuration && (
                  <div className="text-sm text-gray-500 dark:text-gray-400">
                    <i className="fa-regular fa-clock mr-1"></i>
                    已运行: {systemStatus.runningDuration}
                  </div>
                )}
              </>
            )}
          </div>
          
          <button
            onClick={toggleTheme}
            className="flex items-center justify-center rounded-full bg-gray-200 dark:bg-gray-700 p-2 text-gray-700 dark:text-gray-300 transition-colors hover:bg-gray-300 dark:hover:bg-gray-600"
            title={theme === 'light' ? '切换到暗色模式' : '切换到亮色模式'}
          >
            <i className={`fa-solid ${theme === 'light' ? 'fa-moon' : 'fa-sun'}`}></i>
          </button>
        </div>
      </header>

      {/* 主内容区域 */}
      <div className="flex flex-1 overflow-hidden">
        {/* 左侧导航菜单 */}
        <aside className="w-64 border-r border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-800 p-4 shadow-sm">
          <nav className="space-y-1">
            <ModuleNavItem 
              icon="fa-database" 
              label="数据库连接管理" 
              active={activeModule === 'database'} 
              onClick={() => setActiveModule('database')} 
            />
            <ModuleNavItem 
              icon="fa-sitemap" 
              label="事件监听配置" 
              active={activeModule === 'event'} 
              onClick={() => setActiveModule('event')} 
            />
            <ModuleNavItem 
              icon="fa-code" 
              label="JS处理器管理" 
              active={activeModule === 'processor'} 
              onClick={() => setActiveModule('processor')} 
            />
            <ModuleNavItem 
              icon="fa-plug" 
              label="接口发送配置" 
              active={activeModule === 'interface'} 
              onClick={() => setActiveModule('interface')} 
            />
            <ModuleNavItem 
              icon="fa-bug" 
              label="调试与日志" 
              active={activeModule === 'debug'} 
              onClick={() => setActiveModule('debug')} 
            />
            <ModuleNavItem 
              icon="fa-chart-line" 
              label="任务监控" 
              active={activeModule === 'monitor'} 
              onClick={() => setActiveModule('monitor')} 
            />
          </nav>
        </aside>

        {/* 右侧内容区域 */}
        <main className="flex-1 overflow-auto p-6">
          {renderActiveModule()}
        </main>
      </div>

      {/* 底部状态栏 */}
      <footer className="flex items-center justify-end border-t border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-800 px-6 py-3 shadow-inner">
        <div className="flex items-center gap-3">
          <button
            onClick={handleRefresh}
            className="rounded-md px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            title="刷新状态"
            disabled={loading}
          >
            <i className={`fa-solid fa-rotate-right mr-1 ${loading ? 'fa-spin' : ''}`}></i>
            刷新
          </button>
          
          <button 
            className={`rounded-md px-6 py-2 text-sm font-medium transition-colors ${
              systemStatus?.isEnabled 
                ? 'bg-red-600 text-white hover:bg-red-700' 
                : 'bg-green-600 text-white hover:bg-green-700'
            } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
            onClick={systemStatus?.isEnabled ? disableSystem : enableSystem}
            disabled={loading}
          >
            {loading ? (
              <><i className="fa-solid fa-spinner fa-spin mr-1"></i> 处理中...</>
            ) : systemStatus?.isEnabled ? (
              <><i className="fa-solid fa-stop mr-1"></i> 停止系统</>
            ) : (
              <><i className="fa-solid fa-play mr-1"></i> 启动系统</>
            )}
          </button>
        </div>
      </footer>
    </div>
  );
}