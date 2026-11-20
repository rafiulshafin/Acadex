'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useAuth } from '@/contexts/AuthContext';
import { api } from '@/lib/api';
import { SubmissionStatusBadge } from '@/components/ui/Badge';
import { formatDate } from '@/lib/utils';
import { SubmissionItem, ApiResponse } from '@/types';
import { CheckSquare, Search, ExternalLink, Eye, AlertCircle } from 'lucide-react';

export default function SubmissionsPage() {
  const { user } = useAuth();
  const [submissions, setSubmissions] = useState<SubmissionItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const fetchSubmissions = async () => {
    setIsLoading(true);
    try {
      const res = await api.get<ApiResponse<SubmissionItem[]>>('/submissions');
      if (res.data.success) {
        setSubmissions(res.data.data);
      }
    } catch (err) {
      console.error('Failed to load submissions:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchSubmissions();
  }, []);

  const filteredSubmissions = submissions.filter((s) => {
    const matchesSearch =
      s.assignmentTitle.toLowerCase().includes(search.toLowerCase()) ||
      s.studentName.toLowerCase().includes(search.toLowerCase());
    const matchesStatus = !statusFilter || s.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900">Submissions</h1>
        <p className="text-xs text-slate-500 mt-0.5">
          {user?.role === 'Student'
            ? 'Track your submitted assignments, review scores, and read instructor feedback.'
            : 'View and audit student submissions across all assigned courses.'}
        </p>
      </div>

      {/* Filter and Search */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col md:flex-row items-center justify-between gap-4">
        <div className="relative w-full md:w-80">
          <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
          <input
            type="text"
            placeholder="Search by assignment or student..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-3 py-2 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900"
          />
        </div>

        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="px-3 py-2 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-700 bg-white"
        >
          <option value="">All Statuses</option>
          <option value="Submitted">Submitted</option>
          <option value="UnderReview">Under Review</option>
          <option value="Graded">Graded</option>
          <option value="ResubmissionAllowed">Resubmission Allowed</option>
        </select>
      </div>

      {/* Submissions Table */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        {isLoading ? (
          <div className="p-12 text-center text-slate-400">Loading submissions...</div>
        ) : filteredSubmissions.length === 0 ? (
          <div className="p-12 text-center text-slate-400">
            <CheckSquare className="w-12 h-12 mx-auto text-slate-300 mb-3" />
            <h3 className="text-sm font-semibold text-slate-700">No submissions found</h3>
            <p className="text-xs text-slate-500 mt-1">No student submissions match your filter criteria.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50/70 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                  <th className="py-3.5 px-6">Assignment</th>
                  <th className="py-3.5 px-6">Student</th>
                  <th className="py-3.5 px-6">Submitted Date</th>
                  <th className="py-3.5 px-6">Status</th>
                  <th className="py-3.5 px-6">Score</th>
                  <th className="py-3.5 px-6 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 text-slate-700">
                {filteredSubmissions.map((s) => (
                  <tr key={s.id} className="hover:bg-slate-50/70 transition-colors">
                    <td className="py-4 px-6 font-semibold text-slate-900 max-w-xs truncate">
                      {s.assignmentTitle}
                    </td>
                    <td className="py-4 px-6">
                      <div className="font-medium text-slate-900">{s.studentName}</div>
                      <div className="text-[11px] text-slate-400">{s.studentEmail}</div>
                    </td>
                    <td className="py-4 px-6">
                      <div>{formatDate(s.submittedAt)}</div>
                      {s.isLate && (
                        <span className="inline-block px-1.5 py-0.5 rounded text-[10px] bg-rose-100 text-rose-700 font-semibold mt-0.5">
                          Late
                        </span>
                      )}
                    </td>
                    <td className="py-4 px-6">
                      <SubmissionStatusBadge status={s.status} />
                    </td>
                    <td className="py-4 px-6 font-bold text-slate-900">
                      {s.marksAwarded != null ? `${s.marksAwarded} / ${s.assignmentMaxMarks}` : '—'}
                    </td>
                    <td className="py-4 px-6 text-right">
                      <Link
                        href={`/dashboard/assignments/${s.assignmentId}`}
                        className="px-3 py-1.5 rounded-lg bg-indigo-50 text-indigo-700 hover:bg-indigo-100 font-semibold text-xs transition-colors inline-flex items-center space-x-1"
                      >
                        <Eye className="w-3.5 h-3.5" />
                        <span>{user?.role === 'Student' ? 'View Feedback' : 'Review & Grade'}</span>
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
