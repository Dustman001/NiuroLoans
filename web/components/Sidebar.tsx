'use client';

import type { ReactNode } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth';

function NavLink({ href, children, nested = false }: { href: string; children: ReactNode; nested?: boolean }) {
  const pathname = usePathname();
  const className = ['nav-link', nested && 'nav-link--nested', pathname === href && 'nav-link--active']
    .filter(Boolean)
    .join(' ');
  return (
    <Link href={href} className={className}>
      {children}
    </Link>
  );
}

export function Sidebar() {
  const { token, ready, logOut } = useAuth();
  const router = useRouter();

  function handleLogOut() {
    logOut();
    router.push('/application');
  }

  return (
    <nav className="sidebar" aria-label="Main">
      <div className="sidebar__brand">Niuro Loans</div>
      {ready && !token && (
        <>
          <NavLink href="/login">LogIn</NavLink>
          <NavLink href="/application">New Loan Application</NavLink>
        </>
      )}
      {ready && token && (
        <>
          <span className="nav-heading">Configuration</span>
          <NavLink href="/config/states" nested>
            State
          </NavLink>
          <NavLink href="/config/blacklist" nested>
            SSN BlackList
          </NavLink>
          <button type="button" className="nav-link nav-link--button" onClick={handleLogOut}>
            LogOut
          </button>
        </>
      )}
    </nav>
  );
}
