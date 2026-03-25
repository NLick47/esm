import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import CodeMirror from '@uiw/react-codemirror';
import { javascript } from '@codemirror/lang-javascript';
import { sql } from '@codemirror/lang-sql';
import { oneDark } from '@codemirror/theme-one-dark';
import prettier from 'prettier';
import parserBabel from 'prettier/plugins/babel';
import prettierPluginEstree from 'prettier/plugins/estree';
import { createPortal } from 'react-dom';
import {
  getProcessors,
  getProcessor, 
  getEventCodes,
  getSystemTemplates,
  getCustomTemplates,
  getDefaultTemplate,
  validateCode,
  toggleProcessor as toggleProcessorService,
  deleteProcessor as deleteProcessorService,
  createProcessor as createProcessorService,
  updateProcessor as updateProcessorService,
  createCustomTemplate as createCustomTemplateService,
  updateCustomTemplate as updateCustomTemplateService,
  deleteCustomTemplate as deleteCustomTemplateService,
  executeDebug,
  executeExamineDebug,

} from '@/services/processor.service';

import { 
  JSProcessor,
  JSProcessorListResponse,
  JSProcessorDetailResponse,
  CustomSqlTemplate, 
  SystemSqlTemplate,
  SqlTemplateType,
  EventCode 
} from '@/types/processor';

import {
  DatabaseTypeWithActiveConfig
} from '@/types/database';

import{getDatabaseTypesWithActiveConfig}
from '@/services/database.service'




