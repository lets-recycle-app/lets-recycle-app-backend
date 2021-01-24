import { dbControl } from './dbControl.js';
import { asyncList } from './asyncList.js';

const db = dbControl();
const a = asyncList();

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

  // completed('completed: create routes');
});

a.add(db.connect, { region: 'eu-west-2', dbInstance: 'prod-mysql' });
a.add(createFutureDates);
a.add(createRoute);

a.run()
  .then(() => { db.close(); })
  .then(() => { console.log('completed: main async process list.'); });
