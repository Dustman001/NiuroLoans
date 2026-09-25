'use client';

import { useState, type FormEvent } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';
import { useAuth } from '@/lib/auth';

export default function LoginPage() {
  const router = useRouter();
  const { logIn } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [failed, setFailed] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setSubmitting(true);
    try {
      const { token } = await api.logIn(email, password);
      logIn(token);
      router.push('/config/states');
    } catch {
      setEmail('');
      setPassword('');
      setFailed(true);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="card card--narrow">
      <h1>LogIn</h1>
      {failed && <p className="alert">Email or password is incorrect.</p>}
      <form onSubmit={handleSubmit} className="form">
        <label className="field">
          <span>Email</span>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="username"
          />
        </label>
        <label className="field">
          <span>Password</span>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="current-password"
          />
        </label>
        <div className="centered_div">
          <button type="submit" className="button" disabled={submitting}>
            LogIn
          </button>
        </div>
      </form>
    </section>
  );
}
