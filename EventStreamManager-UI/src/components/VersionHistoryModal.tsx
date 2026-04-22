import { useState, useEffect, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { toast } from 'sonner';
import ReactDiffViewer from 'react-diff-viewer-continued';
import { ProcessorVersion, RollbackOptions } from '@/types/processor-version';
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

const defaultRollbackOptions: RollbackOptions = {
  restoreCode: true,
  restoreSqlTemplate: true,
  restoreEventCodes: true,
  restoreDatabaseTypes: true,
  restoreMetadata: true,
};

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
  const [selectedVersion, setSelectedVersion] = useState<ProcessorVersion | null>(null);
  const [showRollbackConfirm, setShowRollbackConfirm] = useState(false);
  const [rollbackOptions, setRollbackOptions] = useState<RollbackOptions>(defaultRollbackOptions);

  const fetchVersions = async () => {
    if (!processorId) return;
    setLoading(true);
    try {
      const data = await getProcessorVersions(processorId);
      setVersions(data);
      if (data.length > 0 && !selectedVersion) {
        setSelectedVersion(data[0]);
      }
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
      setSelectedVersion(null);
      setShowRollbackConfirm(false);
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

  const handleRollbackClick = () => {
    setRollbackOptions({ ...defaultRollbackOptions });
    setShowRollbackConfirm(true);
  };

  const handleConfirmRollback = async () => {
    if (!selectedVersion) return;
    setIsRollingBack(true);
    try {
      const result = await rollbackProcessorVersion(processorId, selectedVersion.id, rollbackOptions);

      if (result.hasWarnings) {
        toast.success(`已回退到版本 v${result.version.version}`, {
          duration: 8000,
          description: (
            <div className="mt-2 space-y-2 text-sm">
              {result.recoveredTemplates.length > 0 && (
                <div className="rounded-md bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 p-2.5">
                  <div className="flex items-start gap-2">
                    <i className="fa-solid fa-file-code text-amber-600 dark:text-amber-400 mt-0.5"></i>
                    <div>
                      <p className="font-medium text-amber-800 dark:text-amber-300">自动恢复的模板</p>
                      <ul className="mt-1 space-y-0.5 text-amber-700 dark:text-amber-400/80">
                        {result.recoveredTemplates.map((id) => (
                          <li key={id} className="font-mono text-xs">• {id}</li>
                        ))}
                      </ul>
                      <p className="mt-1.5 text-xs text-amber-600/70 dark:text-amber-500/60">
                        原模板已被删除，已从快照自动恢复
                      </p>
                    </div>
                  </div>
                </div>
              )}
              {result.missingEventCodes.length > 0 && (
                <div className="rounded-md bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 p-2.5">
                  <div className="flex items-start gap-2">
                    <i className="fa-solid fa-triangle-exclamation text-red-600 dark:text-red-400 mt-0.5"></i>
                    <div>
                      <p className="font-medium text-red-800 dark:text-red-300">已不存在的事件码</p>
                      <ul className="mt-1 space-y-0.5 text-red-700 dark:text-red-400/80">
                        {result.missingEventCodes.map((code) => (
                          <li key={code} className="font-mono text-xs">• {code}</li>
                        ))}
                      </ul>
                      <p className="mt-1.5 text-xs text-red-600/70 dark:text-red-500/60">
                        该事件码已从系统配置中移除，建议检查处理器逻辑
                      </p>
                    </div>
                  </div>
                </div>
              )}
            </div>
          ),
        });
      } else {
        toast.success(`已回退到版本 v${result.version.version}`);
      }

      onRollbackSuccess();
      onClose();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '回退失败');
    } finally {
      setIsRollingBack(false);
      setShowRollbackConfirm(false);
    }
  };

  const toggleOption = (key: keyof RollbackOptions) => {
    setRollbackOptions((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  const isDark = useMemo(() => {
    return document.documentElement.classList.contains('dark');
  }, [isOpen]);

  const hasAnyOptionSelected = Object.values(rollbackOptions).some(Boolean);

  if (!isOpen) return null;

  return createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl w-[95vw] max-w-6xl h-[90vh] flex flex-col overflow-hidden">
        {/* 头部 */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700 flex-shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
              <i className="fa-solid fa-code-branch text-blue-600 dark:text-blue-400"></i>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                版本历史
              </h3>
              <p className="text-xs text-gray-500 dark:text-gray-400">
                {processorName} · 共 {versions.length} 个版本
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="w-8 h-8 flex items-center justify-center rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:text-gray-300 dark:hover:bg-gray-700 transition-colors"
          >
            <i className="fa-solid fa-times text-lg"></i>
          </button>
        </div>

        {/* 主体：左右分栏 */}
        <div className="flex-1 flex min-h-0">
          {/* 左侧：版本列表 */}
          <div className="w-80 flex-shrink-0 border-r border-gray-200 dark:border-gray-700 flex flex-col">
            {/* Commit 区域 */}
            <div className="p-4 border-b border-gray-200 dark:border-gray-700 bg-gray-50/50 dark:bg-gray-900/30">
              <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-2">
                <i className="fa-solid fa-plus mr-1 text-blue-500"></i>
                提交当前处理器配置为新版本
              </label>
              <div className="flex gap-2">
                <input
                  type="text"
                  value={commitMessage}
                  onChange={(e) => setCommitMessage(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') handleCommit();
                  }}
                  placeholder="输入提交信息..."
                  className="flex-1 min-w-0 rounded-lg border border-gray-300 bg-white px-3 py-1.5 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                />
                <button
                  onClick={handleCommit}
                  disabled={isCommitting}
                  className={`rounded-lg px-3 py-1.5 text-sm font-medium text-white transition-colors flex-shrink-0 ${
                    isCommitting
                      ? 'bg-gray-400 cursor-not-allowed'
                      : 'bg-blue-600 hover:bg-blue-700'
                  }`}
                >
                  {isCommitting ? (
                    <i className="fa-solid fa-spinner fa-spin"></i>
                  ) : (
                    <i className="fa-solid fa-check"></i>
                  )}
                </button>
              </div>
            </div>

            {/* 版本时间线 */}
            <div className="flex-1 overflow-y-auto p-4">
              {loading ? (
                <div className="flex items-center justify-center py-12">
                  <i className="fa-solid fa-spinner fa-spin text-xl text-blue-600"></i>
                </div>
              ) : versions.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-12 text-gray-500 dark:text-gray-400">
                  <i className="fa-solid fa-box-open text-3xl mb-2 text-gray-300 dark:text-gray-600"></i>
                  <p className="text-sm">暂无版本历史</p>
                </div>
              ) : (
                <div className="relative">
                  <div className="absolute left-[11px] top-2 bottom-2 w-px bg-gray-200 dark:bg-gray-700"></div>
                  <div className="space-y-1">
                    {versions.map((version, index) => {
                      const isSelected = selectedVersion?.id === version.id;
                      const isLatest = index === 0;

                      return (
                        <button
                          key={version.id}
                          onClick={() => setSelectedVersion(version)}
                          className={`relative w-full text-left rounded-lg px-3 py-2.5 pl-8 transition-colors group ${
                            isSelected
                              ? 'bg-blue-50 dark:bg-blue-900/20'
                              : 'hover:bg-gray-50 dark:hover:bg-gray-700/50'
                          }`}
                        >
                          <div
                            className={`absolute left-[7px] top-3.5 w-[9px] h-[9px] rounded-full border-2 transition-colors ${
                              isSelected
                                ? 'bg-blue-500 border-blue-500'
                                : isLatest
                                ? 'bg-green-500 border-green-500'
                                : 'bg-white dark:bg-gray-800 border-gray-300 dark:border-gray-600 group-hover:border-gray-400'
                            }`}
                          ></div>
                          <div className="flex items-start justify-between gap-2">
                            <div className="min-w-0 flex-1">
                              <div className="flex items-center gap-1.5 mb-0.5">
                                <span className="text-xs font-bold text-blue-600 dark:text-blue-400">
                                  v{version.version}
                                </span>
                                {isLatest && (
                                  <span className="text-[10px] font-medium text-green-600 dark:text-green-400 bg-green-100 dark:bg-green-900/30 px-1 rounded">
                                    最新
                                  </span>
                                )}
                              </div>
                              <p
                                className={`text-sm truncate ${
                                  isSelected
                                    ? 'text-gray-900 dark:text-white font-medium'
                                    : 'text-gray-700 dark:text-gray-300'
                                }`}
                              >
                                {version.commitMessage}
                              </p>
                              <p className="text-[11px] text-gray-400 dark:text-gray-500 mt-0.5">
                                {new Date(version.createdAt).toLocaleString()}
                              </p>
                            </div>
                          </div>
                        </button>
                      );
                    })}
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* 右侧：详情面板 */}
          <div className="flex-1 flex flex-col min-w-0 bg-gray-50/30 dark:bg-gray-900/20 relative">
            {selectedVersion ? (
              <>
                {/* 详情头部 */}
                <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 flex-shrink-0">
                  <div className="flex items-start justify-between gap-4">
                    <div className="min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <span className="inline-flex items-center justify-center rounded-md bg-blue-100 dark:bg-blue-900/30 px-2 py-0.5 text-xs font-bold text-blue-800 dark:text-blue-400">
                          v{selectedVersion.version}
                        </span>
                        <span className="text-xs text-gray-400 dark:text-gray-500">
                          {new Date(selectedVersion.createdAt).toLocaleString()}
                        </span>
                      </div>
                      <p className="text-base font-medium text-gray-900 dark:text-white">
                        {selectedVersion.commitMessage}
                      </p>
                    </div>
                    <button
                      onClick={handleRollbackClick}
                      disabled={isRollingBack}
                      className="flex-shrink-0 flex items-center gap-1.5 rounded-lg px-4 py-2 text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                    >
                      {isRollingBack ? (
                        <i className="fa-solid fa-spinner fa-spin"></i>
                      ) : (
                        <i className="fa-solid fa-rotate-left"></i>
                      )}
                      回滚到此版本
                    </button>
                  </div>
                </div>

                {/* 版本快照信息 */}
                <div className="px-6 py-3 border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 flex-shrink-0">
                  <p className="text-xs font-medium text-gray-500 dark:text-gray-400 mb-2">
                    <i className="fa-solid fa-circle-info mr-1"></i>
                    该版本的处理器配置
                  </p>
                  <div className="grid grid-cols-2 gap-x-6 gap-y-2">
                    <div>
                      <span className="text-[11px] text-gray-400 dark:text-gray-500">数据库类型</span>
                      <div className="flex flex-wrap gap-1 mt-0.5">
                        {selectedVersion.databaseTypes.length > 0 ? (
                          selectedVersion.databaseTypes.map((t) => (
                            <span key={t} className="inline-block rounded bg-gray-100 dark:bg-gray-700 px-1.5 py-0.5 text-[11px] text-gray-700 dark:text-gray-300">
                              {t}
                            </span>
                          ))
                        ) : (
                          <span className="text-[11px] text-gray-400 dark:text-gray-500">未配置</span>
                        )}
                      </div>
                    </div>
                    <div>
                      <span className="text-[11px] text-gray-400 dark:text-gray-500">SQL 模板</span>
                      <div className="mt-0.5 text-xs text-gray-700 dark:text-gray-300">
                        {selectedVersion.sqlTemplateType === 'System' ? (
                          <span><i className="fa-solid fa-server mr-1 text-blue-500"></i>系统模板「{selectedVersion.sqlTemplateName || selectedVersion.sqlTemplateId}」</span>
                        ) : selectedVersion.sqlTemplateType === 'Custom' ? (
                          <span><i className="fa-solid fa-file-code mr-1 text-purple-500"></i>自定义模板「{selectedVersion.sqlTemplateName || selectedVersion.sqlTemplateId}」</span>
                        ) : (
                          <span className="text-gray-400">未配置</span>
                        )}
                      </div>
                    </div>
                    <div>
                      <span className="text-[11px] text-gray-400 dark:text-gray-500">订阅事件码</span>
                      <div className="flex flex-wrap gap-1 mt-0.5">
                        {selectedVersion.eventCodes.length > 0 ? (
                          selectedVersion.eventCodes.map((c) => (
                            <span key={c} className="inline-block rounded bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 px-1.5 py-0.5 text-[11px] text-green-700 dark:text-green-400">
                              {c}
                            </span>
                          ))
                        ) : (
                          <span className="text-[11px] text-gray-400 dark:text-gray-500">未配置</span>
                        )}
                      </div>
                    </div>
                    <div>
                      <span className="text-[11px] text-gray-400 dark:text-gray-500">处理器状态</span>
                      <div className="mt-0.5 text-xs">
                        {selectedVersion.enabled ? (
                          <span className="text-green-600 dark:text-green-400"><i className="fa-solid fa-circle-check mr-1"></i>已启用</span>
                        ) : (
                          <span className="text-gray-500 dark:text-gray-400"><i className="fa-solid fa-circle-stop mr-1"></i>已禁用</span>
                        )}
                      </div>
                    </div>
                  </div>
                  {selectedVersion.description && (
                    <div className="mt-2 pt-2 border-t border-gray-100 dark:border-gray-700">
                      <span className="text-[11px] text-gray-400 dark:text-gray-500">描述</span>
                      <p className="text-xs text-gray-600 dark:text-gray-400 mt-0.5">{selectedVersion.description}</p>
                    </div>
                  )}
                </div>

                {/* Diff 区域 */}
                <div className="flex-1 overflow-hidden flex flex-col min-h-0">
                  <div className="px-4 py-2 bg-gray-100 dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between flex-shrink-0">
                    <div className="text-xs text-gray-500 dark:text-gray-400">
                      <span className="font-medium text-gray-700 dark:text-gray-300">
                        v{selectedVersion.version}
                      </span>
                      <span className="mx-2">→</span>
                      <span className="font-medium text-blue-600 dark:text-blue-400">当前编辑</span>
                    </div>
                    <div className="text-[11px] text-gray-400 dark:text-gray-500">
                      代码比对
                    </div>
                  </div>
                  <div className="flex-1 overflow-auto min-h-0">
                    <ReactDiffViewer
                      oldValue={selectedVersion.code}
                      newValue={currentCode}
                      splitView={true}
                      showDiffOnly={false}
                      useDarkTheme={isDark}
                      leftTitle={`v${selectedVersion.version}`}
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

                {/* 回滚确认覆盖层 */}
                {showRollbackConfirm && (
                  <div className="absolute inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-10">
                    <div className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl w-[480px] max-w-[90%] max-h-[90%] flex flex-col overflow-hidden">
                      <div className="px-5 py-4 border-b border-gray-200 dark:border-gray-700">
                        <h4 className="text-base font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                          <i className="fa-solid fa-rotate-left text-amber-600"></i>
                          回滚确认
                        </h4>
                        <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                          即将回滚到 <span className="font-medium text-blue-600 dark:text-blue-400">v{selectedVersion.version}</span>：{selectedVersion.commitMessage}
                        </p>
                      </div>

                      <div className="px-5 py-4 overflow-y-auto">
                        <p className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
                          请选择要恢复的内容（未勾选的字段保持当前值）：
                        </p>

                        <div className="space-y-2.5">
                          {/* 脚本代码 */}
                          <label className="flex items-start gap-3 p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer transition-colors">
                            <input
                              type="checkbox"
                              checked={rollbackOptions.restoreCode}
                              onChange={() => toggleOption('restoreCode')}
                              className="mt-0.5 w-4 h-4 text-blue-600 rounded border-gray-300 focus:ring-blue-500"
                            />
                            <div className="flex-1 min-w-0">
                              <p className="text-sm font-medium text-gray-800 dark:text-gray-200">脚本代码</p>
                              <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5 truncate">
                                版本内容：{selectedVersion.code.length > 0 ? `${selectedVersion.code.split('\n').length} 行代码` : '空'}
                              </p>
                            </div>
                          </label>

                          {/* SQL 模板 */}
                          <label className="flex items-start gap-3 p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer transition-colors">
                            <input
                              type="checkbox"
                              checked={rollbackOptions.restoreSqlTemplate}
                              onChange={() => toggleOption('restoreSqlTemplate')}
                              className="mt-0.5 w-4 h-4 text-blue-600 rounded border-gray-300 focus:ring-blue-500"
                            />
                            <div className="flex-1 min-w-0">
                              <p className="text-sm font-medium text-gray-800 dark:text-gray-200">SQL 模板配置</p>
                              <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                                版本内容：{selectedVersion.sqlTemplateType === 'System' ? '系统模板' : selectedVersion.sqlTemplateType === 'Custom' ? '自定义模板' : '无模板'}
                                {selectedVersion.sqlTemplateName ? `「${selectedVersion.sqlTemplateName}」` : ''}
                              </p>
                            </div>
                          </label>

                          {/* 事件码 */}
                          <label className="flex items-start gap-3 p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer transition-colors">
                            <input
                              type="checkbox"
                              checked={rollbackOptions.restoreEventCodes}
                              onChange={() => toggleOption('restoreEventCodes')}
                              className="mt-0.5 w-4 h-4 text-blue-600 rounded border-gray-300 focus:ring-blue-500"
                            />
                            <div className="flex-1 min-w-0">
                              <p className="text-sm font-medium text-gray-800 dark:text-gray-200">订阅事件码</p>
                              <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                                版本内容：{selectedVersion.eventCodes.length > 0 ? selectedVersion.eventCodes.join(', ') : '未配置'}
                              </p>
                            </div>
                          </label>

                          {/* 数据库类型 */}
                          <label className="flex items-start gap-3 p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer transition-colors">
                            <input
                              type="checkbox"
                              checked={rollbackOptions.restoreDatabaseTypes}
                              onChange={() => toggleOption('restoreDatabaseTypes')}
                              className="mt-0.5 w-4 h-4 text-blue-600 rounded border-gray-300 focus:ring-blue-500"
                            />
                            <div className="flex-1 min-w-0">
                              <p className="text-sm font-medium text-gray-800 dark:text-gray-200">数据库类型</p>
                              <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                                版本内容：{selectedVersion.databaseTypes.length > 0 ? selectedVersion.databaseTypes.join(', ') : '未配置'}
                              </p>
                            </div>
                          </label>

                          {/* 基本信息 */}
                          <label className="flex items-start gap-3 p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer transition-colors">
                            <input
                              type="checkbox"
                              checked={rollbackOptions.restoreMetadata}
                              onChange={() => toggleOption('restoreMetadata')}
                              className="mt-0.5 w-4 h-4 text-blue-600 rounded border-gray-300 focus:ring-blue-500"
                            />
                            <div className="flex-1 min-w-0">
                              <p className="text-sm font-medium text-gray-800 dark:text-gray-200">处理器基本信息</p>
                              <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                                名称「{selectedVersion.name}」· {selectedVersion.enabled ? '已启用' : '已禁用'}
                                {selectedVersion.description ? ` · ${selectedVersion.description}` : ''}
                              </p>
                            </div>
                          </label>
                        </div>

                        <div className="mt-4 p-3 rounded-lg bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 flex items-start gap-2">
                          <i className="fa-solid fa-triangle-exclamation text-amber-600 dark:text-amber-400 mt-0.5 text-sm"></i>
                          <p className="text-xs text-amber-700 dark:text-amber-400">
                            警告：当前未保存的修改将会丢失。请确认已保存需要保留的内容。
                          </p>
                        </div>
                      </div>

                      <div className="px-5 py-4 border-t border-gray-200 dark:border-gray-700 flex justify-end gap-3">
                        <button
                          onClick={() => setShowRollbackConfirm(false)}
                          className="px-4 py-2 rounded-lg text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
                        >
                          取消
                        </button>
                        <button
                          onClick={handleConfirmRollback}
                          disabled={isRollingBack || !hasAnyOptionSelected}
                          className={`px-4 py-2 rounded-lg text-sm font-medium text-white transition-colors ${
                            isRollingBack || !hasAnyOptionSelected
                              ? 'bg-gray-400 cursor-not-allowed'
                              : 'bg-amber-600 hover:bg-amber-700'
                          }`}
                        >
                          {isRollingBack ? (
                            <><i className="fa-solid fa-spinner fa-spin mr-1"></i> 回滚中</>
                          ) : (
                            <><i className="fa-solid fa-rotate-left mr-1"></i> 确认回滚</>
                          )}
                        </button>
                      </div>
                    </div>
                  </div>
                )}
              </>
            ) : (
              <div className="flex-1 flex flex-col items-center justify-center text-gray-400 dark:text-gray-500">
                <i className="fa-solid fa-code-branch text-4xl mb-3 text-gray-300 dark:text-gray-700"></i>
                <p className="text-sm">选择一个版本查看代码比对</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>,
    document.body
  );
}
