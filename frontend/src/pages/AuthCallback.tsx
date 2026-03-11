import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

export default function AuthCallback() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();

  useEffect(() => {
    const token = searchParams.get('token');
    const error = searchParams.get('error');

    if (error) {
      // Handle OAuth errors
      console.error('OAuth error:', error);
      navigate('/login?error=' + error);
      return;
    }

    if (token) {
      try {
        // Decode JWT to extract user info (basic decode, not validating signature)
        const payload = JSON.parse(atob(token.split('.')[1]));

        // Extract user data from JWT claims
        const user = {
          id: payload.nameid || payload.sub,
          email: payload.email,
          userName: payload.unique_name || payload.email,
          fullName: payload.name,
          accountType: payload.account_type,
          organizationId: payload.organization_id,
          roleInOrganization: payload.organization_role,
          profilePictureUrl: payload.picture,
        };

        // Set authentication state
        setAuth({ token, user });

        // Redirect to chat/dashboard
        navigate('/chat');
      } catch (err) {
        console.error('Failed to parse token:', err);
        navigate('/login?error=invalid_token');
      }
    } else {
      // No token provided
      navigate('/login?error=no_token');
    }
  }, [searchParams, navigate, setAuth]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-green-50 via-white to-green-100">
      <div className="max-w-md w-full bg-white rounded-2xl shadow-2xl p-8 border-t-4 border-green-500">
        <div className="flex flex-col items-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-green-600 mb-4"></div>
          <h2 className="text-xl font-semibold text-gray-800">Signing you in...</h2>
          <p className="text-sm text-gray-600 mt-2">Please wait while we complete your authentication.</p>
        </div>
      </div>
    </div>
  );
}
