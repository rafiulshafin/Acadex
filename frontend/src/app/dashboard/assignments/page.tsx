'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useAuth } from '@/contexts/AuthContext';
import { api } from '@/lib/api';
import { Modal } from '@/components/ui/Modal';
import { AssignmentStatusBadge } from '@/components/ui/Badge';
import { formatDate } from '@/lib/utils';
import {
  AssignmentItem,
  SchoolClass,
  Subject,
  ApiResponse,
  AssignmentStatus,
} from '@/types';
import {
  Plus,
  Search,
  Filter,
  FileText,
  Calendar,
  AlertCircle,
  Trash2,
  Edit,
  Eye,
  CheckCircle,
} from 'lucide-react';

export default function AssignmentsPage() {
  const { user } = useAuth();
  const [assignments, setAssignments] = useState<AssignmentItem[]>([]);
  const [classes, setClasses] = useState<SchoolClass[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Filters
  const [search, setSearch] = useState('');
  const [selectedClass, setSelectedClass] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [studentTab, setStudentTab] = useState<'all' | 'pending' | 'submitted'>('all');

  // Create Assignment Modal
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const [newTitle, setNewTitle] = useState('');
  const [newDescription, setNewDescription] = useState('');
  const [newClassId, setNewClassId] = useState('');
  const [newSubjectId, setNewSubjectId] = useState('');
  const [newDeadline, setNewDeadline] = useState('');
  const [newMaxMarks, setNewMaxMarks] = useState('100');
  const [newStatus, setNewStatus] = useState<AssignmentStatus>('Published');
  const [newAllowLate, setNewAllowLate] = useState(false);

  const fetchData = async () => {
    setIsLoading(true);
    try {
      const [assignRes, classRes, subjRes] = await Promise.all([
        api.get<ApiResponse<AssignmentItem[]>>('/assignments'),
        api.get<ApiResponse<SchoolClass[]>>('/classes'),
        api.get<ApiResponse<Subject[]>>('/subjects'),
      ]);

      if (assignRes.data.success) setAssignments(assignRes.data.data);
      if (classRes.data.success) setClasses(classRes.data.data);
      if (subjRes.data.success) setSubjects(subjRes.data.data);
    } catch (err) {
      console.error('Failed to load assignments:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreateAssignment = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setFormError(null);

    try {
      const payload = {
        title: newTitle.trim(),
        description: newDescription.trim(),
        schoolClassId: newClassId,
        subjectId: newSubjectId,
        deadline: new Date(newDeadline).toISOString(),
        maxMarks: parseFloat(newMaxMarks),
        status: newStatus,
        allowLateSubmissions: newAllowLate,
      };

      const res = await api.post<ApiResponse<AssignmentItem>>('/assignments', payload);
      if (res.data.success) {
        setIsModalOpen(false);
        // Reset form
        setNewTitle('');
        setNewDescription('');
        setNewClassId('');
        setNewSubjectId('');
        setNewDeadline('');
        setNewMaxMarks('100');
        fetchData();
      }
    } catch (err: any) {
      setFormError(err.response?.data?.message || 'Failed to create assignment');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteAssignment = async (id: string) => {
    if (!confirm('Are you sure you want to delete this assignment? All student submissions will also be deleted.')) return;
    try {
      const res = await api.delete<ApiResponse<boolean>>(`/assignments/${id}`);
      if (res.data.success) {
        setAssignments((prev) => prev.filter((a) => a.id !== id));
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to delete assignment');
    }
  };

  const filteredAssignments = assignments.filter((a) => {
    const matchesSearch =
      a.title.toLowerCase().includes(search.toLowerCase()) ||
      a.subjectName.toLowerCase().includes(search.toLowerCase()) ||
      a.schoolClassName.toLowerCase().includes(search.toLowerCase());

    const matchesClass = !selectedClass || a.schoolClassId === selectedClass;
    const matchesStatus = !selectedStatus || a.status === selectedStatus;

    if (user?.role === 'Student') {
      if (studentTab === 'pending') return matchesSearch && !a.hasSubmitted;
      if (studentTab === 'submitted') return matchesSearch && a.hasSubmitted;
    }

    return matchesSearch && matchesClass && matchesStatus;
  });

  return (
    <div className="space-y-6">
      {/* Top Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Assignments</h1>
          <p className="text-xs text-slate-500 mt-0.5">
            {user?.role === 'Student'
              ? 'Browse assigned course tasks, instructions, and track submission progress.'
              : 'Create and manage assignments, deadlines, and view class submissions.'}
          </p>
        </div>

        {(user?.role === 'Teacher' || user?.role === 'Admin') && (
          <button
            onClick={() => setIsModalOpen(true)}
            className="inline-flex items-center space-x-2 px-4 py-2.5 rounded-lg bg-indigo-600 text-white font-medium text-sm hover:bg-indigo-700 transition-colors shadow-sm self-start sm:self-auto"
          >
            <Plus className="w-4 h-4" />
            <span>Create Assignment</span>
          </button>
        )}
      </div>

      {/* Student Tabs */}
      {user?.role === 'Student' && (
        <div className="flex space-x-2 border-b border-slate-200 pb-2">
          <button
            onClick={() => setStudentTab('all')}
            className={`px-4 py-2 text-xs font-bold rounded-lg transition-colors ${
              studentTab === 'all' ? 'bg-indigo-600 text-white' : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            All ({assignments.length})
          </button>
          <button
            onClick={() => setStudentTab('pending')}
            className={`px-4 py-2 text-xs font-bold rounded-lg transition-colors ${
              studentTab === 'pending' ? 'bg-amber-600 text-white' : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            Pending ({assignments.filter((a) => !a.hasSubmitted).length})
          </button>
          <button
            onClick={() => setStudentTab('submitted')}
            className={`px-4 py-2 text-xs font-bold rounded-lg transition-colors ${
              studentTab === 'submitted' ? 'bg-emerald-600 text-white' : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            Submitted ({assignments.filter((a) => a.hasSubmitted).length})
          </button>
        </div>
      )}

      {/* Filters & Search Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col md:flex-row items-center justify-between gap-4">
        <div className="relative w-full md:w-80">
          <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
          <input
            type="text"
            placeholder="Search assignments or subjects..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-3 py-2 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900"
          />
        </div>

        <div className="flex items-center space-x-3 w-full md:w-auto">
          {user?.role !== 'Student' && (
            <select
              value={selectedClass}
              onChange={(e) => setSelectedClass(e.target.value)}
              className="px-3 py-2 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-700 bg-white"
            >
              <option value="">All Classes</option>
              {classes.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name} {c.section}
                </option>
              ))}
            </select>
          )}

          {user?.role !== 'Student' && (
            <select
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value)}
              className="px-3 py-2 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-700 bg-white"
            >
              <option value="">All Statuses</option>
              <option value="Published">Published</option>
              <option value="Draft">Draft</option>
              <option value="Closed">Closed</option>
            </select>
          )}
        </div>
      </div>

      {/* Assignments List */}
      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-32 bg-white rounded-xl border border-slate-200 animate-pulse"></div>
          ))}
        </div>
      ) : filteredAssignments.length === 0 ? (
        <div className="bg-white rounded-xl border border-slate-200 p-12 text-center text-slate-400">
          <FileText className="w-12 h-12 mx-auto text-slate-300 mb-3" />
          <h3 className="text-sm font-semibold text-slate-700">No assignments found</h3>
          <p className="text-xs text-slate-500 mt-1">Try adjusting your filters or search terms.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4">
          {filteredAssignments.map((a) => (
            <div
              key={a.id}
              className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm hover:border-slate-300 transition-all flex flex-col md:flex-row md:items-center justify-between gap-5"
            >
              <div className="space-y-2 flex-1">
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="text-base font-bold text-slate-900">{a.title}</h3>
                  <AssignmentStatusBadge status={a.status} />
                  {a.hasSubmitted && (
                    <span className="inline-flex items-center space-x-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
                      <CheckCircle className="w-3.5 h-3.5" />
                      <span>Submitted</span>
                    </span>
                  )}
                </div>

                <p className="text-xs text-slate-600 line-clamp-2 leading-relaxed">{a.description}</p>

                <div className="flex flex-wrap items-center gap-x-6 gap-y-2 text-xs text-slate-500 pt-1">
                  <span>
                    <strong className="text-slate-700">Course / Class:</strong> {a.schoolClassName}
                  </span>
                  <span>
                    <strong className="text-slate-700">Subject:</strong> {a.subjectName} ({a.subjectCode})
                  </span>
                  <span>
                    <strong className="text-slate-700">Instructor:</strong> {a.teacherName}
                  </span>
                  <span>
                    <strong className="text-slate-700">Max Marks:</strong> {a.maxMarks}
                  </span>
                  <span className="flex items-center space-x-1 text-slate-700 font-medium">
                    <Calendar className="w-3.5 h-3.5 text-slate-400" />
                    <span>Due: {formatDate(a.deadline)}</span>
                  </span>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex items-center space-x-2 shrink-0">
                <Link
                  href={`/dashboard/assignments/${a.id}`}
                  className="px-4 py-2 rounded-lg bg-indigo-50 text-indigo-700 hover:bg-indigo-100 text-xs font-bold transition-colors flex items-center space-x-1.5"
                >
                  <Eye className="w-3.5 h-3.5" />
                  <span>{user?.role === 'Student' ? (a.hasSubmitted ? 'My Submission' : 'Submit Solution') : 'Review & Submissions'}</span>
                </Link>

                {(user?.role === 'Admin' || user?.role === 'Teacher') && (
                  <button
                    onClick={() => handleDeleteAssignment(a.id)}
                    className="p-2 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
                    title="Delete Assignment"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Create Assignment Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Create New Assignment"
        description="Define assignment details, assign class, subject, and deadline."
        maxWidth="lg"
      >
        {formError && (
          <div className="mb-4 p-3 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-xs flex items-center space-x-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{formError}</span>
          </div>
        )}

        <form onSubmit={handleCreateAssignment} className="space-y-4">
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
              Assignment Title
            </label>
            <input
              type="text"
              required
              value={newTitle}
              onChange={(e) => setNewTitle(e.target.value)}
              placeholder="e.g. Relational Database Modeling and Optimization"
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
              Instructions & Description
            </label>
            <textarea
              required
              rows={4}
              value={newDescription}
              onChange={(e) => setNewDescription(e.target.value)}
              placeholder="Detailed guidelines, required artifacts, repository structure, and criteria..."
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                Target Class / Course
              </label>
              <select
                required
                value={newClassId}
                onChange={(e) => setNewClassId(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-white"
              >
                <option value="">Select Class</option>
                {classes.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name} {c.section}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                Subject
              </label>
              <select
                required
                value={newSubjectId}
                onChange={(e) => setNewSubjectId(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-white"
              >
                <option value="">Select Subject</option>
                {subjects.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name} ({s.code})
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                Deadline
              </label>
              <input
                type="datetime-local"
                required
                value={newDeadline}
                onChange={(e) => setNewDeadline(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                Max Marks
              </label>
              <input
                type="number"
                min="1"
                max="1000"
                required
                value={newMaxMarks}
                onChange={(e) => setNewMaxMarks(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                Status
              </label>
              <select
                value={newStatus}
                onChange={(e) => setNewStatus(e.target.value as AssignmentStatus)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-white"
              >
                <option value="Published">Published (Live)</option>
                <option value="Draft">Draft (Hidden)</option>
              </select>
            </div>
          </div>

          <div className="flex items-center space-x-2 pt-2">
            <input
              type="checkbox"
              id="allowLate"
              checked={newAllowLate}
              onChange={(e) => setNewAllowLate(e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-slate-300 rounded"
            />
            <label htmlFor="allowLate" className="text-xs text-slate-700 font-medium cursor-pointer">
              Allow late submissions after deadline
            </label>
          </div>

          <div className="flex justify-end space-x-3 pt-4 border-t border-slate-100">
            <button
              type="button"
              onClick={() => setIsModalOpen(false)}
              className="px-4 py-2 border border-slate-300 rounded-lg text-xs font-medium text-slate-700 hover:bg-slate-50 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-xs font-semibold hover:bg-indigo-700 transition-colors disabled:opacity-50"
            >
              {isSubmitting ? 'Creating...' : 'Create Assignment'}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
