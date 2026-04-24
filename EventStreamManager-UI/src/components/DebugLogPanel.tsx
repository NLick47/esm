import { cn } from '@/lib/utils';

export interface DebugLog {
  type: 'error' | 'warn' | 'success' | 'output' | 'info';
  message: string;
  timestamp?: string;
}

interface DebugLogPanelProps {
  logs: DebugLog[] | string[];
  emptyHint?: string;
  className?: string;
}

const logTypeColors: Record<DebugLog['type'], string> = {
  error: 'text-red-400',
  warn: 'text-yellow-400',
  success: 'text-green-400',
  output: 'text-blue-400',
  info: 'text-gray-300',
};

const logTypeIcons: Record<DebugLog['type'], string> = {
  error: 'fa-circle-xmark',
  warn: 'fa-triangle-exclamation',
  success: 'fa-circle-check',
  output: 'fa-terminal',
  info: 'fa-circle-info',
};

function parseLogType(log: string): DebugLog['type'] {
  if (log.includes('❌') || log.includes('[ERROR]')) return 'error';
  if (log.includes('⚠️') || log.includes('[WARNING]')) return 'warn';
  if (log.includes('✅')) return 'success';
  if (log.includes('📤') || log.includes('[OUTPUT]')) return 'output';
  return 'info';
}

function normalizeLogs(logs: DebugLog[] | string[]): DebugLog[] {
  if (logs.length === 0) return [];
  if (typeof logs[0] === 'string') {
    return (logs as string[]).map(message => ({
      type: parseLogType(message),
      message,
    }));
  }
  return logs as DebugLog[];
}

export function DebugLogPanel({ logs, emptyHint = '准备就绪，请点击运行调试开始', className }: DebugLogPanelProps) {
  const normalized = normalizeLogs(logs);

  return (
    <div className={cn('h-full min-h-[300px] overflow-auto rounded-lg bg-gray-900 p-4 font-mono text-sm', className)}>
      {normalized.length === 0 ? (
        <div className="flex h-full items-center justify-center text-gray-500">
          <div className="text-center">
            <i className="fa-solid fa-bug text-2xl mb-2"></i>
            <p>{emptyHint}</p>
          </div>
        </div>
      ) : (
        <div className="space-y-1">
          {normalized.map((log, index) => (
            <div key={index} className={cn('flex gap-2', logTypeColors[log.type])}>
              {log.timestamp && <span className="shrink-0 text-gray-500">[{log.timestamp}]</span>}
              <i className={cn('fa-solid mt-1 shrink-0', logTypeIcons[log.type])}></i>
              <span className="break-all whitespace-pre-wrap">{log.message}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
