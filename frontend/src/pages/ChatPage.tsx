import { useState, useEffect } from 'react';
import { useAuthStore } from '../stores/authStore';
import { knowledgeService } from '../services/knowledgeService';
import { Link, useLocation } from 'react-router-dom';
import ChatInterface from '../components/ChatInterface';

export default function ChatPage() {
  const { user } = useAuthStore();
  const location = useLocation();
  const [stats, setStats] = useState({ skills: 0, inProgress: 0, completed: 0 });
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [courseStarted, setCourseStarted] = useState(false);

  useEffect(() => {
    loadStats();

    // Check if course was started (from CourseDetailPage)
    if (location.state?.courseStarted) {
      setIsFullscreen(true);
      setCourseStarted(true);
      // Clear the state so it doesn't trigger again
      window.history.replaceState({}, document.title);
    }
  }, [location]);

  const loadStats = async () => {
    try {
      const [skills, inProgress, completed] = await Promise.all([
        knowledgeService.getMySkills(),
        knowledgeService.getInProgressCourses(),
        knowledgeService.getCompletedCourses(),
      ]);
      setStats({
        skills: skills.length,
        inProgress: inProgress.length,
        completed: completed.length,
      });
    } catch (err) {
      console.error('Failed to load stats:', err);
    }
  };

  if (isFullscreen) {
    return (
      <div className="fixed inset-0 z-50 bg-gradient-to-br from-gray-50 to-green-50">
        <div className="h-full flex flex-col">
          <div className="bg-white shadow-lg border-b-4 border-green-500 px-6 py-3 flex items-center justify-between">
            <h2 className="text-xl font-bold text-green-700">Chat - Fullscreen Mode</h2>
            <button
              onClick={() => setIsFullscreen(false)}
              className="px-4 py-2 bg-gradient-to-r from-green-600 to-green-500 text-white rounded-md hover:shadow-lg transition-all font-medium"
            >
              ⬇ Exit Fullscreen
            </button>
          </div>
          <div className="flex-1 min-h-0">
            <ChatInterface scrollToTop={courseStarted} onScrollComplete={() => setCourseStarted(false)} />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 h-[calc(100vh-5rem)]">
      <div className="max-w-6xl mx-auto h-full flex flex-col gap-6">
        <div className="flex items-center justify-between">
          <h1 className="text-3xl font-bold">
            Welcome, {user?.userName || user?.email}!
          </h1>
          <button
            onClick={() => setIsFullscreen(true)}
            className="px-4 py-2 bg-gradient-to-r from-green-600 to-green-500 text-white rounded-md hover:shadow-lg transition-all font-medium"
          >
            ⬆ Fullscreen Chat
          </button>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Link to="/skills" className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition-shadow">
            <h3 className="font-semibold text-lg mb-2">My Skills</h3>
            <p className="text-gray-600 text-sm">View and manage your learning goals</p>
            <p className="text-2xl font-bold mt-4 text-blue-600">{stats.skills}</p>
          </Link>

          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="font-semibold text-lg mb-2">In Progress</h3>
            <p className="text-gray-600 text-sm">Courses you're currently taking</p>
            <p className="text-2xl font-bold mt-4 text-yellow-600">{stats.inProgress}</p>
          </div>

          <Link to="/timeline" className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition-shadow">
            <h3 className="font-semibold text-lg mb-2">Completed</h3>
            <p className="text-gray-600 text-sm">Courses you've finished</p>
            <p className="text-2xl font-bold mt-4 text-green-600">{stats.completed}</p>
          </Link>
        </div>

        <div className="flex-1 min-h-0">
          <ChatInterface scrollToTop={courseStarted} onScrollComplete={() => setCourseStarted(false)} />
        </div>
      </div>
    </div>
  );
}
