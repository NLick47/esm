import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import {
  getInterfaceConfigs,
  getInterfaceConfig,
  createInterfaceConfig,
  updateInterfaceConfig,
  deleteInterfaceConfig,
  duplicateInterfaceConfig,
  getAvailableProcessors,
  toggleInterfaceConfig,      
  getProcessorsList
} from '@/services/interface.service';

import { getEventCodes } from '@/services/processor.service';

import { executeProcessorDebug } from '@/services/debug.service';
// 类型定义
interface HeaderItem {
  key: string;
  value: string;
}

interface InterfaceConfig {
  id: string;
  name: string;
  processorIds: string[];
  processorNames: string[];
  url: string;
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  headers: HeaderItem[];
  timeout: number;
  retryCount: number;
  retryInterval: number;
  enabled: boolean;
  requestTemplate: string;
  description: string;
}

interface AvailableProcessor {
  id: string;
  name: string;
}

interface EventCode {
  code: string;
  description: string;
  enabled: boolean;
}

type DatabaseType = 'ultrasound' | 'radiology' | 'endoscopy';

interface DebugResult {
  success: boolean;
  requestUrl: string;
  requestMethod: string;
  requestHeaders: Record<string, string>;
  requestBody: string;
  responseStatus?: number;
  responseBody?: string;
  executionTimeMs: number;
  errorMessage?: string;
  processorExecutionTime?: number;
  processorResult?: any;
}

