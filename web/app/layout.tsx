import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { Sidebar } from '@/components/Sidebar';
import { AuthProvider } from '@/lib/auth';
import './globals.css';

export const metadata: Metadata = {
  title: 'Niuro Loans',
  description: 'Small loan applications',
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>
        <AuthProvider>
          <div className="shell">
            <Sidebar />
            <main className="content">{children}</main>
          </div>
        </AuthProvider>
      </body>
    </html>
  );
}
