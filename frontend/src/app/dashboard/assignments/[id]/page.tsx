'use client';

import React, { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { api } from '@/lib/api';
import { Modal } from '@/components/ui/Modal';
import { AssignmentStatusBadge, SubmissionStatusBadge } from '@/components/ui/Badge';
import { formatDate, isPastDeadline } from '@/lib/utils';
import {
  AssignmentItem,
  SubmissionItem,
  ApiResponse,
  SubmissionStatus,
} from '@/types';
import {
  Calendar,
  Clock,
  Award,
  AlertCircle,
  CheckCircle2,
  ExternalLink,
  Edit,
  ArrowLeft,
} from 'lucide-react';
import Link from 'next/link';

export default function AssignmentDetailPage() {
  const { id } = useParams();
  const { user } = useAuth();
  const router = useRouter();

  const [assignment, setAssignment] = useState<AssignmentItem | null>(null);
  const [submissions, setSubmissions] = useState<SubmissionItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Student Submission state
  const [submissionContent, setSubmissionContent] = useState('');
  const [attachmentUrl, setAttachmentUrl] = useState('');
  const [isSubmittingAnswer, setIsSubmittingAnswer] = useState(false);
  const [submissionError, setSubmissionError] = useState<string | null>(null);
  const [submissionSuccess, setSubmissionSuccess] = useState<string | null>(null);

  // Teacher Grading Modal state
  const [selectedSubmission, setSelectedSubmission] = useState<SubmissionItem | null>(null);
  const [gradingMarks, setGradingMarks] = useState('');
  const [gradingFeedback, setGradingFeedback] = useState('');
  const [gradingStatus, setGradingStatus] = useState<SubmissionStatus>('Graded');
  const [isGrading, setIsGrading] = useState(false);
  const [gradingError, setGradingError] = useState<string | null>(null);

  const fetchAssignmentData = async () => {
    setIsLoading(true);
    try {
      const res = await api.get<ApiResponse<AssignmentItem>>(`/assignments/${id}`);
      if (res.data.success) {
        setAssignment(res.data.data);

        // Prepopulate existing submission if student
        if (res.data.data.mySubmission) {
          setSubmissionContent(res.data.data.mySubmission.content);
          setAttachmentUrl(res.data.data.mySubmission.attachmentUrl || '');
        }
      }

      // If teacher/admin, fetch all submissions for this assignment
      if (user?.role === 'Teacher' || user?.role === 'Admin') {
        const subRes = await api.get<ApiResponse<SubmissionItem[]>>(`/submissions?assignmentId=${id}`);
        if (subRes.data.success) {
          setSubmissions(subRes.data.data);
        }
      }
    } catch (err: any) {
      console.error('Failed to load assignment details:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (id) {
      fetchAssignmentData();
    }
  }, [id, user]);

  const handleStudentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmittingAnswer(true);
    setSubmissionError(null);
    setSubmissionSuccess(null);

    try {
      if (assignment?.mySubmission) {
        // Update existing submission
        const res = await api.put<ApiResponse<SubmissionItem>>(`/submissions/${assignment.mySubmission.id}`, {
          content: submissionContent.trim(),
          attachmentUrl: attachmentUrl.trim() || null,
        });
        if (res.data.success) {
          setSubmissionSuccess('Submission updated successfully!');
          fetchAssignmentData();
        }
      } else {
        // Create new submission
        const res = await api.post<ApiResponse<SubmissionItem>>('/submissions', {
          assignmentId: id,
          content: submissionContent.trim(),
          attachmentUrl: attachmentUrl.trim() || null,
        });
        if (res.data.success) {
          setSubmissionSuccess('Assignment submitted successfully!');
          fetchAssignmentData();
        }
      }
    } catch (err: any) {
      setSubmissionError(err.response?.data?.message || 'Failed to submit assignment');
    } finally {
      setIsSubmittingAnswer(false);
    }
  };

  const handleGradeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedSubmission) return;

    setIsGrading(true);
    setGradingError(null);

    try {
      const res = await api.post<ApiResponse<SubmissionItem>>(`/submissions/${selectedSubmission.id}/grade`, {
        marksAwarded: parseFloat(gradingMarks),
        feedback: gradingFeedback.trim(),
        status: gradingStatus,
      });

      if (res.data.success) {
        setSelectedSubmission(null);
        fetchAssignmentData();
      }
    } catch (err: any) {
      setGradingError(err.response?.data?.message || 'Failed to submit grading');
    } finally {
      setIsGrading(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (!assignment) {
    return (
      <div className="text-center py-12">
        <p className="text-sm text-slate-500">Assignment not found</p>
        <Link href="/dashboard/assignments" className="mt-2 text-xs font-bold text-indigo-600">
          Back to assignments
        </Link>
      </div>
    );
  }

  const pastDeadline = isPastDeadline(assignment.deadline);
  const canStudentSubmit =
    user?.role === 'Student' &&
    (!pastDeadline || assignment.allowLateSubmissions || assignment.mySubmission?.status === 'ResubmissionAllowed');

  return (
    <div className="space-y-8">
      {/* Back button */}
      <div>
        <Link
          href="/dashboard/assignments"
          className="inline-flex items-center space-x-1.5 text-xs font-semibold text-slate-500 hover:text-slate-800 transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          <span>Back to assignments</span>
        </Link>
      </div>

      {/* Assignment Header Card */}
      <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm space-y-4">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="space-y-1">
            <div className="flex items-center space-x-2">
              <h1 className="text-xl font-bold text-slate-900">{assignment.title}</h1>
              <AssignmentStatusBadge status={assignment.status} />
            </div>
            <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-slate-500">
              <span><strong>Class:</strong> {assignment.schoolClassName}</span>
              <span><strong>Subject:</strong> {assignment.subjectName} ({assignment.subjectCode})</span>
              <span><strong>Instructor:</strong> {assignment.teacherName}</span>
            </div>
          </div>

          <div className="flex items-center space-x-4 bg-slate-50 border border-slate-200 px-4 py-3 rounded-xl text-xs">
            <div>
              <span className="text-slate-400 block font-semibold uppercase text-[10px]">Deadline</span>
              <span className="font-bold text-slate-800">{formatDate(assignment.deadline)}</span>
            </div>
            <div className="h-8 w-px bg-slate-200"></div>
            <div>
              <span className="text-slate-400 block font-semibold uppercase text-[10px]">Max Marks</span>
              <span className="font-bold text-indigo-600">{assignment.maxMarks} pts</span>
            </div>
          </div>
        </div>

        <div className="border-t border-slate-100 pt-4">
          <h3 className="text-xs font-bold uppercase tracking-wider text-slate-500 mb-2">
            Description & Requirements
          </h3>
          <p className="text-sm text-slate-700 whitespace-pre-line leading-relaxed">
            {assignment.description}
          </p>
        </div>
      </div>

      {/* Student Submission Section */}
      {user?.role === 'Student' && (
        <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm space-y-6">
          <div className="border-b border-slate-100 pb-4">
            <h2 className="text-lg font-bold text-slate-900">Your Submission</h2>
            <p className="text-xs text-slate-500 mt-0.5">
              {assignment.mySubmission
                ? `Submitted on ${formatDate(assignment.mySubmission.submittedAt)}`
                : 'Enter your response and attach supporting repository or artifact link.'}
            </p>
          </div>

          {/* Graded Feedback Banner (if graded) */}
          {assignment.mySubmission && assignment.mySubmission.status === 'Graded' && (
            <div className="p-4 rounded-xl bg-emerald-50 border border-emerald-200 space-y-2">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2 text-emerald-800 font-bold text-sm">
                  <Award className="w-5 h-5 text-emerald-600" />
                  <span>Evaluation Completed</span>
                </div>
                <span className="text-sm font-extrabold text-emerald-700 px-3 py-1 bg-white rounded-lg border border-emerald-200">
                  {assignment.mySubmission.marksAwarded} / {assignment.maxMarks} Marks
                </span>
              </div>
              {assignment.mySubmission.feedback && (
                <div className="text-xs text-emerald-900 mt-2 bg-emerald-100/60 p-3 rounded-lg">
                  <strong>Instructor Feedback:</strong> {assignment.mySubmission.feedback}
                </div>
              )}
            </div>
          )}

          {submissionSuccess && (
            <div className="p-3.5 rounded-lg bg-emerald-50 border border-emerald-200 text-emerald-700 text-xs flex items-center space-x-2">
              <CheckCircle2 className="w-4 h-4 shrink-0" />
              <span>{submissionSuccess}</span>
            </div>
          )}

          {submissionError && (
            <div className="p-3.5 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-xs flex items-center space-x-2">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>{submissionError}</span>
            </div>
          )}

          <form onSubmit={handleStudentSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                Solution Response / Explanation
              </label>
              <textarea
                rows={5}
                required
                disabled={!canStudentSubmit}
                value={submissionContent}
                onChange={(e) => setSubmissionContent(e.target.value)}
                placeholder="Provide your solution details, code review explanations, or answers here..."
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:bg-slate-100 disabled:text-slate-500"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                GitHub Repository / Project Attachment URL
              </label>
              <input
                type="url"
                disabled={!canStudentSubmit}
                value={attachmentUrl}
                onChange={(e) => setAttachmentUrl(e.target.value)}
                placeholder="https://github.com/username/project"
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:bg-slate-100 disabled:text-slate-500"
              />
            </div>

            {canStudentSubmit && (
              <div className="flex justify-end pt-2">
                <button
                  type="submit"
                  disabled={isSubmittingAnswer}
                  className="px-5 py-2.5 rounded-lg bg-indigo-600 text-white text-xs font-bold hover:bg-indigo-700 transition-colors shadow-sm disabled:opacity-50"
                >
                  {isSubmittingAnswer
                    ? 'Submitting...'
                    : assignment.mySubmission
                    ? 'Update My Submission'
                    : 'Submit Assignment'}
                </button>
              </div>
            )}

            {!canStudentSubmit && (
              <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg text-amber-800 text-xs flex items-center space-x-2">
                <Clock className="w-4 h-4 shrink-0" />
                <span>
                  {assignment.mySubmission?.status === 'Graded'
                    ? 'This assignment has already been evaluated and graded.'
                    : 'The deadline for this assignment has expired. Updates are disabled.'}
                </span>
              </div>
            )}
          </form>
        </div>
      )}

      {/* Teacher / Admin Submissions Review Table */}
      {(user?.role === 'Teacher' || user?.role === 'Admin') && (
        <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden space-y-4">
          <div className="p-6 border-b border-slate-100 flex items-center justify-between">
            <div>
              <h2 className="text-lg font-bold text-slate-900">Student Submissions</h2>
              <p className="text-xs text-slate-500 mt-0.5">
                Review submitted student answers, assign marks, and give qualitative feedback.
              </p>
            </div>
            <span className="text-xs font-bold px-3 py-1 rounded-full bg-indigo-50 text-indigo-700 border border-indigo-200">
              {submissions.length} Total Submissions
            </span>
          </div>

          {submissions.length === 0 ? (
            <div className="p-12 text-center text-slate-400">
              <p className="text-sm font-medium">No students have submitted solutions yet.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse text-xs">
                <thead>
                  <tr className="bg-slate-50/70 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                    <th className="py-3 px-6">Student</th>
                    <th className="py-3 px-6">Submitted Date</th>
                    <th className="py-3 px-6">Status</th>
                    <th className="py-3 px-6">Marks Awarded</th>
                    <th className="py-3 px-6">Artifact / URL</th>
                    <th className="py-3 px-6 text-right">Action</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 text-slate-700">
                  {submissions.map((sub) => (
                    <tr key={sub.id} className="hover:bg-slate-50/70 transition-colors">
                      <td className="py-4 px-6 font-medium text-slate-900">
                        <div>{sub.studentName}</div>
                        <div className="text-[11px] text-slate-400">{sub.studentEmail}</div>
                      </td>
                      <td className="py-4 px-6">
                        <div>{formatDate(sub.submittedAt)}</div>
                        {sub.isLate && (
                          <span className="inline-block px-1.5 py-0.5 rounded text-[10px] bg-rose-100 text-rose-700 font-semibold mt-0.5">
                            Late Submission
                          </span>
                        )}
                      </td>
                      <td className="py-4 px-6">
                        <SubmissionStatusBadge status={sub.status} />
                      </td>
                      <td className="py-4 px-6 font-bold text-slate-900">
                        {sub.marksAwarded != null ? `${sub.marksAwarded} / ${assignment.maxMarks}` : '—'}
                      </td>
                      <td className="py-4 px-6">
                        {sub.attachmentUrl ? (
                          <a
                            href={sub.attachmentUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="inline-flex items-center space-x-1 text-indigo-600 hover:text-indigo-800 font-medium"
                          >
                            <span>Link</span>
                            <ExternalLink className="w-3 h-3" />
                          </a>
                        ) : (
                          <span className="text-slate-400">None</span>
                        )}
                      </td>
                      <td className="py-4 px-6 text-right">
                        <button
                          onClick={() => {
                            setSelectedSubmission(sub);
                            setGradingMarks(sub.marksAwarded?.toString() || '');
                            setGradingFeedback(sub.feedback || '');
                            setGradingStatus(sub.status === 'Submitted' ? 'Graded' : sub.status);
                          }}
                          className="px-3 py-1.5 rounded-lg bg-indigo-600 text-white font-semibold text-xs hover:bg-indigo-700 transition-colors"
                        >
                          Grade / Review
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* Teacher Grading & Review Modal */}
      {selectedSubmission && (
        <Modal
          isOpen={!!selectedSubmission}
          onClose={() => setSelectedSubmission(null)}
          title={`Evaluate Submission - ${selectedSubmission.studentName}`}
          description={`Assignment: ${assignment.title} (Max Marks: ${assignment.maxMarks})`}
          maxWidth="lg"
        >
          {gradingError && (
            <div className="mb-4 p-3 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-xs flex items-center space-x-2">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>{gradingError}</span>
            </div>
          )}

          <div className="space-y-4">
            <div className="bg-slate-50 p-4 rounded-xl border border-slate-200 space-y-2">
              <div className="flex items-center justify-between text-xs text-slate-500">
                <span>Submitted: {formatDate(selectedSubmission.submittedAt)}</span>
                {selectedSubmission.isLate && (
                  <span className="font-semibold text-rose-600">Late Submission</span>
                )}
              </div>
              <div>
                <h4 className="text-xs font-bold uppercase tracking-wider text-slate-500 mb-1">
                  Submitted Answer:
                </h4>
                <p className="text-xs text-slate-800 whitespace-pre-line bg-white p-3 rounded-lg border border-slate-200">
                  {selectedSubmission.content}
                </p>
              </div>
              {selectedSubmission.attachmentUrl && (
                <div className="pt-1">
                  <a
                    href={selectedSubmission.attachmentUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center space-x-1.5 text-xs font-bold text-indigo-600 hover:text-indigo-800"
                  >
                    <span>Inspect Attachment / Project URL</span>
                    <ExternalLink className="w-3.5 h-3.5" />
                  </a>
                </div>
              )}
            </div>

            <form onSubmit={handleGradeSubmit} className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                    Marks Awarded (0 - {assignment.maxMarks})
                  </label>
                  <input
                    type="number"
                    step="0.5"
                    min="0"
                    max={assignment.maxMarks}
                    required
                    value={gradingMarks}
                    onChange={(e) => setGradingMarks(e.target.value)}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                    Submission Status
                  </label>
                  <select
                    value={gradingStatus}
                    onChange={(e) => setGradingStatus(e.target.value as SubmissionStatus)}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-white"
                  >
                    <option value="Graded">Graded</option>
                    <option value="UnderReview">Under Review</option>
                    <option value="ResubmissionAllowed">Resubmission Allowed</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                  Instructor Feedback & Critique
                </label>
                <textarea
                  rows={4}
                  value={gradingFeedback}
                  onChange={(e) => setGradingFeedback(e.target.value)}
                  placeholder="Provide constructive feedback, areas of improvement, and grading breakdown..."
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <div className="flex justify-end space-x-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setSelectedSubmission(null)}
                  className="px-4 py-2 border border-slate-300 rounded-lg text-xs font-medium text-slate-700 hover:bg-slate-50 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isGrading}
                  className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-xs font-semibold hover:bg-indigo-700 transition-colors disabled:opacity-50"
                >
                  {isGrading ? 'Saving Grade...' : 'Save Grade & Feedback'}
                </button>
              </div>
            </form>
          </div>
        </Modal>
      )}
    </div>
  );
}
