import { useState, useRef, useEffect } from 'react';
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

export default function Layout() {
  const { user, logout } = useAuthStore();
  const location = useLocation();
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const role = user?.role ?? '';
  const isOrgAdmin = role === 'ORGADMIN';
  const isSystemAdmin = role === 'SYSTEMADMIN';

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setUserMenuOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const isActive = (path: string) => location.pathname.startsWith(path);

  const navLinkClass = (path: string) => {
    const base = "px-4 py-2 rounded-md transition-colors font-medium text-sm";
    return isActive(path)
      ? `${base} bg-gradient-to-r from-green-600 to-green-500 text-white shadow-md`
      : `${base} text-gray-700 hover:bg-green-50 hover:text-green-700`;
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim().length >= 2) {
      navigate(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-green-50">
      {/* Navigation Bar */}
      <nav className="bg-white shadow-lg border-b-4 border-green-500">
        <div className="max-w-7xl mx-auto px-6 py-3">
          <div className="flex items-center justify-between gap-4">
            {/* Logo + nav links */}
            <div className="flex items-center gap-6 min-w-0">
              <Link to="/" className="hover:opacity-80 transition-opacity shrink-0">
                <img
                  src="https://prospergenics.com/wp-content/uploads/2026/02/logo_header.png"
                  alt="Prospergenics Learning Platform"
                  className="h-10"
                />
              </Link>
              <div className="flex gap-1 flex-wrap">
                <Link to="/chat" className={navLinkClass('/chat')}>
                  💬 Learn
                </Link>
                <Link to="/skills" className={navLinkClass('/skills')}>
                  🎯 Skills
                </Link>
                <Link to="/progress" className={navLinkClass('/progress')}>
                  📊 Progress
                </Link>
                <Link to="/bookmarks" className={navLinkClass('/bookmarks')}>
                  🔖 Saved
                </Link>
                <Link to="/timeline" className={navLinkClass('/timeline')}>
                  📅 Timeline
                </Link>
                <Link to="/snippets" className={navLinkClass('/snippets')}>
                  📚 Snippets
                </Link>
                <Link to="/about" className={navLinkClass('/about')}>
                  ℹ️ About Us
                </Link>
                {isOrgAdmin && (
                  <Link to="/org/dashboard" className={navLinkClass('/org/')}>
                    🏢 Org
                  </Link>
                )}
                {isSystemAdmin && (
                  <Link to="/admin/users" className={navLinkClass('/admin/')}>
                    ⚙️ Admin
                  </Link>
                )}
              </div>
            </div>

            {/* Search + user menu */}
            <div className="flex items-center gap-3 shrink-0">
              <form onSubmit={handleSearch} className="relative">
                <input
                  type="text"
                  value={searchQuery}
                  onChange={e => setSearchQuery(e.target.value)}
                  placeholder="Search..."
                  className="pl-8 pr-3 py-1.5 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-300 w-40"
                />
                <span className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400 text-xs">🔍</span>
              </form>

              {/* User menu */}
              <div className="relative" ref={menuRef}>
                <button
                  onClick={() => setUserMenuOpen(v => !v)}
                  className="flex items-center gap-2 px-3 py-1.5 rounded-lg hover:bg-gray-100 transition-colors"
                >
                  <div className="w-7 h-7 rounded-full bg-green-500 text-white text-xs flex items-center justify-center font-semibold">
                    {(user?.userName ?? user?.email ?? '?')[0].toUpperCase()}
                  </div>
                  <span className="text-sm text-gray-600 max-w-[120px] truncate">
                    {user?.userName ?? user?.email}
                  </span>
                  <span className="text-gray-400 text-xs">▾</span>
                </button>

                {userMenuOpen && (
                  <div className="absolute right-0 top-full mt-1 w-48 bg-white rounded-xl shadow-lg border border-gray-100 py-1 z-50">
                    <div className="px-4 py-2 border-b border-gray-100">
                      <div className="text-xs text-gray-400">{role}</div>
                      <div className="text-sm font-medium text-gray-700 truncate">{user?.email}</div>
                    </div>
                    <Link
                      to="/profile"
                      onClick={() => setUserMenuOpen(false)}
                      className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                      My Account
                    </Link>
                    <Link
                      to="/progress"
                      onClick={() => setUserMenuOpen(false)}
                      className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                      My Progress
                    </Link>
                    <Link
                      to="/bookmarks"
                      onClick={() => setUserMenuOpen(false)}
                      className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                      Bookmarks
                    </Link>
                    <button
                      onClick={() => { logout(); setUserMenuOpen(false); }}
                      className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                    >
                      Logout
                    </button>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </nav>

      {/* Page Content */}
      <Outlet />
    </div>
  );
}
