'use client';

import { useEffect, useState, type ChangeEvent, type FormEvent, type InputHTMLAttributes } from 'react';
import { useRouter } from 'next/navigation';
import { api, ApiError, type LoanApplicationForm, type State } from '@/lib/api';
import { formatSsn } from '@/lib/ssn';

const EMPTY_FORM: LoanApplicationForm = {
  firstName: '',
  lastName: '',
  email: '',
  addressLine1: '',
  addressLine2: '',
  stateId: '',
  zipCode: '',
  companyName: '',
  requestedAmount: '',
  ssn: '',
};

export default function NewApplicationPage() {
  const router = useRouter();
  const [states, setStates] = useState<State[]>([]);
  const [form, setForm] = useState(EMPTY_FORM);
  const [errors, setErrors] = useState<Record<string, string[]>>({});
  const [submitting, setSubmitting] = useState(false);
  const [approved, setApproved] = useState(false);
  const [failure, setFailure] = useState<string | null>(null);

  useEffect(() => {
    api
      .getStates()
      .then(setStates)
      .catch(() => setFailure('Could not load the list of states.'));
  }, []);

  function update(field: keyof LoanApplicationForm) {
    return (event: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
      const value = field === 'ssn' ? formatSsn(event.target.value) : event.target.value;
      setForm((current) => ({ ...current, [field]: value }));
    };
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setSubmitting(true);
    setErrors({});
    setFailure(null);
    try {
      const result = await api.submitApplication(form);
      if (result.approved) {
        setApproved(true);
        setForm(EMPTY_FORM);
      } else {
        router.push(`/denied?reason=${result.reason}`);
      }
    } catch (error) {
      if (error instanceof ApiError && error.status === 400) {
        setErrors(error.fieldErrors);
      } else {
        setFailure('Something went wrong. Please try again.');
      }
    } finally {
      setSubmitting(false);
    }
  }

  if (approved) {
    return (
      <section className="card">
        <h1>Application approved</h1>
        <p>Thank you. Your loan application was approved and saved.</p>
        <button type="button" className="button" onClick={() => setApproved(false)}>
          Start a new application
        </button>
      </section>
    );
  }

  const field = (name: keyof LoanApplicationForm, label: string, props: InputHTMLAttributes<HTMLInputElement> = {}) => (
    <label className="field">
      <span>{label}</span>
      <input name={name} value={form[name]} onChange={update(name)} aria-invalid={!!errors[name]} {...props} />
      {errors[name] && <small className="field__error">{errors[name][0]}</small>}
    </label>
  );

  return (
    <section className="card">
      <div className="centered_div"><h1>Loan Application</h1></div>
      {failure && <p className="alert">{failure}</p>}
      <form onSubmit={handleSubmit} className="form" noValidate>
        <div className="form__row">
          {field('firstName', 'First name', { required: true, autoComplete: 'given-name' })}
          {field('lastName', 'Last name', { required: true, autoComplete: 'family-name' })}
        </div>
        {field('ssn', 'SSN', {
          placeholder: 'XXX-XX-XXXX',
          inputMode: 'numeric',
          maxLength: 11,
          autoComplete: 'off',
        })}
        {field('email', 'Email', { type: 'email', required: true, autoComplete: 'email' })}

        <fieldset className="form__group">
          <legend>Address</legend>
          {field('addressLine1', 'Street 1', { required: true, autoComplete: 'address-line1' })}
          {field('addressLine2', 'Street 2', { autoComplete: 'address-line2' })}
          <div className="form__row">
            <label className="field">
              <span>State</span>
              <select name="stateId" value={form.stateId} onChange={update('stateId')} aria-invalid={!!errors.stateId}>
                <option value="">Select a state</option>
                {states.map((state) => (
                  <option key={state.id} value={state.id}>
                    {state.name}
                  </option>
                ))}
              </select>
              {errors.stateId && <small className="field__error">{errors.stateId[0]}</small>}
            </label>
            {field('zipCode', 'Zip Code', { inputMode: 'numeric', maxLength: 5, autoComplete: 'postal-code' })}
          </div>
        </fieldset>

        <div className="form__row">
          {field('companyName', 'Company name', { required: true, autoComplete: 'organization' })}
          {field('requestedAmount', 'Requested amount (USD)', { type: 'number', min: 1, step: '0.01' })}
        </div>
        <div className="centered_div">
          <button type="submit" className="button" disabled={submitting}>
            {submitting ? 'Submitting…' : 'Submit application'}
          </button>
        </div>
      </form>
    </section>
  );
}