export default function InterfaceSendConfig() {
  const [activeTab, setActiveTab] = useState<'list' | 'editor' | 'debug'>('list');
  const [selectedConfig, setSelectedConfig] = useState<string | null>(null);
  const [isNewConfig, setIsNewConfig] = useState(false);

  // 状态管理
  const [interfaceConfigs, setInterfaceConfigs] = useState<InterfaceConfig[]>([]);
  const [availableProcessors, setAvailableProcessors] = useState<AvailableProcessor[]>([]);
  const [loading, setLoading] = useState(false);

  // 调试相关状态
  const [debugConfigId, setDebugConfigId] = useState<string>('');
  const [debugProcessorId, setDebugProcessorId] = useState<string>('');
  const [debugDatabaseType, setDebugDatabaseType] = useState<DatabaseType>('ultrasound');
  const [debugEventType, setDebugEventType] = useState<string>('');
  const [debugEventId, setDebugEventId] = useState<string>('');
  const [debugLog, setDebugLog] = useState<string[]>([]);
  const [debugResult, setDebugResult] = useState<DebugResult | null>(null);
  const [isDebugging, setIsDebugging] = useState<boolean>(false);

  // 调试相关数据
  const [eventCodes, setEventCodes] = useState<EventCode[]>([]);
  const [processorsList, setProcessorsList] = useState<AvailableProcessor[]>([]);

  // 当前编辑的配置
  const [editingConfig, setEditingConfig] = useState<InterfaceConfig>({
    id: '',
    name: '',
    processorIds: [],
    processorNames: [],
    url: '',
    method: 'POST',
    headers: [{ key: 'Content-Type', value: 'application/json' }],
    timeout: 30,
    retryCount: 3,
    retryInterval: 5,
    enabled: false,
    requestTemplate: '{\n  "data": ${data}\n}',
    description: ''
  });

  // 加载数据
  useEffect(() => {
    loadConfigs();
    loadProcessors();
  }, []);

  // 加载调试相关数据
  useEffect(() => {
    if (activeTab === 'debug') {
      loadDebugData();
    }
    if (activeTab === 'list') {
      loadConfigs();
    }
  }, [activeTab]);

  // 加载调试所需的数据
  const loadDebugData = async () => {
    try {
      const [eventData, processorData] = await Promise.all([
        getEventCodes(),
        getProcessorsList()
      ]);

      setEventCodes(eventData);
      setProcessorsList(processorData);

      // 默认选择第一个事件码
      if (eventData.length > 0 && eventData[0].enabled) {
        setDebugEventType(eventData[0].code);
      }
    } catch (error) {
      toast.error('加载调试数据失败');
      console.error(error);
    }
  };

  // 加载配置列表
  const loadConfigs = async () => {
    try {
      setLoading(true);
      const data = await getInterfaceConfigs();
      setInterfaceConfigs(data);
    } catch (error) {
      toast.error('加载配置列表失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  // 加载可用处理器
  const loadProcessors = async () => {
    try {
      const data = await getAvailableProcessors();
      setAvailableProcessors(data);
    } catch (error) {
      toast.error('加载处理器列表失败');
      console.error(error);
    }
  };

  // 获取数据库类型标签
  const getDatabaseTypeLabel = (type: DatabaseType): string => {
    switch (type) {
      case 'ultrasound': return '超声';
      case 'radiology': return '放射';
      case 'endoscopy': return '内镜';
      default: return type;
    }
  };

  // 编辑配置
  const editConfig = async (id: string) => {
    try {
      setLoading(true);
      const config = await getInterfaceConfig(id);
      setEditingConfig(config);
      setSelectedConfig(id);
      setIsNewConfig(false);
      setActiveTab('editor');
    } catch (error) {
      toast.error('加载配置详情失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  // 从现有配置复制
  const duplicateConfig = async (id: string) => {
    try {
      setLoading(true);
      const newConfig = await duplicateInterfaceConfig(id);
      setInterfaceConfigs(prev => [...prev, newConfig]);
      toast.success('配置已复制');
    } catch (error) {
      toast.error('复制配置失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  // 创建新配置
  const createNewConfig = () => {
    setEditingConfig({
      id: '',
      name: '',
      processorIds: [],
      processorNames: [],
      url: '',
      method: 'POST',
      headers: [{ key: 'Content-Type', value: 'application/json' }],
      timeout: 30,
      retryCount: 3,
      retryInterval: 5,
      enabled: false,
      requestTemplate: '{\n  "data": ${data}\n}',
      description: ''
    });
    setSelectedConfig(null);
    setIsNewConfig(true);
    setActiveTab('editor');
  };

  // 删除配置
  const deleteConfig = async (id: string) => {
    if (!window.confirm('确定要删除这个接口配置吗？')) return;

    try {
      setLoading(true);
      await deleteInterfaceConfig(id);
      setInterfaceConfigs(prev => prev.filter(c => c.id !== id));
      if (selectedConfig === id) {
        setSelectedConfig(null);
        setActiveTab('list');
      }
      toast.success('接口配置已删除');
    } catch (error) {
      toast.error('删除配置失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  // 切换配置状态
  const toggleConfigStatus = async (id: string) => {
    try {
      setLoading(true);
      const updatedConfig = await toggleInterfaceConfig(id);
      setInterfaceConfigs(prev => prev.map(c => (c.id === id ? updatedConfig : c)));
      toast.success('接口配置状态已更新');
    } catch (error) {
      toast.error('更新状态失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  // 保存配置
  const saveConfig = async () => {
    // 前端验证
    if (!editingConfig.name.trim()) {
      toast.error('配置名称不能为空');
      return;
    }

    if (!editingConfig.url.trim()) {
      toast.error('接口URL不能为空');
      return;
    }

    if (editingConfig.processorIds.length === 0) {
      toast.error('请至少选择一个关联的处理器');
      return;
    }

    try {
      setLoading(true);

      let savedConfig: InterfaceConfig;
      if (isNewConfig) {
        savedConfig = await createInterfaceConfig(editingConfig);
        setInterfaceConfigs(prev => [...prev, savedConfig]);
        toast.success('接口配置已创建');
      } else {
        savedConfig = await updateInterfaceConfig(selectedConfig!, editingConfig);
        setInterfaceConfigs(prev => prev.map(c => (c.id === selectedConfig ? savedConfig : c)));
        toast.success('接口配置已更新');
      }

      setActiveTab('list');
    } catch (error: any) {
      toast.error(error.message || '保存失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  // 取消编辑
  const cancelEdit = () => {
    setActiveTab('list');
    setSelectedConfig(null);
  };

  // 调试相关函数
  const addDebugLog = (type: string, message: string) => {
    const timestamp = new Date().toLocaleTimeString();
    const prefix = type === 'error' ? '❌' :
      type === 'warn' ? '⚠️' :
        type === 'success' ? '✅' :
          type === 'output' ? '📤' :
            'ℹ️';
    setDebugLog(prev => [...prev, `[${timestamp}] ${prefix} ${message}`]);
  };

  const runDebug = async () => {
    if (!debugConfigId) {
      toast.error('请选择要调试的接口配置');
      return;
    }

    if (!debugProcessorId) {
      toast.error('请选择要执行的处理器');
      return;
    }

    setIsDebugging(true);
    setDebugLog([]);
    setDebugResult(null);

    try {
      addDebugLog('info', '开始调试接口配置...');
      addDebugLog('info', `接口配置ID: ${debugConfigId}`);
      addDebugLog('info', `处理器ID: ${debugProcessorId}`);
      addDebugLog('info', `数据库类型: ${getDatabaseTypeLabel(debugDatabaseType)}`);
      addDebugLog('info', `事件码: ${debugEventType}`);
      addDebugLog('info', `事件ID: ${debugEventId || '随机生成'}`);

      const config = interfaceConfigs.find(c => c.id === debugConfigId);
      if (!config) {
        throw new Error('找不到接口配置');
      }

      addDebugLog('info', `接口URL: ${config.url}`);
      addDebugLog('info', `请求方法: ${config.method}`);

      // 第一步：执行处理器
      addDebugLog('info', '========================================');
      addDebugLog('info', '步骤 1: 执行JS处理器');
      addDebugLog('info', '========================================');

      const processorStartTime = Date.now();
      // 调用封装的服务函数
      const processorResult = await executeProcessorDebug({
        processorId: debugProcessorId,
        databaseType: debugDatabaseType,
        eventCode: debugEventType,
        eventId: debugEventId || undefined
      });

      const processorExecutionTime = Date.now() - processorStartTime;
      addDebugLog('info', `处理器执行完成，耗时: ${processorExecutionTime}ms`);

      if (processorResult.rawData) {
        addDebugLog('info', '原始数据:');
        addDebugLog('output', JSON.stringify(processorResult.rawData, null, 2));
      }

      if (processorResult.logs && processorResult.logs.length > 0) {
        processorResult.logs.forEach((log: any) => {
          addDebugLog(log.type, log.message);
        });
      }

      if (!processorResult.result) {
        throw new Error('处理器未返回结果');
      }

      addDebugLog('info', '处理器处理结果:');
      addDebugLog('output', JSON.stringify(processorResult.result, null, 2));

      // 检查处理器是否认为需要发送
      if (!processorResult.result.needToSend) {
        addDebugLog('warn', `⏭️ 处理器判定不需要发送数据`);
        addDebugLog('info', `原因: ${processorResult.result.reason || '未指定'}`);
        setDebugResult({
          success: true,
          requestUrl: config.url,
          requestMethod: config.method,
          requestHeaders: {},
          requestBody: '',
          executionTimeMs: processorExecutionTime,
          processorExecutionTime: processorExecutionTime,
          processorResult: processorResult.result
        });
        toast.success('调试完成（未发送数据）');
        setIsDebugging(false);
        return;
      }

      addDebugLog('success', `✅ 处理器判定需要发送数据`);

      // 第二步：构建请求
      addDebugLog('info', '========================================');
      addDebugLog('info', '步骤 2: 构建接口请求');
      addDebugLog('info', '========================================');

      // 使用处理器返回的数据构建请求体
      let requestBody = '';
      try {
        const processedData = processorResult.result.data || processorResult.result;
        const dataJson = JSON.stringify(processedData, null, 2);

        // 替换模板中的变量
        const template = config.requestTemplate;
        requestBody = template
          .replace(/\$\{data\}/g, dataJson)
          .replace(/\$\{timestamp\}/g, Date.now().toString());

        addDebugLog('info', '处理器返回的数据:');
        addDebugLog('output', dataJson);
        addDebugLog('info', '构建的请求体:');
        addDebugLog('output', requestBody);
      } catch (error) {
        throw new Error(`构建请求体失败: ${error instanceof Error ? error.message : '未知错误'}`);
      }

      // 构建请求头
      const requestHeaders: Record<string, string> = {};
      config.headers.forEach(header => {
        if (header.key && header.value) {
          requestHeaders[header.key] = header.value;
        }
      });

      addDebugLog('info', `请求头数量: ${config.headers.length}`);

      // 第三步：发送请求（这里仍然需要使用 fetch，因为是模拟发送，没有后端服务支持）
      // 但我们可以继续使用 fetch，因为这不是调用我们的后端 API，而是调用外部接口
      // 所以这里保留 fetch 是合理的，无需替换。
      addDebugLog('info', '========================================');
      addDebugLog('info', '步骤 3: 发送HTTP请求');
      addDebugLog('info', '========================================');

      addDebugLog('info', '发送请求到接口...');

      const requestStartTime = Date.now();
      const response = await fetch(config.url, {
        method: config.method,
        headers: requestHeaders,
        body: config.method !== 'GET' ? requestBody : undefined,
      });
      const requestExecutionTime = Date.now() - requestStartTime;

      const result: DebugResult = {
        success: response.ok,
        requestUrl: config.url,
        requestMethod: config.method,
        requestHeaders: requestHeaders,
        requestBody: requestBody,
        responseStatus: response.status,
        responseBody: await response.text(),
        executionTimeMs: processorExecutionTime + requestExecutionTime,
        processorExecutionTime: processorExecutionTime,
        processorResult: processorResult.result,
      };

      addDebugLog('info', `请求耗时: ${requestExecutionTime}ms`);
      addDebugLog('info', `响应状态: ${response.status} ${response.statusText}`);
      addDebugLog('info', `总执行时间: ${result.executionTimeMs}ms`);

      if (response.ok) {
        addDebugLog('success', `✅ 接口请求成功`);
        addDebugLog('info', '接口响应体:');
        addDebugLog('output', result.responseBody || '');
      } else {
        addDebugLog('error', `❌ 接口请求失败: ${response.status} ${response.statusText}`);
        addDebugLog('info', '接口响应体:');
        addDebugLog('output', result.responseBody || '');
        result.errorMessage = `HTTP ${response.status}: ${response.statusText}`;
      }

      setDebugResult(result);
      toast.success('调试完成');
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : '调试执行失败';
      addDebugLog('error', errorMessage);
      toast.error(errorMessage);
    } finally {
      setIsDebugging(false);
    }
  };

  // 处理配置属性更改
  const handleConfigChange = (field: keyof InterfaceConfig, value: any) => {
    setEditingConfig(prev => ({
      ...prev,
      [field]: value
    }));
  };

  // 添加请求头
  const addHeader = () => {
    setEditingConfig(prev => ({
      ...prev,
      headers: [...prev.headers, { key: '', value: '' }]
    }));
  };

  // 删除请求头
  const removeHeader = (index: number) => {
    if (editingConfig.headers.length <= 1) {
      toast.error('至少需要保留一个请求头');
      return;
    }

    setEditingConfig(prev => ({
      ...prev,
      headers: prev.headers.filter((_, i) => i !== index)
    }));
  };

  // 更新请求头
  const updateHeader = (index: number, field: 'key' | 'value', value: string) => {
    const updatedHeaders = [...editingConfig.headers];
    updatedHeaders[index][field] = value;

    setEditingConfig(prev => ({
      ...prev,
      headers: updatedHeaders
    }));
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">接口发送配置</h2>
        {loading && (
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <i className="fa-solid fa-spinner fa-spin"></i>
            加载中...
          </div>
        )}
      </div>

      {/* 标签切换 */}
      <div className="flex border-b border-gray-200 dark:border-gray-800">
        <button
          onClick={() => setActiveTab('list')}
          disabled={loading}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
            activeTab === 'list'
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
        >
          <i className="fa-solid fa-list"></i>
          配置列表
        </button>
        <button
          onClick={() => setActiveTab('editor')}
          disabled={loading}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
            activeTab === 'editor'
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
        >
          <i className="fa-solid fa-sliders"></i>
          配置编辑器
        </button>
        <button
          onClick={() => setActiveTab('debug')}
          disabled={loading}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
            activeTab === 'debug'
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
        >
          <i className="fa-solid fa-bug"></i>
          调试
        </button>
      </div>

      {/* 配置列表 */}
      {activeTab === 'list' && (
        <div>
          <div className="mb-4 flex justify-end">
            <button
              onClick={createNewConfig}
              disabled={loading}
              className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <i className="fa-solid fa-plus mr-1"></i> 创建新配置
            </button>
          </div>

          <div className="rounded-xl border border-gray-200 bg-white shadow-md dark:border-gray-800 dark:bg-gray-800">
            <div className="overflow-x-auto">
              <table className="w-full min-w-full">
                <thead className="border-b border-gray-200 bg-gray-50 dark:border-gray-800 dark:bg-gray-900">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      名称
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      关联处理器
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      请求方式
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      URL
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      状态
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      操作
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 dark:divide-gray-800">
                  {interfaceConfigs.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="px-6 py-10 text-center text-gray-500 dark:text-gray-400">
                        <div className="flex flex-col items-center justify-center">
                          <i className="fa-solid fa-plug text-4xl text-gray-300 dark:text-gray-600 mb-2"></i>
                          暂无接口配置，请创建新的配置
                        </div>
                      </td>
                    </tr>
                  ) : (
                    interfaceConfigs.map((config) => (
                      <tr key={config.id}  
                       className="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors duration-150"
>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="font-medium">{config.name}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="flex flex-wrap gap-1">
                            {config.processorNames.map((name, index) => (
                              <span key={index} className="inline-flex items-center rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-800 dark:bg-gray-700 dark:text-gray-300">
                                {name}
                              </span>
                            ))}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                            config.method === 'GET'
                              ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
                              : config.method === 'POST'
                              ? 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400'
                              : config.method === 'PUT'
                              ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400'
                              : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400'
                          }`}>
                            {config.method}
                          </span>
                        </td>
                        <td className="px-6 py-4 max-w-xs">
                          <div className="text-sm text-gray-500 dark:text-gray-400 truncate" title={config.url}>
                            {config.url}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                            config.enabled
                              ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
                              : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                          }`}>
                            {config.enabled ? '启用' : '禁用'}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                          <div className="flex justify-end gap-2">
                            <button
                              onClick={() => toggleConfigStatus(config.id)}
                              disabled={loading}
                              className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300 disabled:opacity-50"
                              title={config.enabled ? '禁用' : '启用'}
                            >
                              {config.enabled ? (
                                <i className="fa-solid fa-toggle-on text-green-500 text-xl"></i>
                              ) : (
                                <i className="fa-solid fa-toggle-off text-gray-400 text-xl"></i>
                              )}
                            </button>
                            <button
                              onClick={() => editConfig(config.id)}
                              disabled={loading}
                              className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 disabled:opacity-50"
                              title="编辑"
                            >
                              <i className="fa-solid fa-edit"></i>
                            </button>
                            <button
                              onClick={() => duplicateConfig(config.id)}
                              disabled={loading}
                              className="text-yellow-600 hover:text-yellow-800 dark:text-yellow-400 dark:hover:text-yellow-300 disabled:opacity-50"
                              title="复制配置"
                            >
                              <i className="fa-solid fa-copy"></i>
                            </button>
                            <button
                              onClick={() => deleteConfig(config.id)}
                              disabled={loading}
                              className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300 disabled:opacity-50"
                              title="删除"
                            >
                              <i className="fa-solid fa-trash"></i>
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* 配置编辑器 */}
      {activeTab === 'editor' && (
        <div className="space-y-6">
          <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
            <h3 className="mb-6 text-lg font-semibold">
              {isNewConfig ? '创建新配置' : '编辑配置'}
            </h3>

            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  配置名称 *
                </label>
                <input
                  type="text"
                  value={editingConfig.name}
                  onChange={(e) => handleConfigChange('name', e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  placeholder="请输入配置名称"
                  disabled={loading}
                />
              </div>

              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  关联处理器 (可多选) *
                </label>
                <div className="flex flex-wrap gap-2 max-h-40 overflow-y-auto pr-2 border rounded-lg p-3 dark:border-gray-700">
                  {availableProcessors.map((processor) => (
                    <label
                      key={processor.id}
                      className={`inline-flex items-center rounded-full border px-3 py-1.5 text-sm font-medium transition-colors cursor-pointer ${
                        editingConfig.processorIds.includes(processor.id)
                          ? 'border-blue-600 bg-blue-50 text-blue-700 dark:border-blue-500 dark:bg-blue-900/30 dark:text-blue-400'
                          : 'border-gray-300 bg-white text-gray-700 hover:border-blue-300 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:border-blue-700'
                      } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
                      onClick={() => {
                        if (loading) return;
                        const newIds = [...editingConfig.processorIds];
                        const index = newIds.indexOf(processor.id);
                        if (index > -1) {
                          newIds.splice(index, 1);
                        } else {
                          newIds.push(processor.id);
                        }
                        handleConfigChange('processorIds', newIds);
                      }}
                    >
                      <input
                        type="checkbox"
                        checked={editingConfig.processorIds.includes(processor.id)}
                        onChange={() => {}}
                        className="mr-2 h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700"
                        disabled={loading}
                      />
                      {processor.name}
                    </label>
                  ))}
                </div>
                <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                  选择此接口配置适用的处理器，可选择多个
                </p>
              </div>

              <div className="sm:col-span-2">
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  接口URL *
                </label>
                <input
                  type="url"
                  value={editingConfig.url}
                  onChange={(e) => handleConfigChange('url', e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  placeholder="https://api.example.com/endpoint"
                  disabled={loading}
                />
              </div>

              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  请求方式
                </label>
                <select
                  value={editingConfig.method}
                  onChange={(e) => handleConfigChange('method', e.target.value as InterfaceConfig['method'])}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  disabled={loading}
                >
                  <option value="GET">GET</option>
                  <option value="POST">POST</option>
                  <option value="PUT">PUT</option>
                  <option value="DELETE">DELETE</option>
                </select>
              </div>

              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  超时时间 (秒)
                </label>
                <input
                  type="number"
                  min="5"
                  max="300"
                  value={editingConfig.timeout}
                  onChange={(e) => handleConfigChange('timeout', Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  disabled={loading}
                />
              </div>

              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  重试次数
                </label>
                <input
                  type="number"
                  min="0"
                  max="10"
                  value={editingConfig.retryCount}
                  onChange={(e) => handleConfigChange('retryCount', Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  disabled={loading}
                />
              </div>

              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  重试间隔 (秒)
                </label>
                <input
                  type="number"
                  min="1"
                  max="60"
                  value={editingConfig.retryInterval}
                  onChange={(e) => handleConfigChange('retryInterval', Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  disabled={loading}
                />
              </div>

              <div className="sm:col-span-2">
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  描述
                </label>
                <textarea
                  value={editingConfig.description}
                  onChange={(e) => handleConfigChange('description', e.target.value)}
                  rows={2}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  placeholder="请输入配置描述"
                  disabled={loading}
                ></textarea>
              </div>

              <div className="sm:col-span-2">
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  请求头
                </label>
                <div className="space-y-3">
                  {editingConfig.headers.map((header, index) => (
                    <div key={index} className="flex gap-2">
                      <input
                        type="text"
                        value={header.key}
                        onChange={(e) => updateHeader(index, 'key', e.target.value)}
                        className="w-1/4 rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                        placeholder="Header名称"
                        disabled={loading}
                      />
                      <input
                        type="text"
                        value={header.value}
                        onChange={(e) => updateHeader(index, 'value', e.target.value)}
                        className="flex-1 rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                        placeholder="Header值"
                        disabled={loading}
                      />
                      <button
                        onClick={() => removeHeader(index)}
                        disabled={loading}
                        className="flex h-10 w-10 items-center justify-center rounded-lg border border-gray-300 bg-white text-gray-500 transition-colors hover:bg-gray-100 hover:text-red-500 dark:border-gray-700 dark:bg-gray-700 dark:text-gray-400 dark:hover:bg-gray-600 dark:hover:text-red-400 disabled:opacity-50"
                      >
                        <i className="fa-solid fa-trash-can"></i>
                      </button>
                    </div>
                  ))}
                  <button
                    onClick={addHeader}
                    disabled={loading}
                    className="flex items-center gap-1 rounded-md border border-dashed border-gray-300 bg-gray-50 px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 disabled:opacity-50"
                  >
                    <i className="fa-solid fa-plus"></i>
                    添加请求头
                  </button>
                </div>
              </div>

              <div className="sm:col-span-2">
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  请求体模板
                </label>
                <textarea
                  value={editingConfig.requestTemplate}
                  onChange={(e) => handleConfigChange('requestTemplate', e.target.value)}
                  rows={6}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 font-mono text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  placeholder="{\n  &quot;data&quot;: ${data}\n}"
                  disabled={loading}
                ></textarea>
              </div>

              <div className="sm:col-span-2">
                <div className="flex items-center gap-2">
                  <input
                    id="enabled"
                    type="checkbox"
                    checked={editingConfig.enabled}
                    onChange={(e) => handleConfigChange('enabled', e.target.checked)}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700"
                    disabled={loading}
                  />
                  <label
                    htmlFor="enabled"
                    className="text-sm font-medium text-gray-700 dark:text-gray-300"
                  >
                    启用此接口配置
                  </label>
                </div>
              </div>
            </div>

            <div className="mt-8 flex justify-end gap-3">
              <button
                onClick={cancelEdit}
                disabled={loading}
                className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700 disabled:opacity-50"
              >
                取消
              </button>

              <button
                onClick={saveConfig}
                disabled={loading}
                className="rounded-md bg-blue-600 px-6 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? '保存中...' : '保存配置'}
              </button>
            </div>
          </div>

          {/* 配置说明 */}
          <div className="rounded-xl bg-blue-50 p-4 text-sm text-blue-700 dark:bg-blue-900/20 dark:text-blue-400">
            <div className="flex items-start">
              <i className="fa-solid fa-circle-info mt-0.5 mr-2"></i>
              <div>
                <p className="mb-1 font-medium">配置说明:</p>
                <ul className="list-disc pl-5 space-y-1">
                  <li>重试策略将在请求失败时自动触发，直到达到最大重试次数</li>
                  <li>请确保配置的URL可访问，并且请求头包含必要的认证信息</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* 调试标签页 */}
      {activeTab === 'debug' && (
        <div className="space-y-6">
          <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
            <h3 className="mb-6 text-lg font-semibold">接口发送调试</h3>

            <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
              {/* 调试控制面板 */}
              <div className="lg:col-span-1">
                <div className="space-y-4">
                  <div>
                    <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                      选择接口配置
                    </label>
                    <select
                      value={debugConfigId}
                      onChange={(e) => setDebugConfigId(e.target.value)}
                      className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                    >
                      <option value="">请选择接口配置</option>
                      {interfaceConfigs.map((config) => (
                        <option key={config.id} value={config.id}>
                          {config.name} ({config.method})
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                      数据库类型
                    </label>
                    <select
                      value={debugDatabaseType}
                      onChange={(e) => setDebugDatabaseType(e.target.value as DatabaseType)}
                      className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                    >
                      {(['ultrasound', 'radiology', 'endoscopy'] as DatabaseType[]).map((type) => (
                        <option key={type} value={type}>
                          {getDatabaseTypeLabel(type)}数据库
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                      事件码
                    </label>
                    <select
                      value={debugEventType}
                      onChange={(e) => setDebugEventType(e.target.value)}
                      className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                    >
                      {eventCodes.filter(ec => ec.enabled).map((ec) => (
                        <option key={ec.code} value={ec.code}>
                          {ec.code} {ec.description ? `(${ec.description})` : ''}
                        </option>
                      ))}
                      {eventCodes.filter(ec => ec.enabled).length === 0 && (
                        <option value="">暂无可用事件码</option>
                      )}
                    </select>
                  </div>

                  <div>
                    <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                      事件ID
                    </label>
                    <input
                      type="text"
                      value={debugEventId}
                      onChange={(e) => setDebugEventId(e.target.value)}
                      className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                      placeholder="输入事件ID或留空随机获取"
                    />
                  </div>

                  <div>
                    <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                      选择处理器
                    </label>
                    <select
                      value={debugProcessorId}
                      onChange={(e) => setDebugProcessorId(e.target.value)}
                      className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                    >
                      <option value="">请选择处理器</option>
                      {processorsList.map((processor) => (
                        <option key={processor.id} value={processor.id}>
                          {processor.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <button
                    onClick={runDebug}
                    disabled={!debugConfigId || !debugProcessorId || isDebugging}
                    className={`w-full rounded-md px-6 py-2 text-sm font-medium transition-colors ${(!debugConfigId || !debugProcessorId || isDebugging)
                      ? 'bg-gray-400 text-white cursor-not-allowed'
                      : 'bg-blue-600 text-white hover:bg-blue-700'
                      }`}
                  >
                    {isDebugging ? (
                      <>
                        <i className="fa-solid fa-spinner fa-spin mr-1"></i> 运行中...
                      </>
                    ) : (
                      <>
                        <i className="fa-solid fa-play mr-1"></i> 运行调试
                      </>
                    )}
                  </button>

                  {debugLog.length > 0 && (
                    <button
                      onClick={() => setDebugLog([])}
                      className="w-full rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
                    >
                      <i className="fa-solid fa-trash mr-1"></i> 清空日志
                    </button>
                  )}
                </div>
              </div>

              {/* 调试结果 */}
              <div className="lg:col-span-2">
                <div className="rounded-lg border border-gray-300 bg-gray-50 p-4 text-xs text-gray-500 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-400 mb-4">
                  <div className="font-medium mb-1">调试说明:</div>
                  <ul className="list-disc pl-5 space-y-1">
                    <li>选择接口配置、数据库类型、事件码和处理器</li>
                    <li>可输入事件ID进行精确筛选，留空则根据事件码随机获取</li>
                    <li>点击"运行调试"：先执行处理器，再用处理后的数据发送接口请求</li>
                    <li>下方将显示处理器执行日志、请求详情和响应结果</li>
                  </ul>
                </div>

                {/* 调试日志输出区域 */}
                <div className="rounded-lg border border-gray-300 bg-gray-50 p-4 dark:border-gray-700 dark:bg-gray-900">
                  <div className="flex items-center justify-between mb-2">
                    <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300">
                      调试日志输出
                    </h4>
                    {debugResult && (
                      <div className="flex gap-4 text-xs text-gray-500">
                        <span>处理器耗时: {debugResult.processorExecutionTime}ms</span>
                        <span>总耗时: {debugResult.executionTimeMs}ms</span>
                      </div>
                    )}
                  </div>

                  <div className="h-96 overflow-auto rounded-lg bg-gray-900 p-4 text-xs font-mono">
                    {debugLog.length === 0 ? (
                      <div className="flex h-full items-center justify-center text-gray-500">
                        <div className="text-center">
                          <i className="fa-solid fa-bug text-2xl mb-2"></i>
                          <p>准备就绪，请点击"运行调试"开始</p>
                        </div>
                      </div>
                    ) : (
                      <div className="space-y-1">
                        {debugLog.map((log, index) => {
                          const isError = log.includes('❌') || log.includes('[ERROR]');
                          const isWarn = log.includes('⚠️') || log.includes('[WARNING]');
                          const isSuccess = log.includes('✅');
                          const isOutput = log.includes('📤') || log.includes('[OUTPUT]');
                          const isInfo = log.includes('========================================');

                          return (
                            <div
                              key={index}
                              className={`whitespace-pre-wrap break-all ${isError ? 'text-red-400' :
                                isWarn ? 'text-yellow-400' :
                                  isSuccess ? 'text-green-400' :
                                    isOutput ? 'text-blue-400' :
                                      isInfo ? 'text-purple-400 font-bold' :
                                        'text-gray-300'
                                }`}
                            >
                              {log}
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>

                  {/* 结果显示区域 */}
                  {debugResult && (
                    <div className="mt-4 p-3 bg-gray-100 dark:bg-gray-800 rounded-lg">
                      <div className="text-sm font-medium mb-2">执行摘要:</div>
                      <div className="space-y-1 text-xs">
                        <div className="flex items-center">
                          <span className="w-32 text-gray-500">接口URL:</span>
                          <span className="font-mono text-gray-700 dark:text-gray-300">{debugResult.requestUrl}</span>
                        </div>
                        <div className="flex items-center">
                          <span className="w-32 text-gray-500">请求方法:</span>
                          <span className={`font-medium ${debugResult.requestMethod === 'GET' ? 'text-green-600' : 'text-blue-600'}`}>
                            {debugResult.requestMethod}
                          </span>
                        </div>
                        <div className="flex items-center">
                          <span className="w-32 text-gray-500">处理器耗时:</span>
                          <span className="font-medium">{debugResult.processorExecutionTime}ms</span>
                        </div>
                        <div className="flex items-start">
                          <span className="w-32 text-gray-500">请求状态:</span>
                          <span className={debugResult.success ? 'text-green-600 font-medium' : 'text-red-600 font-medium'}>
                            {debugResult.success ? '成功' : `失败 (${debugResult.responseStatus})`}
                          </span>
                        </div>
                        {debugResult.errorMessage && (
                          <div className="flex items-start">
                            <span className="w-32 text-gray-500">错误信息:</span>
                            <span className="text-red-600">{debugResult.errorMessage}</span>
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}