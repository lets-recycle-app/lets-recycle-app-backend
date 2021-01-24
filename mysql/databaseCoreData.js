import { asyncList } from './asyncList.js';
import { dbControl } from './dbControl.js';
import { getRandomInt, createFakeUser } from './dbUtils.js';

const createDepotsData = `

alter table depots auto_increment=1;

insert into depots (depotName, postCode, fleetSize) 
values
('Horwich', 'HO1 8XJ', 26),
('Stockport', 'M18 7TQ', 13),
('Liverpool', 'L1 1DA', 15),
('Dumfirmline', 'KY11 3AE', 11),
('Watford', 'WD17 1AP', 20),
('Milton Keynes', 'MK10 1SZ', 23),
('Crewe', 'CW1 2BS', 14),
('Cardiff', 'CF10 1BE', 8),
('Southampton', 'SO14 0AH', 9),
('Wolverhampton', 'WV1 1HB', 16),
('Brighton', 'BN41 1HS', 6),
('Southend', 'SS2 4DY', 8),
('Middlesbrough', 'TS3 6AF', 23),
('Blackpool', 'FY3 8DF', 11),
('Derby', 'DE4 2HE', 25),
('Leeds', 'LS2 8JS', 18),
('Oldham', 'OL1 3EG', 5),
('Reading', 'RG1 3YL', 13),
('Blackburn', 'BB2 1NA', 19),
('York', 'YO10 3FQ', 9);
`;

const createDriversData = (data) => new Promise((completed) => {
  const depotArray = data.a.fetch('depots');

  // for each depot create a total of depot.fleetSize new drivers

  let sqlDrivers = '';
  if (depotArray) {
    sqlDrivers += 'delete from drivers;\n';
    sqlDrivers += 'alter table drivers auto_increment=1;\n';

    sqlDrivers += 'insert into drivers (driverName, depotId, truckSize, userName, apiKey) values';

    depotArray.forEach((depot) => {
      for (let driverNo = 1; driverNo <= depot.fleetSize; driverNo += 1) {
        const user = createFakeUser();
        sqlDrivers += `('${user.fullName}', ${depot.depotId}, ${getRandomInt(10, 32)}, '${user.userName}', '${user.apiKey}'),`;
      }
    });
    sqlDrivers = sqlDrivers.slice(0, -1);
    sqlDrivers += ';';
  }

  // run using the same database
  // connection as the main thread

  const a = asyncList(data.procOutput);

  if (sqlDrivers) {
    a.add(data.db.sql, { sql: sqlDrivers });
  }

  a.run()
    .then(() => { completed('completed: create random drivers'); });
});

export const createCoreData = () => new Promise((completed) => {
  const a = asyncList();
  const db = dbControl();
  a.add(db.connect, { region: 'eu-west-2', dbInstance: 'prod-mysql' });
  a.add(db.sql, { sql: 'delete from depots' });
  a.add(db.sql, { sql: createDepotsData });
  a.add(db.sql, { sql: 'select * from depots', id: 'depots' });
  a.add(createDriversData, { db, a });
  a.run()
    .then(() => { db.close(); completed('core data installation completed'); });
});

/*
createCoreData()
  .then(() => { console.log('core data installation completed'); });
*/
