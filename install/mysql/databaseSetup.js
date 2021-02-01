import { dbControl } from './dbControl.js';
import { dataStore } from './dbUtils.js';

import {
  createRoutes, createDepots, createDrivers, createAdmins, createAddresses, createPostCodes,
} from './databaseTables.js';
import { createCoreData } from './databaseCoreData.js';

const store = dataStore();
const db = dbControl();

// add sql statement text to an asynchronous
// processing queue in the order of execution
// required.

(async () => {
  await db.connect({ store, region: 'eu-west-2', dbInstance: 'prod-mysql' });
  await db.sql(createRoutes);
  await db.sql(createDepots);
  await db.sql(createAdmins);
  await db.sql(createDrivers);
  await db.sql(createAddresses);
  await db.sql(createPostCodes);

  await createCoreData()
    .then(() => {
      db.close();
      console.log('full database installation completed.');
    });
})();
