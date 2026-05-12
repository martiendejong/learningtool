import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { bookmarkService } from '../services/bookmarkService';
import type { Bookmark } from '../services/bookmarkService';

export default function BookmarksPage() {
  const [bookmarks, setBookmarks] = useState<Bookmark[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editNote, setEditNote] = useState('');

  useEffect(() => {
    load();
  }, []);

  const load = async () => {
    try {
      setLoading(true);
      const data = await bookmarkService.getBookmarks();
      setBookmarks(data);
    } catch (err) {
      console.error('Failed to load bookmarks:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleRemove = async (courseId: number) => {
    try {
      await bookmarkService.toggleBookmark(courseId);
      setBookmarks(prev => prev.filter(b => b.courseId !== courseId));
    } catch (err) {
      console.error('Failed to remove bookmark:', err);
    }
  };

  const handleSaveNote = async (bm: Bookmark) => {
    try {
      await bookmarkService.updateNote(bm.id, editNote);
      setBookmarks(prev => prev.map(b => b.id === bm.id ? { ...b, note: editNote } : b));
      setEditingId(null);
    } catch (err) {
      console.error('Failed to save note:', err);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="text-gray-500">Loading bookmarks...</div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Bookmarks</h1>
        <p className="text-gray-500 mt-1">Courses you've saved for later</p>
      </div>

      {bookmarks.length === 0 ? (
        <div className="bg-white rounded-xl shadow p-12 text-center">
          <div className="text-5xl mb-4">🔖</div>
          <h2 className="text-lg font-semibold text-gray-700 mb-2">No bookmarks yet</h2>
          <p className="text-gray-500">Bookmark courses from the course detail page to save them here.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {bookmarks.map(bm => (
            <div key={bm.id} className="bg-white rounded-xl shadow p-5">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 text-xs text-gray-400 mb-1">
                    {bm.skill && <span>{bm.skill.name}</span>}
                    {bm.skill && bm.topic && <span>›</span>}
                    {bm.topic && <span>{bm.topic.name}</span>}
                  </div>
                  <Link
                    to={`/course-detail/${bm.courseId}`}
                    className="text-lg font-semibold text-gray-800 hover:text-green-700 transition-colors"
                  >
                    {bm.courseName}
                  </Link>
                  <p className="text-sm text-gray-500 mt-1 line-clamp-2">{bm.courseDescription}</p>

                  {/* Note */}
                  {editingId === bm.id ? (
                    <div className="mt-3 flex gap-2">
                      <input
                        type="text"
                        value={editNote}
                        onChange={e => setEditNote(e.target.value)}
                        placeholder="Add a note..."
                        className="flex-1 text-sm border border-gray-300 rounded px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-green-400"
                        autoFocus
                      />
                      <button
                        onClick={() => handleSaveNote(bm)}
                        className="px-3 py-1.5 text-sm bg-green-600 text-white rounded hover:bg-green-700"
                      >
                        Save
                      </button>
                      <button
                        onClick={() => setEditingId(null)}
                        className="px-3 py-1.5 text-sm text-gray-500 hover:bg-gray-100 rounded"
                      >
                        Cancel
                      </button>
                    </div>
                  ) : (
                    <div className="mt-2 flex items-center gap-2">
                      {bm.note ? (
                        <span className="text-sm text-gray-600 italic">"{bm.note}"</span>
                      ) : (
                        <span className="text-sm text-gray-400">No note</span>
                      )}
                      <button
                        onClick={() => { setEditingId(bm.id); setEditNote(bm.note ?? ''); }}
                        className="text-xs text-green-600 hover:underline"
                      >
                        {bm.note ? 'Edit' : 'Add note'}
                      </button>
                    </div>
                  )}
                </div>

                <div className="flex flex-col items-end gap-2 shrink-0">
                  <Link
                    to={`/course-detail/${bm.courseId}`}
                    className="px-3 py-1.5 text-sm bg-green-600 text-white rounded hover:bg-green-700"
                  >
                    View Course
                  </Link>
                  <button
                    onClick={() => handleRemove(bm.courseId)}
                    className="px-3 py-1.5 text-sm text-red-600 hover:bg-red-50 rounded"
                  >
                    Remove
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
