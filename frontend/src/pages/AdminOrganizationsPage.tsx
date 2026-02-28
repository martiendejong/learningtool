import { useState, useEffect } from 'react';
import { useAuthStore } from '../stores/authStore';
import { organizationService, type Organization } from '../services/organizationService';
import { useNavigate } from 'react-router-dom';

export default function AdminOrganizationsPage() {
  const navigate = useNavigate();
  const { user: currentUser } = useAuthStore();
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Create/Edit dialog state
  const [showDialog, setShowDialog] = useState(false);
  const [dialogMode, setDialogMode] = useState<'create' | 'edit'>('create');
  const [selectedOrg, setSelectedOrg] = useState<Organization | null>(null);
  const [formData, setFormData] = useState({ name: '', description: '', isActive: true });

  // Delete confirmation
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [orgToDelete, setOrgToDelete] = useState<Organization | null>(null);

  useEffect(() => {
    // Check if user is admin
    if (currentUser?.role !== 'ADMIN') {
      navigate('/');
      return;
    }

    loadOrganizations();
  }, [currentUser, navigate]);

  async function loadOrganizations() {
    try {
      setLoading(true);
      const data = await organizationService.getOrganizations();
      setOrganizations(data);
      setError(null);
    } catch (err) {
      setError('Failed to load organizations');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  function openCreateDialog() {
    setDialogMode('create');
    setSelectedOrg(null);
    setFormData({ name: '', description: '', isActive: true });
    setShowDialog(true);
  }

  function openEditDialog(org: Organization) {
    setDialogMode('edit');
    setSelectedOrg(org);
    setFormData({
      name: org.name,
      description: org.description || '',
      isActive: org.isActive
    });
    setShowDialog(true);
  }

  function closeDialog() {
    setShowDialog(false);
    setSelectedOrg(null);
    setFormData({ name: '', description: '', isActive: true });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    try {
      if (dialogMode === 'create') {
        await organizationService.createOrganization({
          name: formData.name,
          description: formData.description || undefined,
        });
      } else if (selectedOrg) {
        await organizationService.updateOrganization(selectedOrg.id, {
          name: formData.name !== selectedOrg.name ? formData.name : undefined,
          description: formData.description !== selectedOrg.description ? formData.description : undefined,
          isActive: formData.isActive !== selectedOrg.isActive ? formData.isActive : undefined,
        });
      }

      await loadOrganizations();
      closeDialog();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Operation failed');
    }
  }

  function openDeleteConfirm(org: Organization) {
    setOrgToDelete(org);
    setShowDeleteConfirm(true);
  }

  function closeDeleteConfirm() {
    setOrgToDelete(null);
    setShowDeleteConfirm(false);
  }

  async function handleDelete() {
    if (!orgToDelete) return;

    try {
      await organizationService.deleteOrganization(orgToDelete.id);
      await loadOrganizations();
      closeDeleteConfirm();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Delete failed');
      closeDeleteConfirm();
    }
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="text-xl">Loading organizations...</div>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Organization Management</h1>
        <button
          onClick={openCreateDialog}
          className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
        >
          Create Organization
        </button>
      </div>

      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      <div className="bg-white shadow rounded-lg overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Name
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Description
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Created At
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {organizations.map((org) => (
              <tr key={org.id}>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  {org.name}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  {org.description || <span className="text-gray-400 italic">No description</span>}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span
                    className={`px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${
                      org.isActive
                        ? 'bg-green-100 text-green-800'
                        : 'bg-gray-100 text-gray-800'
                    }`}
                  >
                    {org.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {new Date(org.createdAt).toLocaleDateString()}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
                  <button
                    onClick={() => openEditDialog(org)}
                    className="text-blue-600 hover:text-blue-900"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => openDeleteConfirm(org)}
                    className="text-red-600 hover:text-red-900"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {organizations.length === 0 && (
          <div className="text-center py-12 text-gray-500">No organizations found</div>
        )}
      </div>

      {/* Create/Edit Dialog */}
      {showDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h2 className="text-2xl font-bold mb-4">
              {dialogMode === 'create' ? 'Create Organization' : 'Edit Organization'}
            </h2>

            <form onSubmit={handleSubmit}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Name *
                  </label>
                  <input
                    type="text"
                    required
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Description
                  </label>
                  <textarea
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                {dialogMode === 'edit' && (
                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={formData.isActive}
                        onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                        className="mr-2"
                      />
                      <span className="text-sm font-medium text-gray-700">Active</span>
                    </label>
                  </div>
                )}
              </div>

              <div className="flex justify-end space-x-3 mt-6">
                <button
                  type="button"
                  onClick={closeDialog}
                  className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                >
                  {dialogMode === 'create' ? 'Create' : 'Update'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      {showDeleteConfirm && orgToDelete && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h2 className="text-2xl font-bold mb-4">Confirm Delete</h2>
            <p className="text-gray-700 mb-6">
              Are you sure you want to delete organization <strong>{orgToDelete.name}</strong>?
              {orgToDelete.userCount && orgToDelete.userCount > 0 && (
                <span className="block mt-2 text-red-600 font-medium">
                  Warning: This organization has {orgToDelete.userCount} user(s).
                </span>
              )}
            </p>

            <div className="flex justify-end space-x-3">
              <button
                onClick={closeDeleteConfirm}
                className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={handleDelete}
                className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
