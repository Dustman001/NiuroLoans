import { expect, test, type Page } from '@playwright/test';
import { ADMIN, logIn, randomSsn, submitApplication } from './helpers';

/** Ticks or unticks a state on the State page and waits until the API has saved it. */
async function setNotAllowed(page: Page, label: string, notAllowed: boolean) {
  const checkbox = page.getByLabel(label);
  await expect(checkbox).toBeVisible();
  if ((await checkbox.isChecked()) === notAllowed) return;

  const saved = page.waitForResponse((r) => r.url().includes('/api/states/') && r.request().method() === 'PUT');
  await checkbox.setChecked(notAllowed);
  expect((await saved).status()).toBe(204);
}

test.describe('Login', () => {
  test('wrong password clears the form and shows an error', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Email').fill(ADMIN.email);
    await page.getByLabel('Password').fill('wrong-password');
    await page.getByRole('button', { name: 'LogIn' }).click();

    await expect(page.getByText('Email or password is incorrect.')).toBeVisible();
    await expect(page.getByLabel('Email')).toHaveValue('');
    await expect(page.getByLabel('Password')).toHaveValue('');
  });

  test('valid login swaps the menu and opens the State page', async ({ page }) => {
    await logIn(page);

    const nav = page.getByRole('navigation', { name: 'Main' });
    await expect(nav.getByText('Configuration')).toBeVisible();
    await expect(nav.getByRole('button', { name: 'LogOut' })).toBeVisible();
    await expect(nav.getByRole('link', { name: 'LogIn' })).toHaveCount(0);
    await expect(nav.getByRole('link', { name: 'New Loan Application' })).toHaveCount(0);
  });

  test('configuration pages redirect to Application when logged out', async ({ page }) => {
    await page.goto('/config/blacklist');

    await expect(page).toHaveURL(/\/application$/);
  });

  test('logout restores the public menu', async ({ page }) => {
    await logIn(page);
    await page.getByRole('button', { name: 'LogOut' }).click();

    await expect(page.getByRole('link', { name: 'LogIn' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'New Loan Application' })).toBeVisible();
  });
});

test.describe('Configuration: State', () => {
  test('lists states in 4 columns with New York not allowed', async ({ page }) => {
    await logIn(page);

    await expect(page.getByText('Select the states that are not allowed:')).toBeVisible();
    await expect(page.getByLabel('NY - New York')).toBeChecked();
    await expect(page.getByLabel('CA - California')).not.toBeChecked();
    const columns = await page
      .locator('.state-grid')
      .evaluate((el) => getComputedStyle(el).gridTemplateColumns.split(' ').length);
    expect(columns).toBe(4);
  });

  test('ticking a state saves it and denies applications from it', async ({ page }) => {
    await logIn(page);
    // Start from a known state: a run that timed out earlier may have left Wyoming ticked.
    await setNotAllowed(page, 'WY - Wyoming', false);

    try {
      await setNotAllowed(page, 'WY - Wyoming', true);
      await page.reload();
      await expect(page.getByLabel('WY - Wyoming')).toBeChecked();

      await submitApplication(page, { ssn: randomSsn(), state: 'Wyoming' });
      await expect(page).toHaveURL(/\/denied\?reason=State/);
    } finally {
      await page.goto('/config/states');
      await setNotAllowed(page, 'WY - Wyoming', false);
    }
  });
});

test.describe('Configuration: SSN BlackList', () => {
  test('filters the list and switches the button between Add and Remove', async ({ page }) => {
    await logIn(page);
    await page.getByRole('link', { name: 'SSN BlackList' }).click();

    await expect(page.getByText('Add or Remove the SSN from the blacklist:')).toBeVisible();
    const list = page.getByLabel('Blacklisted SSNs');
    const input = page.getByLabel('SSN', { exact: true });
    await expect(list).toHaveAttribute('size', '10');
    await expect(list.locator('option')).toContainText(['111-11-1111', '222-22-2222']);

    await input.pressSequentially('1');
    await expect(list.locator('option')).toHaveText(['111-11-1111']);
    await expect(page.getByRole('button', { name: 'Remove' })).toBeDisabled();

    await input.fill('');
    await input.pressSequentially('888');
    await expect(list.locator('option')).toHaveCount(0);
    await expect(page.getByRole('button', { name: 'Add' })).toBeVisible();
  });

  test('adds an SSN, blocks applications with it, then removes it', async ({ page }) => {
    const ssn = randomSsn();
    const digits = ssn.replace(/-/g, '');
    await logIn(page);
    await page.getByRole('link', { name: 'SSN BlackList' }).click();
    const list = page.getByLabel('Blacklisted SSNs');
    const input = page.getByLabel('SSN', { exact: true });

    await input.pressSequentially(digits);
    await page.getByRole('button', { name: 'Add' }).click();
    try {
      await expect(input).toHaveValue('');
      await expect(list.locator('option', { hasText: ssn })).toHaveCount(1);
      await expect(page.locator('.alert')).toHaveCount(0);

      await submitApplication(page, { ssn });
      await expect(page).toHaveURL(/\/denied\?reason=Ssn/);
    } finally {
      await page.goto('/config/blacklist');
      await input.pressSequentially(digits);
      await page.getByRole('button', { name: 'Remove' }).click();
      await expect(input).toHaveValue('');
      await expect(list.locator('option', { hasText: ssn })).toHaveCount(0);
    }
  });
});
