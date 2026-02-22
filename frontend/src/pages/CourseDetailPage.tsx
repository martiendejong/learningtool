import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { knowledgeService } from '../services/knowledgeService';
import { chatService } from '../services/chatService';
import type { Course } from '../services/knowledgeService';

export default function CourseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [course, setCourse] = useState<Course | null>(null);
  const [loading, setLoading] = useState(true);
  const [starting, setStarting] = useState(false);

  useEffect(() => {
    loadCourseData();
  }, [id]);

  const loadCourseData = async () => {
    if (!id) return;

    try {
      setLoading(true);
      const courseData = await knowledgeService.getCourseById(id);
      setCourse(courseData);
    } catch (err) {
      console.error('Failed to load course:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleStartCourse = async () => {
    if (!course) return;

    try {
      setStarting(true);
      // Start the course
      await chatService.startCourse(course.id);
      // Navigate to course-specific chat page
      navigate(`/course/${course.id}/learn`);
    } catch (err) {
      console.error('Failed to start course:', err);
      alert('Failed to start course. Please try again.');
    } finally {
      setStarting(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-gray-500">Loading course...</div>
      </div>
    );
  }

  if (!course) {
    return (
      <div className="p-6">
        <div className="text-center text-gray-500 py-12">
          <p className="text-lg">Course not found</p>
          <button
            onClick={() => navigate('/skills')}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Back to Skills
          </button>
        </div>
      </div>
    );
  }

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
          <p className="text-gray-600 mb-4">{course.description}</p>

          {/* Start Course Button */}
          <button
            onClick={handleStartCourse}
            disabled={starting}
            className="px-6 py-3 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {starting ? 'Starting...' : '🚀 Start Course'}
          </button>
        </div>

        {/* Course Details */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Left Column */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold mb-4">Course Info</h2>
            <div className="space-y-3">
              <div>
                <div className="text-sm text-gray-500">Duration</div>
                <div className="font-medium">
                  ⏱️ {Math.round(course.estimatedMinutes / 60)} hours
                  ({course.estimatedMinutes} minutes)
                </div>
              </div>
              <div>
                <div className="text-sm text-gray-500">Content</div>
                <div className="font-medium">{course.content || 'Course content coming soon'}</div>
              </div>
            </div>
          </div>

          {/* Right Column */}
          <div className="space-y-6">
            {/* Prerequisites */}
            {course.prerequisites && course.prerequisites.length > 0 && (
              <div className="bg-white rounded-lg shadow p-6">
                <h2 className="text-xl font-semibold mb-4">Prerequisites</h2>
                <ul className="space-y-2">
                  {course.prerequisites.map((prereq, idx) => (
                    <li key={idx} className="flex items-start gap-2">
                      <span className="text-orange-500 mt-1">⚠️</span>
                      <span>{prereq}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Resources */}
            {course.resourceLinks && course.resourceLinks.length > 0 && (
              <div className="bg-white rounded-lg shadow p-6">
                <h2 className="text-xl font-semibold mb-4">Resources</h2>
                <ul className="space-y-2">
                  {course.resourceLinks.map((resource, idx) => (
                    <li key={idx}>
                      <a
                        href={resource.url}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-600 hover:text-blue-700 hover:underline flex items-center gap-2"
                      >
                        <span>📚</span>
                        <span>{resource.title}</span>
                        <span className="text-xs text-gray-500">({resource.type})</span>
                      </a>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
