import { dbControl } from './dbControl.js';
import { userSelect, tableSelect } from './dbSqls.js';

const db = dbControl();
db.setRegion('eu-west-2');
db.setInstance('prod-mysql');

db.runSql(userSelect);
db.runSql(tableSelect);
