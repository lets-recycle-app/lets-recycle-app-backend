import { dbControl } from './dbControl.js';
import { asyncList } from './asyncList.js';
import {
  getRandomInt, createFutureDates, createDriverDeliveryRoute,
} from './dbUtils.js';

const db = dbControl();

const createDepotDriverRoutes = (data) => new Promise((completed) => {
  const allocatedDrivers = data.r.fetch('allocatedDrivers');
  const { date, depot } = data;

  let sqlRoute = '';

  if (allocatedDrivers) {
    allocatedDrivers.forEach((driver) => {
      // for the current day, depot-driver generate a random route
      const routeList = createDriverDeliveryRoute(date, depot, driver);

      // now create sql to insert this drivers route

      if (routeList) {
        sqlRoute += 'insert into routes '
          + '(depotId, driverId, routeDate, addressId, '
          + 'routeSeqNo, routeAction, itemType, status, refNo) values ';

        routeList.forEach((route) => {
          sqlRoute += `( ${route.depotId}, ${route.driverId}, '${route.routeDate}',  ${route.addressId},`;
          sqlRoute += `${route.routeSeqNo}, '${route.routeAction}', '${route.itemType}',`;
          sqlRoute += `'${route.status}', '${route.refNo}' ),`;
        });

        sqlRoute = sqlRoute.slice(0, -1);
        sqlRoute += ';';
      }
    });
  }

  // run using the same database
  // connection as the main thread

  const z = asyncList();

  if (sqlRoute) {
    z.add(db.sql, { sql: sqlRoute });
  }

  z.run()
    .then(() => { completed('completed:driver routes allocated'); });
});

const generateRouteData = (data) => new Promise((completed) => {
  const dateArray = createFutureDates(2);
  const depotArray = data.a.fetch('depots');

  depotArray.forEach((depot) => {
    dateArray.forEach((date) => {
      // for each depot on each day we have a number of possible
      // routes up to a maximum of the depot fleet size.

      const noRoutesToday = depot.fleetSize - getRandomInt(0, 5);

      // randomly select drivers from the depot fleet to fill the current day's delivery routes
      const randomDepotDrivers = `select * from drivers where depotId = ${depot.depotId} order by rand() limit ${noRoutesToday};`;

      const r = asyncList(data.procOutput);
      r.add(db.sql, { sql: 'delete from routes' });
      r.add(db.sql, { sql: randomDepotDrivers, id: 'allocatedDrivers' });
      // generate routes for this date and depot for every driver selected for delivery duties
      r.add(createDepotDriverRoutes, {
        db, r, date, depot,
      });

      r.run()
        .then(() => { completed('completed: delivery routes all allocated.'); });
    });
  });
});

const a = asyncList();
a.add(db.connect, { region: 'eu-west-2', dbInstance: 'prod-mysql' });
a.add(db.sql, { sql: 'select * from depots', id: 'depots' });
a.add(generateRouteData, { db, a });
a.run()
  //.then(() => { db.close(); console.log('route data generation completed'); });
  .then(() => { console.log('route data generation completed'); });
