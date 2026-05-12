import { useState, useEffect, useRef } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { searchService } from '../services/searchService';
import type { SearchResults } from '../services/searchService';

export default function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [query, setQuery] = useState(searchParams.get('q') ?? '');
  const [results, setResults] = useState<SearchResults | null>(null);
  const [loading, setLoading] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    const q = searchParams.get('q') ?? '';
    if (q.length >= 2) {
      doSearch(q);
    }
  }, []);

  const handleInput = (value: string) => {
    setQuery(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (value.length < 2) {
      setResults(null);
      return;
    }
    debounceRef.current = setTimeout(() => {
      setSearchParams({ q: value });
      doSearch(value);
    }, 300);
  };

  const doSearch = async (q: string) => {
    try {
      setLoading(true);
      const data = await searchService.search(q);
      setResults(data);
    } catch (err) {
      console.error('Search failed:', err);
    } finally {
      setLoading(false);
    }
  };

  const total = results
    ? results.skills.length + results.topics.length + results.courses.length
    : 0;

  return (
    <div className="p-6 max-w-3xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Search</h1>

      <div className="relative mb-6">
        <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 text-lg">🔍</span>
        <input
          type="text"
          value={query}
          onChange={e => handleInput(e.target.value)}
          placeholder="Search skills, topics, courses..."
          className="w-full pl-11 pr-4 py-3 border border-gray-300 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-green-400 text-gray-800"
          autoFocus
        />
        {loading && (
          <span className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 animate-spin">⟳</span>
        )}
      </div>

      {query.length > 0 && query.length < 2 && (
        <p className="text-sm text-gray-400">Type at least 2 characters to search.</p>
      )}

      {results && (
        <div className="space-y-6">
          {total === 0 && (
            <div className="text-center py-12 text-gray-500">
              No results found for "{query}"
            </div>
          )}

          {results.skills.length > 0 && (
            <section>
              <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wide mb-3">
                Skills ({results.skills.length})
              </h2>
              <div className="space-y-2">
                {results.skills.map(s => (
                  <Link
                    key={s.id}
                    to={`/skill/${s.id}`}
                    className="flex items-start gap-3 bg-white rounded-lg shadow-sm p-4 hover:shadow-md hover:border-green-200 border border-transparent transition-all"
                  >
                    <span className="text-xl mt-0.5">🎯</span>
                    <div>
                      <div className="font-semibold text-gray-800">{s.name}</div>
                      {s.description && (
                        <div className="text-sm text-gray-500 line-clamp-1">{s.description}</div>
                      )}
                      <span className="inline-block mt-1 text-xs px-2 py-0.5 bg-green-100 text-green-700 rounded-full">
                        {s.difficulty}
                      </span>
                    </div>
                  </Link>
                ))}
              </div>
            </section>
          )}

          {results.topics.length > 0 && (
            <section>
              <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wide mb-3">
                Topics ({results.topics.length})
              </h2>
              <div className="space-y-2">
                {results.topics.map(t => (
                  <Link
                    key={t.id}
                    to={`/topic/${t.id}`}
                    className="flex items-start gap-3 bg-white rounded-lg shadow-sm p-4 hover:shadow-md hover:border-blue-200 border border-transparent transition-all"
                  >
                    <span className="text-xl mt-0.5">📚</span>
                    <div>
                      <div className="font-semibold text-gray-800">{t.name}</div>
                      <div className="text-xs text-gray-400">{t.skillName}</div>
                      {t.description && (
                        <div className="text-sm text-gray-500 line-clamp-1">{t.description}</div>
                      )}
                    </div>
                  </Link>
                ))}
              </div>
            </section>
          )}

          {results.courses.length > 0 && (
            <section>
              <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wide mb-3">
                Courses ({results.courses.length})
              </h2>
              <div className="space-y-2">
                {results.courses.map(c => (
                  <Link
                    key={c.id}
                    to={`/course-detail/${c.id}`}
                    className="flex items-start gap-3 bg-white rounded-lg shadow-sm p-4 hover:shadow-md hover:border-purple-200 border border-transparent transition-all"
                  >
                    <span className="text-xl mt-0.5">🎓</span>
                    <div>
                      <div className="font-semibold text-gray-800">{c.name}</div>
                      <div className="text-xs text-gray-400">{c.skillName} › {c.topicName}</div>
                      {c.description && (
                        <div className="text-sm text-gray-500 line-clamp-1">{c.description}</div>
                      )}
                    </div>
                  </Link>
                ))}
              </div>
            </section>
          )}
        </div>
      )}
    </div>
  );
}
