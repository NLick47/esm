import { useState, useEffect, useCallback } from 'react';
import { toast } from 'sonner';
import {
  getPipelines,
  getPipeline,
  createPipeline,
  updatePipeline,
  deletePipeline,
  togglePipeline,
} from '@/services/pipeline.service';
import { getEventCodes, getProcessors } from '@/services/processor.service';
import { getDatabaseTypesWithActiveConfig } from '@/services/database.service';

import { TabNav } from '@/components/ui/TabNav';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { DataTable } from '@/components/ui/DataTable';
import { FormField } from '@/components/ui/FormField';
import { PageLoading } from '@/components/ui/PageLoading';
import { buttonVariants } from '@/utils/button-styles';

import type { ProcessorPipeline, PipelineStage, StageFailureAction } from '@/types/pipeline';
import type { EventCode } from '@/types/processor';
import type { DatabaseTypeWithActiveConfig } from '@/types/interface-config';

const FAILURE_ACTION_OPTIONS: { value: StageFailureAction; label: string; description: string }[] = [
  { value: 'Stop', label: '停止', description: '中断整个 Pipeline 执行' },
  { value: 'Continue', label: '继续', description: '忽略失败，继续执行下一个 Stage' },
  { value: 'SkipToSender', label: '跳转到发送', description: '跳过中间 Stage，直接执行 Sender' },
];

