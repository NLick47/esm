import { EventHandle } from '@/types/event-log';

export type StatusType = 'Success' | 'Fail' | 'Exception' | 'Processing' | '';

export function getScriptStatusBadge(scriptSuccess?: boolean, isDeadLetter?: boolean) {
  if (isDeadLetter) {
    return 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400';
  }
  if (scriptSuccess === true) {
    return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400';
  }
  if (scriptSuccess === false) {
    return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400';
  }
  return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
}

export function getSendStatusBadge(needToSend?: boolean, sendSuccess?: boolean) {
  if (needToSend === false) {
    return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
  }
  if (sendSuccess === true) {
    return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400';
  }
  if (sendSuccess === false) {
    return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400';
  }
  return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
}

export function getSendStatusLabel(needToSend?: boolean, sendSuccess?: boolean) {
  if (needToSend === false) {
    return '无需发送';
  }
  if (sendSuccess === true) {
    return '发送成功';
  }
  if (sendSuccess === false) {
    return '发送失败';
  }
  return '-';
}

export function getRowClassName(handle: EventHandle) {
  if (handle.isDeadLetter) return 'bg-purple-50/50 dark:bg-purple-900/10';
  if (handle.scriptSuccess === false) return 'bg-red-50/50 dark:bg-red-900/10';
  if (handle.scriptSuccess === true && handle.sendSuccess !== false) return 'bg-green-50/30 dark:bg-green-900/5';
  return '';
}

export function formatJsonData(data: string): string {
  try {
    const parsed = JSON.parse(data);
    return JSON.stringify(parsed, null, 2);
  } catch {
    return data;
  }
}

export function formatDateTime(dt?: string): string {
  if (!dt) return '-';
  return new Date(dt).toLocaleString('zh-CN', {
    year: '2-digit', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit', second: '2-digit'
  }).replace(/\//g, '-');
}
