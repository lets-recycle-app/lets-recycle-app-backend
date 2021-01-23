import { dbControl } from './dbControl.js';
import { asyncList } from './asyncList.js';

export const getRandomInt = (min, max) => Math.floor(Math.random() * (max - min + 1)) + min;

export const createDrivers = (db) => {
  let sqlText = '';

  sqlText += 'delete from drivers;';
  sqlText += 'alter table drivers auto_increment=1;';

  // fetch depots
  console.log('In create drivers', db);
  const depotData = db.fetchResults('depotId');

  for (let i = 1; i <= getRandomInt(20, 40); i += 1) {
    depotData.forEach((row) => {
      sqlText += `
      insert into drivers (driverName, depotId, truckSize, userName)
      values
      ('Andy', ${row.depotId}, ${getRandomInt(10, 32)}, 'andy1111')
    `;
    });
  }

  console.log('-------------');
  console.log(sqlText);
  console.log('-------------');
  return sqlText;
};

const db = dbControl();
const a = asyncList();

const createRandomDrivers = (data) => new Promise((completed) => {
  let sqlText = '';

  sqlText += 'delete from drivers;\n';
  sqlText += 'alter table drivers auto_increment=1;\n';
  sqlText += 'insert into drivers (driverName, depotId, truckSize, userName) values\n';

  const rowArray = data.procOutput.fetch('depots');

  rowArray.forEach((row) => {
    for (let i = 1; i <= getRandomInt(10, 21); i += 1) {
      sqlText += `('driverName', ${row.depotId}, ${getRandomInt(10, 32)}, 'driverUserName'),\n`;
    }
  });

  sqlText = sqlText.slice(0, -2);
  sqlText += ';';

  console.log(sqlText);
  const r = asyncList();
  r.add(db.sql, { sql: sqlText });
  r.run().then(() => {
    console.log('Random drivers created.');
    completed(null);
  });
});

a.add(db.connect, { region: 'eu-west-2', dbInstance: 'prod-mysql' });
a.add(db.sql, { sql: 'select * from depots', id: 'depots' });
a.add(createRandomDrivers);
a.add(db.close);
a.run().then(() => {
  console.log('All random data creation completed.');
});
