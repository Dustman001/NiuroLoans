'use client';

import { useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { ApiError } from '@/lib/api';
import { useAuth } from '@/lib/auth';

/** Returns a handler that logs out and goes to the login page when the API answers 401. */
export function useSessionExpiry() {
  const { logOut } = useAuth();
  const router = useRouter();

  return useCallback(
    (error: unknown): boolean => {
      if (error instanceof ApiError && error.status === 401) {
        logOut();
        router.push('/application');
        return true;
      }
      return false;
    },
    [logOut, router],
  );
}
