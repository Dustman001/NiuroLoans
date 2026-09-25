import { test, beforeEach, afterEach } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, readFile, rm, writeFile, mkdir } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import request from 'supertest';
import { createApp } from '../src/app.js';
import { createCustomerFileStore } from '../src/customerFileStore.js';

let directory;
let store;
let app;

beforeEach(async () => {
  directory = await mkdtemp(path.join(tmpdir(), 'events-'));
  store = createCustomerFileStore(directory);
  app = createApp(store);
});

afterEach(() => rm(directory, { recursive: true, force: true }));

const event = (type, extra = {}) => ({
  type,
  ssn: '123456789',
  requestedAmount: 1000,
  ...extra,
});
const readStored = async () => JSON.parse(await readFile(store.fileFor('123456789'), 'utf8'));

test('NewCustomer creates a file named by SSN', async () => {
  await request(app).post('/events').send(event('NewCustomer')).expect(204);

  assert.equal((await readStored()).type, 'NewCustomer');
});

test('ReturningCustomer replaces the existing file', async () => {
  await mkdir(directory, { recursive: true });
  await writeFile(store.fileFor('123456789'), JSON.stringify({ old: true }));

  await request(app)
    .post('/events')
    .send(event('ReturningCustomer', { requestedAmount: 5000 }))
    .expect(204);

  const stored = await readStored();
  assert.equal(stored.old, undefined);
  assert.equal(stored.requestedAmount, 5000);
});

test('ReturningCustomer creates the file when none exists', async () => {
  await request(app).post('/events').send(event('ReturningCustomer')).expect(204);

  assert.equal((await readStored()).type, 'ReturningCustomer');
});

test('unknown event type is rejected with 400', async () => {
  await request(app).post('/events').send(event('Other')).expect(400);
});

test('malformed JSON is rejected with 400', async () => {
  await request(app).post('/events').set('Content-Type', 'application/json').send('{"type":').expect(400);
});

test('inherited property names are not event types', async () => {
  await request(app).post('/events').send(event('toString')).expect(400);
});

test('invalid SSN is rejected with 400', async () => {
  await request(app)
    .post('/events')
    .send(event('NewCustomer', { ssn: '../etc/passwd' }))
    .expect(400);
});
