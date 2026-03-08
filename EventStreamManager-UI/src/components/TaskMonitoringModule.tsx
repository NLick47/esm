import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { 
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
  BarChart, Bar, PieChart, Pie, Cell, RadarChart, Radar, PolarGrid, PolarAngleAxis, PolarRadiusAxis
} from 'recharts';

// 任务监控模块
export default function TaskMonitoringModule() {
  const [activeTab, setActiveTab] = useState<'queue' | 'statistics' | 'performance'>('queue');
  
  // 模拟任务队列数据
  const [taskQueue, setTaskQueue] = useState<Task[]>([
    {
      id: 't1',
      type: 'data_processing',
      status: 'running',
      startTime: new Date(Date.now() - 5 * 60 * 1000).toISOString(),
      dataId: 'exam-12347',
      databaseType: 'ultrasound',
      processorName: '超声检查数据处理器',
      progress: 65
    },
    {
      id: 't2',
      type: 'interface_sending',
      status: 'pending',
      startTime: new Date(Date.now() - 2 * 60 * 1000).toISOString(),
      dataId: 'exam-12348',
      databaseType: 'radiology',
      processorName: '放射报告处理器',
      progress: 0
    },
    {
      id: 't3',
      type: 'data_processing',
      status: 'completed',
      startTime: new Date(Date.now() - 15 * 60 * 1000).toISOString(),
      endTime: new Date(Date.now() - 8 * 60 * 1000).toISOString(),
      dataId: 'exam-12346',
      databaseType: 'ultrasound',
      processorName: '超声检查数据处理器',
      progress: 100
    },
    {
      id: 't4',
      type: 'interface_sending',
      status: 'failed',
      startTime: new Date(Date.now() - 20 * 60 * 1000).toISOString(),
      endTime: new Date(Date.now() - 18 * 60 * 1000).toISOString(),
      dataId: 'exam-12345',
      databaseType: 'ultrasound',
      processorName: '超声检查数据处理器',
      progress: 100,
      errorMessage: '网络连接超时'
    }
  ]);
  
  // 模拟性能数据
  const [performanceData, setPerformanceData] = useState<PerformanceRecord[]>([
    { time: '09:00', processing: 45, sending: 30, success: 65, failure: 10 },
    { time: '10:00', processing: 60, sending: 45, success: 85, failure: 20 },
    { time: '11:00', processing: 75, sending: 60, success: 115, failure: 20 },
    { time: '12:00', processing: 40, sending: 35, success: 65, failure: 10 },
    { time: '13:00', processing: 30, sending: 25, success: 45, failure: 10 },
    { time: '14:00', processing: 55, sending: 50, success: 95, failure: 10 },
    { time: '15:00', processing: 70, sending: 65, success: 120, failure: 15 },
    { time: '16:00', processing: 80, sending: 75, success: 140, failure: 15 },
    { time: '17:00', processing: 65, sending: 60, success: 110, failure: 15 },
    { time: '18:00', processing: 40, sending: 35, success: 65, failure: 10 }
  ]);
  
  // 模拟成功率数据
  const [successRateData, setSuccessRateData] = useState<SuccessRateRecord[]>([
    { name: '超声数据', success: 85, failure: 15 },
    { name: '放射数据', success: 90, failure: 10 },
    { name: '内镜数据', success: 80, failure: 20 }
  ]);
  
  // 模拟处理效率数据
  const [efficiencyData, setEfficiencyData] = useState<EfficiencyRecord[]>([
    { subject: '处理速度', A: 80, B: 60, fullMark: 100 },
    { subject: '成功率', A: 85, B: 75, fullMark: 100 },
    { subject: '资源占用', A: 70, B: 80, fullMark: 100 },
    { subject: '响应时间', A: 75, B: 65, fullMark: 100 },
    { subject: '错误率', A: 15, B: 25, fullMark: 100 },
  ]);
  
  // 模拟任务状态变化
  useEffect(() => {
    const interval = setInterval(() => {
      setTaskQueue(prevTasks => 
        prevTasks.map(task => {
          if (task.status === 'running' && task.progress < 100) {
            const newProgress = task.progress + Math.floor(Math.random() * 10);
            const finalProgress = Math.min(newProgress, 100);
            
            // 任务完成逻辑
            if (finalProgress === 100) {
              return {
                ...task,
                progress: 100,
                status: 'completed' as TaskStatus,
                endTime: new Date().toISOString()
              };
            }
            
            return { ...task, progress: finalProgress };
          }
          
          // 待处理任务开始执行
          if (task.status === 'pending' && Math.random() > 0.7) {
            return { ...task, status: 'running' as TaskStatus };
          }
          
          return task;
        })
      );
    }, 3000); // 每3秒更新一次任务状态
    
    return () => clearInterval(interval);
  }, []);
  
  // 暂停任务
  const pauseTask = (taskId: string) => {
    setTaskQueue(prev => 
      prev.map(task => 
        task.id === taskId ? { ...task, status: 'paused' } : task
      )
    );
    toast('任务已暂停');
  };
  
  // 继续任务
  const resumeTask = (taskId: string) => {
    setTaskQueue(prev => 
      prev.map(task => 
        task.id === taskId ? { ...task, status: 'running' } : task
      )
    );
    toast('任务已继续');
  };
  
  // 取消任务
  const cancelTask = (taskId: string) => {
    if (window.confirm('确定要取消这个任务吗？')) {
      setTaskQueue(prev => 
        prev.map(task => 
          task.id === taskId ? { ...task, status: 'cancelled' } : task
        )
      );
      toast('任务已取消');
    }
  };
  
  // 重新尝试任务
  const retryTask = (taskId: string) => {
    setTaskQueue(prev => 
      prev.map(task => 
        task.id === taskId ? { 
          ...task, 
          status: 'pending',
          progress: 0,
          startTime: new Date().toISOString(),
          endTime: undefined,
          errorMessage: undefined
        } : task
      )
    );
    toast('任务已重新添加到队列');
  };
  
  // 格式化时间戳
  const formatTimestamp = (timestamp: string | undefined) => {
    if (!timestamp) return '-';
    return new Date(timestamp).toLocaleString('zh-CN', {
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  };
  
  // 计算任务持续时间
  const getDuration = (startTime: string, endTime?: string) => {
    const start = new Date(startTime).getTime();
    const end = endTime ? new Date(endTime).getTime() : Date.now();
    const duration = end - start;
    
    const minutes = Math.floor(duration / (1000 * 60));
    const seconds = Math.floor((duration % (1000 * 60)) / 1000);
    
    return `${minutes}m ${seconds}s`;
  };
  
  // 任务接口
  type TaskStatus = 'pending' | 'running' | 'paused' | 'completed' | 'failed' | 'cancelled';
  
  interface Task {
    id: string;
    type: 'data_processing' | 'interface_sending';
    status: TaskStatus;
    startTime: string;
    endTime?: string;
    dataId: string;
    databaseType: string;
    processorName: string;
    progress: number;
    errorMessage?: string;
  }
  
  // 性能记录接口
  interface PerformanceRecord {
    time: string;
    processing: number;
    sending: number;
    success: number;
    failure: number;
  }
  
  // 成功率记录接口
  interface SuccessRateRecord {
    name: string;
    success: number;
    failure: number;
  }
  
  // 效率记录接口
  interface EfficiencyRecord {
    subject: string;
    A: number; // 当前系统
    B: number; // 平均水平
    fullMark: number;
  }
  
  // 获取任务状态样式
  const getStatusStyle = (status: TaskStatus) => {
    switch (status) {
      case 'pending':
        return {
          color: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
          icon: 'fa-hourglass-half',
          label: '等待中'
        };
      case 'running':
        return {
          color: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
          icon: 'fa-spinner fa-spin',
          label: '运行中'
        };
      case 'paused':
        return {
          color: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300',
          icon: 'fa-pause',
          label: '已暂停'
        };
      case 'completed':
        return {
          color: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
          icon: 'fa-check-circle',
          label: '已完成'
        };
      case 'failed':
        return {
          color: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
          icon: 'fa-times-circle',
          label: '已失败'
        };
      case 'cancelled':
        return {
          color: 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400',
          icon: 'fa-ban',
          label: '已取消'
        };
      default:
        return {
          color: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300',
          icon: 'fa-question-circle',
          label: '未知'
        };
    }
  };
  
  // 饼图颜色
  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];
  
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">任务监控</h2>
      </div>
      
      {/* 标签切换 */}
      <div className="flex border-b border-gray-200 dark:border-gray-800">
        <button
          onClick={() => setActiveTab('queue')}className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
            activeTab === 'queue'
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          }`}
        >
          <i className="fa-solid fa-list-check"></i>
          任务队列
        </button>
        <button
          onClick={() => setActiveTab('statistics')}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
            activeTab === 'statistics'
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          }`}
        >
          <i className="fa-solid fa-chart-pie"></i>
          统计分析
        </button>
        <button
          onClick={() => setActiveTab('performance')}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
            activeTab === 'performance'
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          }`}
        >
          <i className="fa-solid fa-chart-line"></i>
          性能监控
        </button>
      </div>
      
      {/* 任务队列 */}
      {activeTab === 'queue' && (
        <div>
          {/* 任务状态统计 */}
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-4 mb-6">
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">总任务数</h4>
              <p className="text-2xl font-bold">{taskQueue.length}</p>
            </div>
            
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">运行中</h4>
              <p className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                {taskQueue.filter(t => t.status === 'running').length}
              </p>
            </div>
            
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">已完成</h4>
              <p className="text-2xl font-bold text-green-600 dark:text-green-400">
                {taskQueue.filter(t => t.status === 'completed').length}
              </p>
            </div>
            
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">已失败</h4>
              <p className="text-2xl font-bold text-red-600 dark:text-red-400">
                {taskQueue.filter(t => t.status === 'failed').length}
              </p>
            </div>
          </div>
          
          {/* 任务列表 */}
          <div className="rounded-xl border border-gray-200 bg-white shadow-md dark:border-gray-800 dark:bg-gray-800">
            <div className="overflow-x-auto">
              <table className="w-full min-w-full">
                <thead className="border-b border-gray-200 bg-gray-50 dark:border-gray-800 dark:bg-gray-900">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      任务类型
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      数据ID
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      处理器
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      状态
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      进度
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      开始时间
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      持续时间
                    </th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      操作
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 dark:divide-gray-800">
                  {taskQueue.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="px-4 py-10 text-center text-gray-500 dark:text-gray-400">
                        <div className="flex flex-col items-center justify-center">
                          <i className="fa-solid fa-tasks text-4xl text-gray-300 dark:text-gray-600 mb-2"></i>
                          当前没有任务在队列中
                        </div>
                      </td>
                    </tr>
                  ) : (
                    taskQueue.map((task) => {
                      const statusInfo = getStatusStyle(task.status);
                      return (
                        <tr key={task.id} className="hover:bg-gray-50 dark:hover:bg-gray-750">
                          <td className="px-4 py-3 whitespace-nowrap">
                            <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                              task.type === 'data_processing'
                                ? 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400'
                                : 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
                            }`}>
                              {task.type === 'data_processing' ? '数据处理' : '接口发送'}
                            </span>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap">
                            <div className="font-medium">{task.dataId}</div>
                            <div className="text-xs text-gray-500 dark:text-gray-400">
                              {task.databaseType}
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap">
                            <div className="text-sm text-gray-500 dark:text-gray-400 line-clamp-1">
                              {task.processorName}
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap">
                            <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${statusInfo.color}`}>
                              <i className={`fa-solid ${statusInfo.icon} mr-1`}></i>
                              {statusInfo.label}
                            </span>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap">
                            <div className="w-32">
                              <div className="mb-1 flex items-center justify-between">
                                <span className="text-xs text-gray-500 dark:text-gray-400">{task.progress}%</span>
                              </div>
                              <div className="h-2 w-full overflow-hidden rounded-full bg-gray-200 dark:bg-gray-700">
                                <div 
                                  className={`h-full rounded-full ${
                                    task.progress === 100 
                                      ? 'bg-green-500' 
                                      : task.status === 'running' 
                                      ? 'bg-blue-500' 
                                      : task.status === 'failed' 
                                      ? 'bg-red-500' 
                                      : 'bg-gray-400'
                                  }`} 
                                  style={{ width: `${task.progress}%` }}
                                ></div>
                              </div>
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap">
                            <div className="text-sm text-gray-500 dark:text-gray-400">
                              {formatTimestamp(task.startTime)}
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap">
                            <div className="text-sm text-gray-500 dark:text-gray-400">
                              {getDuration(task.startTime, task.endTime)}
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap text-right text-sm font-medium">
                            <div className="flex justify-end gap-2">
                              {(task.status === 'running' || task.status === 'paused') && (
                                <>
                                  {task.status === 'running' ? (
                                    <button
                                      onClick={() => pauseTask(task.id)}
                                      className="text-yellow-600 hover:text-yellow-800 dark:text-yellow-400 dark:hover:text-yellow-300"
                                      title="暂停任务"
                                    >
                                      <i className="fa-solid fa-pause"></i>
                                    </button>
                                  ) : (
                                    <button
                                      onClick={() => resumeTask(task.id)}
                                      className="text-green-600 hover:text-green-800 dark:text-green-400 dark:hover:text-green-300"
                                      title="继续任务"
                                    >
                                      <i className="fa-solid fa-play"></i>
                                    </button>
                                  )}
                                  <button
                                    onClick={() => cancelTask(task.id)}
                                    className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300"
                                    title="取消任务"
                                  >
                                    <i className="fa-solid fa-ban"></i>
                                  </button>
                                </>
                              )}
                              
                              {(task.status === 'failed' || task.status === 'cancelled') && (
                                <button
                                  onClick={() => retryTask(task.id)}
                                  className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300"
                                  title="重新尝试"
                                >
                                  <i className="fa-solid fa-rotate-right"></i>
                                </button>
                              )}
                            </div>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
      
      {/* 统计分析 */}
      {activeTab === 'statistics' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
            {/* 数据处理成功率饼图 */}
            <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h3 className="mb-4 text-lg font-semibold">数据处理成功率</h3>
              <div className="h-72">
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={successRateData}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="success"
                    >
                      {successRateData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(value) => `${value}%`} />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              </div>
              <div className="mt-4 text-sm text-gray-500 dark:text-gray-400 text-center">
                各类数据处理的成功率分布
              </div>
            </div>
            
            {/* 数据处理vs发送对比柱状图 */}
            <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h3 className="mb-4 text-lg font-semibold">数据处理与发送对比</h3>
              <div className="h-72">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={performanceData}
                    margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="time" />
                    <YAxis />
                    <Tooltip />
                    <Legend />
                    <Bar dataKey="processing" name="数据处理" fill="#8884d8" />
                    <Bar dataKey="sending" name="接口发送" fill="#82ca9d" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
              <div className="mt-4 text-sm text-gray-500 dark:text-gray-400 text-center">
                每小时数据处理和接口发送量对比
              </div>
            </div>
            
            {/* 成功率vs失败率柱状图 */}
            <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h3 className="mb-4 text-lg font-semibold">成功与失败对比</h3>
              <div className="h-72">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={performanceData}
                    margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="time" />
                    <YAxis />
                    <Tooltip />
                    <Legend />
                    <Bar dataKey="success" name="成功数量" fill="#00C49F" />
                    <Bar dataKey="failure" name="失败数量" fill="#FF8042" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
              <div className="mt-4 text-sm text-gray-500 dark:text-gray-400 text-center">
                每小时处理成功与失败数量对比
              </div>
            </div>
            
            {/* 系统效率雷达图 */}
            <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h3 className="mb-4 text-lg font-semibold">系统效率对比</h3>
              <div className="h-72">
                <ResponsiveContainer width="100%" height="100%">
                  <RadarChart cx="50%" cy="50%" outerRadius="80%" data={efficiencyData}>
                    <PolarGrid />
                    <PolarAngleAxis dataKey="subject" />
                    <PolarRadiusAxis angle={30} domain={[0, 100]} />
                    <Radar
                      name="当前系统"
                      dataKey="A"
                      stroke="#8884d8"
                      fill="#8884d8"
                      fillOpacity={0.6}
                    />
                    <Radar
                      name="平均水平"
                      dataKey="B"
                      stroke="#82ca9d"
                      fill="#82ca9d"
                      fillOpacity={0.6}
                    />
                    <Legend />
                    <Tooltip />
                  </RadarChart>
                </ResponsiveContainer>
              </div>
              <div className="mt-4 text-sm text-gray-500 dark:text-gray-400 text-center">
                当前系统与平均水平的各项指标对比
              </div>
            </div>
          </div>
          
          {/* 统计卡片 */}
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-4">
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">总体成功率</h4>
              <p className="text-2xl font-bold text-green-600 dark:text-green-400">83%</p>
            </div>
            
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">平均处理时间</h4>
              <p className="text-2xl font-bold">6m 23s</p>
            </div>
            
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">今日处理总量</h4>
              <p className="text-2xl font-bold">426</p>
            </div>
            
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">错误率</h4>
              <p className="text-2xl font-bold text-red-600 dark:text-red-400">17%</p>
            </div>
          </div>
        </div>
      )}
      
      {/* 性能监控 */}
      {activeTab === 'performance' && (
        <div className="space-y-6">
          {/* 性能趋势图 */}
          <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
            <h3 className="mb-4 text-lg font-semibold">系统性能趋势</h3>
            <div className="h-96">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart
                  data={performanceData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="time" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="processing"
                    name="数据处理量"
                    stroke="#8884d8"
                    activeDot={{ r: 8 }}
                    strokeWidth={2}
                  />
                  <Line
                    type="monotone"
                    dataKey="sending"
                    name="接口发送量"
                    stroke="#82ca9d"
                    strokeWidth={2}
                  />
                  <Line
                    type="monotone"
                    dataKey="success"
                    name="成功数量"
                    stroke="#00C49F"
                    strokeWidth={2}
                  />
                  <Line
                    type="monotone"
                    dataKey="failure"
                    name="失败数量"
                    stroke="#FF8042"
                    strokeWidth={2}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </div>
          
          {/* 性能指标卡片 */}
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-4">
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">处理速度</h4>
              <p className="text-2xl font-bold">80 req/min</p>
              <div className="mt-1 flex items-center text-xs text-green-600 dark:text-green-400">
                <i className="fa-solid fa-arrow-up mr-1"></i> 5% 较昨日
              </div>
            </div>
            
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">响应时间</h4>
              <p className="text-2xl font-bold">125 ms</p>
              <div className="mt-1 flex items-center text-xs text-green-600 dark:text-green-400">
                <i className="fa-solid fa-arrow-down mr-1"></i> 10% 较昨日
              </div>
            </div>
            
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">资源占用</h4>
              <p className="text-2xl font-bold">65%</p>
              <div className="mt-1 flex items-center text-xs text-red-600 dark:text-red-400">
                <i className="fa-solid fa-arrow-up mr-1"></i> 3% 较昨日
              </div>
            </div>
            
            <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">并发连接</h4>
              <p className="text-2xl font-bold">12</p>
              <div className="mt-1 flex items-center text-xs text-gray-500 dark:text-gray-400">
                <i className="fa-solid fa-minus mr-1"></i> 0% 较昨日
              </div>
            </div>
          </div>
          
          {/* 性能分析说明 */}
          <div className="rounded-xl bg-blue-50 p-4 text-sm text-blue-700 dark:bg-blue-900/20 dark:text-blue-400">
            <div className="flex items-start">
              <i className="fa-solid fa-chart-line mt-0.5 mr-2"></i>
              <div>
                <p className="mb-1 font-medium">性能分析:</p>
                <ul className="list-disc pl-5 space-y-1">
                  <li>系统整体运行稳定，处理速度和成功率均在正常范围内</li>
                  <li>下午16:00左右处理量达到高峰，建议关注系统资源使用情况</li>
                  <li>超声数据处理成功率较低，建议检查相关处理器逻辑</li>
                  <li>接口发送失败率有所上升，建议检查网络连接和目标服务器状态</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}