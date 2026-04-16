import { Link, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

export default function Layout() {
  const { user, logout } = useAuthStore();
  const location = useLocation();

  const isActive = (path: string) => {
    return location.pathname === path;
  };

  const navLinkClass = (path: string) => {
    const base = "px-4 py-2 rounded-md transition-colors font-medium";
    return isActive(path)
      ? `${base} bg-gradient-to-r from-green-600 to-green-500 text-white shadow-md`
      : `${base} text-gray-700 hover:bg-green-50 hover:text-green-700`;
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-green-50">
      {/* Navigation Bar */}
      <nav className="bg-white shadow-lg border-b-4 border-green-500">
        <div className="max-w-7xl mx-auto px-6 py-3">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-8">
              <Link to="/" className="hover:opacity-80 transition-opacity">
                <img
                  src="https://prospergenics.com/wp-content/uploads/2026/02/logo_header.png"
                  alt="Prospergenics Learning Platform"
                  className="h-12"
                />
              </Link>
              <div className="flex gap-2">
                <Link to="/chat" className={navLinkClass('/chat')}>
                  💬 Learn
                </Link>
                <Link to="/skills" className={navLinkClass('/skills')}>
                  🎯 My Progress
                </Link>
                <Link to="/timeline" className={navLinkClass('/timeline')}>
                  📅 Journey
                </Link>
                <Link to="/snippets" className={navLinkClass('/snippets')}>
                  📚 Snippets
                </Link>
                <Link to="/about" className={navLinkClass('/about')}>
                  ℹ️ About Us
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
