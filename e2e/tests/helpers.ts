import { expect, type Page } from '@playwright/test';
import { existsSync, readFileSync } from 'node:fs';
import path from 'node:path';

export const ADMIN = { email: 'admin@niuro.test', password: 'Admin123!' };
export const BLACKLISTED_SSN = '111-11-1111';
export const SEEDED_RETURNING_SSN = '333-33-3333';

/** Where API 2 writes its files (the docker-compose bind mount). */
const EVENTS_DATA_DIR = process.env.EVENTS_DATA_DIR ?? path.resolve('..', 'events-api', 'data');

/** A random SSN in the 9xx range, which the seed data never uses. */
export function randomSsn(): string {
  const digits = String(Math.floor(Math.random() * 1e8)).padStart(8, '0');
  return `9${digits.slice(0, 2)}-${digits.slice(2, 4)}-${digits.slice(4, 8)}`;
}

type Applicant = {
  ssn: string;
  state?: string;
  firstName?: string;
  amount?: string;
};

export async function submitApplication(
  page: Page,
  { ssn, state = 'California', firstName = 'Ana', amount = '2500' }: Applicant,
) {
  await page.goto('/application');
  await page.getByLabel('First name').fill(firstName);
  await page.getByLabel('Last name').fill('Lopez');
  await page.getByLabel('Email').fill('ana@example.com');
  await page.getByLabel('Street 1').fill('1 Market St');
  // The select's accessible name includes its selected option ("State Select a state"), so match the prefix.
  await page.getByRole('combobox', { name: /^State/ }).selectOption({ label: state });
  await page.getByLabel('Zip Code').fill('94105');
  await page.getByLabel('Company name').fill('Acme');
  await page.getByLabel('Requested amount (USD)').fill(amount);
  await page.getByLabel('SSN').pressSequentially(ssn.replace(/-/g, ''));
  await page.getByRole('button', { name: 'Submit application' }).click();
}

export async function logIn(page: Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(ADMIN.email);
  await page.getByLabel('Password').fill(ADMIN.password);
  await page.getByRole('button', { name: 'LogIn' }).click();
  await expect(page).toHaveURL(/\/config\/states$/);
}

/** Returns the JSON file API 2 wrote for this SSN, or null when the data folder is not reachable. */
export function readCustomerFile(ssn: string): Record<string, unknown> | null {
  if (!existsSync(EVENTS_DATA_DIR)) return null;
  const file = path.join(EVENTS_DATA_DIR, `${ssn.replace(/-/g, '')}.json`);
  return existsSync(file) ? JSON.parse(readFileSync(file, 'utf8')) : null;
}

export function eventsDataAvailable(): boolean {
  return existsSync(EVENTS_DATA_DIR);
}
