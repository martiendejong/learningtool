import { useState, useEffect } from 'react';
import { useAuthStore } from '../stores/authStore';
import { knowledgeService } from '../services/knowledgeService';
import { Link } from 'react-router-dom';

export default function ChatPage() {
  const { user } = useAuthStore();
  const [stats, setStats] = useState({ skills: 0, inProgress: 0, completed: 0 });

  useEffect(() => {
    loadStats();
  }, []);

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

  return (
    <div className="p-6">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold mb-6">
          Welcome, {user?.userName || user?.email}!
        </h1>

        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">AI Learning Assistant</h2>
          <p className="text-gray-600 mb-4">
            Hi! I'm your AI learning assistant. What skills would you like to learn today?
          </p>

          <div className="bg-blue-50 p-4 rounded-lg">
            <p className="text-sm text-blue-900">
              <strong>Coming soon:</strong> Interactive chat with AI tutor, skill management,
              course recommendations, and progress tracking.
            </p>
          </div>
        </div>

        <div className="mt-6 grid grid-cols-1 md:grid-cols-3 gap-4">
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
      </div>
    </div>
  );
}
