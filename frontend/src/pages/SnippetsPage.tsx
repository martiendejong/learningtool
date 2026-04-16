import { useEffect, useMemo, useState } from 'react';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { oneLight } from 'react-syntax-highlighter/dist/esm/styles/prism';
import { snippetService } from '../services/snippetService';
import type { CodeSnippet, CreateSnippetPayload } from '../services/snippetService';

const LANGUAGES = [
  'plaintext',
  'javascript',
  'typescript',
  'python',
  'csharp',
  'java',
  'go',
  'rust',
  'sql',
  'html',
  'css',
  'bash',
  'json',
];

const emptyDraft: CreateSnippetPayload = {
  title: '',
  code: '',
  language: 'plaintext',
  description: '',
  tags: [],
  isPublic: false,
};

export default function SnippetsPage() {
  const [snippets, setSnippets] = useState<CodeSnippet[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [languageFilter, setLanguageFilter] = useState('');
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const [copiedId, setCopiedId] = useState<number | null>(null);

  const [showEditor, setShowEditor] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [draft, setDraft] = useState<CreateSnippetPayload>(emptyDraft);
  const [tagsInput, setTagsInput] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadSnippets();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadSnippets = async (filters: { language?: string; search?: string } = {}) => {
    try {
      setLoading(true);
      setError(null);
      const data = await snippetService.list(filters);
      setSnippets(data);
    } catch (err) {
      console.error('Failed to load snippets:', err);
      setError('Failed to load your snippets. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = () => {
    loadSnippets({
      language: languageFilter || undefined,
      search: search.trim() || undefined,
    });
  };

  const clearFilters = () => {
    setSearch('');
    setLanguageFilter('');
    loadSnippets();
  };

  const startCreate = () => {
    setEditingId(null);
    setDraft(emptyDraft);
    setTagsInput('');
    setShowEditor(true);
  };

  const startEdit = (snippet: CodeSnippet) => {
    setEditingId(snippet.id);
    setDraft({
      title: snippet.title,
      code: snippet.code,
      language: snippet.language,
      description: snippet.description ?? '',
      tags: snippet.tags,
      isPublic: snippet.isPublic,
    });
    setTagsInput(snippet.tags.join(', '));
    setShowEditor(true);
  };

  const cancelEditor = () => {
    setShowEditor(false);
    setEditingId(null);
    setDraft(emptyDraft);
    setTagsInput('');
  };

  const handleSave = async () => {
    if (!draft.title.trim() || !draft.code.trim()) {
      setError('Title and code are required.');
      return;
    }

    const tags = tagsInput
      .split(',')
      .map((t) => t.trim())
      .filter(Boolean);

    try {
      setSaving(true);
      setError(null);
      if (editingId != null) {
        await snippetService.update(editingId, {
          title: draft.title,
          code: draft.code,
          language: draft.language,
          description: draft.description,
          tags,
          isPublic: draft.isPublic,
        });
      } else {
        await snippetService.create({
          title: draft.title,
          code: draft.code,
          language: draft.language,
          description: draft.description,
          tags,
          isPublic: draft.isPublic,
        });
      }
      cancelEditor();
      await loadSnippets({
        language: languageFilter || undefined,
        search: search.trim() || undefined,
      });
    } catch (err) {
      console.error('Failed to save snippet:', err);
      setError('Failed to save snippet. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this snippet? This cannot be undone.')) return;
    try {
      await snippetService.delete(id);
      setSnippets((prev) => prev.filter((s) => s.id !== id));
      if (expandedId === id) setExpandedId(null);
    } catch (err) {
      console.error('Failed to delete snippet:', err);
      setError('Failed to delete snippet.');
    }
  };

  const handleCopy = async (snippet: CodeSnippet) => {
    try {
      await navigator.clipboard.writeText(snippet.code);
      setCopiedId(snippet.id);
      setTimeout(() => setCopiedId((curr) => (curr === snippet.id ? null : curr)), 1500);
    } catch (err) {
      console.error('Clipboard write failed:', err);
    }
  };

  const availableLanguages = useMemo(() => {
    const fromData = Array.from(new Set(snippets.map((s) => s.language))).sort();
    const union = new Set<string>([...LANGUAGES, ...fromData]);
    return Array.from(union).sort();
  }, [snippets]);

  return (
    <div className="p-6">
      <div className="max-w-5xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">📚 My Snippet Library</h1>
            <p className="text-gray-600 mt-1">
              Save, organize, and re-use your code. Search by title or description; filter by language.
            </p>
          </div>
          <button
            onClick={startCreate}
            className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 font-medium"
          >
            + New Snippet
          </button>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-lg shadow p-4 mb-6 flex flex-wrap gap-3 items-end">
          <div className="flex-1 min-w-[200px]">
            <label className="block text-xs font-medium text-gray-600 mb-1">Search</label>
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') applyFilters();
              }}
              placeholder="Search title or description..."
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Language</label>
            <select
              value={languageFilter}
              onChange={(e) => setLanguageFilter(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
            >
              <option value="">All</option>
              {availableLanguages.map((l) => (
                <option key={l} value={l}>
                  {l}
                </option>
              ))}
            </select>
          </div>
          <button
            onClick={applyFilters}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Apply
          </button>
          <button
            onClick={clearFilters}
            className="px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
          >
            Clear
          </button>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-50 text-red-700 border border-red-200 rounded-md">
            {error}
          </div>
        )}

        {/* Editor */}
        {showEditor && (
          <div className="bg-white rounded-lg shadow p-5 mb-6 border-2 border-green-500">
            <h2 className="text-xl font-semibold mb-4">
              {editingId != null ? 'Edit Snippet' : 'New Snippet'}
            </h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Title *</label>
                <input
                  type="text"
                  value={draft.title}
                  onChange={(e) => setDraft({ ...draft, title: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  maxLength={255}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Language</label>
                <select
                  value={draft.language}
                  onChange={(e) => setDraft({ ...draft, language: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                >
                  {LANGUAGES.map((l) => (
                    <option key={l} value={l}>
                      {l}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <input
                type="text"
                value={draft.description ?? ''}
                onChange={(e) => setDraft({ ...draft, description: e.target.value })}
                placeholder="Optional: what does this snippet solve?"
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
                maxLength={2000}
              />
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Tags (comma-separated)
              </label>
              <input
                type="text"
                value={tagsInput}
                onChange={(e) => setTagsInput(e.target.value)}
                placeholder="authentication, api, loops"
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
              />
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">Code *</label>
              <textarea
                value={draft.code}
                onChange={(e) => setDraft({ ...draft, code: e.target.value })}
                rows={12}
                className="w-full px-3 py-2 border border-gray-300 rounded-md font-mono text-sm"
                spellCheck={false}
              />
            </div>

            <div className="mb-4 flex items-center gap-2">
              <input
                id="isPublic"
                type="checkbox"
                checked={draft.isPublic ?? false}
                onChange={(e) => setDraft({ ...draft, isPublic: e.target.checked })}
              />
              <label htmlFor="isPublic" className="text-sm text-gray-700">
                Make this snippet public (opt-in sharing)
              </label>
            </div>

            <div className="flex gap-2">
              <button
                onClick={handleSave}
                disabled={saving}
                className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 font-medium disabled:opacity-50"
              >
                {saving ? 'Saving...' : editingId != null ? 'Save Changes' : 'Save Snippet'}
              </button>
              <button
                onClick={cancelEditor}
                className="px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
              >
                Cancel
              </button>
            </div>
          </div>
        )}

        {/* List */}
        {loading ? (
          <div className="text-center py-12 text-gray-600">Loading snippets...</div>
        ) : snippets.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-8 text-center">
            <h3 className="text-lg font-semibold text-gray-800 mb-2">No snippets yet</h3>
            <p className="text-gray-600 mb-4">
              Save reusable code from your lessons and build your personal knowledge base.
            </p>
            <button
              onClick={startCreate}
              className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 font-medium"
            >
              Create your first snippet
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            {snippets.map((snippet) => {
              const expanded = expandedId === snippet.id;
              return (
                <div key={snippet.id} className="bg-white rounded-lg shadow">
                  <div className="p-4 flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap">
                        <h3 className="text-lg font-semibold text-gray-900 truncate">
                          {snippet.title}
                        </h3>
                        <span className="px-2 py-0.5 bg-blue-100 text-blue-700 rounded text-xs font-medium">
                          {snippet.language}
                        </span>
                        {snippet.isPublic && (
                          <span className="px-2 py-0.5 bg-purple-100 text-purple-700 rounded text-xs font-medium">
                            public
                          </span>
                        )}
                      </div>
                      {snippet.description && (
                        <p className="text-sm text-gray-600 mt-1">{snippet.description}</p>
                      )}
                      {snippet.tags.length > 0 && (
                        <div className="flex flex-wrap gap-1 mt-2">
                          {snippet.tags.map((tag) => (
                            <span
                              key={tag}
                              className="px-2 py-0.5 bg-gray-100 text-gray-700 rounded text-xs"
                            >
                              #{tag}
                            </span>
                          ))}
                        </div>
                      )}
                      <p className="text-xs text-gray-400 mt-2">
                        Saved {new Date(snippet.createdAt).toLocaleDateString()}
                      </p>
                    </div>
                    <div className="flex items-center gap-1 shrink-0">
                      <button
                        onClick={() => handleCopy(snippet)}
                        className="px-3 py-1.5 text-sm bg-gray-100 hover:bg-gray-200 rounded"
                        title="Copy code"
                      >
                        {copiedId === snippet.id ? '✓ Copied' : '📋 Copy'}
                      </button>
                      <button
                        onClick={() => setExpandedId(expanded ? null : snippet.id)}
                        className="px-3 py-1.5 text-sm bg-gray-100 hover:bg-gray-200 rounded"
                      >
                        {expanded ? 'Hide' : 'View'}
                      </button>
                      <button
                        onClick={() => startEdit(snippet)}
                        className="px-3 py-1.5 text-sm bg-gray-100 hover:bg-gray-200 rounded"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => handleDelete(snippet.id)}
                        className="px-3 py-1.5 text-sm bg-red-50 text-red-700 hover:bg-red-100 rounded"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                  {expanded && (
                    <div className="border-t px-4 py-3 bg-gray-50 overflow-x-auto">
                      <SyntaxHighlighter
                        language={snippet.language === 'plaintext' ? 'text' : snippet.language}
                        style={oneLight}
                        customStyle={{ margin: 0, background: 'transparent', fontSize: '0.85rem' }}
                      >
                        {snippet.code}
                      </SyntaxHighlighter>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
