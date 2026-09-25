'use client';

import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react';
import { api } from '@/lib/api';
import { useAuth } from '@/lib/auth';
import { formatSsn, SSN_PATTERN } from '@/lib/ssn';
import { useSessionExpiry } from '@/lib/useSessionExpiry';

export default function BlacklistPage() {
  const { token } = useAuth();
  const handleExpiry = useSessionExpiry();
  const [blacklist, setBlacklist] = useState<string[]>([]);
  const [ssn, setSsn] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    api
      .getBlacklist(token!)
      .then(setBlacklist)
      .catch((err) => {
        if (!handleExpiry(err)) setError('Could not load the blacklist.');
      });
  }, [token, handleExpiry]);

  const matches = ssn ? blacklist.filter((entry) => entry.startsWith(ssn)) : blacklist;
  const action = ssn && matches.length > 0 ? 'Remove' : 'Add';
  const isComplete = SSN_PATTERN.test(ssn);

  function handleSelect(event: ChangeEvent<HTMLSelectElement>) {
    const selected = Array.from(event.target.selectedOptions, (option) => option.value);
    if (selected.length === 1) setSsn(selected[0]);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!isComplete) return;
    setBusy(true);
    setError(null);
    try {
      if (action === 'Add') {
        await api.addToBlacklist(token!, ssn);
        setBlacklist((current) => [...current, ssn].sort());
      } else {
        await api.removeFromBlacklist(token!, ssn);
        setBlacklist((current) => current.filter((entry) => entry !== ssn));
      }
      setSsn('');
    } catch (err) {
      if (!handleExpiry(err)) setError(`Could not ${action.toLowerCase()} the SSN. Please try again.`);
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="card">
      <h1>SSN BlackList</h1>
      <p>Add or Remove the SSN from the blacklist:</p>
      {error && <p className="alert">{error}</p>}
      <select multiple size={10} className="ssn-list" aria-label="Blacklisted SSNs" onChange={handleSelect}>
        {matches.map((entry) => (
          <option key={entry} value={entry}>
            {entry}
          </option>
        ))}
      </select>
      <form onSubmit={handleSubmit} className="ssn-form">
        <input
          className="ssn-input"
          value={ssn}
          onChange={(e) => setSsn(formatSsn(e.target.value))}
          placeholder="XXX-XX-XXXX"
          inputMode="numeric"
          maxLength={11}
          aria-label="SSN"
        />
        <button type="submit" className="button" disabled={!isComplete || busy}>
          {action}
        </button>
      </form>
    </section>
  );
}
