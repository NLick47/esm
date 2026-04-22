import { useState, useEffect, useRef, useCallback } from 'react';
import { useTheme } from '@/hooks/useTheme';
import DatabaseConnectionManager from '@/components/DatabaseConnectionManager';
import EventListenerConfig from '@/components/EventListenerConfig';
import JSProcessorManager from '@/components/JSProcessorManager';
import InterfaceSendConfig from '@/components/InterfaceSendConfig';
import DebugLogModule from '@/components/DebugLogModule';
import SystemVariableManager from '@/components/SystemVariableManager';
import { toast } from 'sonner';
import * as systemService from '@/services/system.service';
import { ServiceStatus, ProcessorStatus } from '@/types';

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

export default function Home() {
  const { theme, toggleTheme } = useTheme();
  const [activeModule, setActiveModule] = useState<string>('database');
  const [serviceStatus, setServiceStatus] = useState<ServiceStatus | null>(null);
  const [processorStatuses, setProcessorStatuses] = useState<ProcessorStatus[]>([]);
  const [loading, setLoading] = useState(false);
  const [version, setVersion] = useState<string>('');
  
  const mountedRef = useRef(true);
  const timerRef = useRef<ReturnType<typeof setTimeout>>();


/**
 * 格式化持续时间
 * @param durationStr TimeSpan字符串，格式如 "00:00:05.0791501"
 * @returns 格式化后的字符串，如 "1分钟内"、"5分钟"、"2小时3分钟"、"1天2小时"
 */
const formatDuration = useCallback((durationStr: string): string => {
  if (!durationStr) return '0秒';
  
  // 解析 TimeSpan 格式 "00:00:05.0791501"
  const timeMatch = durationStr.match(/(\d+):(\d+):(\d+)(?:\.(\d+))?/);
  if (!timeMatch) return durationStr;
  
  const hours = parseInt(timeMatch[1], 10);
  const minutes = parseInt(timeMatch[2], 10);
  const seconds = parseInt(timeMatch[3], 10);
  
  // 转换为总分钟数
  const totalMinutes = hours * 60 + minutes + (seconds > 0 ? 1 : 0);
  
  // 不足1分钟显示"1分钟内"
  if (totalMinutes < 1) {
    return '1分钟内';
  }
  
  // 计算天、小时、分钟
  const days = Math.floor(totalMinutes / (24 * 60));
  const remainingAfterDays = totalMinutes % (24 * 60);
  const remainingHours = Math.floor(remainingAfterDays / 60);
  const remainingMinutes = remainingAfterDays % 60;
  
  const parts = [];
  
  // 按天显示
  if (days > 0) {
    parts.push(`${days}天`);
    // 如果有天，只显示小时，不显示分钟（或根据需要决定）
    if (remainingHours > 0) {
      parts.push(`${remainingHours}小时`);
    }
    
  } 
  
  else if (remainingHours > 0) {
    parts.push(`${remainingHours}小时`);
    if (remainingMinutes > 0) {
      parts.push(`${remainingMinutes}分钟`);
    }
  } 
  // 按分钟显示
  else {
    parts.push(`${remainingMinutes}分钟`);
  }
  
  return parts.join('');
}, []);

  // 获取服务状态
  const fetchServiceStatus = useCallback(async () => {
    try {
      const data = await systemService.getServiceStatus();
      if (mountedRef.current) {
      
        const formattedDuration = formatDuration(data.runningDuration);
        setServiceStatus({
          ...data,
          runningDuration: formattedDuration
        });
      }
    } catch (error: any) {
      console.error('获取服务状态失败:', error);
      if (mountedRef.current) {
        toast.error(error.message || '获取服务状态失败');
      }
    }
  }, [formatDuration]);

  // 获取处理器状态（可选，用于更详细的监控）
  const fetchProcessorStatuses = useCallback(async () => {
    try {
      const data = await systemService.getAllProcessorStatus();
      if (mountedRef.current) {
        setProcessorStatuses(data);
      }
    } catch (error: any) {
      console.error('获取处理器状态失败:', error);
    }
  }, []);

  // 启动轮询 - 只轮询服务状态
  const startPolling = useCallback(() => {
    if (timerRef.current) clearInterval(timerRef.current);
    timerRef.current = setInterval(() => {
      if (mountedRef.current) {
        fetchServiceStatus();
      }
    }, 5000);
  }, [fetchServiceStatus]);

  // 停止轮询
  const stopPolling = useCallback(() => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = undefined;
    }
  }, []);

  // 启用系统
  const enableSystem = async () => {
    setLoading(true);
    try {
      const data = await systemService.enableSystem();
      if (mountedRef.current) {
        // 格式化运行时长
        const formattedDuration = formatDuration(data.runningDuration);
        setServiceStatus({
          ...data,
          runningDuration: formattedDuration
        });
        toast.success('系统已启动');
      }
    } catch (error: any) {
      toast.error(error.message || '启动失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  // 禁用系统
  const disableSystem = async () => {
    setLoading(true);
    try {
      const data = await systemService.disableSystem();
      if (mountedRef.current) {
        // 禁用时运行时长应该是0
        setServiceStatus({
          ...data,
          runningDuration: '0秒'
        });
        toast.success('系统已停止');
      }
    } catch (error: any) {
      toast.error(error.message || '停止失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  // 刷新状态
  const handleRefresh = async () => {
    setLoading(true);
    await fetchServiceStatus();
    await fetchProcessorStatuses();
    setLoading(false);
    toast.success('状态已刷新');
  };
  // 获取版本号
  const fetchVersion = useCallback(async () => {
    try {
      const data = await systemService.getVersion();
      if (mountedRef.current) {
        setVersion(data.version);
      }
    } catch (error: any) {
      console.error('获取版本号失败:', error);
    }
  }, []);

  // 初始化加载
  useEffect(() => {
    mountedRef.current = true;
    
    // 初始化加载所有数据
    Promise.all([
      fetchServiceStatus(),
      fetchProcessorStatuses(),
      fetchVersion()
    ]);
    
    return () => {
      mountedRef.current = false;
      stopPolling();
    };
  }, [fetchServiceStatus, fetchProcessorStatuses, fetchVersion, stopPolling]);

  // 轮询控制 - 只在系统启用时轮询
  useEffect(() => {
    if (serviceStatus?.isEnabled) {
      startPolling();
    } else {
      stopPolling();
    }
    
    return stopPolling;
  }, [serviceStatus?.isEnabled, startPolling, stopPolling]);

  // 计算活跃处理器数量（从处理器状态中获取）
  const activeProcessorCount = processorStatuses.filter(p => p.isRunning).length;
  const totalProcessorCount = processorStatuses.length;

  // 渲染活动模块
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
      case 'systemvar':
        return <SystemVariableManager />;
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
              <span className={`inline-block h-3 w-3 rounded-full ${serviceStatus?.isEnabled ? 'bg-green-500 animate-pulse' : 'bg-red-500'}`}></span>
              <span className="text-sm font-medium">
                {serviceStatus?.isEnabled ? '系统运行中' : '系统已停止'}
              </span>
            </div>
            
            {serviceStatus?.isEnabled && (
              <>
                <div className="text-sm text-gray-600 dark:text-gray-400">
                  <span className="mr-2">处理器:</span>
                  <span className="font-medium">
                    {activeProcessorCount}/{totalProcessorCount} 活跃
                  </span>
                </div>
                
                {serviceStatus.runningDuration && (
                  <div className="text-sm text-gray-500 dark:text-gray-400">
                    <i className="fa-regular fa-clock mr-1"></i>
                    已运行: {serviceStatus.runningDuration}
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
              icon="fa-sliders" 
              label="系统变量管理" 
              active={activeModule === 'systemvar'} 
              onClick={() => setActiveModule('systemvar')} 
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
          </nav>
        </aside>

        {/* 右侧内容区域 */}
        <main className="flex-1 overflow-auto p-6">
          {renderActiveModule()}
        </main>
      </div>

      {/* 底部状态栏 */}
      <footer className="flex items-center justify-between border-t border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-800 px-6 py-3 shadow-inner">
        <div className="text-sm font-medium text-gray-500 dark:text-gray-400">
          {version ? `v${version}` : ''}
        </div>
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
              serviceStatus?.isEnabled 
                ? 'bg-red-600 text-white hover:bg-red-700' 
                : 'bg-green-600 text-white hover:bg-green-700'
            } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
            onClick={serviceStatus?.isEnabled ? disableSystem : enableSystem}
            disabled={loading}
          >
            {loading ? (
              <><i className="fa-solid fa-spinner fa-spin mr-1"></i> 处理中...</>
            ) : serviceStatus?.isEnabled ? (
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