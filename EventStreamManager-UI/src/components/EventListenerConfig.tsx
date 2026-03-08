import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { 
    EventConfig, 
    DatabaseTypeWithActiveConfig,
    StatisticsResponse,
    StartCondition,
    DatabaseConfig
} from '@/types/event-listener';
import { getApiUrl } from '@/config/api.config';

export default function EventListenerConfig() {
    const [activeDatabase, setActiveDatabase] = useState<string>('');
    const [isLoading, setIsLoading] = useState(false);
    
    // 监听起始条件
    const [startCondition, setStartCondition] = useState<StartCondition>({
        type: 'time',
        timeValue: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString().slice(0, 16),
        idValue: ''
    });
    
    // 事件监听配置数据
    const [eventConfigs, setEventConfigs] = useState<Record<string, EventConfig>>({
        ultrasound: {
            scanFrequency: 60,
            batchSize: 50,
            enabled: true,
            tableName: 'event_log',
            primaryKey: 'event_id',
            timestampField: 'create_time'
        },
        radiology: {
            scanFrequency: 30,
            batchSize: 100,
            enabled: false,
            tableName: 'rad_event_log',
            primaryKey: 'event_id',
            timestampField: 'log_time'
        },
        endoscopy: {
            scanFrequency: 45,
            batchSize: 75,
            enabled: true,
            tableName: 'endo_event_log',
            primaryKey: 'id',
            timestampField: 'create_dt'
        }
    });

    // 数据库类型列表（包含激活配置）
    const [databaseTypes, setDatabaseTypes] = useState<DatabaseTypeWithActiveConfig[]>([]);

    // 当前选中的数据库的激活配置
    const [activeDatabaseConfig, setActiveDatabaseConfig] = useState<DatabaseConfig | null>(null);

    // 统计数据
    const [statistics, setStatistics] = useState<StatisticsResponse | null>(null);

    // 当前配置
    const currentConfig = activeDatabase ? eventConfigs[activeDatabase] : null;

    // 获取API地址
    const getEventListenerApiUrl = (path: string = '') => {
        return getApiUrl(`/api/EventListenerConfig${path}`);
    };

    // 获取数据库配置API地址
    const getDatabaseConfigApiUrl = (path: string = '') => {
        return getApiUrl(`/api/DatabaseConfig${path}`);
    };

    // 加载所有事件监听配置
    const loadAllConfigs = async () => {
        setIsLoading(true);
        try {
            const response = await fetch(getEventListenerApiUrl());
            if (!response.ok) throw new Error('获取配置失败');
            
            const data = await response.json();
            
            // 更新配置
            setEventConfigs(prev => ({
                ...prev,
                ultrasound: data.databases?.ultrasound || prev.ultrasound,
                radiology: data.databases?.radiology || prev.radiology,
                endoscopy: data.databases?.endoscopy || prev.endoscopy
            }));
        } catch (error) {
            console.error('加载配置失败:', error);
            toast.error('加载事件监听配置失败');
        } finally {
            setIsLoading(false);
        }
    };

    // 加载指定类型的配置
    const loadConfigByType = async (type: string) => {
        setIsLoading(true);
        try {
            const response = await fetch(getEventListenerApiUrl(`/${type}`));
            if (!response.ok) throw new Error('获取配置失败');
            
            const data: EventConfig = await response.json();
            
            setEventConfigs(prev => ({
                ...prev,
                [type]: data
            }));

            // 加载配置后同时加载起始条件
            await loadStartCondition(type);
        } catch (error) {
            console.error('加载配置失败:', error);
            toast.error(`加载${getDatabaseTypeLabel(type)}配置失败`);
        } finally {
            setIsLoading(false);
        }
    };

    // 加载起始条件
    const loadStartCondition = async (type: string) => {
        try {
            const response = await fetch(getEventListenerApiUrl(`/${type}/start-condition`));
            if (!response.ok) {
                if (response.status !== 404) {
                    throw new Error('获取起始条件失败');
                }
                return;
            }
            
            const data: StartCondition = await response.json();
            setStartCondition(data);
        } catch (error) {
            console.error('加载起始条件失败:', error);
        }
    };

    // 加载数据库类型列表（包含激活配置）
    const loadDatabaseTypesWithActiveConfig = async () => {
        try {
            const response = await fetch(getDatabaseConfigApiUrl('/types-with-active-config'));
            if (!response.ok) throw new Error('获取数据库类型失败');
            
            const data: DatabaseTypeWithActiveConfig[] = await response.json();
            setDatabaseTypes(data);
            
            // 如果有数据，设置第一个为激活数据库，并保存其激活配置
            if (data.length > 0) {
                setActiveDatabase(data[0].value);
                setActiveDatabaseConfig(data[0].activeConfig);
            }
        } catch (error) {
            console.error('加载数据库类型失败:', error);
            toast.error('加载数据库类型失败');
            
            // 如果接口失败，尝试加载普通类型作为降级方案
            loadDatabaseTypesFallback();
        }
    };

    // 降级方案：加载普通数据库类型
    const loadDatabaseTypesFallback = async () => {
        try {
            const response = await fetch(getDatabaseConfigApiUrl('/types'));
            if (!response.ok) throw new Error('获取数据库类型失败');
            
            const data = await response.json();
            const types: DatabaseTypeWithActiveConfig[] = data.map((item: any) => ({
                value: item.value,
                label: item.label,
                activeConfig: null
            }));
            
            setDatabaseTypes(types);
            
            if (types.length > 0) {
                setActiveDatabase(types[0].value);
            }
        } catch (error) {
            console.error('加载数据库类型失败:', error);
        }
    };

    // 加载指定数据库类型的激活配置
    const loadActiveConfigForType = async (type: string) => {
        try {
            // 先检查是否已经在types列表中有了activeConfig
            const typeInfo = databaseTypes.find(t => t.value === type);
            if (typeInfo?.activeConfig) {
                setActiveDatabaseConfig(typeInfo.activeConfig);
                return;
            }

            // 如果没有，单独请求激活配置
            const response = await fetch(getDatabaseConfigApiUrl(`/${type}/active-config`));
            if (!response.ok) {
                if (response.status !== 404) {
                    throw new Error('获取激活配置失败');
                }
                setActiveDatabaseConfig(null);
                return;
            }
            
            const data: DatabaseConfig = await response.json();
            setActiveDatabaseConfig(data);
        } catch (error) {
            console.error('加载激活配置失败:', error);
            setActiveDatabaseConfig(null);
        }
    };

    // 获取统计数据
    const loadStatistics = async () => {
        try {
            const response = await fetch(getEventListenerApiUrl('/statistics'));
            if (!response.ok) throw new Error('获取统计数据失败');
            
            const data: StatisticsResponse = await response.json();
            setStatistics(data);
        } catch (error) {
            console.error('加载统计数据失败:', error);
        }
    };

    // 初始化加载
    useEffect(() => {
        loadAllConfigs();
        loadDatabaseTypesWithActiveConfig();
        loadStatistics();
    }, []);

    // 当切换数据库时，加载对应数据和激活配置
    useEffect(() => {
        if (activeDatabase) {
            loadConfigByType(activeDatabase);
            loadActiveConfigForType(activeDatabase);
        }
    }, [activeDatabase]);

    // 处理配置更改
    const handleConfigChange = (field: keyof EventConfig, value: any) => {
        if (!activeDatabase) return;
        setEventConfigs(prev => ({
            ...prev,
            [activeDatabase]: {
                ...prev[activeDatabase],
                [field]: value
            }
        }));
    };

    // 处理起始条件更改
    const handleStartConditionChange = (field: keyof StartCondition, value: any) => {
        setStartCondition(prev => ({
            ...prev,
            [field]: value
        }));
    };

    // 保存起始条件
    const saveStartCondition = async () => {
        if (!activeDatabase) return;
        
        try {
            const response = await fetch(getEventListenerApiUrl(`/${activeDatabase}/start-condition`), {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(startCondition)
            });

            if (!response.ok) {
                if (response.status === 404) {
                    throw new Error('未找到数据库配置');
                }
                throw new Error('保存起始条件失败');
            }

            toast.success('监听起始条件已更新');
        } catch (error) {
            console.error('保存起始条件失败:', error);
            toast.error('保存起始条件失败');
            throw error;
        }
    };

    // 应用配置
    const applyConfig = async () => {
        if (!activeDatabase || !currentConfig) return;
        
        setIsLoading(true);
        try {
            // 先保存配置
            const configResponse = await fetch(getEventListenerApiUrl(`/${activeDatabase}`), {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(currentConfig)
            });

            if (!configResponse.ok) throw new Error('保存配置失败');

            const updatedConfig: EventConfig = await configResponse.json();
            
            setEventConfigs(prev => ({
                ...prev,
                [activeDatabase]: updatedConfig
            }));

            // 再保存起始条件
            await saveStartCondition();

            toast.success(`${getDatabaseTypeLabel(activeDatabase)}数据库事件监听配置已更新`);
            
            // 刷新统计数据
            loadStatistics();
        } catch (error) {
            console.error('保存配置失败:', error);
            toast.error('保存配置失败');
        } finally {
            setIsLoading(false);
        }
    };

    // 切换启用状态
    const toggleEnabled = async (enabled: boolean) => {
        if (!activeDatabase) return;
        
        try {
            const response = await fetch(getEventListenerApiUrl(`/${activeDatabase}/toggle?enabled=${enabled}`), {
                method: 'PATCH'
            });

            if (!response.ok) throw new Error('切换状态失败');

            handleConfigChange('enabled', enabled);
            toast.success(enabled ? '监听已启用' : '监听已禁用');
        } catch (error) {
            console.error('切换状态失败:', error);
            toast.error('切换状态失败');
        }
    };

    // 重置配置
    const resetConfig = async () => {
        if (!activeDatabase) return;
        
        try {
            const response = await fetch(getEventListenerApiUrl(`/${activeDatabase}/reset`), {
                method: 'POST'
            });

            if (!response.ok) throw new Error('重置配置失败');

            const defaultConfig: EventConfig = await response.json();
            
            setEventConfigs(prev => ({
                ...prev,
                [activeDatabase]: defaultConfig
            }));

            // 重置后重新加载起始条件
            await loadStartCondition(activeDatabase);

            toast.success('配置已重置为默认值');
        } catch (error) {
            console.error('重置配置失败:', error);
            toast.error('重置配置失败');
        }
    };

    // 获取数据库类型标签
    const getDatabaseTypeLabel = (type: string): string => {
        const found = databaseTypes.find(t => t.value === type);
        return found ? found.label : type;
    };

    // 获取当前数据库的激活配置显示文本
    const getActiveConfigDisplay = (): string => {
        if (!activeDatabaseConfig) return '未配置';
        return `${activeDatabaseConfig.name} (${activeDatabaseConfig.driver})`;
    };

    return (
        <div className="space-y-6">
            {/* 加载指示器 */}
            {isLoading && (
                <div className="fixed top-4 right-4 z-50">
                    <div className="bg-blue-500 text-white px-4 py-2 rounded-lg shadow-lg flex items-center gap-2">
                        <i className="fa-solid fa-spinner fa-spin"></i>
                        <span>加载中...</span>
                    </div>
                </div>
            )}

            <div className="flex items-center justify-between">
                <h2 className="text-2xl font-bold">事件监听配置</h2>
                
                {/* 刷新按钮 */}
                <button
                    onClick={() => {
                        if (activeDatabase) {
                            loadConfigByType(activeDatabase);
                        }
                    }}
                    className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 transition-colors"
                >
                    <i className="fa-solid fa-rotate-right"></i>
                    刷新数据
                </button>
            </div>
            
            {/* 数据库类型选择器 */}
            <div className="flex gap-4 flex-wrap">
                {databaseTypes.length > 0 ? (
                    databaseTypes.map((type) => (
                        <button
                            key={type.value}
                            onClick={() => setActiveDatabase(type.value)}
                            className={`rounded-lg px-6 py-3 font-medium transition-all relative ${
                                activeDatabase === type.value
                                    ? 'bg-blue-600 text-white shadow-md'
                                    : 'bg-white text-gray-700 shadow border border-gray-200 hover:border-blue-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700'
                            }`}
                        >
                            {type.label}
                            {type.activeConfig && (
                                <span className="absolute -top-2 -right-2 w-4 h-4 bg-green-500 rounded-full border-2 border-white dark:border-gray-800" 
                                      title={`激活配置: ${type.activeConfig.name}`}>
                                </span>
                            )}
                        </button>
                    ))
                ) : (
                    <div className="text-gray-500 py-3">加载数据库类型中...</div>
                )}
            </div>
            
            {/* 激活配置信息 */}
            {activeDatabase && (
                <div className="bg-blue-50 border-l-4 border-blue-500 p-4 rounded dark:bg-blue-900/20">
                    <div className="flex items-center">
                        <i className="fa-solid fa-database text-blue-600 mr-3 text-xl"></i>
                        <div>
                            <h3 className="font-medium text-blue-800 dark:text-blue-300">
                                当前激活的数据库配置
                            </h3>
                            <p className="text-sm text-blue-600 dark:text-blue-400">
                                {getActiveConfigDisplay()}
                            </p>
                        </div>
                    </div>
                </div>
            )}
            
            {/* 如果没有数据库类型，显示提示 */}
            {databaseTypes.length === 0 && !isLoading && (
                <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 rounded">
                    <div className="flex">
                        <div className="flex-shrink-0">
                            <i className="fa-solid fa-exclamation-triangle text-yellow-400"></i>
                        </div>
                        <div className="ml-3">
                            <p className="text-sm text-yellow-700">
                                未加载到数据库类型，请检查数据库配置。
                            </p>
                        </div>
                    </div>
                </div>
            )}
            
            {/* 配置表单 - 只在有activeDatabase和currentConfig时显示 */}
            {activeDatabase && currentConfig && (
                <>
                    <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
                        <div className="mb-6 flex items-center justify-between">
                            <h3 className="text-lg font-semibold">{getDatabaseTypeLabel(activeDatabase)} - 监听配置</h3>
                            
                            <div className="flex items-center gap-2">
                                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                                    监听状态
                                </span>
                                <label className="relative inline-flex h-6 w-11 items-center rounded-full bg-gray-200 p-1 transition-colors duration-200 ease-in-out dark:bg-gray-700">
                                    <input
                                        type="checkbox"
                                        checked={currentConfig.enabled}
                                        onChange={(e) => toggleEnabled(e.target.checked)}
                                        className="peer sr-only"
                                    />
                                    <span className={`absolute left-1 flex h-4 w-4 items-center justify-center rounded-full bg-white text-xs transition-all duration-200 ease-in-out peer-checked:translate-x-5 peer-checked:text-blue-600 ${
                                        currentConfig.enabled ? 'bg-blue-600 text-white' : 'text-gray-600'
                                    }`}>
                                        {currentConfig.enabled ? <i className="fa-solid fa-check"></i> : ''}
                                    </span>
                                </label>
                            </div>
                        </div>
                        
                        {/* 配置表单内容保持不变... */}
                        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                            {/* 原有的配置输入控件 */}
                            <div>
                                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                                    扫描频率 (秒)
                                </label>
                                <div className="flex items-center gap-3">
                                    <input
                                        type="range"
                                        min="5"
                                        max="300"
                                        step="5"
                                        value={currentConfig.scanFrequency}
                                        onChange={(e) => handleConfigChange('scanFrequency', Number(e.target.value))}
                                        className="h-2 flex-1 appearance-none rounded-lg bg-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700"
                                    />
                                    <span className="w-12 text-center font-medium">{currentConfig.scanFrequency}</span>
                                </div>
                            </div>
                            
                            <div>
                                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                                    批处理数量
                                </label>
                                <div className="flex items-center gap-3">
                                    <input
                                        type="range"
                                        min="10"
                                        max="500"
                                        step="10"
                                        value={currentConfig.batchSize}
                                        onChange={(e) => handleConfigChange('batchSize', Number(e.target.value))}
                                        className="h-2 flex-1 appearance-none rounded-lg bg-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700"
                                    />
                                    <span className="w-12 text-center font-medium">{currentConfig.batchSize}</span>
                                </div>
                            </div>
                            
                            <div>
                                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                                    事件表名称
                                </label>
                                <input
                                    type="text"
                                    value={currentConfig.tableName}
                                    onChange={(e) => handleConfigChange('tableName', e.target.value)}
                                    className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                                />
                            </div>
                            
                            <div>
                                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                                    主键字段
                                </label>
                                <input
                                    type="text"
                                    value={currentConfig.primaryKey}
                                    onChange={(e) => handleConfigChange('primaryKey', e.target.value)}
                                    className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                                />
                            </div>
                            
                            <div>
                                <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                                    时间戳字段
                                </label>
                                <input
                                    type="text"
                                    value={currentConfig.timestampField}
                                    onChange={(e) => handleConfigChange('timestampField', e.target.value)}
                                    className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                                />
                            </div>
                        </div>

                        {/* 监听起始条件配置 */}
                        <div className="mt-8 border-t border-gray-200 dark:border-gray-700 pt-6">
                            <h4 className="text-md font-semibold mb-4 flex items-center">
                                <i className="fa-solid fa-flag-checkered text-blue-600 mr-2"></i>
                                监听起始条件
                            </h4>
                            <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
                                设置监听器开始处理事件的起始点，可以选择基于时间或基于事件ID
                            </p>

                            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 mb-6">
                                {/* 起始条件类型选择 */}
                                <div className={`rounded-lg border p-4 cursor-pointer transition-all ${
                                    startCondition.type === 'time'
                                        ? 'border-blue-600 bg-blue-50 dark:border-blue-500 dark:bg-blue-900/30'
                                        : 'border-gray-300 bg-white hover:border-blue-300 dark:border-gray-700 dark:bg-gray-800 dark:hover:border-blue-700'
                                }`} onClick={() => handleStartConditionChange('type', 'time')}>
                                    <div className={`w-10 h-10 rounded-full flex items-center justify-center mb-3 ${
                                        startCondition.type === 'time' ? 'bg-blue-100 text-blue-600 dark:bg-blue-900/50 dark:text-blue-400' : 'bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400'
                                    }`}>
                                        <i className="fa-solid fa-calendar"></i>
                                    </div>
                                    <h5 className="font-medium mb-1">基于时间</h5>
                                    <p className="text-sm text-gray-500 dark:text-gray-400">
                                        从指定时间之后的事件开始监听
                                    </p>
                                </div>

                                <div className={`rounded-lg border p-4 cursor-pointer transition-all ${
                                    startCondition.type === 'id'
                                        ? 'border-blue-600 bg-blue-50 dark:border-blue-500 dark:bg-blue-900/30'
                                        : 'border-gray-300 bg-white hover:border-blue-300 dark:border-gray-700 dark:bg-gray-800 dark:hover:border-blue-700'
                                }`} onClick={() => handleStartConditionChange('type', 'id')}>
                                    <div className={`w-10 h-10 rounded-full flex items-center justify-center mb-3 ${
                                        startCondition.type === 'id' ? 'bg-blue-100 text-blue-600 dark:bg-blue-900/50 dark:text-blue-400' : 'bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400'
                                    }`}>
                                        <i className="fa-solid fa-hashtag"></i>
                                    </div>
                                    <h5 className="font-medium mb-1">基于事件ID</h5>
                                    <p className="text-sm text-gray-500 dark:text-gray-400">
                                        从ID大于指定值的事件开始监听
                                    </p>
                                </div>
                            </div>

                            {/* 根据选择的类型显示输入控件 */}
                            <div className="mt-4">
                                {startCondition.type === 'time' ? (
                                    <div className="space-y-2">
                                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                                            起始时间
                                        </label>
                                        <input
                                            type="datetime-local"
                                            value={startCondition.timeValue}
                                            onChange={(e) => handleStartConditionChange('timeValue', e.target.value)}
                                            className="w-full max-w-md rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                                        />
                                        <p className="text-xs text-gray-500 dark:text-gray-400">
                                            监听器将处理 {currentConfig.timestampField} 大于此时间的事件
                                        </p>
                                    </div>
                                ) : (
                                    <div className="space-y-2">
                                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                                            起始事件ID
                                        </label>
                                        <input
                                            type="text"
                                            value={startCondition.idValue}
                                            onChange={(e) => handleStartConditionChange('idValue', e.target.value)}
                                            placeholder={`请输入起始${currentConfig.primaryKey}`}
                                            className="w-full max-w-md rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                                        />
                                        <p className="text-xs text-gray-500 dark:text-gray-400">
                                            监听器将处理 {currentConfig.primaryKey} 大于此值的事件
                                        </p>
                                    </div>
                                )}
                            </div>
                        </div>
                    
                        <div className="mt-8 flex justify-end gap-3 border-t border-gray-200 dark:border-gray-700 pt-6">
                            <button
                                onClick={resetConfig}
                                className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
                            >
                                <i className="fa-solid fa-rotate-left mr-1"></i> 重置
                            </button>
                            
                            <button
                                onClick={applyConfig}
                                disabled={isLoading}
                                className={`rounded-md px-6 py-2 text-sm font-medium transition-colors ${
                                    isLoading
                                        ? 'bg-gray-400 text-white cursor-not-allowed'
                                        : 'bg-blue-600 text-white hover:bg-blue-700'
                                }`}
                            >
                                <i className="fa-solid fa-check mr-1"></i> 应用配置
                            </button>
                        </div>
                    </div>
                    
                    {/* 监听统计信息 */}
                    <div className="grid grid-cols-1 gap-6 sm:grid-cols-3">
                        <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
                            <h4 className="mb-2 text-sm font-medium text-gray-500 dark:text-gray-400">监听状态</h4>
                            <p className={`text-2xl font-bold ${
                                currentConfig.enabled ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
                            }`}>
                                {currentConfig.enabled ? '已启用' : '已禁用'}
                            </p>
                        </div>
                        
                        <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
                            <h4 className="mb-2 text-sm font-medium text-gray-500 dark:text-gray-400">扫描间隔</h4>
                            <p className="text-2xl font-bold">{currentConfig.scanFrequency}秒</p>
                        </div>

                        <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
                            <h4 className="mb-2 text-sm font-medium text-gray-500 dark:text-gray-400">已处理事件</h4>
                            <p className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                                {statistics?.totalEventsProcessed || 0}
                            </p>
                        </div>
                    </div>

                    {/* 当前监听起始条件显示 */}
                    <div className="rounded-xl bg-white p-4 shadow-md dark:bg-gray-800 dark:shadow-lg border-l-4 border-blue-500">
                        <h4 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2">当前监听起始条件</h4>
                        <div className="flex items-center">
                            <i className={`fa-solid mr-3 text-xl ${
                                startCondition.type === 'time' ? 'fa-calendar text-blue-500' : 'fa-hashtag text-green-500'
                            }`}></i>
                            <div>
                                <p className="font-medium">
                                    {startCondition.type === 'time' 
                                        ? `从 ${new Date(startCondition.timeValue).toLocaleString()} 开始监听`
                                        : `从 ${currentConfig.primaryKey} > ${startCondition.idValue || '未设置'} 开始监听`
                                    }
                                </p>
                                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                                    基于 {startCondition.type === 'time' ? currentConfig.timestampField : currentConfig.primaryKey} 字段
                                </p>
                            </div>
                        </div>
                    </div>
                </>
            )}
        </div>
    );
}