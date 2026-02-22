import { useState, useEffect } from 'react';
import { knowledgeService } from '../services/knowledgeService';
import type { UserSkill } from '../services/knowledgeService';
import TreeView from '../components/TreeView';
import SkillBrowserModal from '../components/SkillBrowserModal';

export default function SkillsPage() {
  const [skills, setSkills] = useState<UserSkill[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showBrowser, setShowBrowser] = useState(false);

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


  const handleRemoveSkill = async (skillId: string) => {
    if (!confirm('Remove this skill and all its topics and courses?')) return;

    try {
      await knowledgeService.removeSkill(skillId);
      await loadSkills();
    } catch (err) {
      console.error('Failed to remove skill:', err);
      setError('Failed to remove skill');
    }
  };

  const handleRemoveTopic = async (topicId: string) => {
    if (!confirm('Remove this topic and all its courses?')) return;

    try {
      await knowledgeService.removeTopic(topicId);
      await loadSkills();
    } catch (err) {
      console.error('Failed to remove topic:', err);
      setError('Failed to remove topic');
    }
  };

  const handleStartCourse = async (courseId: string) => {
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

        {/* Browse Skills Button */}
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <button
            onClick={() => setShowBrowser(true)}
            className="w-full px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 text-lg font-medium"
          >
            Browse All Available Skills
          </button>
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

      {/* Skill Browser Modal */}
      <SkillBrowserModal
        isOpen={showBrowser}
        onClose={() => setShowBrowser(false)}
        onSkillAdded={() => {
          loadSkills();
          setShowBrowser(false);
        }}
      />
    </div>
  );
}
