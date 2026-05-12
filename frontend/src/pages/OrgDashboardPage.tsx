import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { orgAdminService } from '../services/orgAdminService';
import type { OrgOverview } from '../services/orgAdminService';

export default function OrgDashboardPage() {
  const [overview, setOverview] = useState<OrgOverview | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    orgAdminService.getOverview()
      .then(setOverview)
      .catch(() => setError('Failed to load dashboard'))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return <div className="flex items-center justify-center py-20 text-gray-500">Loading...</div>;
  }

  if (error || !overview) {
    return <div className="p-6 text-red-600">{error || 'Not found'}</div>;
  }

  return (
    <div className="p-6 max-w-5xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">{overview.orgName}</h1>
        <p className="text-gray-500 mt-1">Organization Dashboard</p>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
        <div className="bg-white rounded-xl shadow p-5 text-center">
          <div className="text-3xl font-bold text-green-600">{overview.totalStudents}</div>
          <div className="text-sm text-gray-500 mt-1">Total Students</div>
        </div>
        <div className="bg-white rounded-xl shadow p-5 text-center">
          <div className="text-3xl font-bold text-blue-600">{overview.activeStudents}</div>
          <div className="text-sm text-gray-500 mt-1">Active (30 days)</div>
        </div>
        <div className="bg-white rounded-xl shadow p-5 text-center">
          <div className="text-3xl font-bold text-purple-600">{overview.totalCourses}</div>
          <div className="text-sm text-gray-500 mt-1">Total Courses</div>
        </div>
        <div className="bg-white rounded-xl shadow p-5 text-center">
          <div className="text-3xl font-bold text-orange-500">{overview.avgCompletionPct}%</div>
          <div className="text-sm text-gray-500 mt-1">Avg. Completion</div>
        </div>
      </div>

      {/* Quick links */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <Link to="/org/students" className="bg-white rounded-xl shadow p-5 hover:shadow-md transition-shadow flex items-center gap-4">
          <span className="text-3xl">👥</span>
          <div>
            <div className="font-semibold text-gray-800">Manage Students</div>
            <div className="text-sm text-gray-500">View & remove students</div>
          </div>
        </Link>
        <Link to="/org/invites" className="bg-white rounded-xl shadow p-5 hover:shadow-md transition-shadow flex items-center gap-4">
          <span className="text-3xl">📨</span>
          <div>
            <div className="font-semibold text-gray-800">Invite Links</div>
            <div className="text-sm text-gray-500">Create & manage invites</div>
          </div>
        </Link>
        <Link to="/org/bundles" className="bg-white rounded-xl shadow p-5 hover:shadow-md transition-shadow flex items-center gap-4">
          <span className="text-3xl">📦</span>
          <div>
            <div className="font-semibold text-gray-800">Bundles</div>
            <div className="text-sm text-gray-500">Assign skill bundles</div>
          </div>
        </Link>
      </div>

      {/* Recent students */}
      {overview.recentStudents.length > 0 && (
        <div className="bg-white rounded-xl shadow p-6">
          <h2 className="text-lg font-semibold text-gray-800 mb-4">Recent Students</h2>
          <div className="divide-y divide-gray-100">
            {overview.recentStudents.map(s => (
              <div key={s.id} className="py-3 flex items-center justify-between">
                <span className="text-gray-700">{s.email}</span>
                <span className="text-sm text-gray-400">
                  Joined {new Date(s.joinedAt).toLocaleDateString()}
                </span>
              </div>
            ))}
          </div>
          <Link to="/org/students" className="mt-4 inline-block text-sm text-green-600 hover:underline">
            View all students →
          </Link>
        </div>
      )}
    </div>
  );
}
