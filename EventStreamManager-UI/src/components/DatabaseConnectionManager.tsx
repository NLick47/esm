import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { DatabaseConfig, DatabaseType, DriverType } from '@/types/database';
import { getApiUrl } from '@/config/api.config';
const API_URL = getApiUrl('/api/DatabaseConfig');

// 数据库连接管理模块
export default function DatabaseConnectionManager() {
  const [activeDatabase, setActiveDatabase] = useState<DatabaseType>('');
  const [currentConfigIndex, setCurrentConfigIndex] = useState(0);
  const [connectionStatus, setConnectionStatus] = useState<'disconnected' | 'connecting' | 'connected'>('disconnected');
  const [isLoading, setIsLoading] = useState(false);
  const [isAddingType, setIsAddingType] = useState(false);
  const [newTypeName, setNewTypeName] = useState('');
  const [newTypeLabel, setNewTypeLabel] = useState('');

  // 数据库配置数据 - 使用动态对象，key是数据库类型标识
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

  // 获取数据库类型列表
  const [databaseTypes, setDatabaseTypes] = useState<{ value: string, label: string }[]>([]);

  // 加载配置数据
  useEffect(() => {
    fetchConfigs();
    fetchDatabaseTypes();
  }, []);

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
      // 如果没有配置，创建一个默认的空配置
      createNewConfig();
    }
  }, [activeDatabase, currentConfigIndex, databaseConfigs]);

  // 获取所有配置
  const fetchConfigs = async () => {
    setIsLoading(true);
    try {
      const response = await fetch(API_URL);
      if (!response.ok) throw new Error('获取配置失败');

      const data = await response.json();
      setDatabaseConfigs(data || {});

      // 如果有数据库类型，默认选中第一个
      if (databaseTypes.length > 0 && !activeDatabase) {
        setActiveDatabase(databaseTypes[0].value);
      }
    } catch (error) {
      console.error('获取配置失败:', error);
      toast.error('加载数据库配置失败');
    } finally {
      setIsLoading(false);
    }
  };

  // 获取指定类型的配置
  const fetchConfigsByType = async (type: string) => {
    try {
      const response = await fetch(`${API_URL}/${type}`);
      if (!response.ok) throw new Error('获取配置失败');

      const data = await response.json();
      setDatabaseConfigs(prev => ({
        ...prev,
        [type]: data
      }));

      // 找到该类型的激活配置并自动选中
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

  // 获取数据库类型列表
  const fetchDatabaseTypes = async () => {
    try {
      const response = await fetch(`${API_URL}/types`);
      if (response.ok) {
        const data = await response.json();
        if (data && data.length > 0) {
          setDatabaseTypes(data);
          if (!activeDatabase) {
            setActiveDatabase(data[0].value);
          }
        }
      }
    } catch (error) {
      console.error('获取数据库类型失败:', error);
    }
  };

  // 添加新的数据库类型
  const addDatabaseType = async () => {
    if (!newTypeName.trim() || !newTypeLabel.trim()) {
      toast.error('请输入类型标识和名称');
      return;
    }

    // 检查是否已存在
    if (databaseTypes.some(t => t.value === newTypeName)) {
      toast.error('该类型标识已存在');
      return;
    }

    setIsLoading(true);
    try {
      const response = await fetch(`${API_URL}/types`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          value: newTypeName,
          label: newTypeLabel
        })
      });

      if (!response.ok) throw new Error('添加失败');

      const newType = await response.json();

      // 更新类型列表
      setDatabaseTypes(prev => [...prev, newType]);

      // 初始化该类型的配置数组
      setDatabaseConfigs(prev => ({
        ...prev,
        [newTypeName]: []
      }));

      // 切换到新类型
      setActiveDatabase(newTypeName);

      // 创建默认配置
      createNewConfig();

      // 重置表单
      setIsAddingType(false);
      setNewTypeName('');
      setNewTypeLabel('');

      toast.success('数据库类型添加成功');
    } catch (error) {
      console.error('添加数据库类型失败:', error);
      toast.error('添加数据库类型失败');
    } finally {
      setIsLoading(false);
    }
  };

  // 删除数据库类型
  const deleteDatabaseType = async (type: string) => {
    if (!window.confirm(`确定要删除${getDatabaseTypeLabel(type)}及其所有配置吗？`)) {
      return;
    }

    setIsLoading(true);
    try {
      const response = await fetch(`${API_URL}/types/${type}`, {
        method: 'DELETE'
      });

      if (!response.ok) throw new Error('删除失败');

      // 更新类型列表
      setDatabaseTypes(prev => prev.filter(t => t.value !== type));

      // 删除该类型的配置
      const newConfigs = { ...databaseConfigs };
      delete newConfigs[type];
      setDatabaseConfigs(newConfigs);

      // 如果删除的是当前选中的类型，切换到第一个
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
      setIsLoading(false);
    }
  };

  // 处理数据库类型切换
  const handleDatabaseTypeChange = (type: string) => {
    setActiveDatabase(type);
    setCurrentConfigIndex(0);
    setConnectionStatus('disconnected');
    fetchConfigsByType(type);
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

    setIsLoading(true);
    try {
      const url = currentConfig.id
        ? `${API_URL}/${activeDatabase}/${currentConfig.id}`
        : `${API_URL}/${activeDatabase}`;

      const method = currentConfig.id ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(currentConfig)
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || '保存失败');
      }

      const savedConfig = await response.json();

      // 更新本地状态
      setDatabaseConfigs(prev => {
        const currentTypeConfigs = prev[activeDatabase] || [];
        const newConfigs = { ...prev };

        if (currentConfig.id) {
          // 更新现有配置
          newConfigs[activeDatabase] = currentTypeConfigs.map(c =>
            c.id === savedConfig.id ? savedConfig : c
          );
        } else {
          // 添加新配置
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
      setIsLoading(false);
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
      const response = await fetch(`${API_URL}/test-connection`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          connectionString: currentConfig.connectionString,
          driver: currentConfig.driver
        })
      });

      const result = await response.json();

      setConnectionStatus(result.success ? 'connected' : 'disconnected');

      if (result.success) {
        toast.success(`数据库连接测试成功 (响应时间: ${result.responseTime}ms)`);
      } else {
        toast.error(`无法连接到数据库: ${result.message}`);
      }
    } catch (error) {
      console.error('连接测试失败:', error);
      setConnectionStatus('disconnected');
      toast.error('连接测试失败，请检查网络或API服务');
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

    setIsLoading(true);
    try {
      const response = await fetch(`${API_URL}/${activeDatabase}/${currentConfig.id}`, {
        method: 'DELETE'
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || '删除失败');
      }

      // 获取删除前的激活配置ID
      const wasActive = currentConfig.isActive;

      // 更新本地状态
      setDatabaseConfigs(prev => {
        const newConfigs = { ...prev };
        newConfigs[activeDatabase] = (prev[activeDatabase] || []).filter(c => c.id !== currentConfig.id);
        return newConfigs;
      });

      // 如果删除的是激活配置，并且还有其他配置，需要设置新的激活配置
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
      setIsLoading(false);
    }
  };

  // 设置为当前使用的配置
  const setAsActiveConfig = async (configId?: string) => {
    const targetId = configId || currentConfig.id;

    if (!targetId) {
      toast.error('请先保存配置');
      return;
    }

    setIsLoading(true);
    try {
      const response = await fetch(`${API_URL}/${activeDatabase}/${targetId}/set-active`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        }
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || '设置失败');
      }

      // 更新所有配置的isActive状态
      setDatabaseConfigs(prev => {
        const currentTypeConfigs = prev[activeDatabase] || [];
        const newConfigs = { ...prev };
        newConfigs[activeDatabase] = currentTypeConfigs.map(config => ({
          ...config,
          isActive: config.id === targetId
        }));
        return newConfigs;
      });

      // 如果设置的是当前选中的配置，更新currentConfig的isActive
      if (currentConfig.id === targetId) {
        setCurrentConfig(prev => ({
          ...prev,
          isActive: true
        }));
      } else {
        // 如果设置的不是当前选中的配置，找到目标配置的索引并切换
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
      setIsLoading(false);
    }
  };

  // 获取数据库类型标签
  const getDatabaseTypeLabel = (type: string): string => {
    const found = databaseTypes.find(t => t.value === type);
    return found?.label || type;
  };

  // 解析连接字符串示例
  const getConnectionStringExamples = (driver: string): string => {
    switch (driver) {
      case 'SQL Server':
        return 'Server=localhost,1433;Database=mydb;User Id=sa;Password=123456;TrustServerCertificate=true;';
      case 'MySQL':
        return 'Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=123456;';
      case 'PostgreSQL':
        return 'Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=123456;';
      case 'Oracle':
        return 'Data Source=localhost:1521/ORCL;User Id=system;Password=123456;';
      default:
        return 'Server=localhost;Database=mydb;User Id=user;Password=pass;';
    }
  };

  // 获取当前激活的配置
  const getActiveConfig = (type: string): DatabaseConfig | undefined => {
    return (databaseConfigs[type] || []).find(config => config.isActive === true);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">数据库连接管理</h2>
        {isLoading && (
          <div className="flex items-center gap-2 text-blue-600">
            <i className="fa-solid fa-spinner fa-spin"></i>
            <span>加载中...</span>
          </div>
        )}
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
                {/* 删除类型按钮 - 只在非活动状态且类型数大于1时显示 */}
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

        {/* 添加新类型按钮 */}
        <button
          onClick={() => setIsAddingType(true)}
          className="rounded-lg bg-green-600 text-white px-4 py-3 font-medium hover:bg-green-700 transition-colors flex items-center gap-2"
        >
          <i className="fa-solid fa-plus"></i>
          新增连接类型
        </button>
      </div>

      {/* 添加新类型的表单 */}
      {isAddingType && (
        <div className="rounded-xl bg-gray-50 dark:bg-gray-700 p-4 border border-gray-200 dark:border-gray-600">
          <h3 className="text-lg font-medium mb-4">新增数据库连接类型</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                类型标识 <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={newTypeName}
                onChange={(e) => setNewTypeName(e.target.value)}
                className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                placeholder="例如：mri"
              />
              <p className="mt-1 text-xs text-gray-500">唯一标识，只能包含字母、数字和下划线</p>
            </div>
            <div>
              <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                显示名称 <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={newTypeLabel}
                onChange={(e) => setNewTypeLabel(e.target.value)}
                className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                placeholder="例如：核磁共振数据库"
              />
            </div>
          </div>
          <div className="flex justify-end gap-3 mt-4">
            <button
              onClick={() => {
                setIsAddingType(false);
                setNewTypeName('');
                setNewTypeLabel('');
              }}
              className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-100 dark:border-gray-600 dark:hover:bg-gray-600"
            >
              取消
            </button>
            <button
              onClick={addDatabaseType}
              disabled={!newTypeName || !newTypeLabel || isLoading}
              className={`px-4 py-2 rounded-lg text-white ${!newTypeName || !newTypeLabel || isLoading
                  ? 'bg-gray-400 cursor-not-allowed'
                  : 'bg-blue-600 hover:bg-blue-700'
                }`}
            >
              添加
            </button>
          </div>
        </div>
      )}

      {/* 配置列表和操作 - 只在有选中的数据库类型时显示 */}
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

                {/* 激活标识 */}
                {currentConfig.isActive && (
                  <span className="inline-flex items-center gap-1 rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800 dark:bg-green-900/30 dark:text-green-400">
                    <i className="fa-solid fa-check-circle"></i>
                    当前使用中
                  </span>
                )}

                {!currentConfig.isActive && currentConfig.id && (
                  <span className="inline-flex items-center gap-1 rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800 dark:bg-gray-700 dark:text-gray-300">
                    <i className="fa-solid fa-circle"></i>
                    未使用
                  </span>
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

              {/* 设为当前使用按钮 - 只在配置不是激活状态时显示 */}
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
              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  配置名称
                </label>
                <input
                  type="text"
                  value={currentConfig.name}
                  onChange={(e) => handleConfigChange('name', e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  placeholder="例如：生产数据库"
                />
              </div>

              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  数据库驱动
                </label>
                <select
                  value={currentConfig.driver}
                  onChange={(e) => handleConfigChange('driver', e.target.value as DriverType)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                >
                  <option value={DriverType.SqlServer}>SQL Server</option>
                  <option value={DriverType.MySql}>MySql</option>
                  <option value={DriverType.PostgreSQL}>PostgreSql</option>
                  <option value={DriverType.Oracle}>Oracle</option>
                  <option value={DriverType.SqLite}>SQLite</option>
                </select>


              </div>

              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  连接字符串
                </label>
                <textarea
                  value={currentConfig.connectionString}
                  onChange={(e) => handleConfigChange('connectionString', e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white font-mono text-sm"
                  rows={3}
                  placeholder={getConnectionStringExamples(currentConfig.driver)}
                />
                <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                  示例: {getConnectionStringExamples(currentConfig.driver)}
                </p>
              </div>

              <div>
                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  连接超时 (秒)
                </label>
                <input
                  type="number"
                  value={currentConfig.timeout}
                  onChange={(e) => handleConfigChange('timeout', Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                  min="1"
                  max="300"
                />
              </div>

              {/* 显示当前配置的激活状态 */}
              {currentConfig.id && (
                <div className="flex items-center gap-2 p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                  <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                    激活状态:
                  </span>
                  {currentConfig.isActive ? (
                    <span className="inline-flex items-center gap-1 rounded-full bg-green-100 px-3 py-1 text-xs font-medium text-green-800 dark:bg-green-900/30 dark:text-green-400">
                      <i className="fa-solid fa-check-circle"></i>
                      当前使用中
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1 rounded-full bg-gray-100 px-3 py-1 text-xs font-medium text-gray-800 dark:bg-gray-600 dark:text-gray-300">
                      <i className="fa-solid fa-circle"></i>
                      未使用
                    </span>
                  )}
                </div>
              )}
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                onClick={saveCurrentConfig}
                disabled={isLoading || !currentConfig.name}
                className={`flex items-center gap-2 rounded-lg px-6 py-3 font-medium transition-colors ${isLoading || !currentConfig.name
                    ? 'bg-gray-400 text-white cursor-not-allowed'
                    : 'bg-green-600 text-white hover:bg-green-700'
                  }`}
              >
                <i className="fa-solid fa-save"></i>
                {currentConfig.id ? '更新配置' : '保存配置'}
              </button>

              <button
                onClick={testConnection}
                disabled={connectionStatus === 'connecting' || !currentConfig.connectionString || isLoading}
                className={`flex items-center gap-2 rounded-lg px-6 py-3 font-medium transition-colors ${connectionStatus === 'connecting' || !currentConfig.connectionString || isLoading
                    ? 'bg-gray-400 text-white cursor-not-allowed'
                    : 'bg-blue-600 text-white hover:bg-blue-700'
                  }`}
              >
                {connectionStatus === 'connecting' ? (
                  <i className="fa-solid fa-spinner fa-spin"></i>
                ) : (
                  <i className="fa-solid fa-plug"></i>
                )}
                {connectionStatus === 'connecting' ? '测试连接中...' : '测试连接'}
              </button>
            </div>
          </div>
        </>
      )}

      {/* 连接帮助信息 */}
      <div className="rounded-xl bg-blue-50 p-4 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800">
        <h4 className="mb-2 text-sm font-medium text-blue-800 dark:text-blue-300 flex items-center gap-2">
          <i className="fa-solid fa-circle-info"></i>
          连接字符串格式说明
        </h4>
        <div className="text-xs text-blue-700 dark:text-blue-400 space-y-1">
          <p><span className="font-semibold">SQL Server:</span> Server=服务器地址,端口;Database=数据库名;User Id=用户名;Password=密码;TrustServerCertificate=true;</p>
          <p><span className="font-semibold">MySQL:</span> Server=服务器地址;Port=端口;Database=数据库名;Uid=用户名;Pwd=密码;</p>
          <p><span className="font-semibold">PostgreSQL:</span> Host=服务器地址;Port=端口;Database=数据库名;Username=用户名;Password=密码;</p>
          <p><span className="font-semibold">Oracle:</span> Data Source=服务器地址:端口/服务名;User Id=用户名;Password=密码;</p>
        </div>
      </div>

      {/* 连接历史和状态信息 */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-4">
        <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
          <h4 className="mb-2 text-sm font-medium text-gray-500 dark:text-gray-400">总配置数</h4>
          <p className="text-2xl font-bold">
            {Object.values(databaseConfigs).reduce((sum, configs) => sum + configs.length, 0)}
          </p>
        </div>

        <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
          <h4 className="mb-2 text-sm font-medium text-gray-500 dark:text-gray-400">已连接数据库</h4>
          <p className="text-2xl font-bold text-green-600 dark:text-green-400">
            {connectionStatus === 'connected' ? 1 : 0}
          </p>
        </div>

        <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
          <h4 className="mb-2 text-sm font-medium text-gray-500 dark:text-gray-400">数据库类型数</h4>
          <p className="text-2xl font-bold">{databaseTypes.length}</p>
        </div>

        {/* 当前使用配置 */}
        <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg">
          <h4 className="mb-2 text-sm font-medium text-gray-500 dark:text-gray-400">当前使用配置</h4>
          <p className="text-lg font-bold truncate" title={currentConfig.name}>
            {(() => {
              if (!activeDatabase) return '未选择';
              const activeConfig = getActiveConfig(activeDatabase);
              return activeConfig ? activeConfig.name : '未设置';
            })()}
          </p>
          {activeDatabase && (() => {
            const activeConfig = getActiveConfig(activeDatabase);
            return activeConfig && (
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                {activeConfig.driver}
              </p>
            );
          })()}
        </div>
      </div>
    </div>
  );
}