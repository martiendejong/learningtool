import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { adminBundleService } from '../services/adminBundleService';
import type { AdminBundle } from '../services/adminBundleService';
import { knowledgeService } from '../services/knowledgeService';
import type { Skill } from '../services/knowledgeService';

export default function AdminBundlesPage() {
  const [bundles, setBundles] = useState<AdminBundle[]>([]);
  const [skills, setSkills] = useState<Skill[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<AdminBundle | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [createForm, setCreateForm] = useState({ name: '', description: '' });
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    Promise.all([
      adminBundleService.getBundles(),
      knowledgeService.getAllSkills(),
    ]).then(([b, s]) => {
      setBundles(b);
      setSkills(s);
    }).catch(console.error).finally(() => setLoading(false));
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!createForm.name.trim()) return;
    setCreating(true);
    setError('');
    try {
      const nb = await adminBundleService.createBundle(createForm.name, createForm.description || undefined);
      setBundles(prev => [...prev, { ...nb, skillCount: 0, orgCount: 0, skills: [] }]);
      setCreateForm({ name: '', description: '' });
      setShowCreate(false);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg ?? 'Failed to create bundle');
    } finally {
      setCreating(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this bundle?')) return;
    try {
      await adminBundleService.deleteBundle(id);
      setBundles(prev => prev.filter(b => b.id !== id));
      if (selected?.id === id) setSelected(null);
    } catch (err) {
      console.error('Failed to delete bundle:', err);
    }
  };

  const toggleSkill = async (skillId: number, inBundle: boolean) => {
    if (!selected) return;
    try {
      if (inBundle) {
        await adminBundleService.removeSkill(selected.id, skillId);
        const updated = { ...selected, skills: selected.skills.filter(s => s.id !== skillId), skillCount: selected.skillCount - 1 };
        setSelected(updated);
        setBundles(prev => prev.map(b => b.id === selected.id ? updated : b));
      } else {
        await adminBundleService.addSkill(selected.id, skillId);
        const skill = skills.find(s => s.id === skillId)!;
        const updated = { ...selected, skills: [...selected.skills, { id: skill.id, name: skill.name }], skillCount: selected.skillCount + 1 };
        setSelected(updated);
        setBundles(prev => prev.map(b => b.id === selected.id ? updated : b));
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      alert(msg ?? 'Failed to update skill');
    }
  };

  return (
    <div className="p-6 max-w-5xl mx-auto">
      <div className="flex items-center gap-4 mb-6">
        <Link to="/admin/users" className="text-green-600 hover:underline text-sm">← Admin</Link>
        <h1 className="text-2xl font-bold text-gray-900">Bundle Management</h1>
        <button
          onClick={() => setShowCreate(true)}
          className="ml-auto px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 text-sm font-medium"
        >
          + New Bundle
        </button>
      </div>

      {error && <div className="mb-4 p-3 bg-red-50 text-red-700 rounded-lg text-sm">{error}</div>}

      {/* Create modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl p-6 max-w-sm w-full mx-4">
            <h3 className="font-semibold text-gray-800 mb-4">Create Bundle</h3>
            <form onSubmit={handleCreate} className="space-y-3">
              <input
                type="text"
                value={createForm.name}
                onChange={e => setCreateForm(f => ({ ...f, name: e.target.value }))}
                placeholder="Bundle name"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-green-400"
                required
                autoFocus
              />
              <textarea
                value={createForm.description}
                onChange={e => setCreateForm(f => ({ ...f, description: e.target.value }))}
                placeholder="Description (optional)"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-green-400 resize-none"
                rows={2}
              />
              <div className="flex gap-2 justify-end">
                <button type="button" onClick={() => setShowCreate(false)} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg text-sm">
                  Cancel
                </button>
                <button type="submit" disabled={creating} className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 text-sm disabled:opacity-60">
                  {creating ? 'Creating...' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {loading ? (
        <div className="text-center text-gray-500 py-12">Loading...</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Bundle list */}
          <div className="space-y-3">
            {bundles.length === 0 && (
              <div className="bg-white rounded-xl shadow p-8 text-center text-gray-500">No bundles yet</div>
            )}
            {bundles.map(b => (
              <div
                key={b.id}
                onClick={() => setSelected(b)}
                className={`bg-white rounded-xl shadow p-5 cursor-pointer hover:shadow-md transition-all ${selected?.id === b.id ? 'ring-2 ring-green-400' : ''}`}
              >
                <div className="flex items-start justify-between mb-1">
                  <div className="font-semibold text-gray-800">{b.name}</div>
                  <button
                    onClick={e => { e.stopPropagation(); handleDelete(b.id); }}
                    className="text-xs text-red-500 hover:text-red-700 px-2 py-1 hover:bg-red-50 rounded"
                  >
                    Delete
                  </button>
                </div>
                {b.description && <p className="text-sm text-gray-500 mb-2">{b.description}</p>}
                <div className="flex gap-3 text-xs text-gray-400">
                  <span>{b.skillCount} skills</span>
                  <span>{b.orgCount} organizations</span>
                </div>
              </div>
            ))}
          </div>

          {/* Skill editor */}
          <div className="bg-white rounded-xl shadow p-6">
            {!selected ? (
              <div className="text-center text-gray-400 py-12">Select a bundle to manage its skills</div>
            ) : (
              <>
                <h2 className="font-semibold text-gray-800 mb-4">Skills in "{selected.name}"</h2>
                <div className="space-y-2 max-h-96 overflow-y-auto">
                  {skills.map(skill => {
                    const inBundle = selected.skills.some(s => s.id === skill.id);
                    return (
                      <div key={skill.id} className="flex items-center justify-between py-2 border-b border-gray-50 last:border-0">
                        <span className="text-sm text-gray-700">{skill.name}</span>
                        <button
                          onClick={() => toggleSkill(skill.id, inBundle)}
                          className={`px-3 py-1 text-xs rounded-lg font-medium transition-colors ${inBundle
                            ? 'bg-red-50 text-red-600 hover:bg-red-100'
                            : 'bg-green-50 text-green-700 hover:bg-green-100'
                          }`}
                        >
                          {inBundle ? 'Remove' : 'Add'}
                        </button>
                      </div>
                    );
                  })}
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
