import faker from 'faker';
import { v4 as uuidv4 } from 'uuid';

export const getRandomInt = (min, max) => Math.floor(Math.random() * (max - min + 1)) + min;

export const createFakeUser = () => {
  faker.locale = 'en_GB';

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

export const showTable = (rowData) => {
  if (rowData.length > 0) {
    let screenOutput = '';
    rowData.forEach((row) => {
      // get string value of the last key in this data row

      const lastKey = (Object.keys(row).slice(-1)).toString();

      screenOutput += '{';

      Object.keys(row).forEach((key) => {
        screenOutput += ` ${row[key]}`;

        if (key !== lastKey) {
          // do not add comma on the last key
          screenOutput += ' ,';
        }
      });
      screenOutput += ' }\n';
    });

    console.log(screenOutput);
  }
};

export const createFutureDates = async (noDaysForward = 7) => {
  const nextDay = new Date();
  const dateArray = [];

  for (let i = 1; i <= noDaysForward; i += 1) {
    nextDay.setDate(nextDay.getDate() + 1);
    dateArray.push(nextDay.toISOString().slice(0, 10));
  }

  return dateArray;
};

export const createRouteForDepotDrivers = (date, depot, driverList) => {
  const routeList = [];
  const noStops = getRandomInt(15, 27);

  driverList.forEach((singleDriver) => {
    for (let seqNo = 1; seqNo <= noStops; seqNo += 1) {
      const routeDetails = {
        routeDate: date,
        depotName: depot.depotName,
        depotPostCode: depot.postCode,
        depotId: depot.depotId,
        driverName: singleDriver.driverName,
        driverId: singleDriver.driverId,
        addressId: 222,
        routeSeqNo: seqNo,
        routeAction: 'delivery',
        itemType: 'fridge',
        status: 'pending',
        refNo: 'AHJSK123',
      };

      routeList.push(routeDetails);
    }
  });

  return routeList;
};
