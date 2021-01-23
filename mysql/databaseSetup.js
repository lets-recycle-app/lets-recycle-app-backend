import { asyncList } from './asyncList.js';
import { dbControl } from './dbControl.js';
import { createUsers, createCollRequest } from './databaseLegacyTables.js';
import {
  createRoutes, createDepots, createDrivers, createAdmins, createAddresses,
} from './databaseTables.js';

const a = asyncList();
const db = dbControl();

// set up the information required to
// obtained the correct connection keys
// from AWS Secrets.

a.add(db.connect, { region: 'eu-west-2', dbInstance: 'prod-mysql' });

// add sql statement text to a single threaded
// processing queue in the order of execution
// required.

a.add(db.sql, { sql: createUsers });
a.add(db.sql, { sql: createCollRequest });

a.add(db.sql, { sql: createRoutes });
a.add(db.sql, { sql: createDepots });
a.add(db.sql, { sql: createDrivers });
a.add(db.sql, { sql: createAdmins });
a.add(db.sql, { sql: createAddresses });

a.add(db.close);
a.run().then(() => {
  console.log('Installation completed.');
});
