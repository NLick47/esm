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

import { useState } from 'react';
import { useTheme } from '@/hooks/useTheme';
import DatabaseConnectionManager from '@/components/DatabaseConnectionManager';
import EventListenerConfig from '@/components/EventListenerConfig';
import JSProcessorManager from '@/components/JSProcessorManager';
import InterfaceSendConfig from '@/components/InterfaceSendConfig';
import DebugLogModule from '@/components/DebugLogModule';
import TaskMonitoringModule from '@/components/TaskMonitoringModule';
import { toast } from 'sonner';

// 主页面组件
export default function Home() {
  const { theme, toggleTheme } = useTheme();
  const [activeModule, setActiveModule] = useState<string>('database');
  const [systemStatus, setSystemStatus] = useState<'running' | 'stopped'>('stopped');

  // 切换系统状态
  const toggleSystemStatus = () => {
    const newStatus = systemStatus === 'running' ? 'stopped' : 'running';
    setSystemStatus(newStatus);
    
    toast(newStatus === 'running' 
      ? '系统已启动，开始监听和处理数据' 
      : '系统已停止，停止所有数据处理任务'
    );
  };

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
          <div className="flex items-center gap-2">
            <span className={`inline-block h-3 w-3 rounded-full ${systemStatus === 'running' ? 'bg-green-500' : 'bg-red-500'}`}></span>
            <span className="text-sm font-medium">{systemStatus === 'running' ? '系统运行中' : '系统已停止'}</span>
          </div>
          
          <button
            onClick={toggleTheme}
            className="flex items-center justify-center rounded-full bg-gray-200 dark:bg-gray-700 p-2 text-gray-700 dark:text-gray-300 transition-colors hover:bg-gray-300 dark:hover:bg-gray-600"
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
      <footer className="flex items-center justify-between border-t border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-800 px-6 py-3 shadow-inner">
        <div className="text-sm text-gray-500 dark:text-gray-400">
          最后更新: {new Date().toLocaleString()}
        </div>
        
        <div className="flex items-center gap-3">
        
          <button 
            className={`rounded-md px-6 py-2 text-sm font-medium transition-colors ${
              systemStatus === 'running' 
                ? 'bg-red-600 text-white hover:bg-red-700' 
                : 'bg-green-600 text-white hover:bg-green-700'
            }`}
            onClick={toggleSystemStatus}
          >
            {systemStatus === 'running' 
              ? <><i className="fa-solid fa-stop mr-1"></i> 停止系统</> 
              : <><i className="fa-solid fa-play mr-1"></i> 启动系统</>
            }
          </button>
        </div>
      </footer>
    </div>
  );
}