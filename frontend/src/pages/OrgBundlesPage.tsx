import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useToast } from '../components/Toast';
import { orgAdminService } from '../services/orgAdminService';
import type { OrgBundle, BundleUser } from '../services/orgAdminService';

export default function OrgBundlesPage() {
  const [bundles, setBundles] = useState<OrgBundle[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<OrgBundle | null>(null);
  const [users, setUsers] = useState<BundleUser[]>([]);
  const toast = useToast();
  const [usersLoading, setUsersLoading] = useState(false);

  useEffect(() => {
    orgAdminService.getBundles()
      .then(setBundles)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const openBundle = async (bundle: OrgBundle) => {
    setSelected(bundle);
    setUsersLoading(true);
    try {
      const data = await orgAdminService.getBundleUsers(bundle.bundleId);
      setUsers(data);
    } catch (err) {
      console.error('Failed to load bundle users:', err);
    } finally {
      setUsersLoading(false);
    }
  };

  const toggleAssign = async (userId: string, isAssigned: boolean) => {
    if (!selected) return;
    try {
      if (isAssigned) {
        await orgAdminService.revokeBundle(selected.bundleId, userId);
      } else {
        await orgAdminService.assignBundle(selected.bundleId, userId);
      }
      setUsers(prev => prev.map(u => u.id === userId ? { ...u, isAssigned: !isAssigned } : u));
      toast.success(isAssigned ? 'Bundle access revoked' : 'Bundle assigned');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg ?? 'Failed to update assignment');
    }
  };

  const assignedCount = users.filter(u => u.isAssigned).length;

  return (
    <div className="p-6 max-w-5xl mx-auto">
      <div className="flex items-center gap-4 mb-6">
        <Link to="/org/dashboard" className="text-green-600 hover:underline text-sm">← Dashboard</Link>
        <h1 className="text-2xl font-bold text-gray-900">Bundles</h1>
      </div>

      {loading ? (
        <div className="text-center text-gray-500 py-12">Loading...</div>
      ) : bundles.length === 0 ? (
        <div className="bg-white rounded-xl shadow p-12 text-center text-gray-500">
          No bundles assigned to your organization yet. Contact your system administrator.
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Bundle list */}
          <div className="space-y-3">
            {bundles.map(b => (
              <div
                key={b.id}
                onClick={() => openBundle(b)}
                className={`bg-white rounded-xl shadow p-5 cursor-pointer hover:shadow-md transition-all ${selected?.id === b.id ? 'ring-2 ring-green-400' : ''}`}
              >
                <div className="flex items-start justify-between mb-2">
                  <div className="font-semibold text-gray-800">{b.bundleName}</div>
                  <span className="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded-full">
                    {b.isUnlimited ? 'Unlimited' : `${b.assignedUsers}/${b.maxUsers} seats`}
                  </span>
                </div>
                {b.bundleDescription && (
                  <p className="text-sm text-gray-500 mb-3">{b.bundleDescription}</p>
                )}
                <div className="flex flex-wrap gap-1">
                  {b.skills.map(s => (
                    <span key={s.id} className="text-xs px-2 py-0.5 bg-gray-100 text-gray-600 rounded-full">
                      {s.name}
                    </span>
                  ))}
                </div>
              </div>
            ))}
          </div>

          {/* Assignment panel */}
          <div className="bg-white rounded-xl shadow p-6">
            {!selected ? (
              <div className="text-center text-gray-400 py-12">Select a bundle to manage assignments</div>
            ) : (
              <>
                <div className="mb-4">
                  <h2 className="font-semibold text-gray-800">{selected.bundleName}</h2>
                  <p className="text-sm text-gray-500">
                    {assignedCount} assigned
                    {!selected.isUnlimited && ` / ${selected.maxUsers} seats`}
                  </p>
                </div>

                {usersLoading ? (
                  <div className="text-center text-gray-500 py-8">Loading...</div>
                ) : (
                  <div className="space-y-2 max-h-96 overflow-y-auto">
                    {users.map(u => (
                      <div key={u.id} className="flex items-center justify-between py-2 border-b border-gray-50 last:border-0">
                        <span className="text-sm text-gray-700">{u.email}</span>
                        <button
                          onClick={() => toggleAssign(u.id, u.isAssigned)}
                          className={`px-3 py-1 text-xs rounded-lg font-medium transition-colors ${u.isAssigned
                            ? 'bg-red-50 text-red-600 hover:bg-red-100'
                            : 'bg-green-50 text-green-700 hover:bg-green-100'
                          }`}
                        >
                          {u.isAssigned ? 'Revoke' : 'Assign'}
                        </button>
                      </div>
                    ))}
                    {users.length === 0 && (
                      <div className="text-center text-gray-400 py-4">No members yet</div>
                    )}
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
