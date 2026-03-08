import { useState, useEffect, useRef } from 'react';
import { toast } from 'sonner';

// 调试与日志模块
export default function DebugLogModule() {
  const [activeTab, setActiveTab] = useState<'logs' | 'errors' | 'sending'>('logs');
  const [logLevel, setLogLevel] = useState<'all' | 'info' | 'warning' | 'error'>('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [dateFilter, setDateFilter] = useState<string>('');
  const [selectedError, setSelectedError] = useState<LogEntry | null>(null);
  const [showErrorDetail, setShowErrorDetail] = useState(false);
  const logsEndRef = useRef<HTMLDivElement>(null);
  
  // 模拟日志数据
  const [logs, setLogs] = useState<LogEntry[]>([
    {
      id: '1',
      timestamp: new Date(Date.now() - 10 * 60 * 1000).toISOString(),
      level: 'info',
      source: 'system',
      message: '系统启动成功',
      details: '所有服务已初始化完毕'
    },
    {
      id: '2',
      timestamp: new Date(Date.now() - 8 * 60 * 1000).toISOString(),
      level: 'info',
      source: 'database',
      message: '数据库连接建立成功',
      details: '已连接到超声数据库'
    },
    {
      id: '3',
      timestamp: new Date(Date.now() - 5 * 60 * 1000).toISOString(),
      level: 'info',
      source: 'event',
      message: '事件监听已开始',
      details: '开始监听超声数据库事件表'
    },
    {
      id: '4',
      timestamp: new Date(Date.now() - 3 * 60 * 1000).toISOString(),
      level: 'warning',
      source: 'processor',
      message: '处理器执行超时',
      details: '超声检查数据处理器执行时间超过阈值',
      errorStack: 'TimeoutError: Operation timed out\n    at processData (/processor.js:123:15)\n    at async handleEvent (/eventHandler.js:45:20)'
    },
    {
      id: '5',
      timestamp: new Date(Date.now() - 2 * 60 * 1000).toISOString(),
      level: 'error',
      source: 'interface',
      message: '接口请求失败',
      details: '无法连接到目标服务器',
      errorStack: 'NetworkError: Failed to connect to api.example.com\n    at sendRequest (/apiClient.js:67:19)\n    at async pushData (/dataSender.js:89:25)',
      dataId: 'exam-12345',
      retryCount: 3
    },
    {
      id: '6',
      timestamp: new Date(Date.now() - 90 * 1000).toISOString(),
      level: 'info',
      source: 'interface',
      message: '数据发送成功',
      details: '已成功发送10条记录',
      dataId: 'exam-12346'
    }
  ]);
  
  // 模拟实时日志
  useEffect(() => {
    const interval = setInterval(() => {
      const now = new Date().toISOString();
      const sources = ['system', 'database', 'event', 'processor', 'interface'];
      const levels = ['info', 'warning', 'error'] as const;
      const messages = {
        system: ['系统状态检查', '配置已加载', '定时任务执行'],
        database: ['数据库连接保持', '查询执行完成', '事务提交成功'],
        event: ['新事件检测到', '事件过滤完成', '事件队列更新'],
        processor: ['处理器初始化', '数据处理完成', '转换逻辑执行'],
        interface: ['请求准备中', '响应接收', '数据同步完成']
      };
      
      const randomSource = sources[Math.floor(Math.random() * sources.length)];
      const randomLevel = levels[Math.floor(Math.random() * levels.length)];
      const sourceMessages = messages[randomSource as keyof typeof messages];
      const randomMessage = sourceMessages[Math.floor(Math.random() * sourceMessages.length)];
      
      // 只有小概率添加警告和错误日志
      const actualLevel = randomLevel === 'error' && Math.random() > 0.1 
        ? 'info' 
        : randomLevel === 'warning' && Math.random() > 0.3 
        ? 'info' 
        : randomLevel;
      
      const newLog: LogEntry = {
        id: Date.now().toString(),
        timestamp: now,
        level: actualLevel,
        source: randomSource,
        message: randomMessage,
        details: `详细信息: ${randomMessage.toLowerCase()}`
      };
      
      setLogs(prev => [newLog, ...prev].slice(0, 100)); // 保持日志数量在100条以内
    }, 15000); // 每15秒添加一条模拟日志
    
    return () => clearInterval(interval);
  }, []);
  
  // 自动滚动到最新日志
  useEffect(() => {
    logsEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [logs]);
  
  // 筛选日志
  const filteredLogs = logs.filter(log => {
    // 级别筛选
    if (logLevel !== 'all' && log.level !== logLevel) return false;
    
    // 标签筛选
    if (activeTab === 'errors' && log.level !== 'error') return false;
    if (activeTab === 'sending' && !log.dataId) return false;
    
    // 搜索筛选
    if (searchTerm && !log.message.toLowerCase().includes(searchTerm.toLowerCase()) && 
        !log.source.toLowerCase().includes(searchTerm.toLowerCase()) &&
        !log.details.toLowerCase().includes(searchTerm.toLowerCase())) {
      return false;
    }
    
    // 日期筛选
    if (dateFilter) {
      const logDate = new Date(log.timestamp).toISOString().split('T')[0];
      if (logDate !== dateFilter) return false;
    }
    
    return true;
  });
  
  // 导出日志
  const exportLogs = () => {
    const logsToExport = JSON.stringify(filteredLogs, null, 2);
    const blob = new Blob([logsToExport], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `pacs-logs-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    toast('日志已导出');
  };
  
  // 重新发送数据
  const resendData = (dataId: string) => {
    const now = new Date().toISOString();
    const newLog: LogEntry = {
      id: Date.now().toString(),
      timestamp: now,
      level: 'info',
      source: 'interface',
      message: `重新发送数据: ${dataId}`,
      details: '用户触发的重新发送操作',
      dataId: dataId
    };
    
    setLogs(prev => [newLog, ...prev]);
    toast(`正在重新发送数据 ${dataId}`);
  };
  
  // 查看错误详情
  const viewErrorDetails = (log: LogEntry) => {
    setSelectedError(log);
    setShowErrorDetail(true);
  };
  
  // 关闭错误详情
  const closeErrorDetail = () => {
    setShowErrorDetail(false);
    setSelectedError(null);
  };
  
  // 格式化时间戳
  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  };
  
  // 日志条目接口
  interface LogEntry {
    id: string;
    timestamp: string;
    level: 'info' | 'warning' | 'error';
    source: string;
    message: string;
    details: string;
    errorStack?: string;
    dataId?: string;
    retryCount?: number;
  }
  
  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <h2 className="text-2xl font-bold">调试与日志</h2>
        
        <div className="flex flex-wrap gap-2">
          <button
            onClick={exportLogs}
            className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            <i className="fa-solid fa-download mr-1"></i> 导出日志
          </button>
        </div>
      </div>
      
      {/* 标签切换 */}
      <div className="flex border-b border-gray-200 dark:border-gray-800">
        <button
          onClick={() => setActiveTab('logs')}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
            activeTab === 'logs'
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          }`}
        >
          <i className="fa-solid fa-file-lines"></i>
          全部日志
        </button>
        <button
          onClick={() => setActiveTab('errors')}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
            activeTab === 'errors'
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          }`}
        >
          <i className="fa-solid fa-triangle-exclamation"></i>
          错误日志
          {logs.filter(log => log.level === 'error').length > 0 && (
            <span className="ml-1 rounded-full bg-red-100 px-2 py-0.5 text-xs font-medium text-red-800 dark:bg-red-900/30 dark:text-red-400">
              {logs.filter(log => log.level === 'error').length}
            </span>
          )}
        </button>
        <button
          onClick={() => setActiveTab('sending')}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
            activeTab === 'sending'
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          }`}
        >
          <i className="fa-solid fa-paper-plane"></i>
          发送记录
        </button>
      </div>
      
      {/* 筛选器 */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div>
          <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
            日志级别
          </label>
          <select
            value={logLevel}
            onChange={(e) => setLogLevel(e.target.value as 'all' | 'info' | 'warning' | 'error')}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
          >
            <option value="all">全部级别</option>
            <option value="info">信息</option>
            <option value="warning">警告</option>
            <option value="error">错误</option>
          </select>
        </div>
        
        <div>
          <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
            搜索
          </label>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
            placeholder="搜索日志消息..."
          />
        </div>
        
        <div>
          <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
            日期筛选
          </label>
          <input
            type="date"
            value={dateFilter}
            onChange={(e) => setDateFilter(e.target.value)}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
          />
        </div>
      </div>
      
      {/* 日志列表 */}
      <div className="rounded-xl border border-gray-200 bg-white shadow-md dark:border-gray-800 dark:bg-gray-800">
        <div className="overflow-x-auto">
          <table className="w-full min-w-full">
            <thead className="border-b border-gray-200 bg-gray-50 dark:border-gray-800 dark:bg-gray-900">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                  时间戳
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                  级别
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                  来源
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                  消息
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                  详细信息
                </th>
                <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                  操作
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-800">
              {filteredLogs.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-10 text-center text-gray-500 dark:text-gray-400">
                    <div className="flex flex-col items-center justify-center">
                      <i className="fa-solid fa-file-circle-exclamation text-4xl text-gray-300 dark:text-gray-600 mb-2"></i>
                      没有找到匹配的日志记录
                    </div>
                  </td>
                </tr>
              ) : (
                filteredLogs.map((log) => (
                  <tr 
                    key={log.id} 
                    className={`hover:bg-gray-50 dark:hover:bg-gray-750 ${
                      log.level === 'error' ? 'bg-red-50/50 dark:bg-red-900/10' : 
                      log.level === 'warning' ? 'bg-yellow-50/50 dark:bg-yellow-900/10' : ''
                    }`}
                  >
                    <td className="px-4 py-3 whitespace-nowrap">
                      <div className="text-sm text-gray-500 dark:text-gray-400">
                        {formatTimestamp(log.timestamp)}
                      </div>
                    </td>
                    <td className="px-4 py-3 whitespace-nowrap">
                      <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                        log.level === 'info'
                          ? 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400'
                          : log.level === 'warning'
                          ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400'
                          : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400'
                      }`}>
                        {log.level === 'info' ? '信息' : log.level === 'warning' ? '警告' : '错误'}
                      </span>
                    </td>
                    <td className="px-4 py-3 whitespace-nowrap">
                      <span className="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800 dark:bg-gray-700 dark:text-gray-300">
                        {log.source}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <div className="font-medium">{log.message}</div>
                      {log.dataId && (
                        <div className="text-xs text-gray-500 dark:text-gray-400">
                          数据ID: {log.dataId}
                        </div>
                      )}
                    </td>
                    <td className="px-4 py-3 max-w-xs">
                      <div className="text-sm text-gray-500 dark:text-gray-400 truncate">{log.details}</div>
                    </td>
                    <td className="px-4 py-3 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex justify-end gap-2">
                        {log.level === 'error' && (
                          <button
                            onClick={() => viewErrorDetails(log)}
                            className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300"
                            title="查看错误详情"
                          >
                            <i className="fa-solid fa-circle-exclamation"></i>
                          </button>
                        )}
                        {log.dataId && (
                          <button
                            onClick={() => resendData(log.dataId)}
                            className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300"
                            title="重新发送"
                          >
                            <i className="fa-solid fa-rotate-right"></i>
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
              <tr ref={logsEndRef} />
            </tbody>
          </table>
        </div>
      </div>
      
      {/* 统计信息 */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
          <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">总日志数</h4>
          <p className="text-2xl font-bold">{logs.length}</p>
        </div>
        
        <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
          <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">错误日志</h4>
          <p className="text-2xl font-bold text-red-600 dark:text-red-400">
            {logs.filter(log => log.level === 'error').length}
          </p>
        </div>
        
        <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
          <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">发送记录</h4>
          <p className="text-2xl font-bold text-blue-600 dark:text-blue-400">
            {logs.filter(log => log.dataId).length}
          </p>
        </div>
      </div>
      
      {/* 错误详情模态框 */}
      {showErrorDetail && selectedError && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="mx-4 w-full max-w-3xl rounded-xl bg-white p-6 shadow-xl dark:bg-gray-800">
            <div className="mb-4 flex items-center justify-between">
              <h3 className="text-xl font-bold">错误详情</h3>
              <button
                onClick={closeErrorDetail}
                className="rounded-full bg-gray-200 p-1 text-gray-700 hover:bg-gray-300 dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600"
              >
                <i className="fa-solid fa-xmark"></i>
              </button>
            </div>
            
            <div className="mb-4 rounded-lg bg-red-50 p-4 text-sm text-red-700 dark:bg-red-900/20 dark:text-red-400">
              <div className="font-medium mb-1">{selectedError.message}</div>
              <div className="text-gray-600 dark:text-gray-300">{selectedError.details}</div>
              {selectedError.dataId && (
                <div className="mt-1">数据ID: {selectedError.dataId}</div>
              )}
              {selectedError.retryCount !== undefined && (
                <div>重试次数: {selectedError.retryCount}</div>
              )}
            </div>
            
            {selectedError.errorStack && (
              <div className="mb-4">
                <h4 className="mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">错误栈</h4>
                <pre className="overflow-auto rounded-lg bg-gray-900 p-4 text-xs text-gray-300">
                  {selectedError.errorStack}
                </pre>
              </div>
            )}
            
            <div className="flex justify-end gap-2">
              {selectedError.dataId && (
                <button
                  onClick={() => {
                    resendData(selectedError.dataId);
                    closeErrorDetail();
                  }}
                  className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700"
                >
                  重新发送数据
                </button>
              )}
              <button
                onClick={closeErrorDetail}
                className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
              >
                关闭
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}