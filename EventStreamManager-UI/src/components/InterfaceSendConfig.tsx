import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import {
  getInterfaceConfigs,
  getInterfaceConfig,
  createInterfaceConfig,
  updateInterfaceConfig,
  deleteInterfaceConfig,
  duplicateInterfaceConfig,
  getUnreferencedProcessors,
  toggleInterfaceConfig,
  GetAvailableProcessors,
  debugInterfaceConfig
} from '@/services/interface.service';

import { getEventCodes,getProcessors } from '@/services/processor.service';
import { getDatabaseTypesWithActiveConfig } from '@/services/database.service';

import { TabNav } from '@/components/ui/TabNav';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { DataTable } from '@/components/ui/DataTable';
import { DebugLogPanel } from '@/components/DebugLogPanel';
import { FormField } from '@/components/ui/FormField';
import { PageLoading } from '@/components/ui/PageLoading';
import { buttonVariants } from '@/utils/button-styles';

// 类型定义
import type {
  InterfaceConfig,
  AvailableProcessor,
  DatabaseTypeWithActiveConfig,
  DebugLogEntry,
  InterfaceDebugResponse
} from '@/types/interface-config';
import type { EventCode } from '@/types/processor';

export default function InterfaceSendConfig() {
  const [activeTab, setActiveTab] = useState<'list' | 'editor' | 'debug'>('list');
  const [selectedConfig, setSelectedConfig] = useState<string | null>(null);
  const [isNewConfig, setIsNewConfig] = useState(false);

  // 状态管理
  const [interfaceConfigs, setInterfaceConfigs] = useState<InterfaceConfig[]>([]);
  const [availableProcessors, setAvailableProcessors] = useState<AvailableProcessor[]>([]);
  const [loading, setLoading] = useState(false);
  const [isDebugging, setIsDebugging] = useState(false);

  // 调试相关状态
  const [debugConfigId, setDebugConfigId] = useState<string>('');
  const [debugProcessorId, setDebugProcessorId] = useState<string>('');
  const [debugDatabaseType, setDebugDatabaseType] = useState<string>('');
  const [debugEventType, setDebugEventType] = useState<string>('');
  const [debugEventId, setDebugEventId] = useState<string>('');
  const [debugLogs, setDebugLogs] = useState<DebugLogEntry[]>([]);
  const [debugResult, setDebugResult] = useState<InterfaceDebugResponse | null>(null);


  // 调试相关数据
  const [eventCodes, setEventCodes] = useState<EventCode[]>([]);
  const [processorsList, setProcessorsList] = useState<AvailableProcessor[]>([]);
  const [databaseTypes, setDatabaseTypes] = useState<DatabaseTypeWithActiveConfig[]>([]);

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
    retryCount: 0,
    retryInterval: 5,
    enabled: false,
    requestTemplate: '${data}',
    description: ''
  });

  // 加载数据
  useEffect(() => {
    loadConfigs();
    loadProcessors('all');
    loadDatabaseTypes();
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

  // 加载数据库类型
  const loadDatabaseTypes = async () => {
    try {
      const data = await getDatabaseTypesWithActiveConfig();
      setDatabaseTypes(data);
      // 默认选择第一个有激活配置的数据库类型
      const firstActive = data.find(dt => dt.activeConfig !== null);
      if (firstActive) {
        setDebugDatabaseType(firstActive.value);
      } else if (data.length > 0) {
        setDebugDatabaseType(data[0].value);
      }
    } catch (error) {
      console.error('加载数据库类型失败', error);
    }
  };

  // 加载调试所需的数据
  const loadDebugData = async () => {
    try {
      const [eventData, processorData] = await Promise.all([
        getEventCodes(),
        getProcessors()
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
  const loadProcessors = async (mode: 'all' | 'unreferenced' = 'all') => {
    try {
      let data;
      if (mode === 'all') {
        data = await GetAvailableProcessors();
      } else {
        data = await getUnreferencedProcessors();
      }
      setAvailableProcessors(data);
    } catch (error) {
      toast.error('加载处理器列表失败');
      console.error(error);
    }
  };


  // 检查数据库类型是否有激活配置
  const hasActiveConfig = (value: string): boolean => {
    const type = databaseTypes.find(dt => dt.value === value);
    return type?.activeConfig !== null;
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
      retryCount: 0,
      retryInterval: 5,
      enabled: false,
      requestTemplate: '${data}',
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

      if (isNewConfig) {
        const unreferencedProcessors = await getUnreferencedProcessors();
        const unreferencedIds = unreferencedProcessors.map(p => p.id);
        
        const invalidProcessors = editingConfig.processorIds.filter(id => !unreferencedIds.includes(id));
        
        if (invalidProcessors.length > 0) {
          const invalidNames = editingConfig.processorNames.filter((_, index) => 
            invalidProcessors.includes(editingConfig.processorIds[index])
          );
          
          toast.error(`以下处理器已被其他配置引用，无法选择：${invalidNames.join('、')}`);
          loadProcessors('unreferenced');
          return;
        }
      }

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
      if (error.message && error.message.includes('已被接口配置引用')) {
        toast.error(error.message);
        loadProcessors(isNewConfig ? 'unreferenced' : 'all');
      } else {
        toast.error(error.message || '保存失败');
      }
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

 
  // 运行调试
  const runDebug = async () => {
    if (!debugConfigId) {
      toast.error('请选择要调试的接口配置');
      return;
    }

    if (!debugProcessorId) {
      toast.error('请选择要执行的处理器');
      return;
    }

    if (!debugDatabaseType) {
      toast.error('请选择数据库类型');
      return;
    }

    if (!hasActiveConfig(debugDatabaseType)) {
      toast.error('所选数据库类型没有激活的配置');
      return;
    }

    setIsDebugging(true);
    setDebugLogs([]);
    setDebugResult(null);

    try {
      const params = {
        interfaceConfigId: debugConfigId,
        processorId: debugProcessorId,
        databaseType: debugDatabaseType,
        eventCode: debugEventType || undefined,
        eventId: debugEventId || undefined
      };

      const result = await debugInterfaceConfig(params);
      
      setDebugLogs(result.logs || []);
      setDebugResult(result);

      if (result.success) {
        toast.success('调试完成');
      } else {
        toast.error(result.errorMessage || '调试失败');
      }
    } catch (error: any) {
      const errorMessage = error instanceof Error ? error.message : '调试执行失败';
      
      const errorLog: DebugLogEntry = {
        type: 'error',
        message: errorMessage,
        timestamp: new Date().toLocaleTimeString()
      };
      
      setDebugLogs([errorLog]);
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

  // 清空调试日志
  const clearDebugLogs = () => {
    setDebugLogs([]);
    setDebugResult(null);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">接口发送配置</h2>

      </div>

      <TabNav
        tabs={[
          { key: 'list', label: '配置列表', icon: 'fa-solid fa-list' },
          { key: 'editor', label: '配置编辑器', icon: 'fa-solid fa-sliders' },
          { key: 'debug', label: '调试', icon: 'fa-solid fa-bug' },
        ]}
        activeKey={activeTab}
        onChange={(key) => setActiveTab(key as 'list' | 'editor' | 'debug')}
      />

      {loading && interfaceConfigs.length === 0 && <PageLoading />}

      {/* 配置列表 */}
      {activeTab === 'list' && (
        <div>
          <div className="mb-4 flex justify-end">
            <button
              onClick={createNewConfig}
              disabled={loading}
              className={buttonVariants.primary + ' px-4 py-2 text-sm flex items-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed'}
            >
              <i className="fa-solid fa-plus"></i> 创建新配置
            </button>
          </div>

          <DataTable
            data={interfaceConfigs}
            columns={[
              {
                key: 'name',
                header: '名称',
                render: (c: InterfaceConfig) => <div className="font-medium">{c.name}</div>
              },
              {
                key: 'processorNames',
                header: '关联处理器',
                render: (c: InterfaceConfig) => (
                  <div className="flex flex-wrap gap-1">
                    {c.processorNames.map((name, index) => (
                      <StatusBadge key={index} variant="default">{name}</StatusBadge>
                    ))}
                  </div>
                )
              },
              {
                key: 'method',
                header: '请求方式',
                render: (c: InterfaceConfig) => (
                  <StatusBadge
                    variant={
                      c.method === 'GET' ? 'success' :
                      c.method === 'POST' ? 'info' :
                      c.method === 'PUT' ? 'warning' : 'danger'
                    }
                  >
                    {c.method}
                  </StatusBadge>
                )
              },
              {
                key: 'url',
                header: 'URL',
                render: (c: InterfaceConfig) => (
                  <div className="text-sm text-gray-500 dark:text-gray-400 truncate max-w-xs" title={c.url}>{c.url}</div>
                )
              },
              {
                key: 'policy',
                header: '策略',
                render: (c: InterfaceConfig) => (
                  <div className="text-xs text-gray-600 dark:text-gray-400">
                    <div>超时 {c.timeout}s</div>
                    <div className="text-gray-400">{c.retryCount > 0 ? `重试 ${c.retryCount} 次` : '不重试'}</div>
                  </div>
                )
              },
              {
                key: 'enabled',
                header: '状态',
                render: (c: InterfaceConfig) => <StatusBadge variant={c.enabled ? 'success' : 'default'}>{c.enabled ? '启用' : '禁用'}</StatusBadge>
              }
            ]}
            keyExtractor={(c: InterfaceConfig) => c.id}
            onRowClick={(c: InterfaceConfig) => editConfig(c.id)}
            rowActions={(c: InterfaceConfig) => (
              <>
                <button
                  onClick={() => toggleConfigStatus(c.id)}
                  disabled={loading}
                  className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300 disabled:opacity-50"
                  title={c.enabled ? '禁用' : '启用'}
                >
                  {c.enabled ? <i className="fa-solid fa-toggle-on text-green-500 text-xl"></i> : <i className="fa-solid fa-toggle-off text-gray-400 text-xl"></i>}
                </button>
                <button
                  onClick={() => editConfig(c.id)}
                  disabled={loading}
                  className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 disabled:opacity-50"
                  title="编辑"
                >
                  <i className="fa-solid fa-edit"></i>
                </button>
                <button
                  onClick={() => duplicateConfig(c.id)}
                  disabled={loading}
                  className="text-yellow-600 hover:text-yellow-800 dark:text-yellow-400 dark:hover:text-yellow-300 disabled:opacity-50"
                  title="复制配置"
                >
                  <i className="fa-solid fa-copy"></i>
                </button>
                <button
                  onClick={() => deleteConfig(c.id)}
                  disabled={loading}
                  className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300 disabled:opacity-50"
                  title="删除"
                >
                  <i className="fa-solid fa-trash"></i>
                </button>
              </>
            )}
            emptyText="暂无接口配置，请创建新的配置"
            emptyIcon="fa-plug"
          />
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
              <FormField label="配置名称" required>
                <input
                  type="text"
                  value={editingConfig.name}
                  onChange={(e) => handleConfigChange('name', e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  placeholder="请输入配置名称"
                  disabled={loading}
                />
              </FormField>

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

              <FormField label="接口URL" required className="sm:col-span-2">
                <input
                  type="url"
                  value={editingConfig.url}
                  onChange={(e) => handleConfigChange('url', e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  placeholder="https://api.example.com/endpoint"
                  disabled={loading}
                />
              </FormField>

              <FormField label="请求方式">
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
              </FormField>

              <FormField label="超时时间 (秒)">
                <input
                  type="number"
                  min="5"
                  max="300"
                  value={editingConfig.timeout}
                  onChange={(e) => handleConfigChange('timeout', Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  disabled={loading}
                />
              </FormField>

              <FormField label="重试次数">
                <input
                  type="number"
                  min="0"
                  max="10"
                  value={editingConfig.retryCount}
                  onChange={(e) => handleConfigChange('retryCount', Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  disabled={loading}
                />
              </FormField>

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
                className={buttonVariants.ghost + ' px-4 py-2 text-sm disabled:opacity-50'}
              >
                取消
              </button>

              <button
                onClick={saveConfig}
                disabled={loading}
                className={buttonVariants.success + ' px-6 py-2 text-sm flex items-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed'}
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
                      onChange={(e) => setDebugDatabaseType(e.target.value)}
                      className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                    >
                      <option value="">请选择数据库类型</option>
                      {databaseTypes.map((type) => (
                        <option 
                          key={type.value} 
                          value={type.value}
                          disabled={!type.activeConfig}
                          className={!type.activeConfig ? 'text-gray-400' : ''}
                        >
                          {type.label} {type.activeConfig ? '' : '(未激活)'}
                        </option>
                      ))}
                    </select>
                    {debugDatabaseType && !hasActiveConfig(debugDatabaseType) && (
                      <p className="mt-1 text-xs text-red-500">
                        当前数据库类型没有激活的配置，无法调试
                      </p>
                    )}
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
                      <option value="">请选择事件码（可选）</option>
                      {eventCodes.filter(ec => ec.enabled).map((ec) => (
                        <option key={ec.code} value={ec.code}>
                          {ec.code} {ec.description ? `(${ec.description})` : ''}
                        </option>
                      ))}
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
                      placeholder="输入事件ID或留空（可选）"
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
                    disabled={!debugConfigId || !debugProcessorId || !debugDatabaseType || !hasActiveConfig(debugDatabaseType) || isDebugging}
                    className={buttonVariants.primary + ' w-full px-6 py-2 text-sm flex items-center justify-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed'}
                  >
                    {isDebugging ? (
                      <>
                        <i className="fa-solid fa-spinner fa-spin"></i> 运行中...
                      </>
                    ) : (
                      <>
                        <i className="fa-solid fa-play"></i> 运行调试
                      </>
                    )}
                  </button>

                  {debugLogs.length > 0 && (
                    <button
                      onClick={clearDebugLogs}
                      className={buttonVariants.ghost + ' w-full px-4 py-2 text-sm flex items-center justify-center gap-1'}
                    >
                      <i className="fa-solid fa-trash"></i> 清空日志
                    </button>
                  )}
                </div>
              </div>

              {/* 调试结果 */}
              <div className="lg:col-span-2">
                <div className="rounded-lg border border-gray-300 bg-gray-50 p-4 text-xs text-gray-500 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-400 mb-4">
                  <div className="font-medium mb-1">调试说明:</div>
                  <ul className="list-disc pl-5 space-y-1">
                    <li>选择接口配置、数据库类型（需要有激活配置）、事件码和处理器</li>
                    <li>可输入事件ID进行精确筛选，留空则自动获取</li>
                    <li>点击"运行调试"执行完整调试流程</li>
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
                        <span>处理器耗时: {debugResult.processorExecutionTime || 0}ms</span>
                        <span>接口耗时: {debugResult.interfaceExecutionTime || 0}ms</span>
                        <span>总耗时: {debugResult.executionTimeMs}ms</span>
                      </div>
                    )}
                  </div>

                  <DebugLogPanel logs={debugLogs} emptyHint="准备就绪，请点击运行调试开始" className="h-96 text-xs" />

                  {/* 结果显示区域 */}
                  {debugResult && (
                    <div className="mt-4 space-y-3">
                      {/* 处理器结果 */}
                      {debugResult.processorResult && (
                        <div className="p-3 bg-gray-100 dark:bg-gray-800 rounded-lg">
                          <div className="text-sm font-medium mb-2">处理器执行结果:</div>
                          <div className="space-y-1 text-xs">
                            <div className="flex items-center">
                              <span className="w-32 text-gray-500">需要发送:</span>
                              <span className={debugResult.processorResult.needToSend ? 'text-green-600 font-medium' : 'text-yellow-600 font-medium'}>
                                {debugResult.processorResult.needToSend ? '是' : '否'}
                              </span>
                            </div>
                            {debugResult.processorResult.reason && (
                              <div className="flex items-start">
                                <span className="w-32 text-gray-500">原因:</span>
                                <span className="text-gray-700 dark:text-gray-300">{debugResult.processorResult.reason}</span>
                              </div>
                            )}
                            {debugResult.processorResult.data && (
                              <div className="mt-2">
                                <span className="text-gray-500">数据:</span>
                                <pre className="mt-1 p-2 bg-gray-800 text-gray-200 rounded text-xs overflow-auto">
                                  {JSON.stringify(debugResult.processorResult.data, null, 2)}
                                </pre>
                              </div>
                            )}
                          </div>
                        </div>
                      )}

                      {/* 请求信息 */}
                      {debugResult.requestInfo && (
                        <div className="p-3 bg-gray-100 dark:bg-gray-800 rounded-lg">
                          <div className="text-sm font-medium mb-2">请求信息:</div>
                          <div className="space-y-1 text-xs">
                            <div className="flex items-center">
                              <span className="w-24 text-gray-500">URL:</span>
                              <span className="font-mono text-gray-700 dark:text-gray-300 break-all">{debugResult.requestInfo.url}</span>
                            </div>
                            <div className="flex items-center">
                              <span className="w-24 text-gray-500">方法:</span>
                              <span className={`font-medium ${
                                debugResult.requestInfo.method === 'GET' ? 'text-green-600' : 'text-blue-600'
                              }`}>
                                {debugResult.requestInfo.method}
                              </span>
                            </div>
                            <div className="flex items-start">
                              <span className="w-24 text-gray-500">请求头:</span>
                              <div className="flex-1">
                                <pre className="text-xs text-gray-700 dark:text-gray-300">
                                  {JSON.stringify(debugResult.requestInfo.headers, null, 2)}
                                </pre>
                              </div>
                            </div>
                            {debugResult.requestInfo.body && (
                              <div className="flex items-start">
                                <span className="w-24 text-gray-500">请求体:</span>
                                <pre className="flex-1 p-2 bg-gray-800 text-gray-200 rounded text-xs overflow-auto">
                                  {debugResult.requestInfo.body}
                                </pre>
                              </div>
                            )}
                          </div>
                        </div>
                      )}

                      {/* 响应信息 */}
                      {debugResult.responseInfo && (
                        <div className="p-3 bg-gray-100 dark:bg-gray-800 rounded-lg">
                          <div className="text-sm font-medium mb-2">响应信息:</div>
                          <div className="space-y-1 text-xs">
                            <div className="flex items-center">
                              <span className="w-24 text-gray-500">状态码:</span>
                              <span className={debugResult.responseInfo.statusCode >= 200 && debugResult.responseInfo.statusCode < 300
                                ? 'text-green-600 font-medium'
                                : 'text-red-600 font-medium'
                              }>
                                {debugResult.responseInfo.statusCode}
                              </span>
                            </div>
                            {debugResult.responseInfo.body && (
                              <div className="flex items-start">
                                <span className="w-24 text-gray-500">响应体:</span>
                                <pre className="flex-1 p-2 bg-gray-800 text-gray-200 rounded text-xs overflow-auto">
                                  {debugResult.responseInfo.body}
                                </pre>
                              </div>
                            )}
                          </div>
                        </div>
                      )}

                      {/* 错误信息 */}
                      {debugResult.errorMessage && (
                        <div className="p-3 bg-red-50 dark:bg-red-900/20 rounded-lg">
                          <div className="text-sm font-medium text-red-600 dark:text-red-400 mb-1">错误信息:</div>
                          <div className="text-xs text-red-600 dark:text-red-400">{debugResult.errorMessage}</div>
                        </div>
                      )}
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