import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { UserSkill, Topic, Course } from '../services/knowledgeService';

interface TreeViewProps {
  skills: UserSkill[];
  onRemoveSkill: (skillId: number) => void;
  onRemoveTopic: (topicId: number) => void;
  onStartCourse: (courseId: number) => void;
}

export default function TreeView({ skills, onRemoveSkill, onRemoveTopic, onStartCourse: _onStartCourse }: TreeViewProps) {
  const navigate = useNavigate();
  const [expandedSkills, setExpandedSkills] = useState<Set<number>>(new Set());
  const [expandedTopics, setExpandedTopics] = useState<Set<number>>(new Set());

  const toggleSkill = (skillId: number) => {
    const newExpanded = new Set(expandedSkills);
    if (newExpanded.has(skillId)) {
      newExpanded.delete(skillId);
    } else {
      newExpanded.add(skillId);
    }
    setExpandedSkills(newExpanded);
  };

  const toggleTopic = (topicId: number) => {
    const newExpanded = new Set(expandedTopics);
    if (newExpanded.has(topicId)) {
      newExpanded.delete(topicId);
    } else {
      newExpanded.add(topicId);
    }
    setExpandedTopics(newExpanded);
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Beginner': return 'bg-green-100 text-green-800';
      case 'Intermediate': return 'bg-yellow-100 text-yellow-800';
      case 'Advanced': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Interested': return 'text-blue-600';
      case 'Learning': return 'text-yellow-600';
      case 'Mastered': return 'text-green-600';
      default: return 'text-gray-600';
    }
  };

  if (skills.length === 0) {
    return (
      <div className="text-center py-12 text-gray-500">
        <p className="text-lg">No skills yet. Add your first skill to get started!</p>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {skills.map((userSkill) => {
        const skill = userSkill.skill;
        const isExpanded = expandedSkills.has(skill.id);

        return (
          <div key={skill.id} className="border border-gray-200 rounded-lg bg-white">
            {/* Skill Header */}
            <div className="p-4 flex items-center justify-between hover:bg-gray-50">
              <div className="flex items-center gap-3 flex-1 cursor-pointer" onClick={() => toggleSkill(skill.id)}>
                <span className="text-gray-400">
                  {isExpanded ? '▼' : '▶'}
                </span>
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <h3 className="font-semibold text-lg">{skill.name}</h3>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        navigate(`/skill/${skill.id}`);
                      }}
                      className="text-blue-600 hover:text-blue-700 text-lg"
                      title="Go to skill page"
                    >
                      →
                    </button>
                  </div>
                  <p className="text-sm text-gray-600">{skill.description}</p>
                  <div className="flex items-center gap-2 mt-1">
                    <span className={`text-xs px-2 py-1 rounded ${getDifficultyColor(skill.difficulty)}`}>
                      {skill.difficulty}
                    </span>
                    <span className={`text-xs font-medium ${getStatusColor(userSkill.status)}`}>
                      {userSkill.status}
                    </span>
                  </div>
                </div>
              </div>
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onRemoveSkill(skill.id);
                }}
                className="px-3 py-1 text-sm text-red-600 hover:bg-red-50 rounded"
              >
                Remove
              </button>
            </div>

            {/* Topics (expanded) */}
            {isExpanded && skill.topics && skill.topics.length > 0 && (
              <div className="pl-8 pr-4 pb-4 space-y-2">
                {skill.topics.map((topic: Topic) => {
                  const isTopicExpanded = expandedTopics.has(topic.id);

                  return (
                    <div key={topic.id} className="border-l-2 border-blue-200 pl-4">
                      <div className="flex items-center justify-between py-2 hover:bg-gray-50 rounded px-2">
                        <div className="flex items-center gap-2 flex-1 cursor-pointer" onClick={() => toggleTopic(topic.id)}>
                          <span className="text-gray-400 text-sm">
                            {isTopicExpanded ? '▼' : '▶'}
                          </span>
                          <div className="flex-1">
                            <div className="flex items-center gap-2">
                              <h4 className="font-medium">{topic.name}</h4>
                              <button
                                onClick={(e) => {
                                  e.stopPropagation();
                                  navigate(`/topic/${topic.id}`);
                                }}
                                className="text-blue-600 hover:text-blue-700"
                                title="Go to topic page"
                              >
                                →
                              </button>
                            </div>
                            <p className="text-xs text-gray-500">{topic.description}</p>
                          </div>
                        </div>
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            onRemoveTopic(topic.id);
                          }}
                          className="px-2 py-1 text-xs text-red-600 hover:bg-red-50 rounded"
                        >
                          Remove
                        </button>
                      </div>

                      {/* Courses (expanded) */}
                      {isTopicExpanded && topic.courses && topic.courses.length > 0 && (
                        <div className="pl-6 pt-2 space-y-1">
                          {topic.courses.map((course: Course) => (
                            <div key={course.id} className="border-l-2 border-green-200 pl-3 py-2 hover:bg-gray-50 rounded">
                              <div className="flex items-center justify-between">
                                <div className="flex-1">
                                  <div className="flex items-center gap-2">
                                    <h5 className="font-medium text-sm">{course.name}</h5>
                                    <button
                                      onClick={() => navigate(`/course-detail/${course.id}`)}
                                      className="text-blue-600 hover:text-blue-700"
                                      title="Go to course page"
                                    >
                                      →
                                    </button>
                                  </div>
                                  <p className="text-xs text-gray-500">{course.description}</p>
                                  <div className="flex items-center gap-2 mt-1">
                                    <span className="text-xs text-gray-500">
                                      {Math.round(course.estimatedMinutes / 60)}h
                                    </span>
                                    {course.prerequisites.length > 0 && (
                                      <span className="text-xs text-orange-600">
                                        Prerequisites: {course.prerequisites.length}
                                      </span>
                                    )}
                                    <span className="text-xs text-blue-600">
                                      {course.resourceLinks.length} resources
                                    </span>
                                  </div>
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
