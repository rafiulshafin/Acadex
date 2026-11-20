export type UserRole = 'Admin' | 'Teacher' | 'Student';

export type AssignmentStatus = 'Draft' | 'Published' | 'Closed';

export type SubmissionStatus = 'Submitted' | 'UnderReview' | 'Graded' | 'ResubmissionAllowed';

export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  roleName: string;
  schoolClassId?: string;
  schoolClassName?: string;
}

export interface UserItem {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  roleName: string;
  schoolClassId?: string;
  schoolClassName?: string;
  isActive: boolean;
  createdAt: string;
}

export interface SchoolClass {
  id: string;
  name: string;
  section?: string;
  description?: string;
  academicYear?: string;
  studentCount: number;
  createdAt: string;
}

export interface Subject {
  id: string;
  code: string;
  name: string;
  description?: string;
  createdAt: string;
}

export interface ClassSubjectAllocation {
  id: string;
  schoolClassId: string;
  schoolClassName: string;
  subjectId: string;
  subjectCode: string;
  subjectName: string;
  teacherId: string;
  teacherName: string;
  teacherEmail: string;
  createdAt: string;
}

export interface SubmissionItem {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  assignmentMaxMarks: number;
  assignmentDeadline: string;
  studentId: string;
  studentName: string;
  studentEmail: string;
  content: string;
  attachmentUrl?: string;
  status: SubmissionStatus;
  statusName: string;
  submittedAt: string;
  updatedAt?: string;
  isLate: boolean;
  marksAwarded?: number;
  feedback?: string;
  gradedAt?: string;
  gradedByName?: string;
}

export interface AssignmentItem {
  id: string;
  title: string;
  description: string;
  schoolClassId: string;
  schoolClassName: string;
  subjectId: string;
  subjectCode: string;
  subjectName: string;
  teacherId: string;
  teacherName: string;
  deadline: string;
  maxMarks: number;
  status: AssignmentStatus;
  statusName: string;
  allowLateSubmissions: boolean;
  createdAt: string;
  updatedAt?: string;
  totalSubmissions: number;
  gradedSubmissions: number;
  hasSubmitted: boolean;
  mySubmission?: SubmissionItem;
}

export interface AdminStats {
  totalStudents: number;
  totalTeachers: number;
  totalClasses: number;
  totalSubjects: number;
  totalAssignments: number;
  totalSubmissions: number;
  pendingReviewCount: number;
}

export interface TeacherStats {
  assignedClassesCount: number;
  assignedSubjectsCount: number;
  totalAssignmentsCreated: number;
  totalSubmissionsReceived: number;
  pendingGradingCount: number;
}

export interface StudentStats {
  totalEnrolledAssignments: number;
  submittedCount: number;
  pendingCount: number;
  gradedCount: number;
  averageScorePercentage: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}
