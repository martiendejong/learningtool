import { useState, useEffect } from 'react';
import { knowledgeService, UserCourse } from '../services/knowledgeService';

export default function TimelinePage() {
  const [completedCourses, setCompletedCourses] = useState<UserCourse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadCompletedCourses();
  }, []);

  const loadCompletedCourses = async () => {
    try {
      setLoading(true);
      const data = await knowledgeService.getCompletedCourses();
      // Sort by completion date (most recent first)
      data.sort((a, b) => {
        if (!a.completedAt || !b.completedAt) return 0;
        return new Date(b.completedAt).getTime() - new Date(a.completedAt).getTime();
      });
      setCompletedCourses(data);
      setError('');
    } catch (err) {
      console.error('Failed to load completed courses:', err);
      setError('Failed to load completed courses');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const getScoreColor = (score: number) => {
    if (score >= 90) return 'text-green-600';
    if (score >= 70) return 'text-yellow-600';
    return 'text-red-600';
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="text-lg text-gray-600">Loading timeline...</div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="max-w-4xl mx-auto">
        <div className="mb-6">
          <h1 className="text-3xl font-bold mb-2">Learning Timeline</h1>
          <p className="text-gray-600">
            Your journey through completed courses
          </p>
        </div>

        {error && (
          <div className="mb-4 text-red-600 text-sm bg-red-50 p-3 rounded">
            {error}
          </div>
        )}

        {completedCourses.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-12 text-center">
            <div className="text-6xl mb-4">📚</div>
            <h2 className="text-xl font-semibold text-gray-700 mb-2">
              No completed courses yet
            </h2>
            <p className="text-gray-500">
              Start learning and your achievements will appear here!
            </p>
          </div>
        ) : (
          <div className="relative">
            {/* Timeline line */}
            <div className="absolute left-8 top-0 bottom-0 w-0.5 bg-gray-200" />

            <div className="space-y-8">
              {completedCourses.map((userCourse, index) => {
                const course = userCourse.course;
                const isFirst = index === 0;

                return (
                  <div key={userCourse.id} className="relative pl-16">
                    {/* Timeline dot */}
                    <div className={`absolute left-6 w-5 h-5 rounded-full border-4 ${
                      isFirst
                        ? 'bg-blue-600 border-blue-600'
                        : 'bg-white border-gray-300'
                    }`} />

                    {/* Course card */}
                    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
                      <div className="flex items-start justify-between mb-3">
                        <div className="flex-1">
                          <h3 className="text-xl font-semibold text-gray-900 mb-1">
                            {course.name}
                          </h3>
                          <p className="text-gray-600 mb-2">{course.description}</p>
                        </div>
                        {userCourse.score !== undefined && (
                          <div className="ml-4">
                            <div className={`text-3xl font-bold ${getScoreColor(userCourse.score)}`}>
                              {userCourse.score}%
                            </div>
                            <div className="text-xs text-gray-500 text-center">Score</div>
                          </div>
                        )}
                      </div>

                      <div className="flex items-center gap-4 text-sm text-gray-500">
                        {userCourse.completedAt && (
                          <div className="flex items-center gap-1">
                            <span>🎉</span>
                            <span>Completed {formatDate(userCourse.completedAt)}</span>
                          </div>
                        )}
                        <div className="flex items-center gap-1">
                          <span>⏱️</span>
                          <span>{Math.round(course.estimatedMinutes / 60)} hours</span>
                        </div>
                        {course.resourceLinks.length > 0 && (
                          <div className="flex items-center gap-1">
                            <span>📎</span>
                            <span>{course.resourceLinks.length} resources</span>
                          </div>
                        )}
                      </div>

                      {course.prerequisites.length > 0 && (
                        <div className="mt-3 pt-3 border-t border-gray-100">
                          <div className="text-xs text-gray-500">
                            Built upon: {course.prerequisites.join(', ')}
                          </div>
                        </div>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
