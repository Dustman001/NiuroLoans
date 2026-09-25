'use client';

import { useEffect, type ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth';

/** Configuration pages are only for logged-in users. */
export default function ConfigurationLayout({ children }: { children: ReactNode }) {
  const { token, ready } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (ready && !token) router.replace('/application');
  }, [ready, token, router]);

  return token ? children : null;
}
