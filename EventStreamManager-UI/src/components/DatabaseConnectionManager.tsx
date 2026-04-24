import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { DatabaseConfig, DatabaseType, DriverType } from '@/types/database';
import * as databaseService from '@/services/database.service';
import { useApiQuery } from '@/hooks/useApiQuery';
import { Modal } from '@/components/ui/Modal';
import { FormField } from '@/components/ui/FormField';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { PageLoading } from '@/components/ui/PageLoading';
import { buttonVariants } from '@/utils/button-styles';

export default function DatabaseConnectionManager() {
  const [activeDatabase, setActiveDatabase] = useState<DatabaseType>('');
  const [currentConfigIndex, setCurrentConfigIndex] = useState(0);
  const [connectionStatus, setConnectionStatus] = useState<'disconnected' | 'connecting' | 'connected'>('disconnected');
  const [isMutating, setIsMutating] = useState(false);
  const [isInitializing, setIsInitializing] = useState(false);
  const [isAddingType, setIsAddingType] = useState(false);
  const [newTypeName, setNewTypeName] = useState('');
  const [newTypeLabel, setNewTypeLabel] = useState('');
  const [typeNameError, setTypeNameError] = useState('');

  // 将类型标识自动转换为显示名称
  const autoGenerateLabel = (name: string): string => {
    if (!name) return '';
    return name
      .replace(/[_-]+/g, ' ')
      .replace(/\b\w/g, c => c.toUpperCase());
  };

  // 校验类型标识格式
  const validateTypeName = (name: string): string => {
    if (!name) return '';
    if (!/^[a-zA-Z0-9_]+$/.test(name)) {
      return '只能包含字母、数字和下划线';
    }
    if (databaseTypes.some(t => t.value === name)) {
      return '该类型标识已存在';
    }
    return '';
  };

  const handleTypeNameChange = (value: string) => {
    setNewTypeName(value);
    setTypeNameError(validateTypeName(value));
    // 如果显示名称为空，自动生成
    if (!newTypeLabel.trim()) {
      setNewTypeLabel(autoGenerateLabel(value));
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && newTypeName.trim() && newTypeLabel.trim() && !typeNameError && !isLoading) {
      addDatabaseType();
    }
  };

  const closeAddTypeModal = () => {
    setIsAddingType(false);
    setNewTypeName('');
    setNewTypeLabel('');
    setTypeNameError('');
  };

  // 数据库配置数据
  const [databaseConfigs, setDatabaseConfigs] = useState<Record<string, DatabaseConfig[]>>({});

  // 当前编辑的配置
  const [currentConfig, setCurrentConfig] = useState<DatabaseConfig>({
    id: '',
    name: '',
    connectionString: '',
    driver: DriverType.SqlServer,
    isActive: false,
    timeout: 30
  });

  const {
    data: databaseTypes = [],
    loading: typesLoading,
    setData: setDatabaseTypes,
  } = useApiQuery(() => databaseService.getDatabaseTypes(), {
    initialData: [],
    errorMessage: '获取数据库类型失败',
  });

  const {
    data: connectionExamples = {},
  } = useApiQuery(() => databaseService.getConnectionExamples(), {
    initialData: {},
    showErrorToast: false,
  });

  // 初始化时加载所有配置
  const { loading: configsLoading, execute: loadConfigs } = useApiQuery(
    () => databaseService.getAllConfigs(),
    { initialData: {}, enabled: false, showErrorToast: false }
  );

  const isLoading = typesLoading || configsLoading || isMutating;

  useEffect(() => {
    loadConfigs().then(data => {
      if (data) setDatabaseConfigs(data || {});
    });
  }, []);

  useEffect(() => {
    if (databaseTypes.length > 0 && !activeDatabase) {
      setActiveDatabase(databaseTypes[0].value);
    }
  }, [databaseTypes]);

  // 当切换数据库类型或索引时更新当前配置
  useEffect(() => {
    if (!activeDatabase) return;

    const configs = databaseConfigs[activeDatabase] || [];
    if (configs.length > 0) {
      if (currentConfigIndex < configs.length) {
        setCurrentConfig(configs[currentConfigIndex]);
      } else {
        setCurrentConfigIndex(0);
      }
    } else {
      createNewConfig();
    }
  }, [activeDatabase, currentConfigIndex, databaseConfigs]);

  // 获取指定类型的配置
  const loadConfigsByType = async (type: string) => {
    try {
      const data = await databaseService.getConfigsByType(type);
      setDatabaseConfigs(prev => ({
        ...prev,
        [type]: data
      }));

      if (data && data.length > 0) {
        const activeIndex = data.findIndex((c: DatabaseConfig) => c.isActive === true);
        if (activeIndex !== -1 && type === activeDatabase) {
          setCurrentConfigIndex(activeIndex);
        }
      }
    } catch (error) {
      console.error('获取配置失败:', error);
      const typeLabel = getDatabaseTypeLabel(type);
      toast.error(`加载${typeLabel}配置失败`);
    }
  };

  // 添加新的数据库类型
  const addDatabaseType = async () => {
    const name = newTypeName.trim();
    const label = newTypeLabel.trim();

    if (!name || !label) {
      toast.error('请输入类型标识和名称');
      return;
    }

    const error = validateTypeName(name);
    if (error) {
      toast.error(error);
      setTypeNameError(error);
      return;
    }

    setIsMutating(true);
    try {
      const newType = await databaseService.addDatabaseType(name, label);

      setDatabaseTypes(prev => [...(prev || []), newType]);
      setDatabaseConfigs(prev => ({
        ...prev,
        [name]: []
      }));

      setActiveDatabase(name);
      createNewConfig();

      closeAddTypeModal();

      toast.success('数据库类型添加成功');
    } catch (error) {
      console.error('添加数据库类型失败:', error);
      toast.error('添加数据库类型失败');
    } finally {
      setIsMutating(false);
    }
  };

  // 删除数据库类型
  const deleteDatabaseType = async (type: string) => {
    if (!window.confirm(`确定要删除${getDatabaseTypeLabel(type)}及其所有配置吗？`)) {
      return;
    }

    setIsMutating(true);
    try {
      await databaseService.deleteDatabaseType(type);

      setDatabaseTypes(prev => (prev || []).filter(t => t.value !== type));

      const newConfigs = { ...databaseConfigs };
      delete newConfigs[type];
      setDatabaseConfigs(newConfigs);

      if (activeDatabase === type) {
        const remainingTypes = databaseTypes.filter(t => t.value !== type);
        if (remainingTypes.length > 0) {
          setActiveDatabase(remainingTypes[0].value);
        } else {
          setActiveDatabase('');
          setCurrentConfig({
            id: '',
            name: '',
            connectionString: '',
            driver: DriverType.SqlServer,
            isActive: false,
            timeout: 30
          });
        }
      }

      toast.success('数据库类型删除成功');
    } catch (error) {
      console.error('删除数据库类型失败:', error);
      toast.error('删除数据库类型失败');
    } finally {
      setIsMutating(false);
    }
  };

  // 处理数据库类型切换
  const handleDatabaseTypeChange = (type: string) => {
    setActiveDatabase(type);
    setCurrentConfigIndex(0);
    setConnectionStatus('disconnected');
    loadConfigsByType(type);
  };

  // 处理配置更改
  const handleConfigChange = (field: keyof DatabaseConfig, value: any) => {
    setCurrentConfig(prev => ({
      ...prev,
      [field]: value
    }));
  };

  // 保存当前配置
  const saveCurrentConfig = async () => {
    if (!currentConfig.name) {
      toast.error('请输入配置名称');
      return;
    }

    setIsMutating(true);
    try {
      let savedConfig: DatabaseConfig;
      
      if (currentConfig.id) {
        savedConfig = await databaseService.updateConfig(activeDatabase, currentConfig.id, currentConfig);
      } else {
        savedConfig = await databaseService.createConfig(activeDatabase, currentConfig);
      }

      setDatabaseConfigs(prev => {
        const currentTypeConfigs = prev[activeDatabase] || [];
        const newConfigs = { ...prev };

        if (currentConfig.id) {
          newConfigs[activeDatabase] = currentTypeConfigs.map(c =>
            c.id === savedConfig.id ? savedConfig : c
          );
        } else {
          newConfigs[activeDatabase] = [...currentTypeConfigs, savedConfig];
          setCurrentConfigIndex(newConfigs[activeDatabase].length - 1);
        }
        return newConfigs;
      });

      toast.success(currentConfig.id ? '配置已更新' : '配置已创建');
    } catch (error: any) {
      console.error('保存失败:', error);
      toast.error(error.message || '保存失败');
    } finally {
      setIsMutating(false);
    }
  };

  // 测试连接
  const testConnection = async () => {
    if (!currentConfig.connectionString) {
      toast.error('请输入连接字符串');
      return;
    }

    setConnectionStatus('connecting');

    try {
      const result = await databaseService.testConnection(currentConfig);
      setConnectionStatus(result.success ? 'connected' : 'disconnected');

      if (result.success) {
        toast.success(`数据库连接测试成功`);
      } else {
        toast.error(`无法连接到数据库: ${result.message}`);
      }
    } catch (error) {
      console.error('连接测试失败:', error);
      setConnectionStatus('disconnected');
      toast.error('连接测试失败，请检查网络或API服务');
    }
  };

  // 初始化表结构
  const initializeTables = async () => {
    if (!currentConfig.id) {
      toast.error('请先保存配置');
      return;
    }

    if (!currentConfig.connectionString) {
      toast.error('请输入连接字符串');
      return;
    }

    setIsInitializing(true);
    try {
      const result = await databaseService.initializeTables(activeDatabase, currentConfig.id);

      if (result.success) {
        const tablesInfo = result.createdTables && result.createdTables.length > 0
          ? `\n已创建表: ${result.createdTables.join(', ')}`
          : '';
        toast.success(`表结构初始化成功！${tablesInfo}`, { duration: 5000 });
      } else {
        toast.error(`初始化失败: ${result.message}`);
      }
    } catch (error) {
      console.error('初始化表结构失败:', error);
      toast.error('初始化表结构失败');
    } finally {
      setIsInitializing(false);
    }
  };

  // 创建新配置
  const createNewConfig = () => {
    const typeLabel = getDatabaseTypeLabel(activeDatabase);
    const newConfig: DatabaseConfig = {
      id: '',
      name: `新${typeLabel}配置`,
      connectionString: '',
      isActive: false,
      driver: DriverType.SqlServer,
      timeout: 30
    };

    setCurrentConfig(newConfig);
    setConnectionStatus('disconnected');
  };

  // 删除配置
  const deleteConfig = async () => {
    if (!currentConfig.id) {
      toast.error('请先保存配置');
      return;
    }

    const currentTypeConfigs = databaseConfigs[activeDatabase] || [];
    if (currentTypeConfigs.length <= 1) {
      toast.error('至少需要保留一个配置');
      return;
    }

    if (!window.confirm('确定要删除这个配置吗？')) {
      return;
    }

    setIsMutating(true);
    try {
      await databaseService.deleteConfig(activeDatabase, currentConfig.id);

      const wasActive = currentConfig.isActive;

      setDatabaseConfigs(prev => {
        const newConfigs = { ...prev };
        newConfigs[activeDatabase] = (prev[activeDatabase] || []).filter(c => c.id !== currentConfig.id);
        return newConfigs;
      });

      if (wasActive && currentTypeConfigs.length > 1) {
        const remainingConfigs = currentTypeConfigs.filter(c => c.id !== currentConfig.id);
        if (remainingConfigs.length > 0) {
          await setAsActiveConfig(remainingConfigs[0].id);
        }
      }

      setCurrentConfigIndex(0);
      toast.success('配置已删除');
    } catch (error: any) {
      console.error('删除失败:', error);
      toast.error(error.message || '删除失败');
    } finally {
      setIsMutating(false);
    }
  };

  // 设置为当前使用的配置
  const setAsActiveConfig = async (configId?: string) => {
    const targetId = configId || currentConfig.id;

    if (!targetId) {
      toast.error('请先保存配置');
      return;
    }

    setIsMutating(true);
    try {
      await databaseService.setActiveConfig(activeDatabase, targetId);

      setDatabaseConfigs(prev => {
        const currentTypeConfigs = prev[activeDatabase] || [];
        const newConfigs = { ...prev };
        newConfigs[activeDatabase] = currentTypeConfigs.map(config => ({
          ...config,
          isActive: config.id === targetId
        }));
        return newConfigs;
      });

      if (currentConfig.id === targetId) {
        setCurrentConfig(prev => ({
          ...prev,
          isActive: true
        }));
      } else {
        const configs = databaseConfigs[activeDatabase] || [];
        const targetIndex = configs.findIndex(c => c.id === targetId);
        if (targetIndex !== -1) {
          setCurrentConfigIndex(targetIndex);
        }
      }

      const typeLabel = getDatabaseTypeLabel(activeDatabase);
      toast.success(`已切换为当前使用的${typeLabel}配置`);
    } catch (error: any) {
      console.error('设置激活配置失败:', error);
      toast.error(error.message || '设置失败');
    } finally {
      setIsMutating(false);
    }
  };

  // 获取数据库类型标签
  const getDatabaseTypeLabel = (type: string): string => {
    const found = databaseTypes.find(t => t.value === type);
    return found?.label || type;
  };

  // 解析连接字符串示例（从后端模板获取）
  const getConnectionStringExamples = (driver: string): string => {
    return connectionExamples[driver] || 'Server=localhost;Database=mydb;User Id=user;Password=pass;';
  };

  // 获取当前激活的配置
  const getActiveConfig = (type: string): DatabaseConfig | undefined => {
    return (databaseConfigs[type] || []).find(config => config.isActive === true);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">数据库连接管理</h2>

      </div>

      {/* 数据库类型选择器和添加按钮 */}
      <div className="flex flex-wrap items-center gap-4">
        <div className="flex gap-2 flex-wrap flex-1">
          {databaseTypes.map((type) => {
            const activeConfig = getActiveConfig(type.value);
            const isActive = activeDatabase === type.value;
            return (
              <div key={type.value} className="relative group">
                <button
                  onClick={() => handleDatabaseTypeChange(type.value)}
                  className={`rounded-lg px-6 py-3 font-medium transition-all ${isActive
                      ? 'bg-blue-600 text-white shadow-md'
                      : 'bg-white text-gray-700 shadow border border-gray-200 hover:border-blue-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700'
                    }`}
                >
                  {type.label}
                  {activeConfig && (
                    <span className="absolute -top-1 -right-1 flex h-3 w-3">
                      <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
                      <span className="relative inline-flex rounded-full h-3 w-3 bg-green-500"></span>
                    </span>
                  )}
                </button>
                {!isActive && databaseTypes.length > 1 && (
                  <button
                    onClick={() => deleteDatabaseType(type.value)}
                    className="absolute -top-2 -right-2 w-6 h-6 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center text-sm hover:bg-red-600"
                    title={`删除${type.label}`}
                  >
                    <i className="fa-solid fa-times"></i>
                  </button>
                )}
              </div>
            );
          })}
        </div>

        <button
          onClick={() => setIsAddingType(true)}
          className="rounded-lg bg-green-600 text-white px-4 py-3 font-medium hover:bg-green-700 transition-colors flex items-center gap-2"
        >
          <i className="fa-solid fa-plus"></i>
          新增连接类型
        </button>
      </div>

      {isLoading && databaseTypes.length === 0 && <PageLoading />}

      {/* 添加新类型的弹窗 */}
      <Modal
        isOpen={isAddingType}
        onClose={closeAddTypeModal}
        title="新增数据库连接类型"
        icon={<i className="fa-solid fa-plus text-blue-600 dark:text-blue-400 text-sm"></i>}
        footer={
          <>
            <button onClick={closeAddTypeModal} className={buttonVariants.ghost + ' px-5 py-2.5'}>
              取消
            </button>
            <button
              onClick={addDatabaseType}
              disabled={!newTypeName.trim() || !newTypeLabel.trim() || !!typeNameError || isLoading}
              className={buttonVariants.primary + ' px-5 py-2.5 flex items-center gap-1.5'}
            >
              {isLoading ? (
                <>
                  <i className="fa-solid fa-spinner fa-spin"></i>
                  添加中...
                </>
              ) : (
                <>
                  <i className="fa-solid fa-plus text-xs"></i>
                  添加
                </>
              )}
            </button>
          </>
        }
      >
        <FormField label="类型标识" required error={typeNameError || undefined} hint={typeNameError ? undefined : '唯一标识，只能包含字母、数字和下划线'}>
          <input
            type="text"
            autoFocus
            value={newTypeName}
            onChange={(e) => handleTypeNameChange(e.target.value)}
            onKeyDown={handleKeyDown}
            className={`w-full rounded-lg border bg-white px-4 py-2.5 shadow-sm focus:outline-none focus:ring-2 dark:bg-gray-800 dark:text-white transition-all ${
              typeNameError
                ? 'border-red-300 focus:border-red-500 focus:ring-red-500/20 dark:border-red-700'
                : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500/20 dark:border-gray-600'
            }`}
            placeholder="例如：mri"
          />
        </FormField>

        <FormField label="显示名称" required hint="用于界面展示的名称">
          <input
            type="text"
            value={newTypeLabel}
            onChange={(e) => setNewTypeLabel(e.target.value)}
            onKeyDown={handleKeyDown}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2.5 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-600 dark:bg-gray-800 dark:text-white transition-all"
            placeholder="例如：核磁共振数据库"
          />
        </FormField>
      </Modal>

      {/* 配置列表和操作 */}
      {activeDatabase && (
        <>
          <div className="flex flex-wrap items-center justify-between gap-4">
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2">
                <select
                  value={currentConfigIndex}
                  onChange={(e) => {
                    setCurrentConfigIndex(Number(e.target.value));
                    setConnectionStatus('disconnected');
                  }}
                  className="rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 min-w-[200px]"
                  disabled={(databaseConfigs[activeDatabase] || []).length === 0}
                >
                  {(databaseConfigs[activeDatabase] || []).map((config, index) => (
                    <option key={config.id} value={index}>
                      {config.name}
                    </option>
                  ))}
                </select>

                {currentConfig.isActive && (
                  <StatusBadge variant="success"><i className="fa-solid fa-check-circle mr-1"></i>当前使用中</StatusBadge>
                )}
                {!currentConfig.isActive && currentConfig.id && (
                  <StatusBadge variant="default"><i className="fa-solid fa-circle mr-1"></i>未使用</StatusBadge>
                )}
              </div>

              <div className="flex items-center gap-1">
                <span className={`inline-block h-2.5 w-2.5 rounded-full ${connectionStatus === 'connected' ? 'bg-green-500' :
                    connectionStatus === 'connecting' ? 'bg-yellow-500 animate-pulse' : 'bg-red-500'
                  }`}></span>
                <span className="text-sm text-gray-500 dark:text-gray-400">
                  {connectionStatus === 'connected' ? '已连接' :
                    connectionStatus === 'connecting' ? '连接中...' : '未连接'}
                </span>
              </div>
            </div>

            <div className="flex gap-2 flex-wrap">
              <button
                onClick={createNewConfig}
                className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
              >
                <i className="fa-solid fa-plus mr-1"></i> 新建配置
              </button>

              {currentConfig.id && !currentConfig.isActive && (
                <button
                  onClick={() => setAsActiveConfig()}
                  disabled={isLoading}
                  className="rounded-md border border-blue-300 bg-white px-4 py-2 text-sm font-medium text-blue-700 transition-colors hover:bg-blue-50 dark:border-blue-800 dark:bg-gray-800 dark:text-blue-400 dark:hover:bg-blue-900/20"
                >
                  <i className="fa-solid fa-star mr-1"></i> 设为当前使用
                </button>
              )}

              <button
                onClick={deleteConfig}
                disabled={!currentConfig.id || (databaseConfigs[activeDatabase] || []).length <= 1}
                className={`rounded-md border px-4 py-2 text-sm font-medium transition-colors ${!currentConfig.id || (databaseConfigs[activeDatabase] || []).length <= 1
                    ? 'border-gray-200 bg-gray-100 text-gray-400 cursor-not-allowed dark:border-gray-800 dark:bg-gray-900 dark:text-gray-600'
                    : 'border-red-300 bg-white text-red-700 hover:bg-red-50 dark:border-red-900/30 dark:bg-gray-800 dark:text-red-400 dark:hover:bg-red-900/20'
                  }`}
              >
                <i className="fa-solid fa-trash mr-1"></i> 删除配置
              </button>
            </div>
          </div>

          {/* 配置表单 */}
          <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
            <h3 className="mb-4 text-lg font-semibold">
              {getDatabaseTypeLabel(activeDatabase)} - 连接配置
            </h3>

            <div className="grid grid-cols-1 gap-6">
              <FormField label="配置名称">
                <input
                  type="text"
                  value={currentConfig.name}
                  onChange={(e) => handleConfigChange('name', e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  placeholder="例如：生产数据库"
                />
              </FormField>

              <FormField label="数据库驱动">
                <select
                  value={currentConfig.driver}
                  onChange={(e) => handleConfigChange('driver', e.target.value as DriverType)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                >
                  <option value={DriverType.SqlServer}>SQL Server</option>
                  <option value={DriverType.MySql}>MySql</option>
                  <option value={DriverType.PostgreSQL}>PostgreSql</option>
                  <option value={DriverType.Oracle}>Oracle</option>
                  <option value={DriverType.SqLite}>SqLite</option>
                </select>
              </FormField>

              <FormField label="连接字符串" hint={`示例: ${getConnectionStringExamples(currentConfig.driver)}`}>
                <textarea
                  value={currentConfig.connectionString}
                  onChange={(e) => handleConfigChange('connectionString', e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white font-mono text-sm"
                  rows={3}
                  placeholder={getConnectionStringExamples(currentConfig.driver)}
                />
              </FormField>

              <FormField label="SQL 执行超时 (秒)" hint="单条 SQL 查询的最大执行等待时间，默认 30 秒">
                <input
                  type="number"
                  value={currentConfig.timeout}
                  onChange={(e) => handleConfigChange('timeout', Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  min="1"
                  max="300"
                />
              </FormField>

              {currentConfig.id && (
                <div className="flex items-center gap-2 p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                  <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                    激活状态:
                  </span>
                  {currentConfig.isActive ? (
                  <StatusBadge variant="success"><i className="fa-solid fa-check-circle mr-1"></i>当前使用中</StatusBadge>
                ) : (
                  <StatusBadge variant="default"><i className="fa-solid fa-circle mr-1"></i>未使用</StatusBadge>
                )}
                </div>
              )}
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                onClick={saveCurrentConfig}
                disabled={isLoading || !currentConfig.name}
                className={buttonVariants.success + ' px-6 py-3 flex items-center gap-2'}
              >
                <i className="fa-solid fa-save"></i>
                {currentConfig.id ? '更新配置' : '保存配置'}
              </button>

              <button
                onClick={testConnection}
                disabled={connectionStatus === 'connecting' || !currentConfig.connectionString || isLoading}
                className={buttonVariants.primary + ' px-6 py-3 flex items-center gap-2'}
              >
                <i className="fa-solid fa-plug"></i>
                测试连接
              </button>

              {currentConfig.id && (
                <button
                  onClick={initializeTables}
                  disabled={!currentConfig.connectionString || isInitializing || isLoading}
                  className={buttonVariants.danger + ' px-6 py-3 flex items-center gap-2'}
                >
                  {isInitializing ? (
                    <>
                      <i className="fa-solid fa-spinner fa-spin"></i>
                      初始化中...
                    </>
                  ) : (
                    <>
                      <i className="fa-solid fa-database"></i>
                      初始化表结构
                    </>
                  )}
                </button>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
