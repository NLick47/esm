import { useState, ReactNode } from 'react';

interface Props {
  title: string;
  children: ReactNode;
  defaultOpen?: boolean;
}

export default function CollapsibleBlock({ title, children, defaultOpen = false }: Props) {
  const [open, setOpen] = useState(defaultOpen);
  return (
    <div className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
      <button
        onClick={() => setOpen(!open)}
        className="w-full flex items-center justify-between px-3 py-2 bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 text-sm font-medium transition-colors"
      >
        <span>{title}</span>
        <i className={`fa-solid fa-chevron-${open ? 'up' : 'down'} text-xs text-gray-400`}></i>
      </button>
      {open && <div className="p-3">{children}</div>}
    </div>
  );
}
