import { dbControl } from './dbControl.js';
import { createCollRequest, createUsers } from './databaseSqls.js';

const db = dbControl();
db.setRegion('eu-west-2');
db.setInstance('prod-mysql');

db.runSql(createUsers);
db.runSql(createCollRequest);
