import { dbControl } from './dbControl.js';
import { asyncStore } from './asyncList.js';

import { createUsers, createCollRequest } from './databaseLegacyTables.js';
import {
  createRoutes, createDepots, createDrivers, createAdmins, createAddresses,
} from './databaseTables.js';
import { createCoreData } from './databaseCoreData.js';

const store = asyncStore();
const db = dbControl();

// add sql statement text to an asynchronous
// processing queue in the order of execution
// required.

(async () => {
  await db.connect({ store, region: 'eu-west-2', dbInstance: 'prod-mysql' });
  await db.sql(createUsers);
  await db.sql(createCollRequest);
  await db.sql(createRoutes);
  await db.sql(createDepots);
  await db.sql(createDrivers);
  await db.sql(createAdmins);
  await db.sql(createAddresses);

  await createCoreData()
    .then(() => {
      db.close();
      console.log('full database installation completed.');
    });
})();
