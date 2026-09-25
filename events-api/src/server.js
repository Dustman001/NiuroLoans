import { createApp } from './app.js';
import { createCustomerFileStore } from './customerFileStore.js';

const port = Number(process.env.PORT ?? 4000);
const dataDirectory = process.env.DATA_DIR ?? './data';

createApp(createCustomerFileStore(dataDirectory)).listen(port, () => {
  console.log(`Events API listening on port ${port}, writing files to ${dataDirectory}`);
});
