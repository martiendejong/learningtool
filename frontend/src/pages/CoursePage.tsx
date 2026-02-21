import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { knowledgeService } from '../services/knowledgeService';
import type { Course, UserCourse } from '../services/knowledgeService';

export default function CoursePage() {
  const { courseId } = useParams<{ courseId: string }>();
  const navigate = useNavigate();
  const [course, setCourse] = useState<Course | null>(null);
  const [userCourse, setUserCourse] = useState<UserCourse | null>(null);
  const [loading, setLoading] = useState(true);
  const [completing, setCompleting] = useState(false);
  const [score, setScore] = useState(75);
  const [error, setError] = useState('');

  useEffect(() => {
    if (courseId) {
      loadCourse(parseInt(courseId));
    }
  }, [courseId]);

  const loadCourse = async (id: number) => {
    try {
      setLoading(true);
      const courseData = await knowledgeService.getCourse(id);
      setCourse(courseData);

      // Check if course is in progress
      const inProgress = await knowledgeService.getInProgressCourses();
      const userCourseData = inProgress.find((uc) => uc.courseId === id);
      setUserCourse(userCourseData || null);
    } catch (err) {
      console.error('Failed to load course:', err);
      setError('Failed to load course details');
    } finally {
      setLoading(false);
    }
  };

  const handleStart = async () => {
    if (!courseId || !course) return;

    try {
      // Start the course in the backend
      const userCourseData = await knowledgeService.startCourse(parseInt(courseId));
      setUserCourse(userCourseData);
      setError('');

      // Import chatService dynamically to avoid circular dependencies
      const { chatService } = await import('../services/chatService');

      // Clear chat history
      await chatService.clearHistory();

      // Navigate to chat with course info
      navigate('/chat', {
        state: {
          startCourse: true,
          courseId: parseInt(courseId),
          courseName: course.name
        }
      });
    } catch (err) {
      console.error('Failed to start course:', err);
      setError('Failed to start course');
    }
  };

  const handleComplete = async () => {
    if (!courseId) return;

    try {
      setCompleting(true);
      await knowledgeService.completeCourse(parseInt(courseId), score);
      navigate('/timeline');
    } catch (err) {
      console.error('Failed to complete course:', err);
      setError('Failed to complete course');
    } finally {
      setCompleting(false);
    }
  };

  const getResourceIcon = (type: string) => {
    switch (type) {
      case 'YouTube':
        return '🎥';
      case 'Article':
        return '📄';
      case 'Documentation':
        return '📚';
      case 'Tutorial':
        return '💪';
      case 'Book':
        return '📖';
      default:
        return '🔗';
    }
  };

  if (loading) {
    return (
      <div className="p-6 flex items-center justify-center">
        <div className="text-lg text-gray-600">Loading course...</div>
      </div>
    );
  }

  if (!course) {
    return (
      <div className="p-6">
        <div className="max-w-4xl mx-auto text-center py-12">
          <h2 className="text-2xl font-semibold text-gray-700">Course not found</h2>
          <button
            onClick={() => navigate('/skills')}
            className="mt-4 px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Back to Skills
          </button>
        </div>
      </div>
    );
  }

  const isStarted = userCourse !== null;
  const isCompleted = userCourse?.status === 'Completed';

  return (
    <div className="p-6">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="mb-6">
          <button
            onClick={() => navigate('/skills')}
            className="text-blue-600 hover:text-blue-700 mb-4"
          >
            ← Back to Skills
          </button>
          <h1 className="text-3xl font-bold mb-2">{course.name}</h1>
          <p className="text-gray-600">{course.description}</p>
          <div className="flex items-center gap-4 mt-3 text-sm text-gray-500">
            <span>⏱️ {Math.round(course.estimatedMinutes / 60)} hours</span>
            {course.prerequisites.length > 0 && (
              <span>📋 {course.prerequisites.length} prerequisites</span>
            )}
            <span>📎 {course.resourceLinks.length} resources</span>
          </div>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-50 text-red-600 rounded-md">
            {error}
          </div>
        )}

        {/* Prerequisites */}
        {course.prerequisites.length > 0 && (
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-6">
            <h3 className="font-semibold text-yellow-900 mb-2">Prerequisites</h3>
            <ul className="list-disc list-inside text-yellow-800">
              {course.prerequisites.map((prereq, index) => (
                <li key={index}>{prereq}</li>
              ))}
            </ul>
          </div>
        )}

        {/* Course Content */}
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h2 className="text-xl font-semibold mb-4">Course Content</h2>
          <div className="prose max-w-none text-gray-700">
            <p className="mb-4">
              This course will cover all aspects of <strong>{course.name}</strong>.
              You'll learn through a combination of theory, practical examples, and hands-on exercises.
            </p>
            <p className="mb-4">
              Follow the resources below to complete your learning journey. Take your time to
              understand each concept thoroughly before moving forward.
            </p>
          </div>
        </div>

        {/* Resources */}
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h2 className="text-xl font-semibold mb-4">Learning Resources</h2>
          {course.resourceLinks.length === 0 ? (
            <p className="text-gray-500">No resources available yet</p>
          ) : (
            <div className="space-y-3">
              {course.resourceLinks.map((resource, index) => (
                <a
                  key={index}
                  href={resource.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-3 p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  <span className="text-2xl">{getResourceIcon(resource.type)}</span>
                  <div className="flex-1">
                    <h4 className="font-medium text-gray-900">{resource.title}</h4>
                    <p className="text-sm text-gray-500">{resource.type}</p>
                  </div>
                  <span className="text-blue-600">→</span>
                </a>
              ))}
            </div>
          )}
        </div>

        {/* Course Actions */}
        {!isStarted && (
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold mb-4">Ready to Start?</h2>
            <p className="text-gray-600 mb-4">
              Once you start this course, your progress will be tracked. Make sure you have time
              to dedicate to learning.
            </p>
            <button
              onClick={handleStart}
              className="px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 font-medium"
            >
              Start Learning
            </button>
          </div>
        )}

        {isStarted && !isCompleted && (
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold mb-4">Complete Course</h2>
            <p className="text-gray-600 mb-4">
              Have you finished all the learning materials? Rate your understanding and complete the course.
            </p>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Self-Assessment Score: {score}%
              </label>
              <input
                type="range"
                min="0"
                max="100"
                step="5"
                value={score}
                onChange={(e) => setScore(parseInt(e.target.value))}
                className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
              />
              <div className="flex justify-between text-xs text-gray-500 mt-1">
                <span>0%</span>
                <span>50%</span>
                <span>100%</span>
              </div>
            </div>

            <button
              onClick={handleComplete}
              disabled={completing}
              className="px-6 py-3 bg-green-600 text-white rounded-md hover:bg-green-700 font-medium disabled:opacity-50"
            >
              {completing ? 'Completing...' : 'Complete Course'}
            </button>
          </div>
        )}

        {isCompleted && (
          <div className="bg-green-50 border border-green-200 rounded-lg p-6">
            <div className="flex items-center gap-3 mb-3">
              <span className="text-3xl">🎉</span>
              <h2 className="text-xl font-semibold text-green-900">Course Completed!</h2>
            </div>
            <p className="text-green-800 mb-4">
              You completed this course with a score of {userCourse.score}%. Great work!
            </p>
            <button
              onClick={() => navigate('/timeline')}
              className="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700"
            >
              View Timeline
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
