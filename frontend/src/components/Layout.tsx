import { Link, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

export default function Layout() {
  const { user, logout } = useAuthStore();
  const location = useLocation();

  const isActive = (path: string) => {
    return location.pathname === path;
  };

  const navLinkClass = (path: string) => {
    const base = "px-4 py-2 rounded-md transition-colors";
    return isActive(path)
      ? `${base} bg-blue-600 text-white`
      : `${base} text-gray-700 hover:bg-gray-100`;
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navigation Bar */}
      <nav className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-8">
              <Link to="/" className="text-2xl font-bold text-blue-600">
                LearningTool
              </Link>
              <div className="flex gap-2">
                <Link to="/chat" className={navLinkClass('/chat')}>
                  💬 Chat
                </Link>
                <Link to="/skills" className={navLinkClass('/skills')}>
                  🎯 My Skills
                </Link>
                <Link to="/timeline" className={navLinkClass('/timeline')}>
                  📅 Timeline
                </Link>
              </div>
            </div>
            <div className="flex items-center gap-4">
              <span className="text-sm text-gray-600">
                {user?.userName || user?.email}
              </span>
              <button
                onClick={logout}
                className="px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Page Content */}
      <Outlet />
    </div>
  );
}
