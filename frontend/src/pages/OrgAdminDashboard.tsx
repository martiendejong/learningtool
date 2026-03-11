import { useState, useEffect } from 'react';
import { useAuthStore } from '../stores/authStore';
import { organizationService } from '../services/organizationService';
import type { OrganizationMember } from '../services/organizationService';

export default function OrgAdminDashboard() {
  const { user } = useAuthStore();
  const [members, setMembers] = useState<OrganizationMember[]>([]);
  const [inviteEmail, setInviteEmail] = useState('');
  const [inviteRole, setInviteRole] = useState('Student');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [inviteUrl, setInviteUrl] = useState('');

  useEffect(() => {
    loadMembers();
  }, []);

  const loadMembers = async () => {
    if (!user?.organizationId) return;

    try {
      const data = await organizationService.getMembers(user.organizationId);
      setMembers(data);
    } catch (err) {
      console.error('Failed to load members:', err);
      setError('Failed to load organization members');
    }
  };

  const handleInvite = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setInviteUrl('');
    setLoading(true);

    try {
      const response = await organizationService.inviteUser(inviteEmail, inviteRole);
      setSuccess(`Invitation sent to ${inviteEmail}`);
      setInviteUrl(response.inviteUrl);
      setInviteEmail('');

      // Reload members (in case user already existed)
      await loadMembers();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send invitation');
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveMember = async (memberId: string, memberEmail: string) => {
    if (!confirm(`Are you sure you want to remove ${memberEmail} from the organization?`)) {
      return;
    }

    try {
      await organizationService.removeMember(memberId);
      setSuccess(`Member ${memberEmail} removed successfully`);
      await loadMembers();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to remove member');
    }
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    setSuccess('Invite URL copied to clipboard!');
  };

  if (!user?.organizationId || user?.roleInOrganization !== 'Admin') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-gray-800 mb-2">Access Denied</h2>
          <p className="text-gray-600">Only organization administrators can access this page.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-6xl mx-auto px-4">
        <h1 className="text-3xl font-bold text-gray-800 mb-8">Organization Dashboard</h1>

        {/* Invite Section */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <h2 className="text-xl font-semibold text-gray-800 mb-4">Invite New Member</h2>

          <form onSubmit={handleInvite} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="md:col-span-2">
                <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
                  Email Address
                </label>
                <input
                  id="email"
                  type="email"
                  value={inviteEmail}
                  onChange={(e) => setInviteEmail(e.target.value)}
                  required
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
                  placeholder="student@example.com"
                />
              </div>

              <div>
                <label htmlFor="role" className="block text-sm font-medium text-gray-700 mb-1">
                  Role
                </label>
                <select
                  id="role"
                  value={inviteRole}
                  onChange={(e) => setInviteRole(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
                >
                  <option value="Student">Student</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="bg-green-600 text-white px-6 py-2 rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 disabled:opacity-50"
            >
              {loading ? 'Sending...' : 'Send Invitation'}
            </button>
          </form>

          {success && (
            <div className="mt-4 bg-green-50 border border-green-200 text-green-800 px-4 py-3 rounded">
              {success}
            </div>
          )}

          {error && (
            <div className="mt-4 bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded">
              {error}
            </div>
          )}

          {inviteUrl && (
            <div className="mt-4 bg-blue-50 border border-blue-200 rounded p-4">
              <p className="text-sm font-medium text-blue-900 mb-2">Invitation Link:</p>
              <div className="flex items-center gap-2">
                <input
                  type="text"
                  value={inviteUrl}
                  readOnly
                  className="flex-1 px-3 py-2 bg-white border border-blue-300 rounded text-sm"
                />
                <button
                  onClick={() => copyToClipboard(inviteUrl)}
                  className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 text-sm"
                >
                  Copy
                </button>
              </div>
              <p className="text-xs text-blue-700 mt-2">Link expires in 7 days</p>
            </div>
          )}
        </div>

        {/* Members List */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-gray-800 mb-4">
            Organization Members ({members.length})
          </h2>

          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Member
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Role
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Joined
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Last Active
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {members.map((member) => (
                  <tr key={member.id}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        {member.profilePictureUrl ? (
                          <img
                            src={member.profilePictureUrl}
                            alt={member.fullName || member.email}
                            className="h-10 w-10 rounded-full mr-3"
                          />
                        ) : (
                          <div className="h-10 w-10 rounded-full bg-green-100 flex items-center justify-center mr-3">
                            <span className="text-green-600 font-semibold">
                              {member.email.charAt(0).toUpperCase()}
                            </span>
                          </div>
                        )}
                        <div>
                          <div className="text-sm font-medium text-gray-900">
                            {member.fullName || member.email}
                          </div>
                          {member.fullName && (
                            <div className="text-sm text-gray-500">{member.email}</div>
                          )}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          member.roleInOrganization === 'Admin'
                            ? 'bg-purple-100 text-purple-800'
                            : 'bg-blue-100 text-blue-800'
                        }`}
                      >
                        {member.roleInOrganization}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {new Date(member.createdAt).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {member.lastLoginAt
                        ? new Date(member.lastLoginAt).toLocaleDateString()
                        : 'Never'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      {member.id !== user.id && (
                        <button
                          onClick={() => handleRemoveMember(member.id, member.email)}
                          className="text-red-600 hover:text-red-900"
                        >
                          Remove
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {members.length === 0 && (
              <div className="text-center py-8 text-gray-500">
                No members yet. Start by inviting your first student!
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
