import { useState, useEffect, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { toast } from 'sonner';
import ReactDiffViewer from 'react-diff-viewer-continued';
import { ProcessorVersion } from '@/types/processor-version';
import {
  getProcessorVersions,
  commitProcessorVersion,
  rollbackProcessorVersion,
} from '@/services/processor-version.service';

interface VersionHistoryModalProps {
  processorId: string;
  processorName: string;
  currentCode: string;
  isOpen: boolean;
  onClose: () => void;
  onRollbackSuccess: () => void;
}

export default function VersionHistoryModal({
  processorId,
  processorName,
  currentCode,
  isOpen,
  onClose,
  onRollbackSuccess,
}: VersionHistoryModalProps) {
  const [versions, setVersions] = useState<ProcessorVersion[]>([]);
  const [loading, setLoading] = useState(false);
  const [commitMessage, setCommitMessage] = useState('');
  const [isCommitting, setIsCommitting] = useState(false);
  const [isRollingBack, setIsRollingBack] = useState(false);
  const [diffVersion, setDiffVersion] = useState<ProcessorVersion | null>(null);
  const [activeTab, setActiveTab] = useState<'history' | 'diff'>('history');

  const fetchVersions = async () => {
    if (!processorId) return;
    setLoading(true);
    try {
      const data = await getProcessorVersions(processorId);
      setVersions(data);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '加载版本历史失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isOpen && processorId) {
      fetchVersions();
      setCommitMessage('');
      setDiffVersion(null);
      setActiveTab('history');
    }
  }, [isOpen, processorId]);

  const handleCommit = async () => {
    if (!commitMessage.trim()) {
      toast.error('请输入提交信息');
      return;
    }
    setIsCommitting(true);
    try {
      await commitProcessorVersion(processorId, commitMessage.trim());
      toast.success('版本提交成功');
      setCommitMessage('');
      fetchVersions();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '提交失败');
    } finally {
      setIsCommitting(false);
    }
  };

  const handleRollback = async (version: ProcessorVersion) => {
    if (!window.confirm(`确定要回退到版本 v${version.version} 吗？\n\n${version.commitMessage}\n\n当前未提交的代码将会丢失。`)) {
      return;
    }
    setIsRollingBack(true);
    try {
      await rollbackProcessorVersion(processorId, version.id);
      toast.success(`已回退到版本 v${version.version}`);
      onRollbackSuccess();
      onClose();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '回退失败');
    } finally {
      setIsRollingBack(false);
    }
  };

  const handleShowDiff = (version: ProcessorVersion) => {
    setDiffVersion(version);
    setActiveTab('diff');
  };

  const isDark = useMemo(() => {
    return document.documentElement.classList.contains('dark');
  }, [isOpen]);

  if (!isOpen) return null;

  return createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60 backdrop-blur-sm">
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl w-[90vw] max-w-5xl max-h-[90vh] flex flex-col overflow-hidden">
        {/* 头部 */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <div>
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
              <i className="fa-solid fa-code-branch mr-2 text-blue-600 dark:text-blue-400"></i>
              版本历史 - {processorName}
            </h3>
            <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
              管理处理器的代码版本，支持提交新版本和回退到历史版本
            </p>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
          >
            <i className="fa-solid fa-times text-xl"></i>
          </button>
        </div>

        {/* 标签页 */}
        <div className="flex border-b border-gray-200 dark:border-gray-700 px-6">
          <button
            onClick={() => setActiveTab('history')}
            className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
              activeTab === 'history'
                ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
          >
            <i className="fa-solid fa-clock-rotate-left"></i>
            历史版本 ({versions.length})
          </button>
          {diffVersion && (
            <button
              onClick={() => setActiveTab('diff')}
              className={`flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors ${
                activeTab === 'diff'
                  ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
                  : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
              }`}
            >
              <i className="fa-solid fa-code-compare"></i>
              代码比对 (v{diffVersion.version})
            </button>
          )}
        </div>

        {/* 内容区 */}
        <div className="flex-1 overflow-hidden flex flex-col min-h-0">
          {activeTab === 'history' && (
            <div className="h-full overflow-y-auto p-6 space-y-6">
              {/* 提交区域 */}
              <div className="rounded-lg border border-blue-200 bg-blue-50 dark:bg-blue-900/20 dark:border-blue-800 p-4">
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  <i className="fa-solid fa-plus mr-1 text-blue-600 dark:text-blue-400"></i>
                  提交当前代码为新版本
                </label>
                <div className="flex gap-2">
                  <input
                    type="text"
                    value={commitMessage}
                    onChange={(e) => setCommitMessage(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') handleCommit();
                    }}
                    placeholder="输入提交信息"
                    className="flex-1 rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                  <button
                    onClick={handleCommit}
                    disabled={isCommitting}
                    className={`rounded-lg px-4 py-2 text-sm font-medium text-white transition-colors ${
                      isCommitting
                        ? 'bg-gray-400 cursor-not-allowed'
                        : 'bg-blue-600 hover:bg-blue-700'
                    }`}
                  >
                    {isCommitting ? (
                      <>
                        <i className="fa-solid fa-spinner fa-spin mr-1"></i> 提交中
                      </>
                    ) : (
                      <>
                        <i className="fa-solid fa-check mr-1"></i> Commit
                      </>
                    )}
                  </button>
                </div>
              </div>

              {/* 版本列表 */}
              {loading ? (
                <div className="flex items-center justify-center py-12">
                  <i className="fa-solid fa-spinner fa-spin text-2xl text-blue-600"></i>
                  <span className="ml-2 text-gray-600 dark:text-gray-400">加载中...</span>
                </div>
              ) : versions.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-12 text-gray-500 dark:text-gray-400">
                  <i className="fa-solid fa-box-open text-4xl mb-3 text-gray-300 dark:text-gray-600"></i>
                  <p>暂无版本历史</p>
                  <p className="text-sm mt-1">在上方输入提交信息，保存当前代码的第一个版本</p>
                </div>
              ) : (
                <div className="space-y-3">
                  {versions.map((version, index) => (
                    <div
                      key={version.id}
                      className="rounded-lg border border-gray-200 bg-white dark:bg-gray-800 dark:border-gray-700 p-4 transition-shadow hover:shadow-md"
                    >
                      <div className="flex items-start justify-between">
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <span className="inline-flex items-center justify-center rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-bold text-blue-800 dark:bg-blue-900/30 dark:text-blue-400">
                              v{version.version}
                            </span>
                            {index === 0 && (
                              <span className="inline-flex items-center rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800 dark:bg-green-900/30 dark:text-green-400">
                                最新
                              </span>
                            )}
                            <span className="text-xs text-gray-400 dark:text-gray-500">
                              {new Date(version.createdAt).toLocaleString()}
                            </span>
                          </div>
                          <p className="text-sm text-gray-800 dark:text-gray-200 font-medium">
                            {version.commitMessage}
                          </p>
                        </div>
                        <div className="flex items-center gap-1 ml-4 flex-shrink-0">
                          <button
                            onClick={() => handleShowDiff(version)}
                            className="p-2 text-gray-500 hover:text-blue-600 hover:bg-blue-50 dark:text-gray-400 dark:hover:text-blue-400 dark:hover:bg-blue-900/30 rounded-md transition-colors"
                            title="代码比对"
                          >
                            <i className="fa-solid fa-code-compare"></i>
                          </button>
                          <button
                            onClick={() => handleRollback(version)}
                            disabled={isRollingBack}
                            className="p-2 text-gray-500 hover:text-amber-600 hover:bg-amber-50 dark:text-gray-400 dark:hover:text-amber-400 dark:hover:bg-amber-900/30 rounded-md transition-colors"
                            title="回退到此版本"
                          >
                            <i className="fa-solid fa-rotate-left"></i>
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {activeTab === 'diff' && diffVersion && (
            <div className="h-full flex flex-col overflow-hidden">
              <div className="px-6 py-2 bg-gray-50 dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between flex-shrink-0">
                <div className="text-sm text-gray-600 dark:text-gray-400">
                  <span className="font-medium text-gray-800 dark:text-gray-200">
                    v{diffVersion.version}
                  </span>
                  <span className="mx-2">→</span>
                  <span className="font-medium text-blue-600 dark:text-blue-400">当前编辑</span>
                </div>
              </div>
              <div className="flex-1 overflow-auto min-h-0">
                <ReactDiffViewer
                  oldValue={diffVersion.code}
                  newValue={currentCode}
                  splitView={true}
                  showDiffOnly={false}
                  useDarkTheme={isDark}
                  leftTitle={`v${diffVersion.version} - ${diffVersion.commitMessage}`}
                  rightTitle="当前编辑"
                  styles={{
                    diffContainer: {
                      fontSize: '13px',
                      fontFamily: 'monospace',
                    },
                  }}
                />
              </div>
            </div>
          )}
        </div>
      </div>
    </div>,
    document.body
  );
}
