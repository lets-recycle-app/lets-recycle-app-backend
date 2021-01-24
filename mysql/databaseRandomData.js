import faker from 'faker';
import { dbControl } from './dbControl.js';
import { asyncList } from './asyncList.js';

export const getRandomInt = (min, max) => Math.floor(Math.random() * (max - min + 1)) + min;

const db = dbControl();
const a = asyncList();
// faker.locale = 'en_GB';

const createRoute = (data) => new Promise((completed) => {
  completed('completed: create routes');
});

const createRandomDrivers = (data) => new Promise((completed) => {
  let sqlText = '';

  sqlText += 'delete from drivers;\n';
  sqlText += 'alter table drivers auto_increment=1;\n';

  const rowArray = data.procOutput.fetch('depots');

  rowArray.forEach((row) => {
    for (let i = 1; i <= getRandomInt(10, 20); i += 1) {
      const mockName = faker.name.findName();
      sqlText += 'insert into drivers (driverName, depotId, truckSize, userName) values';
      sqlText += `('${mockName}', ${row.depotId}, ${getRandomInt(10, 32)}, '${mockName}');`;
    }
  });

  sqlText = sqlText.slice(0, -2);
  sqlText += ';';

  const r = asyncList(data.procOutput);
  r.add(db.sql, { sql: sqlText });
  r.run()
    .then(() => { completed('completed: create random drivers'); });
});

a.add(db.connect, { region: 'eu-west-2', dbInstance: 'prod-mysql' });
a.add(db.sql, { sql: 'select * from depots', id: 'depots' });
a.add(createRandomDrivers);
a.add(createRoute);
a.run()
  .then(() => { db.close(); })
  .then(() => { console.log('completed: main async process list.'); });
