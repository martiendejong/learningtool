import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './stores/authStore';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ChatPage from './pages/ChatPage';
import SkillsPage from './pages/SkillsPage';
import SkillDetailPage from './pages/SkillDetailPage';
import TopicDetailPage from './pages/TopicDetailPage';
import TimelinePage from './pages/TimelinePage';
import CoursePage from './pages/CoursePage';
import CourseDetailPage from './pages/CourseDetailPage';
import CourseChatPage from './pages/CourseChatPage';
import AboutPage from './pages/AboutPage';
import AdminUsersPage from './pages/AdminUsersPage';
import AdminOrganizationsPage from './pages/AdminOrganizationsPage';
import SnippetsPage from './pages/SnippetsPage';
import Layout from './components/Layout';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore();
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" />;
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/" element={<ProtectedRoute><Layout /></ProtectedRoute>}>
          <Route index element={<Navigate to="/chat" />} />
          <Route path="chat" element={<ChatPage />} />
          <Route path="skills" element={<SkillsPage />} />
          <Route path="skill/:id" element={<SkillDetailPage />} />
          <Route path="topic/:id" element={<TopicDetailPage />} />
          <Route path="timeline" element={<TimelinePage />} />
          <Route path="course/:courseId" element={<CoursePage />} />
          <Route path="course-detail/:id" element={<CourseDetailPage />} />
          <Route path="course/:id/learn" element={<CourseChatPage />} />
          <Route path="snippets" element={<SnippetsPage />} />
          <Route path="about" element={<AboutPage />} />
          <Route path="admin/users" element={<AdminUsersPage />} />
          <Route path="admin/organizations" element={<AdminOrganizationsPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