export default function PipelineManager() {
  const [activeTab, setActiveTab] = useState<'list' | 'editor'>('list');
  const [selectedPipelineId, setSelectedPipelineId] = useState<string | null>(null);
  const [isNewPipeline, setIsNewPipeline] = useState(false);

  const [pipelines, setPipelines] = useState<ProcessorPipeline[]>([]);
  const [eventCodes, setEventCodes] = useState<EventCode[]>([]);
  const [databaseTypes, setDatabaseTypes] = useState<DatabaseTypeWithActiveConfig[]>([]);
  const [processors, setProcessors] = useState<{ id: string; name: string; enabled: boolean }[]>([]);
  const [loading, setLoading] = useState(false);

  const [editingPipeline, setEditingPipeline] = useState<ProcessorPipeline>({
    id: '',
    name: '',
    eventCodes: [],
    databaseTypes: [],
    stages: [],
    enabled: false,
    maxRetryCount: 1,
  });

  // 加载数据
  useEffect(() => {
    loadPipelines();
    loadReferenceData();
  }, []);

  // 切换回列表时刷新
  useEffect(() => {
    if (activeTab === 'list') {
      loadPipelines();
    }
  }, [activeTab]);

  const loadPipelines = async () => {
    try {
      setLoading(true);
      const data = await getPipelines();
      setPipelines(data);
    } catch (error) {
      toast.error('加载 Pipeline 列表失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const loadReferenceData = async () => {
    try {
      const [eventData, dbData, procData] = await Promise.all([
        getEventCodes(),
        getDatabaseTypesWithActiveConfig(),
        getProcessors(),
      ]);
      setEventCodes(eventData);
      setDatabaseTypes(dbData);
      setProcessors(procData.map(p => ({ id: p.id, name: p.name, enabled: p.enabled })));
    } catch (error) {
      console.error('加载引用数据失败', error);
    }
  };

  const createNewPipeline = () => {
    setEditingPipeline({
      id: '',
      name: '',
      eventCodes: [],
      databaseTypes: [],
      stages: [],
      enabled: false,
      maxRetryCount: 1,
    });
    setSelectedPipelineId(null);
    setIsNewPipeline(true);
    setActiveTab('editor');
  };

  const editPipeline = async (id: string) => {
    try {
      setLoading(true);
      const pipeline = await getPipeline(id);
      // 确保 stages 按 sortOrder 排序
      pipeline.stages = [...pipeline.stages].sort((a, b) => a.sortOrder - b.sortOrder);
      setEditingPipeline(pipeline);
      setSelectedPipelineId(id);
      setIsNewPipeline(false);
      setActiveTab('editor');
    } catch (error) {
      toast.error('加载 Pipeline 详情失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleDeletePipeline = async (id: string) => {
    if (!window.confirm('确定要删除这个 Pipeline 配置吗？')) return;
    try {
      setLoading(true);
      await deletePipeline(id);
      setPipelines(prev => prev.filter(p => p.id !== id));
      if (selectedPipelineId === id) {
        setSelectedPipelineId(null);
        setActiveTab('list');
      }
      toast.success('Pipeline 已删除');
    } catch (error) {
      toast.error('删除 Pipeline 失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleTogglePipeline = async (id: string) => {
    try {
      setLoading(true);
      await togglePipeline(id);
      setPipelines(prev =>
        prev.map(p => (p.id === id ? { ...p, enabled: !p.enabled } : p))
      );
      toast.success('Pipeline 状态已更新');
    } catch (error) {
      toast.error('更新状态失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const savePipeline = async () => {
    if (!editingPipeline.name.trim()) {
      toast.error('Pipeline 名称不能为空');
      return;
    }
    if (editingPipeline.eventCodes.length === 0) {
      toast.error('请至少选择一个事件码');
      return;
    }
    if (editingPipeline.databaseTypes.length === 0) {
      toast.error('请至少选择一个数据库类型');
      return;
    }
    if (editingPipeline.stages.length === 0) {
      toast.error('请至少配置一个 Stage');
      return;
    }
    // 检查每个 stage 是否选择了 processor
    for (let i = 0; i < editingPipeline.stages.length; i++) {
      if (!editingPipeline.stages[i].processorId) {
        toast.error(`第 ${i + 1} 个 Stage 未选择处理器`);
        return;
      }
    }
    // 检查是否只有一个 Sender
    const senderCount = editingPipeline.stages.filter(s => s.isSender).length;
    if (senderCount === 0) {
      toast.error('请至少标记一个 Stage 为发送器（Sender）');
      return;
    }

    try {
      setLoading(true);
      // 重新分配 sortOrder
      const payload = {
        ...editingPipeline,
        stages: editingPipeline.stages.map((s, index) => ({
          ...s,
          sortOrder: index,
        })),
      };

      if (isNewPipeline) {
        const created = await createPipeline(payload);
        setPipelines(prev => [...prev, created]);
        toast.success('Pipeline 已创建');
      } else {
        await updatePipeline(selectedPipelineId!, payload);
        setPipelines(prev =>
          prev.map(p => (p.id === selectedPipelineId ? { ...payload, id: selectedPipelineId! } as ProcessorPipeline : p))
        );
        toast.success('Pipeline 已更新');
      }
      setActiveTab('list');
    } catch (error: any) {
      toast.error(error.message || '保存失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const cancelEdit = () => {
    setActiveTab('list');
    setSelectedPipelineId(null);
  };

  // Pipeline 字段变更
  const handlePipelineChange = useCallback(<K extends keyof ProcessorPipeline>(field: K, value: ProcessorPipeline[K]) => {
    setEditingPipeline(prev => ({ ...prev, [field]: value }));
  }, []);

  // Stage 操作
  const addStage = () => {
    const newStage: PipelineStage = {
      processorId: '',
      processorName: '',
      sortOrder: editingPipeline.stages.length,
      isSender: false,
      onFailure: 'Stop',
    };
    setEditingPipeline(prev => ({
      ...prev,
      stages: [...prev.stages, newStage],
    }));
  };

  const removeStage = (index: number) => {
    setEditingPipeline(prev => ({
      ...prev,
      stages: prev.stages.filter((_, i) => i !== index),
    }));
  };

  const moveStage = (index: number, direction: 'up' | 'down') => {
    if (direction === 'up' && index === 0) return;
    if (direction === 'down' && index === editingPipeline.stages.length - 1) return;

    const newStages = [...editingPipeline.stages];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    [newStages[index], newStages[targetIndex]] = [newStages[targetIndex], newStages[index]];
    setEditingPipeline(prev => ({ ...prev, stages: newStages }));
  };

  const updateStage = (index: number, updates: Partial<PipelineStage>) => {
    setEditingPipeline(prev => ({
      ...prev,
      stages: prev.stages.map((s, i) => (i === index ? { ...s, ...updates } : s)),
    }));
  };

  const handleProcessorChange = (index: number, processorId: string) => {
    const processor = processors.find(p => p.id === processorId);
    updateStage(index, {
      processorId,
      processorName: processor?.name || '',
    });
  };

  // 多选切换辅助函数
  const toggleArrayItem = (arr: string[], item: string): string[] => {
    const idx = arr.indexOf(item);
    if (idx > -1) {
      return arr.filter((_, i) => i !== idx);
    }
    return [...arr, item];
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Pipeline 配置</h2>
      </div>

      <TabNav
        tabs={[
          { key: 'list', label: '配置列表', icon: 'fa-solid fa-list' },
          { key: 'editor', label: '配置编辑器', icon: 'fa-solid fa-sliders' },
        ]}
        activeKey={activeTab}
        onChange={(key) => setActiveTab(key as 'list' | 'editor')}
      />

      {loading && pipelines.length === 0 && <PageLoading />}

      {/* 配置列表 */}
      {activeTab === 'list' && (
        <div>
          <div className="mb-4 flex justify-end">
            <button
              onClick={createNewPipeline}
              disabled={loading}
              className={buttonVariants.primary + ' px-4 py-2 text-sm flex items-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed'}
            >
              <i className="fa-solid fa-plus"></i> 创建新 Pipeline
            </button>
          </div>

          <DataTable
            data={pipelines}
            columns={[
              {
                key: 'name',
                header: '名称',
                render: (p: ProcessorPipeline) => <div className="font-medium">{p.name}</div>,
              },
              {
                key: 'eventCodes',
                header: '事件码',
                render: (p: ProcessorPipeline) => (
                  <div className="flex flex-wrap gap-1 max-w-xs">
                    {p.eventCodes.map((code, i) => (
                      <StatusBadge key={i} variant="info" size="sm">{code}</StatusBadge>
                    ))}
                  </div>
                ),
              },
              {
                key: 'databaseTypes',
                header: '数据库类型',
                render: (p: ProcessorPipeline) => (
                  <div className="flex flex-wrap gap-1">
                    {p.databaseTypes.map((dt, i) => (
                      <StatusBadge key={i} variant="default" size="sm">{dt}</StatusBadge>
                    ))}
                  </div>
                ),
              },
              {
                key: 'stages',
                header: 'Stages',
                render: (p: ProcessorPipeline) => (
                  <div className="text-sm">
                    <span className="font-medium">{p.stages.length}</span> 个阶段
                    <span className="text-gray-400 mx-1">|</span>
                    <span className="text-green-600">{p.stages.filter(s => s.isSender).length}</span> 个发送器
                  </div>
                ),
              },
              {
                key: 'maxRetryCount',
                header: '重试',
                render: (p: ProcessorPipeline) => (
                  <span className="text-sm text-gray-600 dark:text-gray-400">{p.maxRetryCount} 次</span>
                ),
              },
              {
                key: 'enabled',
                header: '状态',
                render: (p: ProcessorPipeline) => (
                  <StatusBadge variant={p.enabled ? 'success' : 'default'}>
                    {p.enabled ? '启用' : '禁用'}
                  </StatusBadge>
                ),
              },
            ]}
            keyExtractor={(p: ProcessorPipeline) => p.id}
            onRowClick={(p: ProcessorPipeline) => editPipeline(p.id)}
            rowActions={(p: ProcessorPipeline) => (
              <>
                <button
                  onClick={() => handleTogglePipeline(p.id)}
                  disabled={loading}
                  className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300 disabled:opacity-50"
                  title={p.enabled ? '禁用' : '启用'}
                >
                  {p.enabled ? (
                    <i className="fa-solid fa-toggle-on text-green-500 text-xl"></i>
                  ) : (
                    <i className="fa-solid fa-toggle-off text-gray-400 text-xl"></i>
                  )}
                </button>
                <button
                  onClick={() => editPipeline(p.id)}
                  disabled={loading}
                  className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 disabled:opacity-50"
                  title="编辑"
                >
                  <i className="fa-solid fa-edit"></i>
                </button>
                <button
                  onClick={() => handleDeletePipeline(p.id)}
                  disabled={loading}
                  className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300 disabled:opacity-50"
                  title="删除"
                >
                  <i className="fa-solid fa-trash"></i>
                </button>
              </>
            )}
            emptyText="暂无 Pipeline 配置，请创建新的配置"
            emptyIcon="fa-layer-group"
          />
        </div>
      )}

      {/* 配置编辑器 */}
      {activeTab === 'editor' && (
        <div className="space-y-6">
          {/* 基本信息 */}
          <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
            <h3 className="mb-6 text-lg font-semibold">
              {isNewPipeline ? '创建新 Pipeline' : '编辑 Pipeline'}
            </h3>

            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
              <FormField label="Pipeline 名称" required>
                <input
                  type="text"
                  value={editingPipeline.name}
                  onChange={(e) => handlePipelineChange('name', e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  placeholder="请输入 Pipeline 名称"
                  disabled={loading}
                />
              </FormField>

              <FormField label="最大重试次数">
                <input
                  type="number"
                  min={0}
                  max={10}
                  value={editingPipeline.maxRetryCount}
                  onChange={(e) => handlePipelineChange('maxRetryCount', Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  disabled={loading}
                />
              </FormField>

              {/* 事件码多选 */}
              <div className="sm:col-span-2">
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  事件码 <span className="text-red-500">*</span>
                </label>
                <div className="flex flex-wrap gap-2 max-h-32 overflow-y-auto pr-2 border rounded-lg p-3 dark:border-gray-700">
                  {eventCodes.map((ec) => {
                    const isSelected = editingPipeline.eventCodes.includes(ec.code);
                    return (
                      <label
                        key={ec.code}
                        className={`inline-flex items-center rounded-full border px-3 py-1.5 text-sm font-medium transition-colors cursor-pointer ${
                          isSelected
                            ? 'border-blue-600 bg-blue-50 text-blue-700 dark:border-blue-500 dark:bg-blue-900/30 dark:text-blue-400'
                            : 'border-gray-300 bg-white text-gray-700 hover:border-blue-300 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:border-blue-700'
                        } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
                        onClick={() => {
                          if (loading) return;
                          handlePipelineChange('eventCodes', toggleArrayItem(editingPipeline.eventCodes, ec.code));
                        }}
                      >
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => {}}
                          className="mr-2 h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700"
                          disabled={loading}
                        />
                        <span>{ec.code}</span>
                        {ec.description && (
                          <span className="ml-1 text-xs text-gray-400">({ec.description})</span>
                        )}
                      </label>
                    );
                  })}
                </div>
              </div>

              {/* 数据库类型多选 */}
              <div className="sm:col-span-2">
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  数据库类型 <span className="text-red-500">*</span>
                </label>
                <div className="flex flex-wrap gap-2 border rounded-lg p-3 dark:border-gray-700">
                  {databaseTypes.map((dt) => {
                    const isSelected = editingPipeline.databaseTypes.includes(dt.value);
                    return (
                      <label
                        key={dt.value}
                        className={`inline-flex items-center rounded-full border px-3 py-1.5 text-sm font-medium transition-colors cursor-pointer ${
                          isSelected
                            ? 'border-blue-600 bg-blue-50 text-blue-700 dark:border-blue-500 dark:bg-blue-900/30 dark:text-blue-400'
                            : 'border-gray-300 bg-white text-gray-700 hover:border-blue-300 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:border-blue-700'
                        } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
                        onClick={() => {
                          if (loading) return;
                          handlePipelineChange('databaseTypes', toggleArrayItem(editingPipeline.databaseTypes, dt.value));
                        }}
                      >
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => {}}
                          className="mr-2 h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700"
                          disabled={loading}
                        />
                        <span>{dt.label}</span>
                      </label>
                    );
                  })}
                </div>
              </div>

              <div className="sm:col-span-2">
                <div className="flex items-center gap-2">
                  <input
                    id="pipeline-enabled"
                    type="checkbox"
                    checked={editingPipeline.enabled}
                    onChange={(e) => handlePipelineChange('enabled', e.target.checked)}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700"
                    disabled={loading}
                  />
                  <label htmlFor="pipeline-enabled" className="text-sm font-medium text-gray-700 dark:text-gray-300">
                    启用此 Pipeline
                  </label>
                </div>
              </div>
            </div>
          </div>

          {/* Stage 配置 */}
          <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-lg font-semibold">Stage 配置</h3>
              <button
                onClick={addStage}
                disabled={loading}
                className={buttonVariants.primary + ' px-3 py-1.5 text-sm flex items-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed'}
              >
                <i className="fa-solid fa-plus"></i> 添加 Stage
              </button>
            </div>

            {editingPipeline.stages.length === 0 ? (
              <div className="text-center py-10 text-gray-500 dark:text-gray-400 border-2 border-dashed border-gray-200 dark:border-gray-700 rounded-lg">
                <i className="fa-solid fa-layer-group text-3xl mb-2 text-gray-300 dark:text-gray-600"></i>
                <p>暂无 Stage，请点击上方按钮添加</p>
              </div>
            ) : (
              <div className="space-y-4">
                {editingPipeline.stages.map((stage, index) => (
                  <div
                    key={index}
                    className="rounded-lg border border-gray-200 bg-gray-50 p-4 dark:border-gray-700 dark:bg-gray-800/50"
                  >
                    <div className="flex items-start gap-4">
                      {/* 顺序控制 */}
                      <div className="flex flex-col items-center gap-1 pt-1">
                        <span className="text-xs font-bold text-gray-400 w-6 text-center">{index + 1}</span>
                        <button
                          onClick={() => moveStage(index, 'up')}
                          disabled={index === 0 || loading}
                          className="text-gray-400 hover:text-blue-600 disabled:opacity-30 disabled:hover:text-gray-400 dark:hover:text-blue-400"
                          title="上移"
                        >
                          <i className="fa-solid fa-chevron-up"></i>
                        </button>
                        <button
                          onClick={() => moveStage(index, 'down')}
                          disabled={index === editingPipeline.stages.length - 1 || loading}
                          className="text-gray-400 hover:text-blue-600 disabled:opacity-30 disabled:hover:text-gray-400 dark:hover:text-blue-400"
                          title="下移"
                        >
                          <i className="fa-solid fa-chevron-down"></i>
                        </button>
                      </div>

                      {/* Stage 内容 */}
                      <div className="flex-1 grid grid-cols-1 gap-4 sm:grid-cols-3">
                        {/* Processor 选择 */}
                        <div className="sm:col-span-1">
                          <label className="block mb-1.5 text-sm font-medium text-gray-700 dark:text-gray-300">
                            处理器 <span className="text-red-500">*</span>
                          </label>
                          <select
                            value={stage.processorId}
                            onChange={(e) => handleProcessorChange(index, e.target.value)}
                            className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                            disabled={loading}
                          >
                            <option value="">请选择处理器</option>
                            {processors.map((proc) => (
                              <option key={proc.id} value={proc.id} disabled={!proc.enabled}>
                                {proc.name} {!proc.enabled ? '(已禁用)' : ''}
                              </option>
                            ))}
                          </select>
                        </div>

                        {/* 失败策略 */}
                        <div className="sm:col-span-1">
                          <label className="block mb-1.5 text-sm font-medium text-gray-700 dark:text-gray-300">
                            失败策略
                          </label>
                          <select
                            value={stage.onFailure}
                            onChange={(e) => updateStage(index, { onFailure: e.target.value as StageFailureAction })}
                            className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                            disabled={loading}
                          >
                            {FAILURE_ACTION_OPTIONS.map((opt) => (
                              <option key={opt.value} value={opt.value}>
                                {opt.label}
                              </option>
                            ))}
                          </select>
                          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                            {FAILURE_ACTION_OPTIONS.find(o => o.value === stage.onFailure)?.description}
                          </p>
                        </div>

                        {/* 选项 */}
                        <div className="sm:col-span-1 flex items-center gap-4 pt-6">
                          <label className="inline-flex items-center gap-2 cursor-pointer">
                            <input
                              type="checkbox"
                              checked={stage.isSender}
                              onChange={(e) => updateStage(index, { isSender: e.target.checked })}
                              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700"
                              disabled={loading}
                            />
                            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                              发送器
                            </span>
                            <span
                              className="text-gray-400 cursor-help"
                              title="标记为发送器的 Stage 会触发 HTTP 接口发送"
                            >
                              <i className="fa-solid fa-circle-question text-xs"></i>
                            </span>
                          </label>
                        </div>
                      </div>

                      {/* 删除按钮 */}
                      <button
                        onClick={() => removeStage(index)}
                        disabled={loading}
                        className="mt-1 flex h-8 w-8 items-center justify-center rounded-lg text-gray-400 transition-colors hover:bg-red-50 hover:text-red-500 dark:hover:bg-red-900/20 dark:hover:text-red-400 disabled:opacity-50"
                        title="删除 Stage"
                      >
                        <i className="fa-solid fa-trash-can"></i>
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* 操作按钮 */}
          <div className="flex justify-end gap-3">
            <button
              onClick={cancelEdit}
              disabled={loading}
              className={buttonVariants.ghost + ' px-4 py-2 text-sm disabled:opacity-50'}
            >
              取消
            </button>
            <button
              onClick={savePipeline}
              disabled={loading}
              className={buttonVariants.success + ' px-6 py-2 text-sm flex items-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed'}
            >
              {loading ? '保存中...' : '保存配置'}
            </button>
          </div>

          {/* 配置说明 */}
          <div className="rounded-xl bg-blue-50 p-4 text-sm text-blue-700 dark:bg-blue-900/20 dark:text-blue-400">
            <div className="flex items-start">
              <i className="fa-solid fa-circle-info mt-0.5 mr-2"></i>
              <div>
                <p className="mb-1 font-medium">Pipeline 说明:</p>
                <ul className="list-disc pl-5 space-y-1">
                  <li>Pipeline 按 Stage 顺序执行，阶段间通过 SharedContext 共享数据</li>
                  <li>只有标记为「发送器」的 Stage 才会触发 HTTP 接口发送</li>
                  <li>失败策略：停止（中断Pipeline）、继续（忽略失败）、跳转到发送（跳过中间阶段）</li>
                  <li>同一事件码+数据库类型组合，Pipeline 优先于独立 Processor 执行</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
