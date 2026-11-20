'use client';

import React from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { RoleBadge } from '@/components/ui/Badge';
import { LogOut, User, Bell } from 'lucide-react';

export const Navbar: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <header className="h-16 bg-white border-b border-slate-200 px-6 flex items-center justify-between sticky top-0 z-30">
      <div className="flex items-center space-x-3">
        <span className="text-xs font-medium px-2.5 py-1 bg-slate-100 text-slate-600 rounded-md border border-slate-200">
          Acadex v1.0.0
        </span>
        {user?.schoolClassName && (
          <span className="text-xs font-medium text-slate-500 bg-indigo-50/70 border border-indigo-100 px-2.5 py-1 rounded-md">
            {user.schoolClassName}
          </span>
        )}
      </div>

      <div className="flex items-center space-x-4">
        {user && (
          <div className="flex items-center space-x-3 pl-4 border-l border-slate-200">
            <div className="text-right">
              <div className="flex items-center space-x-2 justify-end">
                <span className="text-sm font-semibold text-slate-800">{user.fullName}</span>
                <RoleBadge role={user.role} />
              </div>
              <p className="text-xs text-slate-500">{user.email}</p>
            </div>
            <div className="w-9 h-9 rounded-full bg-indigo-100 text-indigo-700 font-semibold flex items-center justify-center border border-indigo-200">
              {user.fullName.charAt(0).toUpperCase()}
            </div>
            <button
              onClick={logout}
              title="Logout"
              className="p-2 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
            >
              <LogOut className="w-5 h-5" />
            </button>
          </div>
        )}
      </div>
    </header>
  );
};
