import Link from 'next/link';
import type { DenialReason } from '@/lib/api';

/** The wording for each denial reason returned by the API. */
const REASONS: Record<DenialReason, { label: string; detail: string }> = {
  State: { label: 'State', detail: 'Applications from the selected state are not allowed.' },
  Ssn: { label: 'SSN', detail: 'The SSN is on the blacklist.' },
  Unavailable: { label: 'Unavailable', detail: 'Could not be completed at this moment. try in a few minutes.' },
};

export default async function DeniedPage({ searchParams }: { searchParams: Promise<{ reason?: string }> }) {
  const { reason } = await searchParams;
  const denial =
    reason && Object.hasOwn(REASONS, reason)
      ? REASONS[reason as DenialReason]
      : { label: 'Unknown', detail: 'The application could not be approved.' };

  return (
    <section className="card card--denied">
      <h1>Application denied</h1>
      <p>
        <strong>Reason: {denial.label}.</strong> {denial.detail}
      </p>
      <Link href="/application" className="button button--secondary">
        Back to the application
      </Link>
    </section>
  );
}
