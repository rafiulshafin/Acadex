import React from 'react';
import { cn } from '@/lib/utils';
import { AssignmentStatus, SubmissionStatus, UserRole } from '@/types';

interface BadgeProps {
  children: React.ReactNode;
  variant?: 'default' | 'success' | 'warning' | 'danger' | 'info' | 'neutral';
  className?: string;
}

export const Badge: React.FC<BadgeProps> = ({ children, variant = 'default', className }) => {
  const variantStyles = {
    default: 'bg-indigo-50 text-indigo-700 border-indigo-200',
    success: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    warning: 'bg-amber-50 text-amber-700 border-amber-200',
    danger: 'bg-rose-50 text-rose-700 border-rose-200',
    info: 'bg-sky-50 text-sky-700 border-sky-200',
    neutral: 'bg-slate-100 text-slate-700 border-slate-200',
  };

  return (
    <span className={cn('inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border', variantStyles[variant], className)}>
      {children}
    </span>
  );
};

export const RoleBadge: React.FC<{ role: UserRole }> = ({ role }) => {
  switch (role) {
    case 'Admin':
      return <Badge variant="danger">Admin</Badge>;
    case 'Teacher':
      return <Badge variant="info">Teacher</Badge>;
    case 'Student':
      return <Badge variant="success">Student</Badge>;
    default:
      return <Badge variant="neutral">{role}</Badge>;
  }
};

export const AssignmentStatusBadge: React.FC<{ status: AssignmentStatus }> = ({ status }) => {
  switch (status) {
    case 'Published':
      return <Badge variant="success">Published</Badge>;
    case 'Draft':
      return <Badge variant="neutral">Draft</Badge>;
    case 'Closed':
      return <Badge variant="danger">Closed</Badge>;
    default:
      return <Badge variant="neutral">{status}</Badge>;
  }
};

export const SubmissionStatusBadge: React.FC<{ status: SubmissionStatus }> = ({ status }) => {
  switch (status) {
    case 'Submitted':
      return <Badge variant="info">Submitted</Badge>;
    case 'UnderReview':
      return <Badge variant="warning">Under Review</Badge>;
    case 'Graded':
      return <Badge variant="success">Graded</Badge>;
    case 'ResubmissionAllowed':
      return <Badge variant="danger">Resubmission Allowed</Badge>;
    default:
      return <Badge variant="neutral">{status}</Badge>;
  }
};
