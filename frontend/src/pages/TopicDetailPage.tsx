import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { knowledgeService } from '../services/knowledgeService';
import type { Topic, Course } from '../services/knowledgeService';

export default function TopicDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [topic, setTopic] = useState<Topic | null>(null);
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadTopicData();
  }, [id]);

  const loadTopicData = async () => {
    if (!id) return;

    try {
      setLoading(true);
      const topicData = await knowledgeService.getTopicById(parseInt(id));
      setTopic(topicData);
      const coursesData = await knowledgeService.getCoursesForTopic(parseInt(id));
      setCourses(coursesData);
    } catch (err) {
      console.error('Failed to load topic:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-gray-500">Loading topic...</div>
      </div>
    );
  }

  if (!topic) {
    return (
      <div className="p-6">
        <div className="text-center text-gray-500 py-12">
          <p className="text-lg">Topic not found</p>
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
          <h1 className="text-3xl font-bold mb-2">{topic.name}</h1>
          <p className="text-gray-600">{topic.description}</p>
        </div>

        {/* Courses */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">Courses</h2>
          {courses.length === 0 ? (
            <p className="text-gray-500 text-center py-8">No courses yet</p>
          ) : (
            <div className="space-y-4">
              {courses.map((course) => (
                <div
                  key={course.id}
                  onClick={() => navigate(`/course/${course.id}`)}
                  className="border border-gray-200 rounded-lg p-4 hover:bg-gray-50 cursor-pointer"
                >
                  <h3 className="font-medium text-lg mb-1">{course.name}</h3>
                  <p className="text-sm text-gray-600 mb-2">{course.description}</p>
                  <div className="flex items-center gap-3 text-xs text-gray-500">
                    <span>⏱️ {Math.round(course.estimatedMinutes / 60)}h</span>
                    {course.prerequisites.length > 0 && (
                      <span>📋 {course.prerequisites.length} prerequisites</span>
                    )}
                    <span>📚 {course.resourceLinks.length} resources</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
