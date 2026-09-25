import express from 'express';

const SSN_PATTERN = /^\d{9}$/;

/**
 * POST /events receives customer events from the loans API.
 *   NewCustomer       -> create the file
 *   ReturningCustomer -> delete the old file if it exists, then create it again
 * Any non-2xx tells the loans API to roll back its transaction.
 */
export function createApp(store) {
  const app = express();
  app.use(express.json());

  const handlers = {
    NewCustomer: store.create,
    ReturningCustomer: store.replace,
  };

  app.post('/events', async (req, res) => {
    const event = req.body ?? {};
    const handle = Object.hasOwn(handlers, event.type ?? '') ? handlers[event.type] : undefined;
    if (!handle) {
      return res.status(400).json({ error: `Unknown event type: ${event.type}` });
    }
    if (!SSN_PATTERN.test(event.ssn ?? '')) {
      return res.status(400).json({ error: 'ssn must be 9 digits' });
    }

    await handle(event.ssn, event);
    res.status(204).end();
  });

  app.get('/health', (_req, res) => res.json({ status: 'ok' }));

  app.use((err, _req, res, _next) => {
    // Body-parser errors (e.g. malformed JSON) carry their own 4xx status.
    if (err.status >= 400 && err.status < 500) {
      return res.status(err.status).json({ error: 'Invalid request body' });
    }
    console.error(err);
    res.status(500).json({ error: 'Could not store the event' });
  });

  return app;
}
