'use client';

import React, { useState } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { api } from '@/lib/api';
import { ApiResponse, UserProfile } from '@/types';
import { ShieldCheck, BookOpen, GraduationCap, Lock, Mail, ArrowRight, AlertCircle } from 'lucide-react';

interface LoginApiResponse {
  token: string;
  expiresAt: string;
  user: UserProfile;
}

export default function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleSubmit = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const response = await api.post<ApiResponse<LoginApiResponse>>('/auth/login', {
        email: email.trim(),
        password: password,
      });

      if (response.data.success && response.data.data) {
        login(response.data.data.token, response.data.data.user);
      } else {
        setErrorMessage(response.data.message || 'Login failed. Please verify credentials.');
      }
    } catch (err: any) {
      setErrorMessage(
        err.response?.data?.message || 'Unable to connect to the backend API. Please make sure the backend is running.'
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleQuickLogin = (quickEmail: string, quickPass: string) => {
    setEmail(quickEmail);
    setPassword(quickPass);
    setIsLoading(true);
    setErrorMessage(null);

    api
      .post<ApiResponse<LoginApiResponse>>('/auth/login', {
        email: quickEmail,
        password: quickPass,
      })
      .then((res) => {
        if (res.data.success && res.data.data) {
          login(res.data.data.token, res.data.data.user);
        } else {
          setErrorMessage(res.data.message || 'Login failed');
        }
      })
      .catch((err) => {
        setErrorMessage(
          err.response?.data?.message || 'Unable to connect to API. Verify backend is running.'
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  return (
    <div className="min-h-screen bg-slate-100 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-md">
        <div className="flex justify-center">
          <div className="w-12 h-12 rounded-xl bg-indigo-600 text-white flex items-center justify-center font-extrabold text-2xl shadow-md">
            A
          </div>
        </div>
        <h2 className="mt-4 text-center text-3xl font-extrabold tracking-tight text-slate-900">
          Acadex Portal
        </h2>
        <p className="mt-1 text-center text-sm text-slate-600">
          Assignment & Submission Management System
        </p>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md px-4 sm:px-0">
        <div className="bg-white py-8 px-6 shadow-sm rounded-2xl border border-slate-200 sm:px-10">
          {errorMessage && (
            <div className="mb-5 p-3.5 rounded-lg bg-rose-50 border border-rose-200 flex items-start space-x-2 text-rose-700 text-sm">
              <AlertCircle className="w-5 h-5 shrink-0 mt-0.5" />
              <span>{errorMessage}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                Email Address
              </label>
              <div className="relative rounded-lg shadow-sm">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400">
                  <Mail className="h-4 w-4" />
                </div>
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="block w-full pl-9 pr-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 text-slate-900 placeholder-slate-400"
                  placeholder="name@acadex.edu"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-700 mb-1">
                Password
              </label>
              <div className="relative rounded-lg shadow-sm">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400">
                  <Lock className="h-4 w-4" />
                </div>
                <input
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="block w-full pl-9 pr-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 text-slate-900 placeholder-slate-400"
                  placeholder="••••••••"
                />
              </div>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full mt-2 flex justify-center items-center py-2.5 px-4 border border-transparent rounded-lg shadow-sm text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 transition-colors"
            >
              {isLoading ? (
                <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
              ) : (
                <div className="flex items-center space-x-1.5">
                  <span>Sign In</span>
                  <ArrowRight className="w-4 h-4" />
                </div>
              )}
            </button>
          </form>

          {/* Quick Demo Credentials for Fast Evaluation */}
          <div className="mt-8 border-t border-slate-200 pt-6">
            <p className="text-xs font-bold uppercase tracking-wider text-slate-500 text-center mb-3">
              Fast Demo Accounts (One-Click Login)
            </p>
            <div className="grid grid-cols-3 gap-2">
              <button
                type="button"
                onClick={() => handleQuickLogin('admin@acadex.edu', 'Admin@12345')}
                className="flex flex-col items-center justify-center p-2.5 border border-slate-200 rounded-xl hover:border-indigo-400 hover:bg-indigo-50/50 transition-all text-center group"
              >
                <div className="p-1.5 rounded-lg bg-rose-50 text-rose-600 group-hover:bg-rose-100 mb-1">
                  <ShieldCheck className="w-4 h-4" />
                </div>
                <span className="text-xs font-bold text-slate-800">Admin</span>
                <span className="text-[10px] text-slate-500">System</span>
              </button>

              <button
                type="button"
                onClick={() => handleQuickLogin('teacher@acadex.edu', 'Teacher@12345')}
                className="flex flex-col items-center justify-center p-2.5 border border-slate-200 rounded-xl hover:border-indigo-400 hover:bg-indigo-50/50 transition-all text-center group"
              >
                <div className="p-1.5 rounded-lg bg-sky-50 text-sky-600 group-hover:bg-sky-100 mb-1">
                  <BookOpen className="w-4 h-4" />
                </div>
                <span className="text-xs font-bold text-slate-800">Teacher</span>
                <span className="text-[10px] text-slate-500">Instructor</span>
              </button>

              <button
                type="button"
                onClick={() => handleQuickLogin('student@acadex.edu', 'Student@12345')}
                className="flex flex-col items-center justify-center p-2.5 border border-slate-200 rounded-xl hover:border-indigo-400 hover:bg-indigo-50/50 transition-all text-center group"
              >
                <div className="p-1.5 rounded-lg bg-emerald-50 text-emerald-600 group-hover:bg-emerald-100 mb-1">
                  <GraduationCap className="w-4 h-4" />
                </div>
                <span className="text-xs font-bold text-slate-800">Student</span>
                <span className="text-[10px] text-slate-500">Class 2026</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
