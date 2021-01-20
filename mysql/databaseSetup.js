import { dbControl } from './dbControl.js';
import { createUsers, createCollRequest } from './databaseLegacyTables.js';
import {
  createRoutes, createDepots, createDrivers, createAdmins, createAddresses,
} from './databaseTables.js';

const db = dbControl();
db.setRegion('eu-west-2');
db.setInstance('prod-mysql');

db.runSql(createUsers);
db.runSql(createCollRequest);

db.runSql(createRoutes);
db.runSql(createDepots);
db.runSql(createDrivers);
db.runSql(createAdmins);
db.runSql(createAddresses);
