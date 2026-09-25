import { expect, test } from '@playwright/test';
import {
  BLACKLISTED_SSN,
  SEEDED_RETURNING_SSN,
  eventsDataAvailable,
  randomSsn,
  readCustomerFile,
  submitApplication,
} from './helpers';

test.describe('New loan application', () => {
  test('first load redirects to the application page', async ({ page }) => {
    await page.goto('/');

    await expect(page).toHaveURL(/\/application$/);
    await expect(page.getByRole('link', { name: 'LogIn' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'New Loan Application' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'State' })).toHaveCount(0);
  });

  test('SSN input is masked as XXX-XX-XXXX', async ({ page }) => {
    await page.goto('/application');
    await page.getByLabel('SSN').pressSequentially('12345678999');

    await expect(page.getByLabel('SSN')).toHaveValue('123-45-6789');
  });

  test('empty submit shows a validation message per field', async ({ page }) => {
    await page.goto('/application');
    await page.getByRole('button', { name: 'Submit application' }).click();

    await expect(page.getByText('SSN must have the format XXX-XX-XXXX.')).toBeVisible();
    await expect(page.getByText('Select a state.')).toBeVisible();
    await expect(page.getByText('Enter a valid email address.')).toBeVisible();
    await expect(page).toHaveURL(/\/application$/);
  });

  test('new customer is approved and a NewCustomer file is written', async ({ page }) => {
    const ssn = randomSsn();

    await submitApplication(page, { ssn });

    await expect(page.getByRole('heading', { name: 'Application approved' })).toBeVisible();
    test.skip(!eventsDataAvailable(), 'events-api/data is not reachable from the test runner');
    expect(readCustomerFile(ssn)).toMatchObject({ type: 'NewCustomer', ssn: ssn.replace(/-/g, '') });
  });

  test('same SSN again is a returning customer and the file is replaced', async ({ page }) => {
    const ssn = randomSsn();
    await submitApplication(page, { ssn, amount: '1000' });
    await expect(page.getByRole('heading', { name: 'Application approved' })).toBeVisible();

    await submitApplication(page, { ssn, amount: '9000', firstName: 'Ana-Updated' });

    await expect(page.getByRole('heading', { name: 'Application approved' })).toBeVisible();
    test.skip(!eventsDataAvailable(), 'events-api/data is not reachable from the test runner');
    expect(readCustomerFile(ssn)).toMatchObject({
      type: 'ReturningCustomer',
      requestedAmount: 9000,
      firstName: 'Ana-Updated',
    });
  });

  test('seeded customer is treated as returning', async ({ page }) => {
    await submitApplication(page, { ssn: SEEDED_RETURNING_SSN, amount: '12000' });

    await expect(page.getByRole('heading', { name: 'Application approved' })).toBeVisible();
    test.skip(!eventsDataAvailable(), 'events-api/data is not reachable from the test runner');
    expect(readCustomerFile(SEEDED_RETURNING_SSN)).toMatchObject({ type: 'ReturningCustomer' });
  });

  test('New York is denied with the State reason', async ({ page }) => {
    await submitApplication(page, { ssn: randomSsn(), state: 'New York' });

    await expect(page).toHaveURL(/\/denied\?reason=State/);
    await expect(page.getByText('Reason: State.')).toBeVisible();
  });

  test('blacklisted SSN is denied with the SSN reason', async ({ page }) => {
    await submitApplication(page, { ssn: BLACKLISTED_SSN });

    await expect(page).toHaveURL(/\/denied\?reason=Ssn/);
    await expect(page.getByText('Reason: SSN.')).toBeVisible();
  });
});
