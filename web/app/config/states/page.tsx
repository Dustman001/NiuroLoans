'use client';

import { useEffect, useState } from 'react';
import { api, type State } from '@/lib/api';
import { useAuth } from '@/lib/auth';
import { useSessionExpiry } from '@/lib/useSessionExpiry';

export default function StatesPage() {
  const { token } = useAuth();
  const handleExpiry = useSessionExpiry();
  const [states, setStates] = useState<State[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .getStates()
      .then(setStates)
      .catch(() => setError('Could not load the states.'));
  }, []);

  function setNotAllowed(stateId: number, isNotAllowed: boolean) {
    setStates((current) => current.map((s) => (s.id === stateId ? { ...s, isNotAllowed } : s)));
    setError(null);
    api.setStateNotAllowed(token!, stateId, isNotAllowed).catch((err) => {
      if (handleExpiry(err)) return;
      setStates((current) => current.map((s) => (s.id === stateId ? { ...s, isNotAllowed: !isNotAllowed } : s)));
      setError('The change could not be saved. Please try again.');
    });
  }

  return (
    <section className="card">
      <h1>State</h1>
      <p>Select the states that are not allowed:</p>
      {error && <p className="alert">{error}</p>}
      <div className="state-grid">
        {states.map((state) => (
          <label key={state.id} className="state-grid__item">
            <input
              type="checkbox"
              checked={state.isNotAllowed}
              onChange={(e) => setNotAllowed(state.id, e.target.checked)}
            />
            {state.abbreviation} - {state.name}
          </label>
        ))}
      </div>
    </section>
  );
}
