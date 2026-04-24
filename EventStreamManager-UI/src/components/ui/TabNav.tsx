import { cn } from '@/lib/utils';

interface TabItem {
  key: string;
  label: string;
  icon?: string;
}

interface TabNavProps {
  tabs: TabItem[];
  activeKey: string;
  onChange: (key: string) => void;
  className?: string;
}

export function TabNav({ tabs, activeKey, onChange, className }: TabNavProps) {
  return (
    <div className={cn('flex border-b border-gray-200 dark:border-gray-700', className)}>
      {tabs.map((tab) => (
        <button
          key={tab.key}
          onClick={() => onChange(tab.key)}
          className={cn(
            'flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors',
            activeKey === tab.key
              ? 'border-b-2 border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400'
              : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          )}
        >
          {tab.icon && <i className={tab.icon}></i>}
          {tab.label}
        </button>
      ))}
    </div>
  );
}
