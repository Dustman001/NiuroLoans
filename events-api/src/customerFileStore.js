import { mkdir, rm, writeFile } from 'node:fs/promises';
import path from 'node:path';

/** Stores one JSON file per customer, named by SSN (e.g. data/123456789.json). */
export function createCustomerFileStore(directory) {
  const fileFor = (ssn) => path.join(directory, `${ssn}.json`);

  async function create(ssn, payload) {
    await mkdir(directory, { recursive: true });
    await writeFile(fileFor(ssn), JSON.stringify(payload, null, 2));
  }

  async function replace(ssn, payload) {
    await rm(fileFor(ssn), { force: true });
    await create(ssn, payload);
  }

  return { create, replace, fileFor };
}
