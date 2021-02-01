import { dbControl } from './dbControl.js';
import { getRandomInt, dataStore, createFakeUser } from './dbUtils.js';

export const createDepotsData = `

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

export const createAdminsData = `

alter table admins auto_increment=1;

insert into admins (adminName, userName, apiKey) 
values
('Sarah Conor', 'sarah.conor', 'p06189a0-g75a-4b35-881d-1c71b88a503b'),
('Mike Smith', 'mike.smith', '3c706694-0f70-4ce5-a5a7-74957c44f162');
`;

const createDriversData = (db, depotArray) => new Promise((completed) => {
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

  if (sqlDrivers) {
    db.sql(sqlDrivers)
      .then(() => { completed('completed: create random drivers'); });
  } else {
    completed('warning: no random drivers added.');
  }
});

export const createCoreData = () => new Promise((completed) => {
  (async () => {
    const store = dataStore();
    const db = dbControl();
    await db.connect({ store, region: 'eu-west-2', dbInstance: 'prod-mysql' });
    await db.sql('delete from admins');
    await db.sql(createAdminsData);
    await db.sql('delete from depots');
    await db.sql(createDepotsData);
    await db.sql('select * from depots', 'depots');


    await createDriversData(db, store.get('depots')[0])
      .then(() => {
        db.close();
        console.log('core data installation completed');
        completed(null);
      });
  })();
});

// createCoreData();
