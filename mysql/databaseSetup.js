import { dbControl } from './dbControl.js';
import { createUsers, createCollRequest, createDepots, createAll } from './databaseSqls.js';

const db = dbControl();
db.setRegion('eu-west-2');
db.setInstance('prod-mysql');

db.runSql(createUsers);
db.runSql(createCollRequest);
db.runSql(createDepots);
db.runSql(createAll);
