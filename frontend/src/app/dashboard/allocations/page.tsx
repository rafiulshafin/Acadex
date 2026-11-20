'use client';

import React, { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Modal } from '@/components/ui/Modal';
import {
  ClassSubjectAllocation,
  SchoolClass,
  Subject,
  UserItem,
  ApiResponse,
} from '@/types';
import { Plus, Layers, Trash2, AlertCircle } from 'lucide-react';

export default function AllocationsPage() {
  const [allocations, setAllocations] = useState<ClassSubjectAllocation[]>([]);
  const [classes, setClasses] = useState<SchoolClass[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [teachers, setTeachers] = useState<UserItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Modal
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const [selectedClassId, setSelectedClassId] = useState('');
  const [selectedSubjectId, setSelectedSubjectId] = useState('');
  const [selectedTeacherId, setSelectedTeacherId] = useState('');

  const fetchAllocationsAndResources = async () => {
    setIsLoading(true);
    try {
      const [allocRes, classRes, subjRes, userRes] = await Promise.all([
        api.get<ApiResponse<ClassSubjectAllocation[]>>('/classsubjects'),
        api.get<ApiResponse<SchoolClass[]>>('/classes'),
        api.get<ApiResponse<Subject[]>>('/subjects'),
        api.get<ApiResponse<UserItem[]>>('/users?role=Teacher'),
      ]);

      if (allocRes.data.success) setAllocations(allocRes.data.data);
      if (classRes.data.success) setClasses(classRes.data.data);
      if (subjRes.data.success) setSubjects(subjRes.data.data);
      if (userRes.data.success) setTeachers(userRes.data.data);
    } catch (err) {
      console.error('Failed to load allocations:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchAllocationsAndResources();
  }, []);

  const handleAssignTeacher = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setFormError(null);

    try {
      const payload = {
        schoolClassId: selectedClassId,
        subjectId: selectedSubjectId,
        teacherId: selectedTeacherId,
      };

      const res = await api.post<ApiResponse<ClassSubjectAllocation>>('/classsubjects', payload);
      if (res.data.success) {
        setIsModalOpen(false);
        setSelectedClassId('');
        setSelectedSubjectId('');
        setSelectedTeacherId('');
        fetchAllocationsAndResources();
      }
    } catch (err: any) {
      setFormError(err.response?.data?.message || 'Failed to assign teacher');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRemoveAllocation = async (id: string) => {
    if (!confirm('Are you sure you want to remove this teacher assignment?')) return;

    try {
      const res = await api.delete<ApiResponse<boolean>>(`/classsubjects/${id}`);
      if (res.data.success) {
        setAllocations((prev) => prev.filter((a) => a.id !== id));
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to remove allocation');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Teacher Allocations</h1>
          <p className="text-xs text-slate-500 mt-0.5">
            Assign instructors to specific subjects and classes to authorize assignment creation and grading.
          </p>
        </div>

        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center space-x-2 px-4 py-2.5 rounded-lg bg-indigo-600 text-white font-medium text-sm hover:bg-indigo-700 transition-colors shadow-sm self-start sm:self-auto"
        >
          <Plus className="w-4 h-4" />
          <span>Assign Teacher</span>
        </button>
      </div>

      {/* Allocations Table */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        {isLoading ? (
          <div className="p-12 text-center text-slate-400">Loading allocations...</div>
        ) : allocations.length === 0 ? (
          <div className="p-12 text-center text-slate-400">
            <Layers className="w-12 h-12 mx-auto text-slate-300 mb-3" />
            <h3 className="text-sm font-semibold text-slate-700">No teacher allocations configured</h3>
            <p className="text-xs text-slate-500 mt-1">Assign teachers to classes and subjects to enable coursework.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50/70 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                  <th className="py-3.5 px-6">Class / Course</th>
                  <th className="py-3.5 px-6">Subject</th>
                  <th className="py-3.5 px-6">Assigned Instructor</th>
                  <th className="py-3.5 px-6 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 text-slate-700">
                {allocations.map((a) => (
                  <tr key={a.id} className="hover:bg-slate-50/70 transition-colors">
                    <td className="py-4 px-6 font-semibold text-slate-900">{a.schoolClassName}</td>
                    <td className="py-4 px-6 font-medium text-slate-800">
                      {a.subjectName}{' '}
                      <span className="font-mono text-slate-400 text-[11px]">({a.subjectCode})</span>
                    </td>
                    <td className="py-4 px-6">
                      <div className="font-semibold text-slate-900">{a.teacherName}</div>
                      <div className="text-[11px] text-slate-400">{a.teacherEmail}</div>
                    </td>
                    <td className="py-4 px-6 text-right">
                      <button
                        onClick={() => handleRemoveAllocation(a.id)}
                        className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
                        title="Remove Assignment"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Assign Teacher Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Assign Teacher to Subject & Class"
        description="Connect an instructor to an academic subject and student class cohort."
        maxWidth="md"
      >
        {formError && (
          <div className="mb-4 p-3 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-xs flex items-center space-x-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{formError}</span>
          </div>
        )}

        <form onSubmit={handleAssignTeacher} className="space-y-4">
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
              Select Class / Course
            </label>
            <select
              required
              value={selectedClassId}
              onChange={(e) => setSelectedClassId(e.target.value)}
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
              Select Subject
            </label>
            <select
              required
              value={selectedSubjectId}
              onChange={(e) => setSelectedSubjectId(e.target.value)}
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

          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
              Select Instructor (Teacher)
            </label>
            <select
              required
              value={selectedTeacherId}
              onChange={(e) => setSelectedTeacherId(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-white"
            >
              <option value="">Select Instructor</option>
              {teachers.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.fullName} ({t.email})
                </option>
              ))}
            </select>
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
              {isSubmitting ? 'Assigning...' : 'Confirm Allocation'}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
