import { useState } from 'react';
import Editor from '@monaco-editor/react';

interface CodeBlockProps {
  code: string;
  language: string;
}

export default function CodeBlock({ code: initialCode, language }: CodeBlockProps) {
  const [code, setCode] = useState(initialCode);
  const [output, setOutput] = useState('');
  const [showOutput, setShowOutput] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  // Check if this is a web language that can be run
  const isRunnable = ['html', 'css', 'javascript', 'js'].includes(language.toLowerCase());

  const handleRun = () => {
    if (!isRunnable) return;

    const lang = language.toLowerCase();

    if (lang === 'html') {
      setOutput(code);
    } else if (lang === 'css') {
      setOutput(`
        <!DOCTYPE html>
        <html>
          <head>
            <style>${code}</style>
          </head>
          <body>
            <div class="demo">This is a demo element</div>
            <p>Styled content</p>
            <button>Button</button>
          </body>
        </html>
      `);
    } else if (['javascript', 'js'].includes(lang)) {
      setOutput(`
        <!DOCTYPE html>
        <html>
          <body>
            <div id="output"></div>
            <script>
              // Capture console.log
              const originalLog = console.log;
              console.log = function(...args) {
                const output = document.getElementById('output');
                output.innerHTML += args.join(' ') + '<br>';
                originalLog.apply(console, args);
              };

              try {
                ${code}
              } catch (error) {
                document.getElementById('output').innerHTML += '<span style="color: red;">Error: ' + error.message + '</span>';
              }
            </script>
          </body>
        </html>
      `);
    }

    setShowOutput(true);
  };

  const handleCopy = () => {
    navigator.clipboard.writeText(code);
  };

  return (
    <div className="my-4 border-2 border-green-200 rounded-lg overflow-hidden bg-white shadow-lg">
      {/* Header */}
      <div className="bg-gradient-to-r from-green-600 to-green-500 px-4 py-2 flex items-center justify-between text-white relative z-20">
        <div className="flex items-center gap-2">
          <span className="font-mono text-sm font-bold">{language.toUpperCase()}</span>
          <span className="text-xs opacity-80">Interactive Editor</span>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={handleCopy}
            className="px-3 py-1 bg-white/20 hover:bg-white/30 rounded text-xs font-medium transition-colors"
            title="Copy code"
          >
            📋 Copy
          </button>
          <button
            onClick={handleRun}
            disabled={!isRunnable}
            className={`px-4 py-1.5 rounded text-xs font-bold transition-all ${
              isRunnable
                ? 'bg-white text-green-600 hover:bg-green-50 hover:shadow-md'
                : 'bg-white/10 text-white/50 cursor-not-allowed'
            }`}
            title={isRunnable ? 'Run this code and see the output' : 'This language needs an external compiler'}
          >
            ▶ Run Code
          </button>
          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className="px-3 py-1 bg-white/20 hover:bg-white/30 rounded text-xs font-medium transition-colors"
            title={isExpanded ? 'Collapse' : 'Expand'}
          >
            {isExpanded ? '⬇ Collapse' : '⬆ Expand'}
          </button>
        </div>
      </div>

      {/* Editor */}
      <div className="bg-gray-50 relative z-10">
        <Editor
          height={isExpanded ? '400px' : '200px'}
          language={language.toLowerCase() === 'js' ? 'javascript' : language.toLowerCase()}
          value={code}
          onChange={(value) => setCode(value || '')}
          theme="vs-dark"
          options={{
            minimap: { enabled: false },
            fontSize: 14,
            lineNumbers: 'on',
            scrollBeyondLastLine: false,
            automaticLayout: true,
            tabSize: 2,
            wordWrap: 'on',
          }}
        />
      </div>

      {/* Output Preview */}
      {showOutput && isRunnable && (
        <div className="border-t-2 border-green-200">
          <div className="bg-green-50 px-4 py-2 flex items-center justify-between">
            <span className="text-sm font-bold text-green-800">Output:</span>
            <button
              onClick={() => setShowOutput(false)}
              className="text-xs text-green-600 hover:text-green-800"
            >
              ✕ Close
            </button>
          </div>
          <div className="bg-white p-4">
            <iframe
              srcDoc={output}
              className="w-full h-64 border border-gray-300 rounded"
              title="Code output"
              sandbox="allow-scripts"
            />
          </div>
        </div>
      )}

      {/* Help Text */}
      <div className="bg-green-50 px-4 py-2 text-xs text-green-700 border-t border-green-200">
        💡 <strong>Tip:</strong> Edit the code above and click "Run Code" to see the results!
        {!isRunnable && ' (This language needs an external compiler to run)'}
      </div>
    </div>
  );
}
