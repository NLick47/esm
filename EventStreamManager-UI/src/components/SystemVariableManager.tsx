import { useState, useEffect, useMemo } from 'react';
import { toast } from 'sonner';
import { SystemVariable, SystemVariableRequest } from '@/types/system-variable';
import * as systemVariableService from '@/services/system-variable.service';

const EMPTY_FORM: SystemVariableRequest = {
  key: '',
  value: '',
  description: '',
  category: 'General'
};

export default function SystemVariableManager() {
  const [variables, setVariables] = useState<SystemVariable[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [editingVariable, setEditingVariable] = useState<SystemVariable | null>(null);
  const [formData, setFormData] = useState<SystemVariableRequest>({ ...EMPTY_FORM });
  const [showForm, setShowForm] = useState(false);

  // 加载变量列表
  const loadVariables = async () => {
    setIsLoading(true);
    try {
      const data = await systemVariableService.getAllVariables();
      setVariables(data || []);
    } catch (error: any) {
      console.error('获取系统变量失败:', error);
      toast.error(error.message || '加载系统变量失败');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadVariables();
  }, []);

  // 提取所有分类
  const categories = useMemo(() => {
    const set = new Set(variables.map(v => v.category));
    return Array.from(set).sort();
  }, [variables]);

  // 筛选后的变量
  const filteredVariables = useMemo(() => {
    return variables.filter(v => {
      const matchSearch = !searchTerm ||
        v.key.toLowerCase().includes(searchTerm.toLowerCase()) ||
        v.value.toLowerCase().includes(searchTerm.toLowerCase()) ||
        v.description.toLowerCase().includes(searchTerm.toLowerCase());
      const matchCategory = !categoryFilter || v.category === categoryFilter;
      return matchSearch && matchCategory;
    });
  }, [variables, searchTerm, categoryFilter]);

  // 打开新增表单
  const handleAdd = () => {
    setEditingVariable(null);
    setFormData({ ...EMPTY_FORM });
    setShowForm(true);
  };

  // 打开编辑表单
  const handleEdit = (variable: SystemVariable) => {
    setEditingVariable(variable);
    setFormData({
      key: variable.key,
      value: variable.value,
      description: variable.description,
      category: variable.category
    });
    setShowForm(true);
  };

  // 删除变量
  const handleDelete = async (variable: SystemVariable) => {
    if (!window.confirm(`确定要删除变量 "${variable.key}" 吗？`)) {
      return;
    }
    setIsLoading(true);
    try {
      await systemVariableService.deleteVariable(variable.id);
      setVariables(prev => prev.filter(v => v.id !== variable.id));
      toast.success(`变量 "${variable.key}" 已删除`);
    } catch (error: any) {
      console.error('删除变量失败:', error);
      toast.error(error.message || '删除失败');
    } finally {
      setIsLoading(false);
    }
  };

  // 复制值到剪贴板
  const handleCopyValue = async (value: string, key: string) => {
    try {
      await navigator.clipboard.writeText(value);
      toast.success(`已复制 "${key}" 的值`);
    } catch {
      toast.error('复制失败');
    }
  };

  // 保存表单
  const handleSave = async () => {
    if (!formData.key.trim()) {
      toast.error('变量键名不能为空');
      return;
    }

    setIsLoading(true);
    try {
      const saved = await systemVariableService.saveVariable(formData);

      setVariables(prev => {
        const index = prev.findIndex(v => v.id === saved.id);
        if (index >= 0) {
          const updated = [...prev];
          updated[index] = saved;
          return updated;
        }
        return [...prev, saved];
      });

      toast.success(editingVariable ? '变量已更新' : '变量已创建');
      setShowForm(false);
      setEditingVariable(null);
      setFormData({ ...EMPTY_FORM });
    } catch (error: any) {
      console.error('保存变量失败:', error);
      toast.error(error.message || '保存失败');
    } finally {
      setIsLoading(false);
    }
  };

  // 取消表单
  const handleCancel = () => {
    setShowForm(false);
    setEditingVariable(null);
    setFormData({ ...EMPTY_FORM });
  };

  // 表单字段变化
  const handleChange = (field: keyof SystemVariableRequest, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  return (
    <div className="space-y-6">
      {/* 标题栏 */}
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">系统变量管理</h2>
        <div className="flex items-center gap-3">
          {isLoading && (
            <div className="flex items-center gap-2 text-blue-600">
              <i className="fa-solid fa-spinner fa-spin"></i>
              <span>加载中...</span>
            </div>
          )}
          <button
            onClick={handleAdd}
            className="rounded-lg bg-green-600 text-white px-4 py-2 font-medium hover:bg-green-700 transition-colors flex items-center gap-2"
          >
            <i className="fa-solid fa-plus"></i>
            新增变量
          </button>
        </div>
      </div>

      {/* 搜索和筛选 */}
      <div className="flex flex-wrap items-center gap-4">
        <div className="flex-1 min-w-[200px]">
          <div className="relative">
            <i className="fa-solid fa-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"></i>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="搜索键名、值或描述..."
              className="w-full rounded-lg border border-gray-300 bg-white pl-10 pr-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-800 dark:text-white"
            />
          </div>
        </div>
        <div className="min-w-[160px]">
          <select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-800 dark:text-white"
          >
            <option value="">全部分类</option>
            {categories.map(cat => (
              <option key={cat} value={cat}>{cat}</option>
            ))}
          </select>
        </div>
        <button
          onClick={loadVariables}
          className="rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 hover:bg-gray-100 transition-colors dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
          title="刷新"
        >
          <i className="fa-solid fa-rotate-right"></i>
        </button>
      </div>

      {/* 变量列表表格 */}
      <div className="rounded-xl bg-white shadow-md dark:bg-gray-800 dark:shadow-lg overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="px-4 py-3 font-semibold text-gray-700 dark:text-gray-300">键名</th>
                <th className="px-4 py-3 font-semibold text-gray-700 dark:text-gray-300">值</th>
                <th className="px-4 py-3 font-semibold text-gray-700 dark:text-gray-300">描述</th>
                <th className="px-4 py-3 font-semibold text-gray-700 dark:text-gray-300">分类</th>
                <th className="px-4 py-3 font-semibold text-gray-700 dark:text-gray-300">更新时间</th>
                <th className="px-4 py-3 font-semibold text-gray-700 dark:text-gray-300 text-right">操作</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {filteredVariables.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-gray-500 dark:text-gray-400">
                    {isLoading ? '加载中...' : '暂无系统变量'}
                  </td>
                </tr>
              ) : (
                filteredVariables.map(variable => (
                  <tr key={variable.id} className="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
                    <td className="px-4 py-3 font-mono text-blue-600 dark:text-blue-400 font-medium">
                      {variable.key}
                    </td>
                    <td className="px-4 py-3 max-w-xs">
                      <div className="truncate font-mono text-xs text-gray-600 dark:text-gray-400" title={variable.value}>
                        {variable.value}
                      </div>
                    </td>
                    <td className="px-4 py-3 text-gray-600 dark:text-gray-400">
                      {variable.description || '-'}
                    </td>
                    <td className="px-4 py-3">
                      <span className="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800 dark:bg-gray-700 dark:text-gray-300">
                        {variable.category}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                      {new Date(variable.updatedAt).toLocaleString()}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => handleCopyValue(variable.value, variable.key)}
                          className="rounded p-1.5 text-gray-500 hover:bg-gray-100 hover:text-gray-700 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-gray-200"
                          title="复制值"
                        >
                          <i className="fa-regular fa-copy"></i>
                        </button>
                        <button
                          onClick={() => handleEdit(variable)}
                          className="rounded p-1.5 text-blue-500 hover:bg-blue-50 hover:text-blue-700 dark:text-blue-400 dark:hover:bg-blue-900/20 dark:hover:text-blue-300"
                          title="编辑"
                        >
                          <i className="fa-solid fa-pen-to-square"></i>
                        </button>
                        <button
                          onClick={() => handleDelete(variable)}
                          className="rounded p-1.5 text-red-500 hover:bg-red-50 hover:text-red-700 dark:text-red-400 dark:hover:bg-red-900/20 dark:hover:text-red-300"
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
        <div className="border-t border-gray-200 dark:border-gray-700 px-4 py-2 text-xs text-gray-500 dark:text-gray-400">
          共 {filteredVariables.length} 条记录
        </div>
      </div>

      {/* 新增/编辑表单 */}
      {showForm && (
        <div className="rounded-xl bg-white p-6 shadow-md dark:bg-gray-800 dark:shadow-lg">
          <h3 className="mb-4 text-lg font-semibold">
            {editingVariable ? '编辑变量' : '新增变量'}
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                键名 <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.key}
                onChange={(e) => handleChange('key', e.target.value)}
                disabled={!!editingVariable}
                className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white disabled:opacity-60 disabled:cursor-not-allowed font-mono"
                placeholder="例如：mysql_connection"
              />
              {editingVariable && (
                <p className="mt-1 text-xs text-gray-500">键名不可修改</p>
              )}
            </div>
            <div>
              <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                分类
              </label>
              <input
                type="text"
                value={formData.category}
                onChange={(e) => handleChange('category', e.target.value)}
                className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                placeholder="例如：Database"
                list="category-options"
              />
              <datalist id="category-options">
                {categories.map(cat => (
                  <option key={cat} value={cat} />
                ))}
              </datalist>
            </div>
            <div className="md:col-span-2">
              <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                值 <span className="text-red-500">*</span>
              </label>
              <textarea
                value={formData.value}
                onChange={(e) => handleChange('value', e.target.value)}
                rows={3}
                className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white font-mono text-sm"
                placeholder="变量值..."
              />
            </div>
            <div className="md:col-span-2">
              <label className="block mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                描述
              </label>
              <input
                type="text"
                value={formData.description}
                onChange={(e) => handleChange('description', e.target.value)}
                className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white"
                placeholder="变量用途描述..."
              />
            </div>
          </div>
          <div className="mt-6 flex justify-end gap-3">
            <button
              onClick={handleCancel}
              className="rounded-lg border border-gray-300 px-6 py-2 text-gray-700 hover:bg-gray-100 transition-colors dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
            >
              取消
            </button>
            <button
              onClick={handleSave}
              disabled={isLoading || !formData.key.trim()}
              className={`rounded-lg px-6 py-2 text-white font-medium transition-colors ${
                isLoading || !formData.key.trim()
                  ? 'bg-gray-400 cursor-not-allowed'
                  : 'bg-green-600 hover:bg-green-700'
              }`}
            >
              <i className="fa-solid fa-save mr-1"></i>
              {isLoading ? '保存中...' : '保存'}
            </button>
          </div>
        </div>
      )}

      {/* 使用说明 */}
      <div className="rounded-xl bg-blue-50 p-4 dark:bg-blue-900/20">
        <h4 className="text-sm font-semibold text-blue-800 dark:text-blue-300 mb-2">
          <i className="fa-solid fa-circle-info mr-1"></i>
          在 JS 处理器中使用
        </h4>
        <p className="text-sm text-blue-700 dark:text-blue-400">
          通过 <code className="bg-blue-100 dark:bg-blue-800 px-1 py-0.5 rounded font-mono">sys_var_get('键名')</code> 获取变量值，例如：
          <code className="block mt-2 bg-white dark:bg-gray-800 p-2 rounded border border-blue-200 dark:border-blue-800 font-mono text-xs">
            var dbConn = sys_var_get('mysql_connection');
          </code>
        </p>
      </div>
    </div>
  );
}
