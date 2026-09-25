const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export type State = { id: number; name: string; abbreviation: string; isNotAllowed: boolean };

export type DenialReason = 'State' | 'Ssn' | 'Unavailable';

export type LoanApplicationResponse = { approved: boolean; reason: DenialReason | null };

export type LoanApplicationForm = {
  firstName: string;
  lastName: string;
  email: string;
  addressLine1: string;
  addressLine2: string;
  stateId: string;
  zipCode: string;
  companyName: string;
  requestedAmount: string;
  ssn: string;
};

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly fieldErrors: Record<string, string[]> = {},
  ) {
    super(`Request failed with status ${status}`);
  }
}

async function request<T>(
  path: string,
  options: { method?: string; body?: unknown; token?: string | null } = {},
): Promise<T> {
  const { method = 'GET', body, token } = options;
  const headers: Record<string, string> = {};
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  if (token) headers.Authorization = `Bearer ${token}`;

  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  const isJson = response.headers.get('content-type')?.includes('json') ?? false;
  const json = isJson ? await response.json() : undefined;
  if (!response.ok) {
    throw new ApiError(response.status, json?.errors ?? {});
  }
  return json as T;
}

export const api = {
  logIn: (email: string, password: string) =>
    request<{ token: string }>('/api/auth/login', { method: 'POST', body: { email, password } }),

  getStates: () => request<State[]>('/api/states'),

  setStateNotAllowed: (token: string, stateId: number, isNotAllowed: boolean) =>
    request<void>(`/api/states/${stateId}`, { method: 'PUT', body: { isNotAllowed }, token }),

  getBlacklist: async (token: string) =>
    (await request<{ ssn: string }[]>('/api/blacklist', { token })).map((entry) => entry.ssn),

  addToBlacklist: (token: string, ssn: string) =>
    request<{ ssn: string }>('/api/blacklist', { method: 'POST', body: { ssn }, token }),

  removeFromBlacklist: (token: string, ssn: string) =>
    request<void>(`/api/blacklist/${ssn}`, { method: 'DELETE', token }),

  submitApplication: (form: LoanApplicationForm) =>
    request<LoanApplicationResponse>('/api/applications', {
      method: 'POST',
      body: {
        ...form,
        addressLine2: form.addressLine2 || null,
        stateId: form.stateId ? Number(form.stateId) : null,
        requestedAmount: form.requestedAmount ? Number(form.requestedAmount) : null,
      },
    }),
};
