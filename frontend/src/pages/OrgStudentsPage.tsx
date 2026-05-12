import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useToast } from '../components/Toast';
import { orgAdminService } from '../services/orgAdminService';
import type { OrgStudent, OrgStudentDetail } from '../services/orgAdminService';

export default function OrgStudentsPage() {
  const [students, setStudents] = useState<OrgStudent[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [selected, setSelected] = useState<OrgStudentDetail | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const toast = useToast();
  const [confirmRemove, setConfirmRemove] = useState<string | null>(null);

  useEffect(() => {
    load();
  }, []);

  const load = async (q?: string) => {
    try {
      setLoading(true);
      const data = await orgAdminService.getStudents(q);
      setStudents(data);
    } catch (err) {
      console.error('Failed to load students:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    load(search.trim() || undefined);
  };

  const openDetail = async (userId: string) => {
    setDetailLoading(true);
    try {
      const data = await orgAdminService.getStudent(userId);
      setSelected(data);
    } catch (err) {
      console.error('Failed to load student detail:', err);
    } finally {
      setDetailLoading(false);
    }
  };

  const handleRemove = async (userId: string) => {
    try {
      await orgAdminService.removeStudent(userId);
      setStudents(prev => prev.filter(s => s.id !== userId));
      setConfirmRemove(null);
      setSelected(null);
      toast.success('Student removed from organization');
    } catch (err) {
      toast.error('Failed to remove student');
      console.error('Failed to remove student:', err);
    }
  };

  return (
    <div className="p-6 max-w-5xl mx-auto">
      <div className="flex items-center gap-4 mb-6">
        <Link to="/org/dashboard" className="text-green-600 hover:underline text-sm">← Dashboard</Link>
        <h1 className="text-2xl font-bold text-gray-900">Students</h1>
      </div>

      <form onSubmit={handleSearch} className="flex gap-2 mb-6">
        <input
          type="text"
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search by email..."
          className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
        />
        <button type="submit" className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700">
          Search
        </button>
        {search && (
          <button type="button" onClick={() => { setSearch(''); load(); }} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg">
            Clear
          </button>
        )}
      </form>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* List */}
        <div className="bg-white rounded-xl shadow overflow-hidden">
          {loading ? (
            <div className="p-8 text-center text-gray-500">Loading...</div>
          ) : students.length === 0 ? (
            <div className="p-8 text-center text-gray-500">No students found</div>
          ) : (
            <table className="w-full">
              <thead className="bg-gray-50 border-b">
                <tr>
                  <th className="text-left px-4 py-3 text-xs text-gray-500 font-semibold uppercase">Email</th>
                  <th className="text-right px-4 py-3 text-xs text-gray-500 font-semibold uppercase">Progress</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {students.map(s => (
                  <tr
                    key={s.id}
                    onClick={() => openDetail(s.id)}
                    className={`cursor-pointer hover:bg-green-50 transition-colors ${selected?.id === s.id ? 'bg-green-50' : ''}`}
                  >
                    <td className="px-4 py-3 text-sm text-gray-700">{s.email}</td>
                    <td className="px-4 py-3 text-right text-sm font-semibold text-green-600">{s.completionPct}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {/* Detail */}
        <div className="bg-white rounded-xl shadow p-6">
          {detailLoading && <div className="text-center text-gray-500">Loading...</div>}
          {!detailLoading && !selected && (
            <div className="text-center text-gray-400 py-8">Select a student to view details</div>
          )}
          {!detailLoading && selected && (
            <div>
              <div className="flex items-start justify-between mb-4">
                <div>
                  <div className="font-semibold text-gray-800">{selected.email}</div>
                  <div className="text-sm text-gray-400">Joined {new Date(selected.joinedAt).toLocaleDateString()}</div>
                </div>
                <button
                  onClick={() => setConfirmRemove(selected.id)}
                  className="px-3 py-1.5 text-sm text-red-600 hover:bg-red-50 border border-red-200 rounded-lg"
                >
                  Remove
                </button>
              </div>

              <div className="grid grid-cols-2 gap-3 mb-5">
                <div className="bg-gray-50 rounded-lg p-3 text-center">
                  <div className="text-2xl font-bold text-green-600">{selected.totalCoursesCompleted}</div>
                  <div className="text-xs text-gray-500">Courses Done</div>
                </div>
                <div className="bg-gray-50 rounded-lg p-3 text-center">
                  <div className="text-2xl font-bold text-blue-600">{selected.completionPct}%</div>
                  <div className="text-xs text-gray-500">Overall</div>
                </div>
              </div>

              <h3 className="text-sm font-semibold text-gray-600 mb-3">Skills</h3>
              <div className="space-y-3">
                {selected.skills.map(skill => (
                  <div key={skill.id}>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-gray-700">{skill.name}</span>
                      <span className="text-gray-500">{skill.completedCourses}/{skill.totalCourses}</span>
                    </div>
                    <div className="w-full bg-gray-100 rounded-full h-1.5">
                      <div
                        className="bg-green-500 h-1.5 rounded-full"
                        style={{ width: `${skill.completionPct}%` }}
                      />
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Confirm remove modal */}
      {confirmRemove && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl p-6 max-w-sm w-full mx-4">
            <h3 className="font-semibold text-gray-800 mb-2">Remove student?</h3>
            <p className="text-sm text-gray-500 mb-5">
              This will remove the student from your organization and revert them to an individual account.
            </p>
            <div className="flex gap-3 justify-end">
              <button onClick={() => setConfirmRemove(null)} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg">
                Cancel
              </button>
              <button
                onClick={() => handleRemove(confirmRemove)}
                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
              >
                Remove
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
