"use client";

import React, { createContext, useCallback, useEffect, useState, startTransition } from "react";
import type { User, AuthState } from "@/types/auth";
import { authApi } from "@/services/auth";

interface AuthContextType extends AuthState {
  login: (email: string, password: string) => Promise<void>;
  register: (data: {
    email: string;
    firstName: string;
    lastName: string;
    password: string;
    confirmPassword: string;
  }) => Promise<void>;
  logout: () => Promise<void>;
  refreshAuth: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

const STORAGE_KEY = "auth_tokens";

function getStoredAuth(): { user: User | null; accessToken: string | null; refreshToken: string | null } {
  if (typeof window === "undefined") return { user: null, accessToken: null, refreshToken: null };

  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored);
      return {
        user: parsed.user || null,
        accessToken: parsed.accessToken || null,
        refreshToken: parsed.refreshToken || null,
      };
    }
  } catch {
    // ignore
  }
  return { user: null, accessToken: null, refreshToken: null };
}

function storeAuth(user: User, accessToken: string, refreshToken: string) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ user, accessToken, refreshToken }));
}

function clearAuth() {
  localStorage.removeItem(STORAGE_KEY);
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
  });

  useEffect(() => {
    const { user } = getStoredAuth();
    startTransition(() => {
      setState({
        user,
        isAuthenticated: !!user,
        isLoading: false,
      });
    });
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const response = await authApi.login({ email, password });
    storeAuth(response.user, response.accessToken, response.refreshToken);
    setState({
      user: response.user,
      isAuthenticated: true,
      isLoading: false,
    });
  }, []);

  const register = useCallback(async (data: {
    email: string;
    firstName: string;
    lastName: string;
    password: string;
    confirmPassword: string;
  }) => {
    await authApi.register(data);
  }, []);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } catch {
      // ignore logout errors
    }
    clearAuth();
    setState({
      user: null,
      isAuthenticated: false,
      isLoading: false,
    });
  }, []);

  const refreshAuth = useCallback(async () => {
    const { refreshToken } = getStoredAuth();
    if (!refreshToken) {
      clearAuth();
      setState({ user: null, isAuthenticated: false, isLoading: false });
      return;
    }

    try {
      const response = await authApi.refresh(refreshToken);
      storeAuth(response.user, response.accessToken, response.refreshToken);
      setState({
        user: response.user,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch {
      clearAuth();
      setState({ user: null, isAuthenticated: false, isLoading: false });
    }
  }, []);

  return (
    <AuthContext.Provider value={{ ...state, login, register, logout, refreshAuth }}>
      {children}
    </AuthContext.Provider>
  );
}
