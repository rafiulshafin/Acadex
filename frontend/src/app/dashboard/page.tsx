'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useAuth } from '@/contexts/AuthContext';
import { api } from '@/lib/api';
import { StatCard } from '@/components/ui/StatCard';
import { RoleBadge, AssignmentStatusBadge, SubmissionStatusBadge } from '@/components/ui/Badge';
import { formatDate } from '@/lib/utils';
import {
  Users,
  GraduationCap,
  BookOpen,
  FileText,
  CheckSquare,
  Clock,
  ArrowRight,
  PlusCircle,
  Award,
} from 'lucide-react';
import {
  AdminStats,
  TeacherStats,
  StudentStats,
  AssignmentItem,
  ApiResponse,
} from '@/types';

export default function DashboardOverviewPage() {
  const { user } = useAuth();
  const [adminStats, setAdminStats] = useState<AdminStats | null>(null);
  const [teacherStats, setTeacherStats] = useState<TeacherStats | null>(null);
  const [studentStats, setStudentStats] = useState<StudentStats | null>(null);
  const [recentAssignments, setRecentAssignments] = useState<AssignmentItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchDashboardData = async () => {
      setIsLoading(true);
      try {
        if (user?.role === 'Admin') {
          const [statsRes, assignRes] = await Promise.all([
            api.get<ApiResponse<AdminStats>>('/dashboard/admin'),
            api.get<ApiResponse<AssignmentItem[]>>('/assignments'),
          ]);
          if (statsRes.data.success) setAdminStats(statsRes.data.data);
          if (assignRes.data.success) setRecentAssignments(assignRes.data.data.slice(0, 5));
        } else if (user?.role === 'Teacher') {
          const [statsRes, assignRes] = await Promise.all([
            api.get<ApiResponse<TeacherStats>>('/dashboard/teacher'),
            api.get<ApiResponse<AssignmentItem[]>>('/assignments'),
          ]);
          if (statsRes.data.success) setTeacherStats(statsRes.data.data);
          if (assignRes.data.success) setRecentAssignments(assignRes.data.data.slice(0, 5));
        } else if (user?.role === 'Student') {
          const [statsRes, assignRes] = await Promise.all([
            api.get<ApiResponse<StudentStats>>('/dashboard/student'),
            api.get<ApiResponse<AssignmentItem[]>>('/assignments'),
          ]);
          if (statsRes.data.success) setStudentStats(statsRes.data.data);
          if (assignRes.data.success) setRecentAssignments(assignRes.data.data.slice(0, 5));
        }
      } catch (err) {
        console.error('Failed to load dashboard data:', err);
      } finally {
        setIsLoading(false);
      }
    };

    if (user) {
      fetchDashboardData();
    }
  }, [user]);

  return (
    <div className="space-y-8">
      {/* Welcome Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <div className="flex items-center space-x-2">
            <h1 className="text-2xl font-bold tracking-tight text-slate-900">
              Welcome back, {user?.fullName}!
            </h1>
            {user?.role && <RoleBadge role={user.role} />}
          </div>
          <p className="text-sm text-slate-500 mt-1">
            {user?.role === 'Admin' && 'Manage educational resources, teacher allocations, and review global submissions.'}
            {user?.role === 'Teacher' && 'Create assignments, evaluate student submissions, and provide grades & feedback.'}
            {user?.role === 'Student' && `View assignments for ${user.schoolClassName || 'your class'}, submit answers, and check marks.`}
          </p>
        </div>

        {user?.role === 'Teacher' && (
          <Link
            href="/dashboard/assignments"
            className="inline-flex items-center space-x-2 px-4 py-2.5 rounded-lg bg-indigo-600 text-white font-medium text-sm hover:bg-indigo-700 transition-colors shadow-sm self-start md:self-auto"
          >
            <PlusCircle className="w-4 h-4" />
            <span>Manage Assignments</span>
          </Link>
        )}
      </div>

      {/* Metrics Section */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-28 bg-white rounded-xl border border-slate-200 animate-pulse"></div>
          ))}
        </div>
      ) : (
        <>
          {user?.role === 'Admin' && adminStats && (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
              <StatCard title="Total Students" value={adminStats.totalStudents} icon={GraduationCap} variant="emerald" />
              <StatCard title="Active Teachers" value={adminStats.totalTeachers} icon={Users} variant="blue" />
              <StatCard title="Courses / Classes" value={adminStats.totalClasses} icon={BookOpen} variant="indigo" />
              <StatCard title="Submissions" value={adminStats.totalSubmissions} subtitle={`${adminStats.pendingReviewCount} pending review`} icon={CheckSquare} variant="amber" />
            </div>
          )}

          {user?.role === 'Teacher' && teacherStats && (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
              <StatCard title="Assigned Classes" value={teacherStats.assignedClassesCount} icon={GraduationCap} variant="blue" />
              <StatCard title="Assigned Subjects" value={teacherStats.assignedSubjectsCount} icon={BookOpen} variant="indigo" />
              <StatCard title="Assignments Created" value={teacherStats.totalAssignmentsCreated} icon={FileText} variant="emerald" />
              <StatCard title="Submissions Received" value={teacherStats.totalSubmissionsReceived} subtitle={`${teacherStats.pendingGradingCount} pending grading`} icon={CheckSquare} variant="amber" />
            </div>
          )}

          {user?.role === 'Student' && studentStats && (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
              <StatCard title="Active Assignments" value={studentStats.totalEnrolledAssignments} icon={FileText} variant="indigo" />
              <StatCard title="Submitted" value={studentStats.submittedCount} icon={CheckSquare} variant="blue" />
              <StatCard title="Pending Submissions" value={studentStats.pendingCount} icon={Clock} variant="amber" />
              <StatCard title="Average Score" value={`${studentStats.averageScorePercentage}%`} subtitle={`${studentStats.gradedCount} graded`} icon={Award} variant="emerald" />
            </div>
          )}
        </>
      )}

      {/* Recent Assignments Preview */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <div className="p-6 border-b border-slate-100 flex items-center justify-between">
          <div>
            <h2 className="text-lg font-bold text-slate-900">
              {user?.role === 'Student' ? 'My Class Assignments' : 'Recent Assignments'}
            </h2>
            <p className="text-xs text-slate-500 mt-0.5">Overview of active academic course tasks and deadlines</p>
          </div>
          <Link
            href="/dashboard/assignments"
            className="text-xs font-semibold text-indigo-600 hover:text-indigo-800 flex items-center space-x-1"
          >
            <span>View all</span>
            <ArrowRight className="w-3.5 h-3.5" />
          </Link>
        </div>

        {recentAssignments.length === 0 ? (
          <div className="p-12 text-center text-slate-400">
            <FileText className="w-10 h-10 mx-auto text-slate-300 mb-2" />
            <p className="text-sm font-medium">No assignments found</p>
          </div>
        ) : (
          <div className="divide-y divide-slate-100">
            {recentAssignments.map((a) => (
              <div key={a.id} className="p-5 flex flex-col md:flex-row md:items-center justify-between gap-4 hover:bg-slate-50/70 transition-colors">
                <div className="space-y-1">
                  <div className="flex items-center space-x-2">
                    <h4 className="text-sm font-bold text-slate-900">{a.title}</h4>
                    <AssignmentStatusBadge status={a.status} />
                    {a.hasSubmitted && (
                      <span className="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-semibold bg-emerald-100 text-emerald-800">
                        Submitted
                      </span>
                    )}
                  </div>
                  <p className="text-xs text-slate-500 line-clamp-1">{a.description}</p>
                  <div className="flex items-center space-x-4 text-xs text-slate-500 pt-1">
                    <span><strong>Class:</strong> {a.schoolClassName}</span>
                    <span><strong>Subject:</strong> {a.subjectName} ({a.subjectCode})</span>
                    <span><strong>Max Marks:</strong> {a.maxMarks}</span>
                    <span><strong>Deadline:</strong> {formatDate(a.deadline)}</span>
                  </div>
                </div>

                <div className="flex items-center space-x-3 shrink-0">
                  <Link
                    href={`/dashboard/assignments/${a.id}`}
                    className="px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-semibold text-slate-700 hover:bg-slate-100 transition-colors"
                  >
                    {user?.role === 'Student' ? (a.hasSubmitted ? 'View Submission' : 'Submit Now') : 'View Details'}
                  </Link>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