export default function JSProcessorManager() {
  // 状态定义
  const [processors, setProcessors] = useState<JSProcessorListResponse[]>([]);  
  const [eventCodes, setEventCodes] = useState<EventCode[]>([]);
  const [systemTemplates, setSystemTemplates] = useState<SystemSqlTemplate[]>([]);
  const [customTemplates, setCustomTemplates] = useState<CustomSqlTemplate[]>([]);
  const [databaseTypes, setDatabaseTypes] = useState<DatabaseTypeWithActiveConfig[]>([]);
  const [loading, setLoading] = useState(true);
  const [defaultTemplate, setDefaultTemplate] = useState<string>('');
  const [editingTemplate, setEditingTemplate] = useState<{
    id: string;
    name: string;
    sql: string;
  } | null>(null);
  const [activeTab, setActiveTab] = useState<'list' | 'editor' | 'debug'>('list');
  const [selectedProcessor, setSelectedProcessor] = useState<string | null>(null);
  const [isNewProcessor, setIsNewProcessor] = useState(false);
  const [debugEventId, setDebugEventId] = useState<string>('');
  const [debugEventType, setDebugEventType] = useState<string>('Examine');
  const [debugDatabaseType, setDebugDatabaseType] = useState<string>('');
  const [debugLog, setDebugLog] = useState<string[]>([]);
  const [isDebugging, setIsDebugging] = useState<boolean>(false);

  // 编辑器调试状态
  const [showEditorDebug, setShowEditorDebug] = useState(true);
  const [editorDebugExamineId, setEditorDebugExamineId] = useState('');
  const [editorDebugLog, setEditorDebugLog] = useState<string[]>([]);
  const [editorDebugResult, setEditorDebugResult] = useState<any>(null);
  const [isEditorDebugging, setIsEditorDebugging] = useState(false);

  const [selectedSystemTemplateId, setSelectedSystemTemplateId] = useState<string | null>(null);
  const [selectedCustomTemplateId, setSelectedCustomTemplateId] = useState<string | null>(null);

  const [debugResult, setDebugResult] = useState<any>(null);

  // 编辑器布局和字体大小状态
  const [editorLayout, setEditorLayout] = useState<'horizontal' | 'vertical'>('horizontal');
  const [editorFontSize, setEditorFontSize] = useState<number>(15);


  interface ValidationResult {
    isValid: boolean;
    message?: string;
    lineNumber?: number;
    column?: number;
    source?: string;
    hasProcessFunction: boolean;
  }

  
  const [editingProcessor, setEditingProcessor] = useState<JSProcessorDetailResponse>({
    id: '',
    name: '',
    databaseTypes: [],
    eventCodes: [],
    sqlTemplateType: SqlTemplateType.System,
    sqlTemplateId: '',
    sqlTemplate: '',
    code: defaultTemplate || '',
    enabled: false,
    description: '',
  });

  // 阻止自动滚动的处理函数
  const handleEditorScroll = (e: React.UIEvent) => {
    e.stopPropagation();
  };

  // 加载所有数据
  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const [procData, eventData, sysData, custData, templateData, typesData] = await Promise.all([
          getProcessors(),
          getEventCodes(),
          getSystemTemplates(),
          getCustomTemplates(),
          getDefaultTemplate(),
          getDatabaseTypesWithActiveConfig()
        ]);

        setProcessors(procData);
        setEventCodes(eventData);
        setSystemTemplates(sysData);
        setCustomTemplates(custData);
        setDefaultTemplate(templateData.code);
        setDatabaseTypes(typesData);

        // 如果有数据库类型，设置第一个为默认调试数据库类型
        if (typesData.length > 0) {
          setDebugDatabaseType(typesData[0].value);
        }

        // 更新editingProcessor的默认代码
        setEditingProcessor(prev => ({
          ...prev,
          code: templateData.code || ''
        }));
      } catch (error) {
        toast.error(error instanceof Error ? error.message : '数据加载失败');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  // 当数据库类型加载完成后，设置默认的数据库类型
  useEffect(() => {
    if (databaseTypes.length > 0 && editingProcessor.databaseTypes.length === 0) {
      setEditingProcessor(prev => ({
        ...prev,
        databaseTypes: [databaseTypes[0].value]
      }));
    }
  }, [databaseTypes]);

  // 获取数据库类型标签
  const getDatabaseTypeLabel = (type: string): string => {
    const found = databaseTypes.find(t => t.value === type);
    return found ? found.label : type;
  };

  // 处理器操作
  const toggleProcessorStatus = async (id: string) => {
    try {
      const updated = await toggleProcessorService(id);
      setProcessors(prev => prev.map(p => p.id === id ? updated : p));
      toast.success('处理器状态已更新');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '操作失败');
    }
  };

  const deleteProcessor = async (id: string) => {
    if (!window.confirm('确定要删除这个处理器吗？')) return;
    try {
      await deleteProcessorService(id);
      setProcessors(prev => prev.filter(p => p.id !== id));
      if (selectedProcessor === id) {
        setSelectedProcessor(null);
        setActiveTab('list');
      }
      toast.success('处理器已删除');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '操作失败');
    }
  };

  const editProcessor = async (id: string) => {
    try {
      const detail = await getProcessor(id);
    
      const updatedProcessor = {
        ...detail,
        code: detail.code || defaultTemplate,
        sqlTemplate: detail.sqlTemplate || '',
      };
      setEditingProcessor(updatedProcessor);
      setSelectedProcessor(id);
      setIsNewProcessor(false);
      setActiveTab('editor');
      if (detail.sqlTemplateType === SqlTemplateType.System) {
        setSelectedSystemTemplateId(detail.sqlTemplateId);
        setSelectedCustomTemplateId(null);
      } else if (detail.sqlTemplateType === SqlTemplateType.Custom) {
        setSelectedSystemTemplateId(null);
        setSelectedCustomTemplateId(detail.sqlTemplateId);
      } else {
        setSelectedSystemTemplateId(null);
        setSelectedCustomTemplateId(null);
      }
  
      if (!detail.code) {
        console.log('处理器代码为空，已使用默认模板');
      }
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '加载处理器详情失败');
    }
  };

  const createNewProcessor = () => {
    setEditingProcessor({
      id: '',
      name: '',
      databaseTypes: databaseTypes.length > 0 ? [databaseTypes[0].value] : [],
      eventCodes: [],
      sqlTemplateType: SqlTemplateType.System,
      sqlTemplateId: '',
      sqlTemplate: '',
      code: defaultTemplate || '',
      enabled: false,
      description: '',
    });
    setSelectedProcessor(null);
    setIsNewProcessor(true);
    setActiveTab('editor');
    setSelectedSystemTemplateId(null);
    setSelectedCustomTemplateId(null);
  };


 const saveProcessor = async () => {
  if (!editingProcessor.name.trim()) {
    toast.error('处理器名称不能为空');
    return;
  }

  const processorToSave: Partial<JSProcessor> = {
    name: editingProcessor.name,
    databaseTypes: editingProcessor.databaseTypes,
    eventCodes: editingProcessor.eventCodes,
    sqlTemplateType: editingProcessor.sqlTemplateType,
    sqlTemplateId: editingProcessor.sqlTemplateId,
    code: editingProcessor.code,
    enabled: editingProcessor.enabled,
    description: editingProcessor.description,
  };

  try {
    if (isNewProcessor || !editingProcessor.id || editingProcessor.id.trim() === '') {
      const newProcessor = await createProcessorService(processorToSave);
      setProcessors(prev => [...prev, {
        id: newProcessor.id,
        name: newProcessor.name,
        databaseTypes: newProcessor.databaseTypes,
        eventCodes: newProcessor.eventCodes,
        sqlTemplateType: newProcessor.sqlTemplateType,
        sqlTemplateId: newProcessor.sqlTemplateId,
        enabled: newProcessor.enabled,
        description: newProcessor.description,
      }]);
      setSelectedProcessor(newProcessor.id);
      setIsNewProcessor(false);
      toast.success('处理器已创建');
    } else {
      await updateProcessorService(editingProcessor.id, processorToSave);
      setProcessors(prev => prev.map(p =>
        p.id === selectedProcessor ? {
          ...p,
          name: editingProcessor.name,
          databaseTypes: editingProcessor.databaseTypes,
          eventCodes: editingProcessor.eventCodes,
          sqlTemplateType: editingProcessor.sqlTemplateType,
          sqlTemplateId: editingProcessor.sqlTemplateId,
          enabled: editingProcessor.enabled,
          description: editingProcessor.description,
        } : p
      ));
      toast.success('处理器已更新');
    }
    setActiveTab('list');
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败');
  }
};

  const cancelEdit = () => {
    setActiveTab('list');
    setSelectedProcessor(null);
  };

  const handleProcessorChange = (field: keyof JSProcessorDetailResponse, value: any) => {
    setEditingProcessor(prev => ({ ...prev, [field]: value }));
  };

  const toggleEventCode = (code: string) => {
    const eventCodes = [...editingProcessor.eventCodes];
    const index = eventCodes.indexOf(code);
    if (index > -1) {
      eventCodes.splice(index, 1);
    } else {
      eventCodes.push(code);
    }
    handleProcessorChange('eventCodes', eventCodes);
  };

  // 模板应用
  const applySystemTemplate = (templateId: string) => {
    const template = systemTemplates.find(t => t.id === templateId);
    if (template) {
      setEditingProcessor(prev => ({
        ...prev,
        eventCodes: [...template.eventCodes],
        sqlTemplate: template.sqlTemplate,
        sqlTemplateId: templateId,
        sqlTemplateType: SqlTemplateType.System,
      }));
      setSelectedSystemTemplateId(templateId);
      setSelectedCustomTemplateId(null);
      toast.success(`已应用模板: ${template.name}`);
    }
  };

  const applyCustomTemplate = (templateId: string) => {
    const template = customTemplates.find(t => t.id === templateId);
    if (template) {
      setEditingProcessor(prev => ({
        ...prev,
        sqlTemplate: template.sqlTemplate,
        sqlTemplateId: templateId, 
        sqlTemplateType: SqlTemplateType.Custom, 
      }));
      setSelectedCustomTemplateId(templateId);
      setSelectedSystemTemplateId(null);
      toast.success(`已应用模板: ${template.name}`);
    }
  };

 
  const addCustomTemplate = async () => {
    const newTemplate: Omit<CustomSqlTemplate, 'id'> = {
      name: '新自定义查询模板',
      sqlTemplate: 'SELECT * FROM tblexamine where strexamineId = ${strEventReferenceId}'
    };
    try {
      const saved = await createCustomTemplateService(newTemplate);
      setCustomTemplates(prev => [...prev, saved]);
      toast.success('模板已创建');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '操作失败');
    }
  };

  const updateCustomTemplate = async (id: string, updates: Partial<CustomSqlTemplate>) => {
    const template = customTemplates.find(t => t.id === id);
    if (!template) return;

    // 检查是否有实际变化
    const hasChanges = Object.keys(updates).some(key =>
      template[key as keyof CustomSqlTemplate] !== updates[key as keyof CustomSqlTemplate]
    );
    if (!hasChanges) return;

    const updated = { ...template, ...updates };
    try {
      await updateCustomTemplateService(id, updated);
      setCustomTemplates(prev => prev.map(t => t.id === id ? updated : t));
      toast.success('模板已更新');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '操作失败');
    }
  };

  const deleteCustomTemplate = async (id: string) => {
    if (!window.confirm('确定要删除这个自定义查询模板吗？')) return;
    try {
      await deleteCustomTemplateService(id);
      setCustomTemplates(prev => prev.filter(t => t.id !== id));
      if (selectedCustomTemplateId === id) {
        setSelectedCustomTemplateId(null);
      }
      toast.success('模板已删除');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '操作失败');
    }
  };

  // 运行调试（主调试标签页）
  const runDebug = async () => {
    if (!selectedProcessor) {
      toast.error('请选择要调试的处理器');
      return;
    }

    setIsDebugging(true);
    setDebugLog([]);
    setDebugResult(null);

    try {
      addDebugLog('info', `开始调试处理器...`);
      addDebugLog('info', `数据库类型: ${getDatabaseTypeLabel(debugDatabaseType)}`);
      addDebugLog('info', `事件码: ${debugEventType}`);
      addDebugLog('info', `事件ID: ${debugEventId || '随机生成'}`);

      const result = await executeDebug({
        processorId: selectedProcessor,
        databaseType: debugDatabaseType,
        eventCode: debugEventType,
        eventId: debugEventId || undefined
      });

      if (result.rawData) {
        addDebugLog('info', '原始数据:');
        addDebugLog('output', JSON.stringify(result.rawData, null, 2));
      }

      if (result.logs && result.logs.length > 0) {
        result.logs.forEach((log: any) => {
          addDebugLog(log.type, log.message);
        });
      }

      if (result.result) {
        addDebugLog('info', '处理结果:');
        addDebugLog('output', JSON.stringify(result.result, null, 2));

        if (result.result.needToSend) {
          addDebugLog('success', `✅ 需要发送到API，请求信息: ${JSON.stringify(result.result.requestInfo)}`);
        } else {
          addDebugLog('warn', `⏭️ 不需要发送，原因: ${result.result.reason || '未指定'}`);
        }
      }

      addDebugLog('info', `执行时间: ${result.executionTimeMs}ms`);

      if (!result.success) {
        addDebugLog('error', `执行失败: ${result.errorMessage}`);
      }

      setDebugResult(result);

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : '调试执行失败';
      addDebugLog('error', errorMessage);
      toast.error(errorMessage);
    } finally {
      setIsDebugging(false);
    }
  };

  // 编辑器调试函数
  const runEditorDebug = async () => {
    if (!editorDebugExamineId.trim()) {
      toast.error('请输入检查ID');
      return;
    }

    setIsEditorDebugging(true);
    setEditorDebugLog([]);
    setEditorDebugResult(null);

    try {
      addEditorDebugLog('info', `开始编辑器调试...`);
      addEditorDebugLog('info', `检查ID: ${editorDebugExamineId}`);

      const result = await executeExamineDebug({
        processorId: selectedProcessor,
        javascriptCode: editingProcessor.code,
        examineId: editorDebugExamineId,
        databaseType: editingProcessor.databaseTypes[0] || (databaseTypes.length > 0 ? databaseTypes[0].value : ''),
        sqlTemplate: editingProcessor.sqlTemplate,
        validateCode: true
      });

      // 显示代码验证结果
      if (result.codeValidation) {
        if (result.codeValidation.errors.length > 0) {
          result.codeValidation.errors.forEach((error: string) => {
            addEditorDebugLog('error', `代码验证错误: ${error}`);
          });
        }
        if (result.codeValidation.warnings.length > 0) {
          result.codeValidation.warnings.forEach((warning: string) => {
            addEditorDebugLog('warn', `代码警告: ${warning}`);
          });
        }
        if (!result.codeValidation.hasProcessFunction) {
          addEditorDebugLog('error', '❌ 未找到 process 函数，请确保定义了 function process(data) 作为入口点');
        }
      }

      if (result.rawData) {
        addEditorDebugLog('info', '原始数据:');
        addEditorDebugLog('output', JSON.stringify(result.rawData, null, 2));
      }

      if (result.logs && result.logs.length > 0) {
        result.logs.forEach((log: any) => {
          addEditorDebugLog(log.type, log.message);
        });
      }

      if (result.result) {
        addEditorDebugLog('info', '处理结果:');
        addEditorDebugLog('output', JSON.stringify(result.result, null, 2));

        if (result.result.needToSend) {
          addEditorDebugLog('success', `✅ 需要发送到API，请求信息: ${JSON.stringify(result.result.requestInfo)}`);
        } else {
          addEditorDebugLog('warn', `⏭️ 不需要发送，原因: ${result.result.reason || '未指定'}`);
        }
      }

      addEditorDebugLog('info', `执行时间: ${result.executionTimeMs}ms`);

      if (!result.success) {
        addEditorDebugLog('error', `执行失败: ${result.errorMessage}`);
      }

      setEditorDebugResult(result);

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : '调试执行失败';
      addEditorDebugLog('error', errorMessage);
      toast.error(errorMessage);
    } finally {
      setIsEditorDebugging(false);
    }
  };

  // 添加日志的辅助函数
  const addDebugLog = (type: string, message: string) => {
    const timestamp = new Date().toLocaleTimeString();
    const prefix = type === 'error' ? '❌' :
      type === 'warn' ? '⚠️' :
        type === 'success' ? '✅' :
          type === 'output' ? '📤' :
            'ℹ️';
    setDebugLog(prev => [...prev, `[${timestamp}] ${prefix} ${message}`]);
  };

  // 添加编辑器调试日志函数
  const addEditorDebugLog = (type: string, message: string) => {
    const timestamp = new Date().toLocaleTimeString();
    const prefix = type === 'error' ? '❌' :
      type === 'warn' ? '⚠️' :
        type === 'success' ? '✅' :
          type === 'output' ? '📤' :
            'ℹ️';
    setEditorDebugLog(prev => [...prev, `[${timestamp}] ${prefix} ${message}`]);
  };

  // 清除编辑器调试日志
  const clearEditorDebugLog = () => {
    setEditorDebugLog([]);
    setEditorDebugResult(null);
  };

  // 如果正在加载，显示加载状态
  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <i className="fa-solid fa-spinner fa-spin text-3xl text-blue-600"></i>
        <span className="ml-2 text-gray-600 dark:text-gray-400">加载数据中...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">JS处理器管理</h2>
      </div>

      <div className="flex border-b border-gray-200 dark:border-gray-800">
        <button
          onClick={() => setActiveTab('list')}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${activeTab === 'list'
            ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
            : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
        >
          <i className="fa-solid fa-list"></i>
          处理器列表
        </button>
        <button
          onClick={() => setActiveTab('editor')}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${activeTab === 'editor'
            ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
            : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
        >
          <i className="fa-solid fa-code"></i>
          编辑器
        </button>
        <button
          onClick={() => setActiveTab('debug')}
          className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${activeTab === 'debug'
            ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
            : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
        >
          <i className="fa-solid fa-bug"></i>
          调试
        </button>
      </div>

      {/* 处理器列表标签页 */}
      {activeTab === 'list' && (
        <div>
          <div className="mb-4 flex justify-end">
            <button
              onClick={createNewProcessor}
              className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700"
            >
              <i className="fa-solid fa-plus mr-1"></i> 创建新处理器
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
                      数据库类型
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      事件码过滤
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      描述
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      状态
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      操作
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {processors.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="px-6 py-10 text-center text-gray-500 dark:text-gray-400">
                        <div className="flex flex-col items-center justify-center">
                          <i className="fa-solid fa-code text-4xl text-gray-300 dark:text-gray-600 mb-2"></i>
                          暂无处理器，请创建新的处理器
                        </div>
                      </td>
                    </tr>
                  ) : (
                    processors.map((processor) => (
                      <tr
                        key={processor.id}
                        className="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors duration-150"
                      >
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="font-medium">{processor.name}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="flex flex-wrap gap-1">
                            {processor.databaseTypes.map((dbType) => (
                              <span key={dbType} className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${databaseTypes.find(t => t.value === dbType) ? 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400' : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                                }`}>
                                {getDatabaseTypeLabel(dbType)}
                              </span>
                            ))}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="flex flex-wrap gap-1">
                            {processor.eventCodes.map((code, index) => (
                              <span key={index} className="inline-flex items-center rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-800 dark:bg-gray-700 dark:text-gray-300">
                                {code}
                              </span>
                            ))}
                          </div>
                        </td>
                        <td className="px-6 py-4">
                          <div className="text-sm text-gray-500 dark:text-gray-400 line-clamp-1">
                            {processor.description || '无描述'}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${processor.enabled
                            ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
                            : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                            }`}>
                            {processor.enabled ? '启用' : '禁用'}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                          <div className="flex justify-end gap-2">
                            <button
                              onClick={() => toggleProcessorStatus(processor.id)}
                              className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300 transition-colors duration-150"
                            >
                              {processor.enabled ? (
                                <i className="fa-solid fa-toggle-on text-green-500"></i>
                              ) : (
                                <i className="fa-solid fa-toggle-off text-gray-400"></i>
                              )}
                            </button>
                            <button
                              onClick={() => editProcessor(processor.id)}
                              className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 transition-colors duration-150"
                            >
                              <i className="fa-solid fa-edit"></i>
                            </button>
                            <button
                              onClick={() => deleteProcessor(processor.id)}
                              className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300 transition-colors duration-150"
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

      {/* 代码编辑器标签页 */}
      {activeTab === 'editor' && (
        <div className="space-y-6">
          {/* 布局切换工具栏 */}
          <div className="flex items-center justify-between bg-white dark:bg-gray-800 p-4 rounded-xl shadow-md">
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <span className="text-sm text-gray-600 dark:text-gray-400">编辑器布局：</span>
                <button
                  onClick={() => setEditorLayout('horizontal')}
                  className={`flex items-center gap-2 px-3 py-1.5 rounded-md transition-colors ${editorLayout === 'horizontal'
                    ? 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400'
                    : 'text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700'
                    }`}
                >
                  <i className="fa-solid fa-columns"></i>
                  <span className="text-sm">左右分栏</span>
                </button>
                <button
                  onClick={() => setEditorLayout('vertical')}
                  className={`flex items-center gap-2 px-3 py-1.5 rounded-md transition-colors ${editorLayout === 'vertical'
                    ? 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400'
                    : 'text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700'
                    }`}
                >
                  <i className="fa-solid fa-bars-staggered"></i>
                  <span className="text-sm">上下堆叠</span>
                </button>
              </div>

              <div className="flex items-center gap-2 ml-4">
                <span className="text-sm text-gray-600 dark:text-gray-400">字体大小：</span>
                <button
                  onClick={() => setEditorFontSize(prev => Math.max(12, prev - 1))}
                  className="w-8 h-8 flex items-center justify-center rounded-md hover:bg-gray-100 dark:hover:bg-gray-700"
                >
                  <i className="fa-solid fa-minus text-sm"></i>
                </button>
                <span className="text-sm w-8 text-center">{editorFontSize}px</span>
                <button
                  onClick={() => setEditorFontSize(prev => Math.min(24, prev + 1))}
                  className="w-8 h-8 flex items-center justify-center rounded-md hover:bg-gray-100 dark:hover:bg-gray-700"
                >
                  <i className="fa-solid fa-plus text-sm"></i>
                </button>
              </div>
            </div>

            <div className="flex items-center gap-2">
              <button
                onClick={() => setShowEditorDebug(!showEditorDebug)}
                className={`rounded-md px-4 py-2 text-sm font-medium transition-colors ${showEditorDebug
                    ? 'bg-purple-600 text-white hover:bg-purple-700'
                    : 'border border-gray-300 bg-white text-gray-700 hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700'
                  }`}
              >
                <i className="fa-solid fa-bug mr-1"></i>
                {showEditorDebug ? '隐藏调试' : '显示调试'}
              </button>
              <button
                onClick={saveProcessor}
                className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700"
              >
                <i className="fa-solid fa-save mr-1"></i> 保存处理器
              </button>
              <button
                onClick={cancelEdit}
                className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
              >
                <i className="fa-solid fa-times mr-1"></i> 取消
              </button>
            </div>
          </div>

          {/* 根据布局模式显示不同的内容 */}
          {editorLayout === 'horizontal' ? (
            /* 左右分栏布局 */
            <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
              {/* 左侧配置面板 */}
              <div className="lg:col-span-1">
                <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg flex flex-col h-full">
                  <h3 className="mb-4 text-lg font-semibold flex-shrink-0">处理器配置</h3>

                  {/* 主要内容区域 - 添加 flex-1 和 overflow-y-auto 使其可滚动 */}
                  <div className="flex-1 overflow-y-auto space-y-4 pr-1">
                    {/* 处理器名称输入 */}
                    <div>
                      <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                        处理器名称 *
                      </label>
                      <input
                        type="text"
                        value={editingProcessor.name}
                        onChange={(e) => handleProcessorChange('name', e.target.value)}
                        className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                        placeholder="请输入处理器名称"
                      />
                    </div>

                    {/* 数据库类型选择 */}
                    <div>
                      <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                        适用数据库类型 (可多选)
                      </label>
                      <div className="flex flex-wrap gap-2">
                        {databaseTypes.length > 0 ? (
                          databaseTypes.map((type) => (
                            <label
                              key={type.value}
                              className={`inline-flex items-center rounded-full border px-3 py-1.5 text-sm font-medium transition-colors cursor-pointer ${editingProcessor.databaseTypes.includes(type.value)
                                  ? 'border-blue-600 bg-blue-50 text-blue-700 dark:border-blue-500 dark:bg-blue-900/30 dark:text-blue-400'
                                  : 'border-gray-300 bg-white text-gray-700 hover:border-blue-300 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:border-blue-700'
                                }`}
                            >
                              <input
                                type="checkbox"
                                checked={editingProcessor.databaseTypes.includes(type.value)}
                                onChange={() => {
                                  const newTypes = [...editingProcessor.databaseTypes];
                                  const index = newTypes.indexOf(type.value);
                                  if (index > -1) {
                                    if (newTypes.length > 1) {
                                      newTypes.splice(index, 1);
                                    } else {
                                      toast.info('至少需要选择一个数据库类型');
                                      return;
                                    }
                                  } else {
                                    newTypes.push(type.value);
                                  }
                                  handleProcessorChange('databaseTypes', newTypes);
                                }}
                                className="mr-2 h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700"
                              />
                              {type.label}
                            </label>
                          ))
                        ) : (
                          <div className="text-sm text-gray-500">加载数据库类型中...</div>
                        )}
                      </div>
                      <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                        选择处理器适用的数据库系统，至少选择一个
                      </p>
                    </div>

                    {/* 事件码过滤 */}
                    <div>
                      <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                        事件码过滤 (可多选)
                      </label>
                      <div className="flex flex-wrap gap-2 max-h-80 overflow-y-auto p-2 border border-gray-200 rounded-lg dark:border-gray-700">
                        {eventCodes.filter(ec => ec.enabled).map((ec) => (
                          <label
                            key={ec.code}
                            className={`inline-flex items-center rounded-full border px-3 py-1 text-sm font-medium transition-colors ${editingProcessor.eventCodes.includes(ec.code)
                              ? 'border-blue-600 bg-blue-50 text-blue-700 dark:border-blue-500 dark:bg-blue-900/30 dark:text-blue-400'
                              : 'border-gray-300 bg-white text-gray-700 hover:border-blue-300 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:border-blue-700'
                              }`}
                          >
                            <input
                              type="checkbox"
                              checked={editingProcessor.eventCodes.includes(ec.code)}
                              onChange={() => toggleEventCode(ec.code)}
                              className="mr-2 h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700"
                            />
                            {ec.code}
                            {ec.description && <span className="ml-1 text-xs text-gray-500">({ec.description})</span>}
                          </label>
                        ))}
                      </div>
                      <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                        选择处理器需要监听的事件码，留空表示不过滤
                      </p>
                    </div>

                    {/* 系统预设模板 */}
                    <div>
                      <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                        预设sql查询
                      </label>
                      <div className="flex flex-wrap gap-2">
                        {systemTemplates.map((template) => (
                          <button
                            key={template.id}
                            onClick={() => applySystemTemplate(template.id)}
                            className={`inline-flex items-center rounded-full border px-3 py-1.5 text-sm font-medium transition-colors ${selectedSystemTemplateId === template.id
                              ? 'border-blue-600 bg-blue-100 text-blue-800 ring-2 ring-blue-600 ring-offset-1 dark:border-blue-400 dark:bg-blue-900/50 dark:text-blue-300 dark:ring-blue-400'
                              : 'border-gray-300 bg-white text-gray-700 hover:border-blue-300 hover:bg-blue-50 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:border-blue-700 dark:hover:bg-blue-900/30'
                              }`}
                          >
                            <i className={`fa-solid fa-database mr-1 ${selectedSystemTemplateId === template.id
                              ? 'text-blue-600 dark:text-blue-400'
                              : 'text-blue-500'
                              }`}></i>
                            {template.name}
                            {selectedSystemTemplateId === template.id && (
                              <i className="fa-solid fa-check ml-1 text-blue-600 dark:text-blue-400"></i>
                            )}
                          </button>
                        ))}
                      </div>
                    </div>

                    
                    <div>
                      <div className="flex items-center justify-between mb-2">
                        <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                          自定义查询模板
                        </label>
                        <button
                          onClick={addCustomTemplate}
                          className="text-xs text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300"
                        >
                          <i className="fa-solid fa-plus mr-1"></i>新建模板
                        </button>
                      </div>

                      {customTemplates.length === 0 ? (
                        <div className="text-sm text-gray-500 dark:text-gray-400 italic p-2 border border-dashed border-gray-300 rounded-lg dark:border-gray-700">
                          暂无自定义查询模板，点击"新建模板"创建
                        </div>
                      ) : (
                       // 在自定义模板列表渲染部分，确保选中样式正确应用
<div className="space-y-2 max-h-60 overflow-y-auto p-2 border border-gray-200 rounded-lg dark:border-gray-700">
  {customTemplates.map((template) => (
    <div
      key={template.id}
      className={`flex items-center gap-2 p-2 rounded-lg border transition-colors ${
        selectedCustomTemplateId === template.id
          ? 'bg-blue-50 border-blue-300 dark:bg-blue-900/30 dark:border-blue-700 ring-1 ring-blue-500'
          : 'bg-gray-50 border-gray-200 dark:bg-gray-800/50 dark:border-gray-700 hover:bg-gray-100 dark:hover:bg-gray-800'
      }`}
    >
      <div className="flex-1 min-w-0">
        <input
          type="text"
          value={template.name}
          onChange={(e) => {
            setCustomTemplates(prev =>
              prev.map(t => t.id === template.id ? { ...t, name: e.target.value } : t)
            );
          }}
          onBlur={(e) => {
            updateCustomTemplate(template.id, { name: e.target.value });
          }}
          className={`w-full text-sm bg-transparent border-b ${
            selectedCustomTemplateId === template.id
              ? 'border-blue-300 dark:border-blue-700'
              : 'border-transparent hover:border-gray-300 dark:hover:border-gray-600'
          } focus:border-blue-500 dark:focus:border-blue-400 focus:outline-none text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500`}
          placeholder="模板名称"
        />
      </div>
      <div className="flex items-center gap-1 flex-shrink-0">
       
        <button
          onClick={() => applyCustomTemplate(template.id)}
          className={`p-1.5 rounded-md transition-colors ${
            selectedCustomTemplateId === template.id
              ? 'text-blue-600 dark:text-blue-400 bg-blue-100 dark:bg-blue-900/50'
              : 'text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 hover:bg-blue-50 dark:hover:bg-blue-900/30'
          }`}
          title="应用模板"
        >
          <i className="fa-solid fa-check text-sm"></i>
        </button>
        <button
          onClick={() => {
            setEditingTemplate({
              id: template.id,
              name: template.name,
              sql: template.sqlTemplate
            });
          }}
          className="p-1.5 text-gray-600 hover:text-gray-800 dark:text-gray-400 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-md transition-colors"
          title="编辑SQL"
        >
          <i className="fa-solid fa-pen text-sm"></i>
        </button>
        <button
          onClick={() => deleteCustomTemplate(template.id)}
          className="p-1.5 text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300 hover:bg-red-50 dark:hover:bg-red-900/30 rounded-md transition-colors"
          title="删除模板"
        >
          <i className="fa-solid fa-trash text-sm"></i>
        </button>
      </div>
    </div>
  ))}
</div>
                      )}
                    </div>

                    {/* 描述输入 */}
                    <div>
                      <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                        描述
                      </label>
                      <textarea
                        value={editingProcessor.description}
                        onChange={(e) => handleProcessorChange('description', e.target.value)}
                        rows={3}
                        className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                        placeholder="请输入处理器描述"
                      />
                    </div>
                  </div>

                  {/* 提示区域 - 固定在底部 */}
                  <div className="mt-2 rounded-lg bg-yellow-50 p-4 text-sm text-yellow-700 dark:bg-yellow-900/20 dark:text-yellow-400 flex-shrink-0">
                    <div className="flex items-start">
                      <i className="fa-solid fa-lightbulb mt-0.5 mr-2"></i>
                      <div>
                        <p className="mb-1 font-medium">提示:</p>
                        <ul className="list-disc pl-5 space-y-1">
                          <li>确保实现process函数作为入口点，并且保留模板process函数外的代码</li>
                          <li>process函数data参数为sql查询结果转换为对象后注入的产物</li>
                          <li>请按照规范返回ProcessResult对象</li>
                          <li>可以定义辅助函数来分离复杂逻辑</li>
                          <li>如有必要进行try catch处理</li>
                          <li>脚本库函数全部为函数注入，添加库函数需要进行插件注册</li>
                        </ul>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* 右侧编辑器 */}
              <div className="lg:col-span-2">
                <div className="space-y-6">
                  {/* JS代码编辑器 */}
                  <div
                    className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg"
                    onScroll={handleEditorScroll}
                  >
                    <div className="mb-4 flex items-center justify-between">
                      <h3 className="text-lg font-semibold">JavaScript 代码</h3>
                      <div className="text-xs text-gray-500 dark:text-gray-400">
                        请实现process函数，接收数据并返回处理后的结果
                      </div>
                    </div>

                    <div className="rounded-lg border border-gray-300 overflow-hidden dark:border-gray-700">
                      {/* 编辑器工具栏 */}
                      <div className="flex items-center justify-between px-3 py-2 bg-gray-100 border-b border-gray-300 dark:bg-gray-700 dark:border-gray-600">
                        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">JavaScript</span>
                        <div className="flex gap-2">
                          <button
                            type="button"
                            onClick={() => {
                              navigator.clipboard.writeText(editingProcessor.code);
                              toast.success('代码已复制到剪贴板');
                            }}
                            className="flex items-center gap-1 px-2 py-1 text-xs bg-white border border-gray-300 rounded hover:bg-gray-50 dark:bg-gray-600 dark:border-gray-500 dark:text-gray-200 dark:hover:bg-gray-500"
                          >
                            <i className="fa-solid fa-copy"></i> 复制
                          </button>
                          <button
                            type="button"
                            onClick={async () => {
                              try {
                                const formatted = await prettier.format(editingProcessor.code, {
                                  parser: 'babel',
                                  plugins: [parserBabel, prettierPluginEstree],
                                  semi: true,
                                  singleQuote: true,
                                  tabWidth: 2,
                                  trailingComma: 'es5',
                                });
                                handleProcessorChange('code', formatted.trim());
                                toast.success('代码已格式化');
                              } catch (error) {
                                toast.error('格式化失败，请检查代码语法');
                              }
                            }}
                            className="flex items-center gap-1 px-2 py-1 text-xs bg-white border border-gray-300 rounded hover:bg-gray-50 dark:bg-gray-600 dark:border-gray-500 dark:text-gray-200 dark:hover:bg-gray-500"
                          >
                            <i className="fa-solid fa-wand-magic-sparkles"></i> 格式化
                          </button>
                          <button
  type="button"
  onClick={async () => {
    try {
     
      const result: ValidationResult = await validateCode(editingProcessor.code);

      if (result.isValid) {
        if (!result.hasProcessFunction) {
          toast.warning(
            <div>
              <p className="font-medium">代码语法正确</p>
              <p className="text-sm opacity-90">但未找到 process 函数</p>
            </div>,
            { duration: 5000 }
          );
        } else {
          toast.success('校验通过 ✓');
        }
      } else {
        // 构建详细的错误信息组件
        toast.error(
          <div className="space-y-1">
            <p className="font-medium">{result.message || '代码语法错误'}</p>
            {(result.lineNumber || result.column) && (
              <p className="text-sm opacity-90">
                位置: 第 {result.lineNumber || '?'} 行，第 {result.column || '?'} 列
              </p>
            )}
            {result.source && (
              <p className="text-sm opacity-90">来源: {result.source}</p>
            )}
          </div>,
          { duration: 8000 }
        );
      }
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '校验失败');
    }
  }}
  className="flex items-center gap-1 px-2 py-1 text-xs bg-white border border-gray-300 rounded hover:bg-gray-50 dark:bg-gray-600 dark:border-gray-500 dark:text-gray-200 dark:hover:bg-gray-500"
>
  <i className="fa-solid fa-check-circle"></i> 校验
</button>

                          <button
                            type="button"
                            onClick={() => {
                              window.open('/src/doc/func.html', '_blank');
                            }}
                            className="flex items-center gap-1 px-2 py-1 text-xs bg-blue-50 border border-blue-300 text-blue-700 rounded hover:bg-blue-100 dark:bg-blue-900 dark:border-blue-700 dark:text-blue-200 dark:hover:bg-blue-800"
                          >
                            <i className="fa-solid fa-book"></i> 文档
                          </button>
                        </div>
                      </div>

                      {/* CodeMirror 编辑器 */}
                      <div
                        style={{ height: '800px' }}
                        onWheel={(e) => e.stopPropagation()}
                        onScroll={(e) => e.stopPropagation()}
                      >
                        <CodeMirror
                          value={editingProcessor.code}
                          height="800px"
                          style={{ fontSize: `${editorFontSize}px` }}
                          extensions={[javascript()]}
                          theme={oneDark}
                          onChange={(value) => handleProcessorChange('code', value)}
                          basicSetup={{
                            lineNumbers: true,
                            highlightActiveLineGutter: true,
                            highlightSpecialChars: true,
                            foldGutter: true,
                            drawSelection: true,
                            dropCursor: true,
                            allowMultipleSelections: true,
                            indentOnInput: true,
                            syntaxHighlighting: true,
                            bracketMatching: true,
                            closeBrackets: true,
                            autocompletion: true,
                            rectangularSelection: true,
                            crosshairCursor: true,
                            highlightActiveLine: true,
                            highlightSelectionMatches: true,
                          }}
                        />
                      </div>
                    </div>
                  </div>

                  {/* SQL模板编辑器 */}
                  <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
                    <div className="mb-4 flex items-center justify-between">
                      <h3 className="text-lg font-semibold">SQL预览</h3>
                    </div>

                    <div className="rounded-lg border border-gray-300 overflow-hidden dark:border-gray-700">
                      <div style={{ height: '250px' }}>
                        <CodeMirror
                          style={{ fontSize: '15px' }}
                          value={editingProcessor.sqlTemplate}
                          height="250px"
                          extensions={[sql()]}
                          theme={oneDark}
                          onChange={(value) => handleProcessorChange('sqlTemplate', value)}
                          basicSetup={{
                            lineNumbers: true,
                            highlightActiveLineGutter: true,
                            highlightSpecialChars: true,
                            foldGutter: true,
                            drawSelection: true,
                            dropCursor: true,
                            allowMultipleSelections: true,
                            indentOnInput: true,
                            syntaxHighlighting: true,
                            bracketMatching: true,
                            closeBrackets: true,
                            autocompletion: true,
                            rectangularSelection: true,
                            crosshairCursor: true,
                            highlightActiveLine: true,
                            highlightSelectionMatches: true,
                          }}
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ) : (
            /* 上下堆叠布局 - 优化代码编写体验 */
            <div className="space-y-6">
              {/* 顶部配置面板 - 紧凑设计 */}
              <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                  {/* 处理器名称 */}
                  <div>
                    <label className="block mb-1 text-xs font-medium text-gray-700 dark:text-gray-300">
                      处理器名称 *
                    </label>
                    <input
                      type="text"
                      value={editingProcessor.name}
                      onChange={(e) => handleProcessorChange('name', e.target.value)}
                      className="w-full rounded-lg border border-gray-300 bg-white px-3 py-1.5 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                      placeholder="请输入处理器名称"
                    />
                  </div>

                  {/* 数据库类型选择 */}
                  <div>
                    <label className="block mb-1 text-xs font-medium text-gray-700 dark:text-gray-300">
                      数据库类型
                    </label>
                    <div className="flex flex-wrap gap-1">
                      {databaseTypes.length > 0 ? (
                        databaseTypes.map((type) => (
                          <button
                            key={type.value}
                            onClick={() => {
                              const newTypes = [...editingProcessor.databaseTypes];
                              const index = newTypes.indexOf(type.value);
                              if (index > -1) {
                                if (newTypes.length > 1) {
                                  newTypes.splice(index, 1);
                                }
                              } else {
                                newTypes.push(type.value);
                              }
                              handleProcessorChange('databaseTypes', newTypes);
                            }}
                            className={`px-2 py-1 text-xs rounded-full transition-colors ${editingProcessor.databaseTypes.includes(type.value)
                                ? 'bg-blue-600 text-white'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-gray-700 dark:text-gray-300'
                              }`}
                          >
                            {type.label}
                          </button>
                        ))
                      ) : (
                        <div className="text-xs text-gray-500">加载中...</div>
                      )}
                    </div>
                  </div>

                  {/* 事件码快速选择 */}
                  <div>
                    <label className="block mb-1 text-xs font-medium text-gray-700 dark:text-gray-300">
                      事件码
                    </label>
                    <select
                      className="w-full rounded-lg border border-gray-300 bg-white px-3 py-1.5 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                      onChange={(e) => {
                        if (e.target.value) {
                          toggleEventCode(e.target.value);
                        }
                      }}
                      value=""
                    >
                      <option value="">添加事件码</option>
                      {eventCodes.filter(ec => ec.enabled).map((ec) => (
                        <option key={ec.code} value={ec.code}>
                          {ec.code} {ec.description ? `(${ec.description})` : ''}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* 模板选择 */}
                  <div>
                    <label className="block mb-1 text-xs font-medium text-gray-700 dark:text-gray-300">
                      预设模板
                    </label>
                    <select
                      className="w-full rounded-lg border border-gray-300 bg-white px-3 py-1.5 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                      onChange={(e) => {
                        const [type, id] = e.target.value.split(':');
                        if (type === 'system') {
                          applySystemTemplate(id);
                        } else if (type === 'custom') {
                          applyCustomTemplate(id);
                        }
                      }}
                      value=""
                    >
                      <option value="">选择模板</option>
                      <optgroup label="系统模板">
                        {systemTemplates.map(t => (
                          <option key={`system:${t.id}`} value={`system:${t.id}`}>
                            {t.name}
                          </option>
                        ))}
                      </optgroup>
                      <optgroup label="自定义查询模板">
                        {customTemplates.map(t => (
                          <option key={`custom:${t.id}`} value={`custom:${t.id}`}>
                            {t.name}
                          </option>
                        ))}
                      </optgroup>
                    </select>
                  </div>
                </div>

                {/* 已选事件码标签 */}
                <div className="mt-2 flex flex-wrap gap-1">
                  {editingProcessor.eventCodes.map((code) => (
                    <span
                      key={code}
                      className="inline-flex items-center gap-1 px-2 py-0.5 bg-blue-100 text-blue-800 rounded-full text-xs dark:bg-blue-900/30 dark:text-blue-400"
                    >
                      {code}
                      <button
                        onClick={() => toggleEventCode(code)}
                        className="hover:text-blue-600 dark:hover:text-blue-300"
                      >
                        <i className="fa-solid fa-times"></i>
                      </button>
                    </span>
                  ))}
                </div>

                {/* 描述输入 */}
                <div className="mt-2">
                  <input
                    type="text"
                    value={editingProcessor.description}
                    onChange={(e) => handleProcessorChange('description', e.target.value)}
                    className="w-full rounded-lg border border-gray-300 bg-white px-3 py-1.5 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                    placeholder="处理器描述（可选）"
                  />
                </div>
              </div>

              <div
                className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg"
                onScroll={handleEditorScroll}
              >
                <div className="mb-2 flex items-center justify-between">
                  <h3 className="text-md font-semibold">JavaScript 代码</h3>
                  <div className="flex gap-2">
                    <button
                      type="button"
                      onClick={() => {
                        navigator.clipboard.writeText(editingProcessor.code);
                        toast.success('代码已复制到剪贴板');
                      }}
                      className="flex items-center gap-1 px-2 py-1 text-xs bg-white border border-gray-300 rounded hover:bg-gray-50 dark:bg-gray-600 dark:border-gray-500 dark:text-gray-200 dark:hover:bg-gray-500"
                    >
                      <i className="fa-solid fa-copy"></i> 复制
                    </button>
                    <button
                      type="button"
                      onClick={async () => {
                        try {
                          const formatted = await prettier.format(editingProcessor.code, {
                            parser: 'babel',
                            plugins: [parserBabel, prettierPluginEstree],
                            semi: true,
                            singleQuote: true,
                            tabWidth: 2,
                            trailingComma: 'es5',
                          });
                          handleProcessorChange('code', formatted.trim());
                          toast.success('代码已格式化');
                        } catch (error) {
                          toast.error('格式化失败，请检查代码语法');
                        }
                      }}
                      className="flex items-center gap-1 px-2 py-1 text-xs bg-white border border-gray-300 rounded hover:bg-gray-50 dark:bg-gray-600 dark:border-gray-500 dark:text-gray-200 dark:hover:bg-gray-500"
                    >
                      <i className="fa-solid fa-wand-magic-sparkles"></i> 格式化
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        try {
                          new Function(editingProcessor.code);
                          const processFn = new Function(editingProcessor.code + '\nreturn process;')();
                          if (typeof processFn !== 'function') {
                            toast.error('未找到 process 函数');
                            return;
                          }
                          toast.success('语法校验通过');
                        } catch (error) {
                          toast.error(error instanceof Error ? error.message : '代码语法错误');
                        }
                      }}
                      className="flex items-center gap-1 px-2 py-1 text-xs bg-white border border-gray-300 rounded hover:bg-gray-50 dark:bg-gray-600 dark:border-gray-500 dark:text-gray-200 dark:hover:bg-gray-500"
                    >
                      <i className="fa-solid fa-check-circle"></i> 校验
                    </button>
                  </div>
                </div>

                <div className="rounded-lg border border-gray-300 overflow-hidden dark:border-gray-700">
                  <div
                    style={{ height: '600px' }}
                    onWheel={(e) => e.stopPropagation()}
                    onScroll={(e) => e.stopPropagation()}
                  >
                    <CodeMirror
                      value={editingProcessor.code}
                      height="600px"
                      style={{ fontSize: `${editorFontSize}px` }}
                      extensions={[javascript()]}
                      theme={oneDark}
                      onChange={(value) => handleProcessorChange('code', value)}
               
                      basicSetup={{
                        lineNumbers: true,
                        highlightActiveLineGutter: true,
                        highlightSpecialChars: true,
                        foldGutter: true,
                        drawSelection: true,
                        dropCursor: true,
                        allowMultipleSelections: true,
                        indentOnInput: true,
                        syntaxHighlighting: true,
                        bracketMatching: true,
                        closeBrackets: true,
                        autocompletion: true,
                        rectangularSelection: true,
                        crosshairCursor: true,
                        highlightActiveLine: true,
                        highlightSelectionMatches: true,
                        foldKeymap: true,
                      }}
                    />
                  </div>
                </div>
              </div>

              {/* 底部：SQL模板 */}
              <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
                <h3 className="mb-2 text-md font-semibold">SQL 预览</h3>
                <div className="rounded-lg border border-gray-300 overflow-hidden dark:border-gray-700">
                  <div style={{ height: '200px' }}>
                    <CodeMirror
                      style={{ fontSize: '15px' }}
                      value={editingProcessor.sqlTemplate}
                      height="200px"
                      extensions={[sql()]}
                      theme={oneDark}
                      onChange={(value) => handleProcessorChange('sqlTemplate', value)}
                      basicSetup={{
                        lineNumbers: true,
                        highlightActiveLineGutter: true,
                        highlightSpecialChars: true,
                        foldGutter: true,
                        drawSelection: true,
                        dropCursor: true,
                        allowMultipleSelections: true,
                        indentOnInput: true,
                        syntaxHighlighting: true,
                        bracketMatching: true,
                        closeBrackets: true,
                        autocompletion: true,
                        rectangularSelection: true,
                        crosshairCursor: true,
                        highlightActiveLine: true,
                        highlightSelectionMatches: true,
                      }}
                    />
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* 编辑器内嵌调试面板 */}
          {showEditorDebug && (
            <div className="mt-6 rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold">快速调试</h3>
                <div className="flex items-center gap-2">
                  <span className="text-sm text-gray-500 dark:text-gray-400">
                    调试 Examine 事件
                  </span>
                  {editorDebugLog.length > 0 && (
                    <button
                      onClick={clearEditorDebugLog}
                      className="text-sm text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
                    >
                      <i className="fa-solid fa-trash mr-1"></i>清空日志
                    </button>
                  )}
                </div>
              </div>

              <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
                {/* 左侧输入区域 */}
                <div className="lg:col-span-1">
                  <div className="space-y-4">
                    <div>
                      <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                        检查ID (strExamineId)
                      </label>
                      <input
                        type="text"
                        value={editorDebugExamineId}
                        onChange={(e) => setEditorDebugExamineId(e.target.value)}
                        className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                        placeholder="请输入检查ID"
                      />
                    </div>

                    <div>
                      <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                        数据库类型
                      </label>
                      <select
                        value={editingProcessor.databaseTypes[0] || (databaseTypes.length > 0 ? databaseTypes[0].value : '')}
                        onChange={(e) => {
                          handleProcessorChange('databaseTypes', [e.target.value]);
                        }}
                        className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                      >
                        {databaseTypes.length > 0 ? (
                          databaseTypes.map((type) => (
                            <option key={type.value} value={type.value}>
                              {type.label}
                            </option>
                          ))
                        ) : (
                          <option value="">加载中...</option>
                        )}
                      </select>
                      <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                        使用处理器配置的第一个数据库类型
                      </p>
                    </div>

                    <button
                      onClick={runEditorDebug}
                      disabled={isEditorDebugging || !editorDebugExamineId.trim()}
                      className={`w-full rounded-md px-6 py-2 text-sm font-medium transition-colors ${(isEditorDebugging || !editorDebugExamineId.trim())
                          ? 'bg-gray-400 text-white cursor-not-allowed'
                          : 'bg-purple-600 text-white hover:bg-purple-700'
                        }`}
                    >
                      {isEditorDebugging ? (
                        <>
                          <i className="fa-solid fa-spinner fa-spin mr-1"></i> 调试中...
                        </>
                      ) : (
                        <>
                          <i className="fa-solid fa-play mr-1"></i> 运行调试
                        </>
                      )}
                    </button>


                  </div>
                </div>

                {/* 右侧日志输出区域 */}
                <div className="lg:col-span-2">
                  <div className="rounded-lg border border-gray-300 bg-gray-50 p-4 dark:border-gray-700 dark:bg-gray-900">
                    <div className="flex items-center justify-between mb-2">
                      <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300">
                        调试日志输出
                      </h4>
                      {editorDebugResult && (
                        <span className="text-xs text-gray-500">
                          执行时间: {editorDebugResult.executionTimeMs}ms
                        </span>
                      )}
                    </div>

                    <div className="h-80 overflow-auto rounded-lg bg-gray-900 p-4 text-xs font-mono">
                      {editorDebugLog.length === 0 ? (
                        <div className="flex h-full items-center justify-center text-gray-500">
                          <div className="text-center">
                            <i className="fa-solid fa-bug text-2xl mb-2"></i>
                            <p>请输入检查ID并点击"运行调试"开始</p>
                          </div>
                        </div>
                      ) : (
                        <div className="space-y-1">
                          {editorDebugLog.map((log, index) => {
                            const isError = log.includes('❌');
                            const isWarn = log.includes('⚠️');
                            const isSuccess = log.includes('✅');
                            const isOutput = log.includes('📤');

                            return (
                              <div
                                key={index}
                                className={`whitespace-pre-wrap break-all ${isError ? 'text-red-400' :
                                    isWarn ? 'text-yellow-400' :
                                      isSuccess ? 'text-green-400' :
                                        isOutput ? 'text-blue-400' :
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
                    {editorDebugResult?.result && (
                      <div className="mt-4 p-3 bg-gray-100 dark:bg-gray-800 rounded-lg">
                        <div className="text-sm font-medium mb-2">处理结果摘要:</div>
                        <div className="space-y-1 text-xs">
                          <div className="flex items-center">
                            <span className="w-24 text-gray-500">是否需要发送:</span>
                            <span className={editorDebugResult.result.needToSend ? 'text-green-600 font-medium' : 'text-yellow-600'}>
                              {editorDebugResult.result.needToSend ? '是' : '否'}
                            </span>
                          </div>
                          {editorDebugResult.result.reason && (
                            <div className="flex items-center">
                              <span className="w-24 text-gray-500">原因:</span>
                              <span>{editorDebugResult.result.reason}</span>
                            </div>
                          )}
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {/* 调试标签页 */}
      {activeTab === 'debug' && (
        <div className="space-y-6">
          <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
            <h3 className="mb-6 text-lg font-semibold">处理器调试</h3>

            <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
              {/* 调试控制面板 */}
              <div className="lg:col-span-1">
                <div className="space-y-4">
                  <div>
                    <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                      数据库类型
                    </label>
                    <select
                      value={debugDatabaseType}
                      onChange={(e) => setDebugDatabaseType(e.target.value)}
                      className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                    >
                      {databaseTypes.length > 0 ? (
                        databaseTypes.map((type) => (
                          <option key={type.value} value={type.value}>
                            {type.label}
                          </option>
                        ))
                      ) : (
                        <option value="">加载中...</option>
                      )}
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
                      value={selectedProcessor || ''}
                      onChange={(e) => setSelectedProcessor(e.target.value)}
                      className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                    >
                      <option value="">请选择处理器</option>
                      {processors.map((processor) => (
                        <option key={processor.id} value={processor.id}>
                          {processor.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <button
                    onClick={runDebug}
                    disabled={!selectedProcessor || isDebugging}
                    className={`w-full rounded-md px-6 py-2 text-sm font-medium transition-colors ${(!selectedProcessor || isDebugging)
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
                    <li>选择数据库类型、事件码和处理器</li>
                    <li>可输入事件ID进行精确筛选，留空则根据事件码随机获取</li>
                    <li>点击"运行调试"执行处理器但不实际发送数据</li>
                    <li>下方将显示处理器的执行日志和输出结果</li>
                  </ul>
                </div>

                {/* 调试日志输出区域 */}
                <div className="rounded-lg border border-gray-300 bg-gray-50 p-4 dark:border-gray-700 dark:bg-gray-900">
                  <div className="flex items-center justify-between mb-2">
                    <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300">
                      调试日志输出
                    </h4>
                    {debugResult && (
                      <span className="text-xs text-gray-500">
                        执行时间: {debugResult.executionTimeMs}ms
                      </span>
                    )}
                  </div>

                  <div className="h-80 overflow-auto rounded-lg bg-gray-900 p-4 text-xs font-mono">
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

                          return (
                            <div
                              key={index}
                              className={`whitespace-pre-wrap break-all ${isError ? 'text-red-400' :
                                isWarn ? 'text-yellow-400' :
                                  isSuccess ? 'text-green-400' :
                                    isOutput ? 'text-blue-400' :
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
                  {debugResult?.result && (
                    <div className="mt-4 p-3 bg-gray-100 dark:bg-gray-800 rounded-lg">
                      <div className="text-sm font-medium mb-2">处理结果摘要:</div>
                      <div className="space-y-1 text-xs">
                        <div className="flex items-center">
                          <span className="w-24 text-gray-500">是否需要发送:</span>
                          <span className={debugResult.result.needToSend ? 'text-green-600 font-medium' : 'text-yellow-600'}>
                            {debugResult.result.needToSend ? '是' : '否'}
                          </span>
                        </div>
                        {debugResult.result.reason && (
                          <div className="flex items-center">
                            <span className="w-24 text-gray-500">原因:</span>
                            <span>{debugResult.result.reason}</span>
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

      {/* SQL编辑器模态框 */}
      {editingTemplate && createPortal(
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-3/4 max-w-4xl max-h-[80vh] overflow-hidden">
            <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200 dark:border-gray-700">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                编辑SQL模板 - {editingTemplate.name}
              </h3>
              <button
                onClick={() => setEditingTemplate(null)}
                className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <i className="fa-solid fa-times"></i>
              </button>
            </div>
            <div className="p-4">
              <div className="border border-gray-300 dark:border-gray-700 rounded-lg overflow-hidden" style={{ height: '400px' }}>
                <CodeMirror
                  value={editingTemplate.sql}
                  height="400px"
                  style={{ fontSize: '16px' }}
                  extensions={[sql()]}
                  theme={oneDark}
                  onChange={(value) => {
                    setEditingTemplate(prev => prev ? { ...prev, sql: value } : null);
                  }}
                  basicSetup={{
                    lineNumbers: true,
                    highlightActiveLineGutter: true,
                    highlightSpecialChars: true,
                    foldGutter: true,
                    drawSelection: true,
                    dropCursor: true,
                    allowMultipleSelections: true,
                    indentOnInput: true,
                    syntaxHighlighting: true,
                    bracketMatching: true,
                    closeBrackets: true,
                    autocompletion: true,
                    rectangularSelection: true,
                    crosshairCursor: true,
                    highlightActiveLine: true,
                    highlightSelectionMatches: true,
                  }}
                />
              </div>
            </div>
            <div className="flex justify-end gap-2 px-4 py-3 border-t border-gray-200 dark:border-gray-700">
              <button
                onClick={() => setEditingTemplate(null)}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-md"
              >
                取消
              </button>
              <button
                onClick={() => {
                  if (editingTemplate) {
                    updateCustomTemplate(editingTemplate.id, { sqlTemplate: editingTemplate.sql });
                    setEditingTemplate(null);
                  }
                }}
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-md"
              >
                保存
              </button>
            </div>
          </div>
        </div>,
        document.body
      )}
    </div>
  );
}