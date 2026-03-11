import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import { organizationService } from '../services/organizationService';

export default function InvitePage() {
  const { token } = useParams<{ token: string }>();
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [fullName, setFullName] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!token) {
      setError('Invalid invitation link');
    }
  }, [token]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    if (password.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }

    if (!token) {
      setError('Invalid invitation token');
      return;
    }

    setLoading(true);

    try {
      const response = await organizationService.acceptInvite({
        token,
        email,
        password,
        fullName: fullName || undefined,
      });

      // Set authentication state
      setAuth(response);

      // Redirect to chat/dashboard
      navigate('/chat');
    } catch (err: any) {
      console.error('Failed to accept invitation:', err);
      setError(err.response?.data?.message || 'Failed to accept invitation');
    } finally {
      setLoading(false);
    }
  };

  if (!token) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-green-50 via-white to-green-100">
        <div className="max-w-md w-full bg-white rounded-2xl shadow-2xl p-8 border-t-4 border-red-500">
          <h2 className="text-2xl font-bold text-red-600 mb-4">Invalid Invitation</h2>
          <p className="text-gray-600 mb-4">
            This invitation link is invalid or has expired.
          </p>
          <button
            onClick={() => navigate('/login')}
            className="w-full bg-green-600 text-white py-2 px-4 rounded-md hover:bg-green-700"
          >
            Go to Login
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-green-50 via-white to-green-100">
      <div className="max-w-md w-full bg-white rounded-2xl shadow-2xl p-8 border-t-4 border-green-500">
        <div className="flex flex-col items-center mb-8">
          <img
            src="https://prospergenics.com/wp-content/uploads/2026/02/logo_header.png"
            alt="Prospergenics"
            className="h-16 mb-4"
          />
          <h1 className="text-2xl font-bold text-green-700">Prospergenics Learning</h1>
          <p className="text-sm text-gray-600">Empowering through education</p>
        </div>

        <h2 className="text-2xl font-semibold mb-2 text-gray-800">Accept Invitation</h2>
        <p className="text-sm text-gray-600 mb-6">
          You've been invited to join an organization. Create your account to get started.
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="fullName" className="block text-sm font-medium text-gray-700 mb-1">
              Full Name
            </label>
            <input
              id="fullName"
              type="text"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
              placeholder="John Doe"
            />
          </div>

          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
              placeholder="you@example.com"
            />
          </div>

          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
              Password
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
              placeholder="At least 6 characters"
            />
          </div>

          <div>
            <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700 mb-1">
              Confirm Password
            </label>
            <input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
              placeholder="Confirm your password"
            />
          </div>

          {error && (
            <div className="text-red-600 text-sm bg-red-50 p-3 rounded">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-gradient-to-r from-green-600 to-green-500 text-white py-2 px-4 rounded-md hover:shadow-lg hover:from-green-700 hover:to-green-600 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 disabled:opacity-50"
          >
            {loading ? 'Accepting...' : 'Accept Invitation & Join'}
          </button>
        </form>

        <p className="mt-4 text-center text-sm text-gray-600">
          Already have an account?{' '}
          <button
            onClick={() => navigate('/login')}
            className="text-green-600 hover:text-green-700 font-medium"
          >
            Sign in
          </button>
        </p>
      </div>
    </div>
  );
}
