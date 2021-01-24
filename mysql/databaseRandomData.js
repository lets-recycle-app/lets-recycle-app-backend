import faker from 'faker';
import { v4 as uuidv4 } from 'uuid';
import { dbControl } from './dbControl.js';
import { asyncList } from './asyncList.js';

export const getRandomInt = (min, max) => Math.floor(Math.random() * (max - min + 1)) + min;

const db = dbControl();
const a = asyncList();
faker.locale = 'en_GB';

const createFakeUser = () => {
  const userDetails = {
    firstName: faker.name.firstName(),
    lastName: faker.name.lastName(),
    address: faker.address.streetAddress(),
    address2: faker.address.secondaryAddress(),
    county: faker.address.county(),
    city: faker.address.city(),
    apiKey: uuidv4(),
    fullName: '',
    userName: '',
  };

  Object.keys(userDetails).forEach((key) => {
    // escape single quotes for mySql insert
    userDetails[key] = userDetails[key].replace(/'/gm, "''");
  });

  userDetails.fullName = `${userDetails.firstName} ${userDetails.lastName}`;
  userDetails.userName = `${userDetails.firstName.toLowerCase()}.${userDetails.lastName.toLowerCase()}`;

  return userDetails;
};

const createFutureDates = (data) => new Promise((completed) => {
  const noDaysForward = 1;

  const nextDay = new Date();
  const dateArray = [];

  for (let i = 1; i <= noDaysForward; i += 1) {
    nextDay.setDate(nextDay.getDate() + 1);
    dateArray.push(nextDay.toISOString().slice(0, 10));
  }

  data.procOutput.put('routeDates', dateArray);
  completed('completed: create future dates.');
});

const createDrivers = (data) => new Promise((completed) => {
  const depotArray = data.procOutput.fetch('depots');

  let sqlText = '';

  sqlText += 'delete from drivers;\n';
  sqlText += 'alter table drivers auto_increment=1;\n';

  sqlText += 'insert into drivers (driverName, depotId, truckSize, userName, apiKey) values';

  // for each depot create a total of depot.fleetSize new drivers

  depotArray.forEach((depot) => {
    for (let driverNo = 1; driverNo <= depot.fleetSize; driverNo += 1) {
      const user = createFakeUser();
      sqlText += `('${user.fullName}', ${depot.depotId}, ${getRandomInt(10, 32)}, '${user.userName}', '${user.apiKey}'),`;
    }
  });

  sqlText = sqlText.slice(0, -1);
  sqlText += ';';

  const r = asyncList(data.procOutput);
  r.add(db.sql, { sql: sqlText });
  r.run()
    .then(() => { completed('completed: create random drivers'); });
});

const createRoute = (data) => new Promise((completed) => {
  const depotArray = data.procOutput.fetch('depots');
  const dateArray = data.procOutput.fetch('routeDates');

  depotArray.map((depot) => {
    dateArray.map((date) => {
      // for each depo on each day we have a number of possible
      // routes determined by the max fleet size.
      const noRoutesToday = depot.fleetSize - getRandomInt(0, 5);
      const depotPostCode = depot.postCode;

      const randomDepotDrivers = `select * from drivers where depotId = ${depot.depotId} order by rand() limit ${noRoutesToday};`;

      // console.log(`${date} ${depot.depotName} ${depotPostCode} : ${noRoutesToday}/${depot.fleetSize}`);
      // console.log(randomDepotDrivers);

      const r = asyncList(data.procOutput);
      r.add(db.sql, { sql: randomDepotDrivers, id: 'routeDrivers' });
      r.run()
        .then(() => {
          const routeDriverArray = data.procOutput.fetch('routeDrivers');

          console.log(`YYY: ${date} ${depot.depotName} selected ${noRoutesToday}/${depot.fleetSize} random drivers`);
          let routeNo = 1;
          routeDriverArray.forEach((driver) => {
            console.log(`R ${routeNo} ${driver.depotId} ${driver.driverName}`);

            routeNo += 1;
          });
        })
        .then(() => { completed(`XXX: ${date} ${depot.depotName} select ${noRoutesToday}/${depot.fleetSize} random drivers`); });

      /*
      for (let routeNo = 1; routeNo <= noRoutesToday; routeNo += 1) {
        console.log(depot.depotName, '-', date, '{', depot.fleetSize, '}', routeNo);
      }

       */
    });
  });

  //completed('completed: create routes');
});

a.add(db.connect, { region: 'eu-west-2', dbInstance: 'prod-mysql' });
a.add(createFutureDates);
a.add(db.sql, { sql: 'select * from depots', id: 'depots' });

a.add(createDrivers);
a.add(createRoute);

a.run()
  .then(() => { db.close(); })
  .then(() => { console.log('completed: main async process list.'); });
