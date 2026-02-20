import { useState, useEffect } from 'react';
import { knowledgeService, UserSkill } from '../services/knowledgeService';
import TreeView from '../components/TreeView';

export default function SkillsPage() {
  const [skills, setSkills] = useState<UserSkill[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [newSkillName, setNewSkillName] = useState('');
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    loadSkills();
  }, []);

  const loadSkills = async () => {
    try {
      setLoading(true);
      const data = await knowledgeService.getMySkills();
      setSkills(data);
      setError('');
    } catch (err) {
      console.error('Failed to load skills:', err);
      setError('Failed to load skills');
    } finally {
      setLoading(false);
    }
  };

  const handleAddSkill = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newSkillName.trim()) return;

    try {
      setAdding(true);
      await knowledgeService.addSkill(newSkillName);
      setNewSkillName('');
      await loadSkills();
    } catch (err) {
      console.error('Failed to add skill:', err);
      setError('Failed to add skill');
    } finally {
      setAdding(false);
    }
  };

  const handleRemoveSkill = async (skillId: number) => {
    if (!confirm('Remove this skill and all its topics and courses?')) return;

    try {
      await knowledgeService.removeSkill(skillId);
      await loadSkills();
    } catch (err) {
      console.error('Failed to remove skill:', err);
      setError('Failed to remove skill');
    }
  };

  const handleRemoveTopic = async (topicId: number) => {
    if (!confirm('Remove this topic and all its courses?')) return;

    try {
      await knowledgeService.removeTopic(topicId);
      await loadSkills();
    } catch (err) {
      console.error('Failed to remove topic:', err);
      setError('Failed to remove topic');
    }
  };

  const handleStartCourse = async (courseId: number) => {
    try {
      await knowledgeService.startCourse(courseId);
      alert('Course started! Check your chat for course content.');
      await loadSkills();
    } catch (err) {
      console.error('Failed to start course:', err);
      setError('Failed to start course');
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-lg text-gray-600">Loading skills...</div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="max-w-5xl mx-auto">
        <div className="mb-6">
          <h1 className="text-3xl font-bold mb-2">My Learning Path</h1>
          <p className="text-gray-600">
            Explore your skills, topics, and courses in a hierarchical view
          </p>
        </div>

        {/* Add Skill Form */}
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h2 className="text-xl font-semibold mb-4">Add New Skill</h2>
          <form onSubmit={handleAddSkill} className="flex gap-3">
            <input
              type="text"
              value={newSkillName}
              onChange={(e) => setNewSkillName(e.target.value)}
              placeholder="e.g., Machine Learning, Web Development, Data Science..."
              className="flex-1 px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              disabled={adding}
            />
            <button
              type="submit"
              disabled={adding || !newSkillName.trim()}
              className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
            >
              {adding ? 'Adding...' : 'Add Skill'}
            </button>
          </form>
          {error && (
            <div className="mt-3 text-red-600 text-sm bg-red-50 p-3 rounded">
              {error}
            </div>
          )}
        </div>

        {/* Skills Tree */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">Skills Hierarchy</h2>
          <TreeView
            skills={skills}
            onRemoveSkill={handleRemoveSkill}
            onRemoveTopic={handleRemoveTopic}
            onStartCourse={handleStartCourse}
          />
        </div>
      </div>
    </div>
  );
}
