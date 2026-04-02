import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import CodeMirror from '@uiw/react-codemirror';
import { javascript } from '@codemirror/lang-javascript';
import { oneDark } from '@codemirror/theme-one-dark';
import {
  getLibraries,
  getCategories,
  getFunctions
} from '@/services/documentation.service';
import type {
  LibraryDefinition,
  FunctionDefinition
} from '@/types/documentation';

const DocumentationPage: React.FC = () => {
  const [libraries, setLibraries] = useState<LibraryDefinition[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [functions, setFunctions] = useState<FunctionDefinition[]>([]);
  const [selectedLibrary, setSelectedLibrary] = useState<string>('');
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [selectedFunction, setSelectedFunction] = useState<FunctionDefinition | null>(null);

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    loadFunctions();
  }, [selectedLibrary, selectedCategory]);

  const loadInitialData = async () => {
    try {
      setLoading(true);
      const [libsData, catsData] = await Promise.all([
        getLibraries(),
        getCategories()
      ]);

      if (libsData) setLibraries(libsData);
      if (catsData) setCategories(catsData);
    } catch (error) {
      toast.error('加载数据失败');
      console.error('加载文档数据失败:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadFunctions = async () => {
    try {
      const params: any = {};
      if (selectedLibrary) params.library = selectedLibrary;
      if (selectedCategory) params.category = selectedCategory;

      const data = await getFunctions(params);
      if (data) setFunctions(data);
    } catch (error) {
      toast.error('加载函数列表失败');
      console.error('加载函数列表失败:', error);
    }
  };

  const filteredFunctions = functions.filter(func => {
    if (!searchQuery) return true;
    const query = searchQuery.toLowerCase();
    return (
      func.name.toLowerCase().includes(query) ||
      func.description.toLowerCase().includes(query)
    );
  });

  const copyExample = async (example: string) => {
    try {
      await navigator.clipboard.writeText(example);
      toast.success('已复制到剪贴板');
    } catch (error) {
      toast.error('复制失败');
    }
  };

  // 函数详情模态框
  const renderFunctionDetail = () => {
    if (!selectedFunction) return null;

    return (
      <div
        className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
        onClick={() => setSelectedFunction(null)}
      >
        <div
          className="bg-white rounded-lg max-w-3xl w-full max-h-[80vh] overflow-y-auto"
          onClick={(e) => e.stopPropagation()}
        >
          {/* 头部 */}
          <div className="sticky top-0 bg-white border-b px-6 py-4">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-2xl font-bold font-mono">
                  {selectedFunction.name}()
                </h2>
                <div className="flex items-center gap-2 mt-1 text-sm text-gray-600">
                  <span>{selectedFunction.category}</span>
                  <span>·</span>
                  <span>{selectedFunction.providerName}</span>
                </div>
              </div>
              <button
                onClick={() => setSelectedFunction(null)}
                className="text-gray-400 hover:text-gray-600 text-2xl"
              >
                ×
              </button>
            </div>
          </div>

          {/* 内容 */}
          <div className="px-6 py-4 space-y-4">
            {/* 描述 */}
            <div>
              <h3 className="text-sm font-semibold text-gray-700 mb-2">描述</h3>
              <p className="text-gray-600">{selectedFunction.description}</p>
            </div>

            {/* 参数 */}
            {selectedFunction.parameters.length > 0 && (
              <div>
                <h3 className="text-sm font-semibold text-gray-700 mb-2">参数</h3>
                <div className="space-y-2">
                  {selectedFunction.parameters.map((param, idx) => (
                    <div
                      key={idx}
                      className="bg-gray-50 border rounded p-3"
                    >
                      <div className="flex items-center gap-2 mb-1">
                        <code className="font-mono font-semibold text-blue-600">
                          {param.name}
                        </code>
                        <span className="text-xs bg-gray-200 px-2 py-0.5 rounded font-mono">
                          {param.type}
                        </span>
                        {param.isOptional && (
                          <span className="text-xs text-gray-500">可选</span>
                        )}
                      </div>
                      <p className="text-sm text-gray-600">{param.description}</p>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* 返回值 */}
            <div>
              <h3 className="text-sm font-semibold text-gray-700 mb-2">返回值</h3>
              <div className="bg-gray-50 border rounded p-3">
                <span className="font-mono font-semibold">
                  {selectedFunction.returnType}
                </span>
              </div>
            </div>

            {/* 示例 */}
            {selectedFunction.example && (
              <div>
                <div className="flex items-center justify-between mb-2">
                  <h3 className="text-sm font-semibold text-gray-700">示例</h3>
                  <button
                    onClick={() => copyExample(selectedFunction.example)}
                    className="text-sm text-blue-600 hover:text-blue-800"
                  >
                    复制
                  </button>
                </div>
                <div className="border rounded overflow-hidden">
                  <CodeMirror
                    value={selectedFunction.example}
                    extensions={[javascript()]}
                    theme={oneDark}
                    editable={false}
                    basicSetup={{
                      lineNumbers: false,
                      foldGutter: false,
                      highlightActiveLine: false,
                    }}
                  />
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-white p-6">
      <div className="max-w-6xl mx-auto">
        {/* 页面标题 */}
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900 mb-1">
            函数库文档
          </h1>
          <p className="text-gray-600 text-sm">
            浏览所有可用的 JavaScript 函数
          </p>
        </div>

        {/* 工具栏 */}
        <div className="bg-white border rounded-lg p-4 mb-4">
          <div className="flex items-center gap-4">
            {/* 搜索框 */}
            <div className="flex-1">
              <input
                type="text"
                placeholder="搜索函数..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full px-3 py-2 border rounded focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
              />
            </div>

            {/* 库选择器 */}
            <select
              value={selectedLibrary}
              onChange={(e) => setSelectedLibrary(e.target.value)}
              className="px-3 py-2 border rounded text-sm"
            >
              <option value="">所有库</option>
              {libraries.map(lib => (
                <option key={lib.name} value={lib.name}>
                  {lib.name}
                </option>
              ))}
            </select>

            {/* 分类选择器 */}
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="px-3 py-2 border rounded text-sm"
            >
              <option value="">所有分类</option>
              {categories.map(cat => (
                <option key={cat} value={cat}>{cat}</option>
              ))}
            </select>
          </div>
        </div>

        {/* 函数列表 */}
        {loading ? (
          <div className="flex items-center justify-center h-64">
            <div className="text-gray-500">加载中...</div>
          </div>
        ) : (
          <div className="border rounded-lg overflow-hidden">
            <table className="w-full">
              <thead className="bg-gray-50 border-b">
                <tr>
                  <th className="text-left px-4 py-3 text-sm font-semibold text-gray-700">函数名</th>
                  <th className="text-left px-4 py-3 text-sm font-semibold text-gray-700">描述</th>
                  <th className="text-left px-4 py-3 text-sm font-semibold text-gray-700 w-24">分类</th>
                  <th className="text-left px-4 py-3 text-sm font-semibold text-gray-700 w-32">库</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {filteredFunctions.map((func, index) => (
                  <tr
                    key={`${func.providerName}-${func.name}-${index}`}
                    className="hover:bg-gray-50 cursor-pointer"
                    onClick={() => setSelectedFunction(func)}
                  >
                    <td className="px-4 py-3">
                      <code className="text-blue-600 font-mono text-sm">
                        {func.name}()
                      </code>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {func.description}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {func.category}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {func.providerName}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {filteredFunctions.length === 0 && (
              <div className="text-center py-12 text-gray-500">
                暂无数据
              </div>
            )}
          </div>
        )}

        {/* 函数详情模态框 */}
        {renderFunctionDetail()}
      </div>
    </div>
  );
};

export default DocumentationPage;
