import { dbControl } from './dbControl.js';
import { createUsers, createCollRequest } from './databaseLegacyTables.js';
import {
  createRoutes, createDepots, createDrivers, createAdmins, createAddresses,
} from './databaseTables.js';

// set up the information required to
// obtained the correct connection keys
// from AWS Secrets.

const db = dbControl();
db.setRegion('eu-west-2');
db.setInstance('prod-mysql');

// add sql statement text to a single threaded
// processing queue in the order of execution
// required.

db.addQueue(createUsers);
db.addQueue(createCollRequest);

db.addQueue(createRoutes);
db.addQueue(createDepots);
db.addQueue(createDrivers);
db.addQueue(createAdmins);
db.addQueue(createAddresses);

db.processQueue().then(() => {
  console.log('Installation completed.');
});
