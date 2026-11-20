'use client';

import React from 'react';
import Link from 'next/navigation';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import {
  LayoutDashboard,
  BookOpen,
  FileText,
  Users,
  GraduationCap,
  Layers,
  CheckSquare,
  ShieldCheck,
} from 'lucide-react';
import LinkItem from 'next/link';

export const Sidebar: React.FC = () => {
  const pathname = usePathname();
  const { user } = useAuth();

  const getNavLinks = () => {
    const common = [
      { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
      { name: 'Assignments', href: '/dashboard/assignments', icon: FileText },
      { name: 'Submissions', href: '/dashboard/submissions', icon: CheckSquare },
    ];

    if (user?.role === 'Admin') {
      return [
        ...common,
        { name: 'User Management', href: '/dashboard/users', icon: Users },
        { name: 'Classes & Courses', href: '/dashboard/classes', icon: GraduationCap },
        { name: 'Subjects', href: '/dashboard/subjects', icon: BookOpen },
        { name: 'Teacher Allocations', href: '/dashboard/allocations', icon: Layers },
      ];
    }

    return common;
  };

  const navLinks = getNavLinks();

  return (
    <aside className="w-64 bg-slate-900 text-slate-300 flex flex-col shrink-0 min-h-screen border-r border-slate-800">
      <div className="h-16 flex items-center px-6 border-b border-slate-800 space-x-3">
        <div className="w-9 h-9 rounded-lg bg-indigo-600 text-white flex items-center justify-center font-bold text-lg shadow-sm">
          A
        </div>
        <div>
          <span className="text-lg font-bold tracking-tight text-white">Acadex</span>
          <span className="block text-[10px] uppercase font-semibold text-indigo-400 -mt-1 tracking-wider">
            Academic Portal
          </span>
        </div>
      </div>

      <div className="flex-1 py-6 px-4 space-y-1">
        <div className="text-[11px] font-semibold text-slate-500 uppercase tracking-wider px-3 mb-2">
          Navigation
        </div>
        {navLinks.map((item) => {
          const isActive = pathname === item.href;
          const Icon = item.icon;
          return (
            <LinkItem
              key={item.name}
              href={item.href}
              className={`flex items-center space-x-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-indigo-600 text-white shadow-sm'
                  : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60'
              }`}
            >
              <Icon className="w-4 h-4 shrink-0" />
              <span>{item.name}</span>
            </LinkItem>
          );
        })}
      </div>

      <div className="p-4 border-t border-slate-800 text-xs text-slate-500">
        <p className="font-medium text-slate-400">OnnoRokom Projukti Ltd</p>
        <p className="text-[11px] mt-0.5">Recruitment Project 2026</p>
      </div>
    </aside>
  );
};
