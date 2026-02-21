import { useState, useEffect } from 'react';
import { knowledgeService, type Skill } from '../services/knowledgeService';

interface SkillBrowserModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSkillAdded: () => void;
}

export default function SkillBrowserModal({ isOpen, onClose, onSkillAdded }: SkillBrowserModalProps) {
  const [skills, setSkills] = useState<Skill[]>([]);
  const [filteredSkills, setFilteredSkills] = useState<Skill[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(false);
  const [adding, setAdding] = useState<number | null>(null);

  useEffect(() => {
    if (isOpen) {
      loadSkills();
    }
  }, [isOpen]);

  useEffect(() => {
    if (search.trim()) {
      const filtered = skills.filter(skill =>
        skill.name.toLowerCase().includes(search.toLowerCase()) ||
        skill.description.toLowerCase().includes(search.toLowerCase())
      );
      setFilteredSkills(filtered);
    } else {
      setFilteredSkills(skills);
    }
  }, [search, skills]);

  const loadSkills = async () => {
    try {
      setLoading(true);
      const data = await knowledgeService.getAllSkills();
      setSkills(data);
      setFilteredSkills(data);
    } catch (err) {
      console.error('Failed to load skills:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddSkill = async (skill: Skill) => {
    try {
      setAdding(skill.id);
      await knowledgeService.addSkill(skill.name);
      onSkillAdded();
      onClose();
    } catch (err) {
      console.error('Failed to add skill:', err);
      alert('Failed to add skill');
    } finally {
      setAdding(null);
    }
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Beginner': return 'bg-green-100 text-green-800';
      case 'Intermediate': return 'bg-yellow-100 text-yellow-800';
      case 'Advanced': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-3xl w-full max-h-[80vh] flex flex-col">
        {/* Header */}
        <div className="p-6 border-b">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-2xl font-bold">Browse Available Skills</h2>
            <button
              onClick={onClose}
              className="text-gray-500 hover:text-gray-700 text-2xl"
            >
              ×
            </button>
          </div>
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search skills..."
            className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Skills List */}
        <div className="flex-1 overflow-y-auto p-6">
          {loading ? (
            <div className="text-center py-12 text-gray-500">Loading skills...</div>
          ) : filteredSkills.length === 0 ? (
            <div className="text-center py-12 text-gray-500">
              {search ? 'No skills found matching your search' : 'No skills available'}
            </div>
          ) : (
            <div className="space-y-3">
              {filteredSkills.map((skill) => (
                <div
                  key={skill.id}
                  className="border border-gray-200 rounded-lg p-4 hover:bg-gray-50 flex items-start justify-between"
                >
                  <div className="flex-1">
                    <h3 className="font-semibold text-lg mb-1">{skill.name}</h3>
                    <p className="text-sm text-gray-600 mb-2">{skill.description}</p>
                    <span className={`inline-block text-xs px-2 py-1 rounded ${getDifficultyColor(skill.difficulty)}`}>
                      {skill.difficulty}
                    </span>
                  </div>
                  <button
                    onClick={() => handleAddSkill(skill)}
                    disabled={adding === skill.id}
                    className="ml-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
                  >
                    {adding === skill.id ? 'Adding...' : 'Add'}
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
