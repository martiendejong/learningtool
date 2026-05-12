import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useToast } from '../components/Toast';
import { orgAdminService } from '../services/orgAdminService';
import type { OrgInvite, CreateInviteResult } from '../services/orgAdminService';

export default function OrgInvitesPage() {
  const [invites, setInvites] = useState<OrgInvite[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [newInvite, setNewInvite] = useState<CreateInviteResult | null>(null);
  const [form, setForm] = useState({ expiresInDays: 7, maxUses: 10 });
  const toast = useToast();
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    load();
  }, []);

  const load = async () => {
    try {
      setLoading(true);
      const data = await orgAdminService.getInvites();
      setInvites(data);
    } catch (err) {
      console.error('Failed to load invites:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async () => {
    setCreating(true);
    try {
      const result = await orgAdminService.createInvite(form.expiresInDays, form.maxUses);
      setNewInvite(result);
      await load();
    } catch (err) {
      console.error('Failed to create invite:', err);
    } finally {
      setCreating(false);
    }
  };

  const handleRevoke = async (inviteId: number) => {
    try {
      await orgAdminService.revokeInvite(inviteId);
      setInvites(prev => prev.filter(i => i.id !== inviteId));
    } catch (err) {
      console.error('Failed to revoke invite:', err);
    }
  };

  const copyLink = (url: string) => {
    const fullUrl = `${window.location.origin}${url}`;
    navigator.clipboard.writeText(fullUrl);
    setCopied(true);
    toast.success('Invite link copied to clipboard');
    setTimeout(() => setCopied(false), 2000);
  };

  const formatDate = (d: string) => new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });

  return (
    <div className="p-6 max-w-3xl mx-auto">
      <div className="flex items-center gap-4 mb-6">
        <Link to="/org/dashboard" className="text-green-600 hover:underline text-sm">← Dashboard</Link>
        <h1 className="text-2xl font-bold text-gray-900">Invite Links</h1>
      </div>

      {/* Create form */}
      <div className="bg-white rounded-xl shadow p-6 mb-6">
        <h2 className="font-semibold text-gray-800 mb-4">Create Invite Link</h2>
        <div className="flex gap-4 items-end">
          <div>
            <label className="block text-xs text-gray-500 mb-1">Expires in (days)</label>
            <input
              type="number"
              min={1}
              max={365}
              value={form.expiresInDays}
              onChange={e => setForm(f => ({ ...f, expiresInDays: Number(e.target.value) }))}
              className="w-28 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-green-400"
            />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">Max uses</label>
            <input
              type="number"
              min={1}
              max={500}
              value={form.maxUses}
              onChange={e => setForm(f => ({ ...f, maxUses: Number(e.target.value) }))}
              className="w-28 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-green-400"
            />
          </div>
          <button
            onClick={handleCreate}
            disabled={creating}
            className="px-5 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-60 text-sm font-medium"
          >
            {creating ? 'Creating...' : '+ Create Link'}
          </button>
        </div>

        {/* New invite result */}
        {newInvite && (
          <div className="mt-4 p-4 bg-green-50 border border-green-200 rounded-lg">
            <div className="text-sm font-semibold text-green-800 mb-2">Invite link created!</div>
            <div className="flex items-center gap-2">
              <code className="flex-1 text-xs bg-white border border-green-200 rounded p-2 overflow-x-auto">
                {window.location.origin}{newInvite.inviteUrl}
              </code>
              <button
                onClick={() => copyLink(newInvite.inviteUrl)}
                className="px-3 py-1.5 text-xs bg-green-600 text-white rounded hover:bg-green-700"
              >
                {copied ? 'Copied!' : 'Copy'}
              </button>
            </div>
            <div className="mt-2 text-xs text-green-700">
              Expires {formatDate(newInvite.expiresAt)} · Max {newInvite.maxUses} uses
            </div>
          </div>
        )}
      </div>

      {/* Invites list */}
      <div className="bg-white rounded-xl shadow overflow-hidden">
        <div className="px-6 py-4 border-b">
          <h2 className="font-semibold text-gray-800">Active Invite Links</h2>
        </div>
        {loading ? (
          <div className="p-8 text-center text-gray-500">Loading...</div>
        ) : invites.length === 0 ? (
          <div className="p-8 text-center text-gray-500">No invite links yet</div>
        ) : (
          <div className="divide-y divide-gray-100">
            {invites.map(invite => (
              <div key={invite.id} className="px-6 py-4 flex items-center justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${invite.isExpired || invite.isExhausted ? 'bg-red-100 text-red-600' : 'bg-green-100 text-green-700'}`}>
                      {invite.isExpired ? 'Expired' : invite.isExhausted ? 'Used up' : 'Active'}
                    </span>
                    <span className="text-sm text-gray-500">
                      {invite.usedCount}/{invite.maxUses} uses
                    </span>
                  </div>
                  <div className="text-xs text-gray-400 mt-1">
                    Expires {formatDate(invite.expiresAt)} · Created {formatDate(invite.createdAt)}
                  </div>
                </div>
                <button
                  onClick={() => handleRevoke(invite.id)}
                  className="text-sm text-red-600 hover:bg-red-50 px-3 py-1.5 rounded-lg"
                >
                  Revoke
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
