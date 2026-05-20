import { useState } from 'react';
import { toast } from 'sonner';

interface Props {
  text: string;
  label: string;
}

export default function CopyButton({ text, label }: Props) {
  const [copied, setCopied] = useState(false);
  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(text);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      toast.error('复制失败');
    }
  };
  return (
    <button
      onClick={handleCopy}
      className="text-xs text-gray-500 hover:text-blue-600 dark:text-gray-400 dark:hover:text-blue-400 flex items-center gap-1 transition-colors"
    >
      <i className={`fa-solid ${copied ? 'fa-check text-green-500' : 'fa-copy'}`}></i>
      {copied ? '已复制' : label}
    </button>
  );
}
