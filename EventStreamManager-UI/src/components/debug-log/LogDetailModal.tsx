import { EventHandleDetail } from '@/types/event-log';
import {
  getScriptStatusBadge,
  getSendStatusBadge,
  getSendStatusLabel,
  formatJsonData
} from './utils';
import CopyButton from './CopyButton';
import CollapsibleBlock from './CollapsibleBlock';

interface Props {
  handle: EventHandleDetail | null;
  loading: boolean;
  onClose: () => void;
}

export default function LogDetailModal({ handle, loading, onClose }: Props) {
  if (!handle) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={onClose}>
      <div
        className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-4xl w-full mx-4 max-h-[90vh] overflow-auto"
        onClick={(e) => e.stopPropagation()}
      >
        {/* 弹窗头部 */}
        <div className="flex items-center justify-between mb-5 pb-4 border-b border-gray-200 dark:border-gray-700">
          <div className="flex items-center gap-3">
            <h3 className="text-lg font-semibold">处理记录详情</h3>
            <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${getScriptStatusBadge(handle.scriptSuccess, handle.isDeadLetter)}`}>
              {handle.isDeadLetter ? '死信' : handle.scriptSuccess === true ? '脚本成功' : handle.scriptSuccess === false ? '脚本失败' : '未执行'}
            </span>
            <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${getSendStatusBadge(handle.needToSend, handle.sendSuccess)}`}>
              {getSendStatusLabel(handle.needToSend, handle.sendSuccess)}
            </span>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
          >
            <i className="fa-solid fa-times text-lg"></i>
          </button>
        </div>

        {loading && (
          <div className="flex items-center justify-center py-8 text-gray-500">
            <i className="fa-solid fa-spinner fa-spin mr-2"></i>
            加载详情中...
          </div>
        )}

        <div className="space-y-6">
          {/* 基本信息 */}
          <div>
            <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-3">基本信息</h4>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-y-4 gap-x-6 text-sm">
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">记录 ID</div>
                <div className="font-mono mt-0.5">{handle.id}</div>
              </div>
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">事件 ID</div>
                <div className="font-mono mt-0.5">{handle.eventId}</div>
              </div>
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">事件码</div>
                <div className="mt-0.5">{handle.eventCode || '-'}</div>
              </div>
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">事件名称</div>
                <div className="mt-0.5">{handle.eventName || '-'}</div>
              </div>
              <div className="col-span-2">
                <div className="text-xs text-gray-500 dark:text-gray-400">引用 ID</div>
                <div className="font-mono mt-0.5">{handle.strEventReferenceId || '-'}</div>
              </div>
              <div className="col-span-2">
                <div className="text-xs text-gray-500 dark:text-gray-400">处理器</div>
                <div className="mt-0.5 flex items-center gap-2">
                  <span>{handle.processorName} <span className="text-gray-400">({handle.processorId})</span></span>
                  <a
                    href={`#/processors?highlight=${handle.processorId}`}
                    className="text-xs text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
                    title="跳转到处理器配置"
                    onClick={(e) => e.stopPropagation()}
                  >
                    <i className="fa-solid fa-arrow-up-right-from-square mr-0.5"></i>配置
                  </a>
                </div>
              </div>
            </div>
          </div>

          {/* 执行结果 */}
          <div>
            <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-3">执行结果</h4>
            <div className="grid grid-cols-2 md:grid-cols-5 gap-y-4 gap-x-6 text-sm">
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">脚本执行</div>
                <div className="mt-1">
                  <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${getScriptStatusBadge(handle.scriptSuccess, handle.isDeadLetter)}`}>
                    {handle.isDeadLetter ? '死信' : handle.scriptSuccess === true ? '成功' : handle.scriptSuccess === false ? '失败' : '-'}
                  </span>
                </div>
              </div>
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">接口发送</div>
                <div className="mt-1">
                  <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${getSendStatusBadge(handle.needToSend, handle.sendSuccess)}`}>
                    {getSendStatusLabel(handle.needToSend, handle.sendSuccess)}
                  </span>
                </div>
              </div>
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">处理次数</div>
                <div className="mt-0.5">{handle.handleTimes}</div>
              </div>
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">耗时</div>
                <div className="mt-0.5">{handle.lastHandleElapsedMs ? `${handle.lastHandleElapsedMs} ms` : '-'}</div>
              </div>
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">是否完成</div>
                <div className="mt-0.5">{handle.isFinished ? '是' : '否'}</div>
              </div>
              {handle.reason && (
                <div className="col-span-2 md:col-span-3">
                  <div className="text-xs text-gray-500 dark:text-gray-400">脚本返回信息</div>
                  <div className="mt-0.5 text-gray-700 dark:text-gray-300">{handle.reason}</div>
                </div>
              )}
            </div>
          </div>

          {/* 时间 */}
          <div>
            <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-3">时间</h4>
            <div className="grid grid-cols-2 gap-6 text-sm">
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">事件创建</div>
                <div className="font-mono mt-0.5">{handle.createDatetime ? new Date(handle.createDatetime).toLocaleString() : '-'}</div>
              </div>
              <div>
                <div className="text-xs text-gray-500 dark:text-gray-400">最后处理</div>
                <div className="font-mono mt-0.5">{handle.lastHandleDatetime ? new Date(handle.lastHandleDatetime).toLocaleString() : '-'}</div>
              </div>
            </div>
          </div>

          {/* 消息 */}
          {handle.lastHandleMessage && (
            <div>
              <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-2">处理消息</h4>
              <div className="text-sm text-gray-800 dark:text-gray-200 whitespace-pre-wrap break-words font-mono bg-gray-50 dark:bg-gray-900/50 p-3 rounded border border-gray-200 dark:border-gray-700">
                {handle.lastHandleMessage}
              </div>
            </div>
          )}

          {/* 请求与响应 */}
          {(handle.requestData || handle.responseData) && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {handle.requestData && (
                <CollapsibleBlock title="请求数据" defaultOpen={true}>
                  <div className="flex justify-end mb-1">
                    <CopyButton text={formatJsonData(handle.requestData)} label="复制" />
                  </div>
                  <pre className="text-xs text-gray-800 dark:text-gray-200 whitespace-pre-wrap break-words font-mono bg-gray-50 dark:bg-gray-900/50 p-3 rounded border border-gray-200 dark:border-gray-700 max-h-80 overflow-auto">
                    {formatJsonData(handle.requestData)}
                  </pre>
                </CollapsibleBlock>
              )}
              {handle.responseData && (
                <CollapsibleBlock title="响应数据" defaultOpen={true}>
                  <div className="flex justify-end mb-1">
                    <CopyButton text={formatJsonData(handle.responseData)} label="复制" />
                  </div>
                  <pre className="text-xs text-gray-800 dark:text-gray-200 whitespace-pre-wrap break-words font-mono bg-gray-50 dark:bg-gray-900/50 p-3 rounded border border-gray-200 dark:border-gray-700 max-h-80 overflow-auto">
                    {formatJsonData(handle.responseData)}
                  </pre>
                </CollapsibleBlock>
              )}
            </div>
          )}

          {/* 脚本诊断 */}
          {handle.detail && (handle.detail.errorStack || handle.detail.consoleOutput || handle.detail.inputDataSnapshot || handle.detail.errorJavaScriptStackTrace || handle.detail.errorSourceContext) && (
            <div>
              <h4 className="text-sm font-semibold text-red-600 dark:text-red-400 mb-3">
                <i className="fa-solid fa-bug mr-1"></i>脚本诊断
              </h4>
              <div className="space-y-3">
                {(handle.detail.errorLineNumber || handle.detail.errorColumn) && (
                  <div className="text-xs bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-400 p-2 rounded border border-red-200 dark:border-red-800">
                    <i className="fa-solid fa-location-crosshairs mr-1"></i>
                    报错位置: 行 {handle.detail.errorLineNumber ?? '?'}, 列 {handle.detail.errorColumn ?? '?'}
                  </div>
                )}

                {handle.detail.errorSourceContext && (
                  <CollapsibleBlock title="源码上下文" defaultOpen={true}>
                    <div className="flex justify-end mb-1">
                      <CopyButton text={handle.detail.errorSourceContext} label="复制" />
                    </div>
                    <pre className="text-xs text-gray-800 dark:text-gray-200 whitespace-pre-wrap break-words font-mono bg-gray-50 dark:bg-gray-900/50 p-3 rounded border border-gray-200 dark:border-gray-700 max-h-80 overflow-auto">
                      {handle.detail.errorSourceContext}
                    </pre>
                  </CollapsibleBlock>
                )}

                {handle.detail.errorJavaScriptStackTrace && (
                  <CollapsibleBlock title="JavaScript 堆栈" defaultOpen={true}>
                    <div className="flex justify-end mb-1">
                      <CopyButton text={handle.detail.errorJavaScriptStackTrace} label="复制" />
                    </div>
                    <pre className="text-xs text-orange-700 dark:text-orange-400 whitespace-pre-wrap break-words font-mono bg-orange-50 dark:bg-orange-900/20 p-3 rounded border border-orange-200 dark:border-orange-800 max-h-80 overflow-auto">
                      {handle.detail.errorJavaScriptStackTrace}
                    </pre>
                  </CollapsibleBlock>
                )}

                {handle.detail.inputDataSnapshot && (
                  <CollapsibleBlock title="脚本输入数据">
                    <div className="flex justify-end mb-1">
                      <CopyButton text={formatJsonData(handle.detail.inputDataSnapshot)} label="复制" />
                    </div>
                    <pre className="text-xs text-gray-800 dark:text-gray-200 whitespace-pre-wrap break-words font-mono bg-gray-50 dark:bg-gray-900/50 p-3 rounded border border-gray-200 dark:border-gray-700 max-h-60 overflow-auto">
                      {formatJsonData(handle.detail.inputDataSnapshot)}
                    </pre>
                  </CollapsibleBlock>
                )}

                {handle.detail.consoleOutput && (
                  <CollapsibleBlock title="控制台输出">
                    <div className="flex justify-end mb-1">
                      <CopyButton text={handle.detail.consoleOutput} label="复制" />
                    </div>
                    <pre className="text-xs text-gray-800 dark:text-gray-200 whitespace-pre-wrap break-words font-mono bg-gray-50 dark:bg-gray-900/50 p-3 rounded border border-gray-200 dark:border-gray-700 max-h-60 overflow-auto">
                      {handle.detail.consoleOutput}
                    </pre>
                  </CollapsibleBlock>
                )}

                {handle.detail.errorStack && (
                  <CollapsibleBlock title=".NET 异常堆栈">
                    <div className="flex justify-end mb-1">
                      <CopyButton text={handle.detail.errorStack} label="复制" />
                    </div>
                    <pre className="text-xs text-red-700 dark:text-red-400 whitespace-pre-wrap break-words font-mono bg-red-50 dark:bg-red-900/20 p-3 rounded border border-red-200 dark:border-red-800 max-h-80 overflow-auto">
                      {handle.detail.errorStack}
                    </pre>
                  </CollapsibleBlock>
                )}
              </div>
            </div>
          )}
        </div>

        {/* 弹窗底部 */}
        <div className="mt-6 pt-4 border-t border-gray-200 dark:border-gray-700 flex justify-end">
          <button
            onClick={onClose}
            className="px-4 py-2 rounded bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600 text-sm"
          >
            关闭
          </button>
        </div>
      </div>
    </div>
  );
}
