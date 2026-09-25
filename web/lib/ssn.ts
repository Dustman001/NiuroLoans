export const SSN_PATTERN = /^\d{3}-\d{2}-\d{4}$/;

/** Formats whatever was typed as XXX-XX-XXXX, keeping only the first 9 digits. */
export function formatSsn(input: string): string {
  const digits = input.replace(/\D/g, '').slice(0, 9);
  const parts = [digits.slice(0, 3), digits.slice(3, 5), digits.slice(5)].filter(Boolean);
  return parts.join('-');
}
