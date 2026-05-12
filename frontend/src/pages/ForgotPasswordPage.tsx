import { useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../services/api';

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [sent, setSent] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await api.post('/auth/forgot-password', { email });
      setSent(true);
    } catch {
      setError('Something went wrong. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-green-50 via-white to-green-100">
      <div className="max-w-md w-full bg-white rounded-2xl shadow-2xl p-8 border-t-4 border-green-500">
        <div className="flex flex-col items-center mb-6">
          <img
            src="https://prospergenics.com/wp-content/uploads/2026/02/logo_header.png"
            alt="Prospergenics"
            className="h-14 mb-3"
          />
        </div>

        {sent ? (
          <div className="text-center">
            <div className="text-5xl mb-4">📧</div>
            <h2 className="text-xl font-bold text-gray-800 mb-2">Check your email</h2>
            <p className="text-gray-500 text-sm mb-6">
              If <strong>{email}</strong> is registered, we've sent a password reset link.
              Check your inbox (and spam folder).
            </p>
            <Link to="/login" className="text-green-600 hover:underline text-sm font-medium">
              ← Back to login
            </Link>
          </div>
        ) : (
          <>
            <h2 className="text-2xl font-semibold mb-2 text-gray-800">Forgot password?</h2>
            <p className="text-gray-500 text-sm mb-6">
              Enter your email and we'll send you a reset link.
            </p>

            {error && (
              <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded text-sm">
                {error}
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                <input
                  type="email"
                  value={email}
                  onChange={e => setEmail(e.target.value)}
                  required
                  autoFocus
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
                  placeholder="you@example.com"
                />
              </div>
              <button
                type="submit"
                disabled={loading}
                className="w-full bg-gradient-to-r from-green-600 to-green-500 text-white py-2 px-4 rounded-md hover:opacity-90 disabled:opacity-50 font-medium"
              >
                {loading ? 'Sending...' : 'Send reset link'}
              </button>
            </form>

            <p className="mt-4 text-center text-sm text-gray-500">
              <Link to="/login" className="text-green-600 hover:underline">← Back to login</Link>
            </p>
          </>
        )}
      </div>
    </div>
  );
}
