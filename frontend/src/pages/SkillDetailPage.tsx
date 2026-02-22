import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { knowledgeService } from '../services/knowledgeService';
import type { Skill, Topic } from '../services/knowledgeService';

export default function SkillDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [skill, setSkill] = useState<Skill | null>(null);
  const [topics, setTopics] = useState<Topic[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadSkillData();
  }, [id]);

  const loadSkillData = async () => {
    if (!id) return;

    try {
      setLoading(true);
      const skillData = await knowledgeService.getSkillById(id);
      setSkill(skillData);
      const topicsData = await knowledgeService.getTopicsForSkill(id);
      setTopics(topicsData);
    } catch (err) {
      console.error('Failed to load skill:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-gray-500">Loading skill...</div>
      </div>
    );
  }

  if (!skill) {
    return (
      <div className="p-6">
        <div className="text-center text-gray-500 py-12">
          <p className="text-lg">Skill not found</p>
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
          <h1 className="text-3xl font-bold mb-2">{skill.name}</h1>
          <p className="text-gray-600">{skill.description}</p>
          <div className="flex items-center gap-2 mt-3">
            <span className={`text-xs px-2 py-1 rounded ${
              skill.difficulty === 'Beginner' ? 'bg-green-100 text-green-800' :
              skill.difficulty === 'Intermediate' ? 'bg-yellow-100 text-yellow-800' :
              'bg-red-100 text-red-800'
            }`}>
              {skill.difficulty}
            </span>
          </div>
        </div>

        {/* Topics */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">Topics</h2>
          {topics.length === 0 ? (
            <p className="text-gray-500 text-center py-8">No topics yet</p>
          ) : (
            <div className="space-y-4">
              {topics.map((topic) => (
                <div
                  key={topic.id}
                  onClick={() => navigate(`/topic/${topic.id}`)}
                  className="border border-gray-200 rounded-lg p-4 hover:bg-gray-50 cursor-pointer"
                >
                  <h3 className="font-medium text-lg mb-1">{topic.name}</h3>
                  <p className="text-sm text-gray-600">{topic.description}</p>
                  {topic.courses && topic.courses.length > 0 && (
                    <div className="mt-2 text-xs text-blue-600">
                      {topic.courses.length} course(s) available
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
