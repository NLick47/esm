import { useState, useEffect, useMemo } from 'react';
import CodeMirror from '@uiw/react-codemirror';
import { javascript } from '@codemirror/lang-javascript';
import { oneDark } from '@codemirror/theme-one-dark';
import { autocompletion, CompletionContext } from '@codemirror/autocomplete';
import { getFunctions } from '@/services/documentation.service';
import type { FunctionDefinition } from '@/types/documentation';

/**
 * 函数自动补全 Hook
 */
export function useFunctionAutocomplete() {
  const [functions, setFunctions] = useState<FunctionDefinition[]>([]);

  useEffect(() => {
    loadFunctions();
  }, []);

  const loadFunctions = async () => {
    try {
      const data = await getFunctions();
      if (data) setFunctions(data);
    } catch (error) {
      console.error('加载函数列表失败:', error);
    }
  };

  // 创建补全源
  const functionCompletionSource = useMemo(() => {
    return (context: CompletionContext) => {
      const word = context.matchBefore(/\w*/);
      if (!word || (word.from === word.to && !context.explicit)) return null;

      return {
        from: word.from,
        options: functions.map(func => ({
          label: func.name,
          type: 'function',
          info: func.description,
          apply: (view: any, completion: any, from: number, to: number) => {
            // 插入函数名和括号
            view.dispatch({
              changes: { from, to, insert: `${func.name}()` },
              selection: { anchor: from + func.name.length + 1 }
            });
          },
          detail: func.category,
          boost: 99 // 提高优先级
        })),
        validFor: /^\w*$/
      };
    };
  }, [functions]);

  return {
    functions,
    functionCompletionSource
  };
}

/**
 * 带函数补全的代码编辑器组件
 */
interface AutocompleteCodeEditorProps {
  code: string;
  height?: string;
  fontSize?: number;
  onChange: (code: string) => void;
  showLineNumbers?: boolean;
}

export const AutocompleteCodeEditor: React.FC<AutocompleteCodeEditorProps> = ({
  code,
  height = '400px',
  fontSize = 14,
  onChange,
  showLineNumbers = true
}) => {
  const { functionCompletionSource } = useFunctionAutocomplete();

  // 创建自动补全扩展
  const autocompleteExtension = useMemo(() => {
    return autocompletion({
      override: [functionCompletionSource],
      maxRenderedOptions: 50,
      activateOnTyping: true
    });
  }, [functionCompletionSource]);

  return (
    <CodeMirror
      value={code}
      height={height}
      style={{ fontSize: `${fontSize}px` }}
      extensions={[javascript(), autocompleteExtension]}
      theme={oneDark}
      onChange={(value) => onChange(value)}
      basicSetup={{
        lineNumbers: showLineNumbers,
        foldGutter: true,
        highlightActiveLineGutter: true,
        highlightSpecialChars: true,
        drawSelection: true,
        dropCursor: true,
        allowMultipleSelections: true,
        indentOnInput: true,
        syntaxHighlighting: true,
        bracketMatching: true,
        closeBrackets: true,
        autocompletion: true,
        rectangularSelection: true,
        crosshairCursor: true,
        highlightActiveLine: true,
        highlightSelectionMatches: true,
        foldKeymap: true
      }}
    />
  );
};

export default AutocompleteCodeEditor;
