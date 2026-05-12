import { useEffect, lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate, useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from './stores/authStore';
import api from './services/api';
import Layout from './components/Layout';

// Auth pages — load immediately (needed before any JS runs)
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import RegisterOrgPage from './pages/RegisterOrgPage';
import VerifyEmailPage from './pages/VerifyEmailPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import ResetPasswordPage from './pages/ResetPasswordPage';

// App pages — lazy loaded
const ChatPage            = lazy(() => import('./pages/ChatPage'));
const SkillsPage          = lazy(() => import('./pages/SkillsPage'));
const SkillDetailPage     = lazy(() => import('./pages/SkillDetailPage'));
const TopicDetailPage     = lazy(() => import('./pages/TopicDetailPage'));
const TimelinePage        = lazy(() => import('./pages/TimelinePage'));
const ProgressPage        = lazy(() => import('./pages/ProgressPage'));
const BookmarksPage       = lazy(() => import('./pages/BookmarksPage'));
const SearchPage          = lazy(() => import('./pages/SearchPage'));
const CoursePage          = lazy(() => import('./pages/CoursePage'));
const CourseDetailPage    = lazy(() => import('./pages/CourseDetailPage'));
const CourseChatPage      = lazy(() => import('./pages/CourseChatPage'));
const AboutPage           = lazy(() => import('./pages/AboutPage'));
const ProfilePage         = lazy(() => import('./pages/ProfilePage'));
const AdminUsersPage      = lazy(() => import('./pages/AdminUsersPage'));
const AdminOrganizationsPage = lazy(() => import('./pages/AdminOrganizationsPage'));
const AdminBundlesPage    = lazy(() => import('./pages/AdminBundlesPage'));
const OrgDashboardPage    = lazy(() => import('./pages/OrgDashboardPage'));
const OrgStudentsPage     = lazy(() => import('./pages/OrgStudentsPage'));
const OrgInvitesPage      = lazy(() => import('./pages/OrgInvitesPage'));
const OrgBundlesPage      = lazy(() => import('./pages/OrgBundlesPage'));

function PageLoader() {
  return (
    <div className="flex items-center justify-center py-20">
      <div className="w-6 h-6 border-2 border-green-500 border-t-transparent rounded-full animate-spin" />
    </div>
  );
}

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore();
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" />;
}

// Picks up ?token= injected by Google OAuth redirect and logs the user in
function OAuthTokenHandler() {
  const { setAuth } = useAuthStore();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const token = params.get('token');
    if (!token) return;

    // Strip the token from the URL immediately
    navigate(location.pathname, { replace: true });

    // Fetch user info with the new token then store it
    localStorage.setItem('token', token);
    api.get('/api/user/current')
      .then(res => {
        setAuth({ token, user: res.data });
      })
      .catch(() => {
        localStorage.removeItem('token');
      });
  }, [location.search]);

  return null;
}

function AppRoutes() {
  return (
    <>
      <OAuthTokenHandler />
      <Suspense fallback={<PageLoader />}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/register-organization" element={<RegisterOrgPage />} />
          <Route path="/verify-email" element={<VerifyEmailPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
          <Route path="/" element={<ProtectedRoute><Layout /></ProtectedRoute>}>
            <Route index element={<Navigate to="/chat" />} />
            <Route path="chat" element={<ChatPage />} />
            <Route path="skills" element={<SkillsPage />} />
            <Route path="skill/:id" element={<SkillDetailPage />} />
            <Route path="topic/:id" element={<TopicDetailPage />} />
            <Route path="timeline" element={<TimelinePage />} />
            <Route path="progress" element={<ProgressPage />} />
            <Route path="bookmarks" element={<BookmarksPage />} />
            <Route path="search" element={<SearchPage />} />
            <Route path="course/:courseId" element={<CoursePage />} />
            <Route path="course-detail/:id" element={<CourseDetailPage />} />
            <Route path="course/:id/learn" element={<CourseChatPage />} />
            <Route path="about" element={<AboutPage />} />
            <Route path="profile" element={<ProfilePage />} />
            {/* Admin */}
            <Route path="admin/users" element={<AdminUsersPage />} />
            <Route path="admin/organizations" element={<AdminOrganizationsPage />} />
            <Route path="admin/bundles" element={<AdminBundlesPage />} />
            {/* Org admin */}
            <Route path="org/dashboard" element={<OrgDashboardPage />} />
            <Route path="org/students" element={<OrgStudentsPage />} />
            <Route path="org/invites" element={<OrgInvitesPage />} />
            <Route path="org/bundles" element={<OrgBundlesPage />} />
          </Route>
        </Routes>
      </Suspense>
    </>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AppRoutes />
    </BrowserRouter>
  );
}

export default App;
