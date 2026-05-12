import { useState, useEffect } from 'react';
import { progressService } from '../services/progressService';
import type { ProgressSummary, HeatmapEntry, Achievement, LeaderboardEntry, Certificate } from '../services/progressService';

export default function ProgressPage() {
  const [summary, setSummary] = useState<ProgressSummary | null>(null);
  const [heatmap, setHeatmap] = useState<HeatmapEntry[]>([]);
  const [achievements, setAchievements] = useState<Achievement[]>([]);
  const [leaderboard, setLeaderboard] = useState<LeaderboardEntry[]>([]);
  const [certificates, setCertificates] = useState<Certificate[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'overview' | 'achievements' | 'leaderboard' | 'certificates'>('overview');

  useEffect(() => {
    loadAll();
  }, []);

  const loadAll = async () => {
    try {
      setLoading(true);
      const [s, h, a, l, c] = await Promise.all([
        progressService.getSummary(),
        progressService.getHeatmap(),
        progressService.getAchievements(),
        progressService.getLeaderboard(),
        progressService.getCertificates(),
      ]);
      setSummary(s);
      setHeatmap(h);
      setAchievements(a);
      setLeaderboard(l);
      setCertificates(c);
    } catch (err) {
      console.error('Failed to load progress:', err);
    } finally {
      setLoading(false);
    }
  };

  // Build a 365-day grid for the heatmap
  const buildHeatmapGrid = () => {
    const map = new Map(heatmap.map(e => [e.date.substring(0, 10), e.count]));
    const today = new Date();
    const days: { date: string; count: number }[] = [];
    for (let i = 364; i >= 0; i--) {
      const d = new Date(today);
      d.setDate(d.getDate() - i);
      const key = d.toISOString().substring(0, 10);
      days.push({ date: key, count: map.get(key) ?? 0 });
    }
    return days;
  };

  const getHeatColor = (count: number) => {
    if (count === 0) return 'bg-gray-100';
    if (count === 1) return 'bg-green-200';
    if (count === 2) return 'bg-green-400';
    if (count === 3) return 'bg-green-500';
    return 'bg-green-700';
  };

  const heatmapDays = buildHeatmapGrid();
  // Pad so weeks start on Monday
  const firstDay = heatmapDays.length > 0 ? new Date(heatmapDays[0].date).getDay() : 0;
  const paddingDays = firstDay === 0 ? 6 : firstDay - 1;

  const tabClass = (tab: typeof activeTab) => {
    const base = 'px-4 py-2 rounded-md text-sm font-medium transition-colors';
    return activeTab === tab
      ? `${base} bg-green-600 text-white`
      : `${base} text-gray-600 hover:bg-green-50 hover:text-green-700`;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="text-gray-500">Loading progress...</div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-5xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">My Progress</h1>

      {/* Summary cards */}
      {summary && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          <div className="bg-white rounded-xl shadow p-5 text-center">
            <div className="text-3xl font-bold text-green-600">{summary.totalCoursesCompleted}</div>
            <div className="text-sm text-gray-500 mt-1">Courses Completed</div>
          </div>
          <div className="bg-white rounded-xl shadow p-5 text-center">
            <div className="text-3xl font-bold text-blue-600">{summary.totalCoursesInProgress}</div>
            <div className="text-sm text-gray-500 mt-1">In Progress</div>
          </div>
          <div className="bg-white rounded-xl shadow p-5 text-center">
            <div className="text-3xl font-bold text-orange-500">🔥 {summary.currentStreak}</div>
            <div className="text-sm text-gray-500 mt-1">Day Streak</div>
          </div>
          <div className="bg-white rounded-xl shadow p-5 text-center">
            <div className="text-3xl font-bold text-purple-600">{summary.overallCompletionPct}%</div>
            <div className="text-sm text-gray-500 mt-1">Overall Progress</div>
          </div>
        </div>
      )}

      {/* Heatmap */}
      <div className="bg-white rounded-xl shadow p-6 mb-8">
        <h2 className="text-lg font-semibold text-gray-800 mb-4">Activity Heatmap — Last 365 Days</h2>
        <div className="overflow-x-auto">
          <div className="grid gap-1" style={{ gridTemplateColumns: `repeat(${Math.ceil((heatmapDays.length + paddingDays) / 7)}, 12px)`, gridAutoRows: '12px', gridAutoFlow: 'column', gridTemplateRows: 'repeat(7, 12px)' }}>
            {Array.from({ length: paddingDays }).map((_, i) => (
              <div key={`pad-${i}`} className="w-3 h-3" />
            ))}
            {heatmapDays.map(d => (
              <div
                key={d.date}
                title={`${d.date}: ${d.count} completion${d.count !== 1 ? 's' : ''}`}
                className={`w-3 h-3 rounded-sm ${getHeatColor(d.count)}`}
              />
            ))}
          </div>
        </div>
        {summary && (
          <div className="mt-3 text-sm text-gray-500">
            Longest streak: <span className="font-semibold text-green-600">{summary.longestStreak} days</span>
          </div>
        )}
      </div>

      {/* Tabs */}
      <div className="flex gap-2 mb-6">
        <button className={tabClass('overview')} onClick={() => setActiveTab('overview')}>Overview</button>
        <button className={tabClass('achievements')} onClick={() => setActiveTab('achievements')}>Achievements</button>
        <button className={tabClass('leaderboard')} onClick={() => setActiveTab('leaderboard')}>Leaderboard</button>
        <button className={tabClass('certificates')} onClick={() => setActiveTab('certificates')}>Certificates</button>
      </div>

      {/* Overview tab */}
      {activeTab === 'overview' && certificates.length > 0 && (
        <div className="space-y-4">
          {certificates.map(cert => (
            <div key={cert.skillId} className="bg-white rounded-xl shadow p-5">
              <div className="flex items-center justify-between mb-2">
                <h3 className="font-semibold text-gray-800">{cert.skillName}</h3>
                <span className="text-sm text-gray-500">{cert.coursesCompleted}/{cert.totalCourses} courses</span>
              </div>
              <div className="w-full bg-gray-100 rounded-full h-2">
                <div
                  className="bg-gradient-to-r from-green-500 to-green-400 h-2 rounded-full transition-all"
                  style={{ width: `${cert.completionPct}%` }}
                />
              </div>
              <div className="text-right text-sm text-green-600 font-medium mt-1">{cert.completionPct}%</div>
            </div>
          ))}
        </div>
      )}

      {activeTab === 'overview' && certificates.length === 0 && (
        <div className="bg-white rounded-xl shadow p-12 text-center text-gray-500">
          No skills enrolled yet. Start learning to see your progress!
        </div>
      )}

      {/* Achievements tab */}
      {activeTab === 'achievements' && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {achievements.map(a => (
            <div key={a.id} className={`rounded-xl shadow p-5 ${a.earned ? 'bg-white' : 'bg-gray-50 opacity-60'}`}>
              <div className="text-3xl mb-2">{a.emoji}</div>
              <div className="font-semibold text-gray-800">{a.name}</div>
              <div className="text-sm text-gray-500 mt-1">{a.description}</div>
              {a.earned && a.earnedAt && (
                <div className="text-xs text-green-600 mt-2">
                  Earned {new Date(a.earnedAt).toLocaleDateString()}
                </div>
              )}
              {!a.earned && (
                <div className="text-xs text-gray-400 mt-2">Not yet earned</div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Leaderboard tab */}
      {activeTab === 'leaderboard' && (
        <div className="bg-white rounded-xl shadow overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="text-left px-6 py-3 text-sm text-gray-500">Rank</th>
                <th className="text-left px-6 py-3 text-sm text-gray-500">User</th>
                <th className="text-right px-6 py-3 text-sm text-gray-500">Completed</th>
                <th className="text-right px-6 py-3 text-sm text-gray-500">Streak</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {leaderboard.map(entry => (
                <tr key={entry.userId} className="hover:bg-gray-50">
                  <td className="px-6 py-4">
                    <span className={`font-bold ${entry.rank === 1 ? 'text-yellow-500' : entry.rank === 2 ? 'text-gray-400' : entry.rank === 3 ? 'text-orange-400' : 'text-gray-700'}`}>
                      {entry.rank === 1 ? '🥇' : entry.rank === 2 ? '🥈' : entry.rank === 3 ? '🥉' : `#${entry.rank}`}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-700">{entry.email}</td>
                  <td className="px-6 py-4 text-right text-sm font-semibold text-green-600">{entry.coursesCompleted}</td>
                  <td className="px-6 py-4 text-right text-sm text-orange-500">🔥 {entry.currentStreak}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Certificates tab */}
      {activeTab === 'certificates' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {certificates.filter(c => c.completionPct === 100).map(cert => (
            <div key={cert.skillId} className="bg-gradient-to-br from-green-50 to-emerald-50 border-2 border-green-300 rounded-xl p-6">
              <div className="text-4xl mb-3">🏆</div>
              <div className="font-bold text-xl text-gray-800 mb-1">{cert.skillName}</div>
              <div className="text-sm text-gray-500 mb-3">Certificate of Completion</div>
              {cert.issuedAt && (
                <div className="text-xs text-gray-500 mb-3">
                  Issued {new Date(cert.issuedAt).toLocaleDateString()}
                </div>
              )}
              <div className="font-mono text-xs text-gray-400 break-all bg-white/60 rounded p-2">
                {cert.certId}
              </div>
            </div>
          ))}
          {certificates.filter(c => c.completionPct === 100).length === 0 && (
            <div className="col-span-2 text-center text-gray-500 py-12">
              Complete all courses in a skill to earn a certificate!
            </div>
          )}
        </div>
      )}
    </div>
  );
}
