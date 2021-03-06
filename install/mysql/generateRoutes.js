import { dbControl } from './dbControl.js';
import {
  getRandomInt, dataStore, createFutureDates, createRouteForDepotDrivers,
} from './dbUtils.js';

const store = dataStore();
const db = dbControl();
const db = dbControl();

const generateRouteSql = (routeList) => {
  let sqlText = '';

  if (routeList) {
    sqlText += 'insert into routes '
    + '(depotId, driverId, routeDate, addressId, addressPostCode,'
    + 'routeSeqNo, routeAction, itemType, status, refNo) values ';

    routeList.forEach((route) => {
      sqlText += `( ${route.depotId}, ${route.driverId}, '${route.routeDate}', ${route.addressId}, '${route.addressPostCode}', `;
      sqlText += `${route.routeSeqNo}, '${route.routeAction}', '${route.itemType}',`;
      sqlText += `'${route.status}', '${route.refNo}' ),`;
    });
  }

  sqlText = sqlText.slice(0, -1);
  sqlText += ';';

  return sqlText;
};

const getDepotList = async () => {
  await db.sql('select * from depots;', 'depots');
  return store.get('depots')[0];
};

const getAllocatedDrivers = async (depotId, noRoutesToday) => {
  await db.sql(`select * from drivers where depotId = ${depotId} order by rand() limit ${noRoutesToday};`,
    `allocatedDrivers-${depotId}`);
  console.log('------------------', depotId, noRoutesToday, '---------------');
  return store.get(`allocatedDrivers-${depotId}`)[0];
};

const sqlInsertRoutes = async (sqlText) => {
  await db.sql(sqlText);
};

const generateRouteData = () => new Promise((completed) => {
  (async () => {
    const depotList = await getDepotList();
    const datesList = await createFutureDates(7);

    depotList.forEach((depot) => {
      datesList.forEach((date) => {
        const noRoutesToday = depot.fleetSize - getRandomInt(0, 5);

        getAllocatedDrivers(depot.depotId, noRoutesToday)
          .then((allocatedDrivers) => {
            const routeList = createRouteForDepotDrivers(date, depot, allocatedDrivers);
            const sqlText = generateRouteSql(routeList);
            sqlInsertRoutes(sqlText).then(() => {
              completed(null);
            });
          });
      });
    });
  })();
});

(async () => {
  await db.connect({ store, region: 'eu-west-2', dbInstance: 'prod-mysql' });
  await db.sql('delete from routes;');
  await generateRouteData()
    .then(() => { db.close(); });
})();
