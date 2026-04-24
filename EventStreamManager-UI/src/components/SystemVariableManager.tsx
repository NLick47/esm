import { useState, useMemo } from 'react';
import { toast } from 'sonner';
import { SystemVariable, SystemVariableRequest } from '@/types/system-variable';
import * as systemVariableService from '@/services/system-variable.service';
import { useApiQuery } from '@/hooks/useApiQuery';
import { useApiMutation } from '@/hooks/useApiMutation';
import { Modal } from '@/components/ui/Modal';
import { FormField } from '@/components/ui/FormField';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { DataTable } from '@/components/ui/DataTable';
import { PageLoading } from '@/components/ui/PageLoading';
import { buttonVariants } from '@/utils/button-styles';

const EMPTY_FORM: SystemVariableRequest = {
  key: '',
  value: '',
  description: '',
  category: 'General'
};

export default function SystemVariableManager() {
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [editingVariable, setEditingVariable] = useState<SystemVariable | null>(null);
  const [formData, setFormData] = useState<SystemVariableRequest>({ ...EMPTY_FORM });
  const [showForm, setShowForm] = useState(false);

  const {
    data: variables = [],
    loading: isLoading,
    refresh,
    setData: setVariables,
  } = useApiQuery(() => systemVariableService.getAllVariables(), {
    initialData: [],
    errorMessage: '加载系统变量失败',
  });

  const { mutate: saveVariable, loading: isSaving } = useApiMutation(
    systemVariableService.saveVariable,
    {
      successMessage: '保存成功',
      errorMessage: '保存失败',
      onSuccess: (saved) => {
        setVariables(prev => {
          const list = prev || [];
          const index = list.findIndex(v => v.id === saved.id);
          if (index >= 0) {
            const updated = [...list];
            updated[index] = saved;
            return updated;
          }
          return [...list, saved];
        });
        setShowForm(false);
        setEditingVariable(null);
        setFormData({ ...EMPTY_FORM });
      },
    }
  );

  const { mutate: deleteVariable } = useApiMutation(
    (id: string) => systemVariableService.deleteVariable(id),
    {
      errorMessage: '删除失败',
    }
  );

  const categories = useMemo(() => {
    const set = new Set(variables.map(v => v.category));
    return Array.from(set).sort();
  }, [variables]);

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

  const handleAdd = () => {
    setEditingVariable(null);
    setFormData({ ...EMPTY_FORM });
    setShowForm(true);
  };

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

  const handleDelete = async (variable: SystemVariable) => {
    if (!window.confirm(`确定要删除变量 "${variable.key}" 吗？`)) {
      return;
    }
    await deleteVariable(variable.id);
    setVariables(prev => (prev || []).filter(v => v.id !== variable.id));
    toast.success(`变量 "${variable.key}" 已删除`);
  };

  const handleCopyValue = async (value: string, key: string) => {
    try {
      await navigator.clipboard.writeText(value);
      toast.success(`已复制 "${key}" 的值`);
    } catch {
      toast.error('复制失败');
    }
  };

  const handleSave = async () => {
    if (!formData.key.trim()) {
      toast.error('变量键名不能为空');
      return;
    }
    await saveVariable(formData);
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingVariable(null);
    setFormData({ ...EMPTY_FORM });
  };

  const handleChange = (field: keyof SystemVariableRequest, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const inputBase = 'w-full rounded-lg border border-gray-300 bg-white px-4 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-700 dark:bg-gray-700 dark:text-white';

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">系统变量管理</h2>
        <div className="flex items-center gap-3">

          <button
            onClick={handleAdd}
            className={buttonVariants.success + ' px-4 py-2 flex items-center gap-2'}
          >
            <i className="fa-solid fa-plus"></i>
            新增变量
          </button>
        </div>
      </div>

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
          onClick={refresh}
          className={buttonVariants.ghost + ' px-4 py-2'}
          title="刷新"
        >
          <i className="fa-solid fa-rotate-right"></i>
        </button>
      </div>

      {isLoading && variables.length === 0 ? (
        <PageLoading />
      ) : (
        <>
          <DataTable
            data={filteredVariables}
            columns={[
              {
                key: 'key',
                header: '键名',
                render: (v: SystemVariable) => (
                  <span className="font-mono text-blue-600 dark:text-blue-400 font-medium">{v.key}</span>
                )
              },
              {
                key: 'value',
                header: '值',
                render: (v: SystemVariable) => (
                  <div className="truncate font-mono text-xs text-gray-600 dark:text-gray-400 max-w-xs" title={v.value}>
                    {v.value}
                  </div>
                )
              },
              {
                key: 'description',
                header: '描述',
                render: (v: SystemVariable) => v.description || '-'
              },
              {
                key: 'category',
                header: '分类',
                render: (v: SystemVariable) => <StatusBadge variant="default">{v.category}</StatusBadge>
              },
              {
                key: 'updatedAt',
                header: '更新时间',
                render: (v: SystemVariable) => (
                  <span className="whitespace-nowrap text-gray-500 dark:text-gray-400">
                    {new Date(v.updatedAt).toLocaleString()}
                  </span>
                )
              }
            ]}
            keyExtractor={(v: SystemVariable) => v.id}
            onRowClick={(v: SystemVariable) => handleEdit(v)}
            rowActions={(v: SystemVariable) => (
              <>
                <button
                  onClick={() => handleCopyValue(v.value, v.key)}
                  className="rounded p-1.5 text-gray-500 hover:bg-gray-100 hover:text-gray-700 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-gray-200"
                  title="复制值"
                >
                  <i className="fa-regular fa-copy"></i>
                </button>
                <button
                  onClick={() => handleEdit(v)}
                  className="rounded p-1.5 text-blue-500 hover:bg-blue-50 hover:text-blue-700 dark:text-blue-400 dark:hover:bg-blue-900/20 dark:hover:text-blue-300"
                  title="编辑"
                >
                  <i className="fa-solid fa-pen-to-square"></i>
                </button>
                <button
                  onClick={() => handleDelete(v)}
                  className="rounded p-1.5 text-red-500 hover:bg-red-50 hover:text-red-700 dark:text-red-400 dark:hover:bg-red-900/20 dark:hover:text-red-300"
                  title="删除"
                >
                  <i className="fa-solid fa-trash"></i>
                </button>
              </>
            )}
            emptyText="暂无系统变量"
          />

          <div className="text-xs text-gray-500 dark:text-gray-400">
            共 {filteredVariables.length} 条记录
          </div>
        </>
      )}

      <Modal
        isOpen={showForm}
        onClose={handleCancel}
        title={editingVariable ? '编辑变量' : '新增变量'}
        footer={
          <>
            <button onClick={handleCancel} className={buttonVariants.ghost + ' px-6 py-2'}>
              取消
            </button>
            <button
              onClick={handleSave}
              disabled={isSaving || !formData.key.trim()}
              className={buttonVariants.success + ' px-6 py-2 flex items-center gap-1.5 disabled:opacity-50 disabled:cursor-not-allowed'}
            >
              <i className="fa-solid fa-save"></i>
              {isSaving ? '保存中...' : '保存'}
            </button>
          </>
        }
      >
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          <FormField label="键名" required hint={editingVariable ? '键名不可修改' : undefined}>
            <input
              type="text"
              autoFocus={!editingVariable}
              value={formData.key}
              onChange={(e) => handleChange('key', e.target.value)}
              disabled={!!editingVariable}
              className={inputBase + ' disabled:opacity-60 disabled:cursor-not-allowed font-mono'}
              placeholder="例如：mysql_connection"
            />
          </FormField>

          <FormField label="分类">
            <input
              type="text"
              value={formData.category}
              onChange={(e) => handleChange('category', e.target.value)}
              className={inputBase}
              placeholder="例如：Database"
              list="category-options"
            />
            <datalist id="category-options">
              {categories.map(cat => (
                <option key={cat} value={cat} />
              ))}
            </datalist>
          </FormField>

          <FormField label="值" required className="md:col-span-2">
            <textarea
              autoFocus={!!editingVariable}
              value={formData.value}
              onChange={(e) => handleChange('value', e.target.value)}
              rows={3}
              className={inputBase + ' font-mono text-sm'}
              placeholder="变量值..."
            />
          </FormField>

          <FormField label="描述" className="md:col-span-2">
            <input
              type="text"
              value={formData.description}
              onChange={(e) => handleChange('description', e.target.value)}
              className={inputBase}
              placeholder="变量用途描述..."
            />
          </FormField>
        </div>
      </Modal>

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
