'use client';

import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';

type Auth = {
  token: string | null;
  /** False until the token has been read from session storage, so pages don't redirect too early. */
  ready: boolean;
  logIn: (token: string) => void;
  logOut: () => void;
};

const TOKEN_KEY = 'loans.token';
const AuthContext = createContext<Auth | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    setToken(sessionStorage.getItem(TOKEN_KEY));
    setReady(true);
  }, []);

  const logIn = useCallback((newToken: string) => {
    sessionStorage.setItem(TOKEN_KEY, newToken);
    setToken(newToken);
  }, []);

  const logOut = useCallback(() => {
    sessionStorage.removeItem(TOKEN_KEY);
    setToken(null);
  }, []);

  return <AuthContext.Provider value={{ token, ready, logIn, logOut }}>{children}</AuthContext.Provider>;
}

export function useAuth(): Auth {
  const auth = useContext(AuthContext);
  if (!auth) throw new Error('useAuth must be used inside AuthProvider');
  return auth;
}
