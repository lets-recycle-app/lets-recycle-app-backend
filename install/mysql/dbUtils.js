import faker from 'faker';
import { v4 as uuidv4 } from 'uuid';
import postcodes from 'node-postcodes.io';

export const round = (value, decimals = 0) => Number(`${Math.round(Number(`${value}e${decimals}`))}e-${decimals}`);

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

export const dataStore = () => {
  const storeObject = {};

  return {
    get: (key) => storeObject[key],
    put: (key, data) => {
      if (storeObject[key]) {
        storeObject[key].push(data);
      } else {
        storeObject[key] = [data];
      }
    },
    wipe: (key) => delete storeObject[key],
    getAll: () => storeObject,
    getKeys: () => Object.keys(storeObject),
  };
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
  const itemArray = ['fridge', 'freezer', 'washer', 'dryer', 'oven', 'cooker', 'dishwasher'];

  driverList.forEach((singleDriver) => {
    for (let seqNo = 0; seqNo <= noStops; seqNo += 1) {
      const routeDetails = {
        routeDate: date,
        depotName: depot.depotName,
        depotPostCode: depot.postcode,
        depotId: depot.depotId,
        driverName: singleDriver.driverName,
        driverId: singleDriver.driverId,
        addressId: 222,
        addressPostCode: depot.postcode,
        routeSeqNo: seqNo,
        routeAction: seqNo === 0 ? 'depot' : 'delivery',
        itemType: seqNo === 0 ? 'depot' : itemArray[getRandomInt(0, itemArray.length - 1)],
        status: 'pending',

        refNo: `D${depot.depotId.toString().padStart(2, '0')}R${singleDriver.driverId.toString().padStart(2, '0')}S${seqNo.toString().padStart(2, '0')}T${date.slice(-2)}`,
      };

      routeList.push(routeDetails);
    }
  });

  return routeList;
};

export const getPostCodeDetails = async (lookUpCode) => {
  let info = {};
  await postcodes.lookup(lookUpCode)
    .then((data) => {
      if (data.status === 200) {
        const c = data.result;

        info = {
          postCode: c.postcode,
          longitude: c.longitude,
          latitude: c.latitude,
          town: c.admin_ward,
          country: c.country,
        };
      }
    });
  return info;
};

export const isPostCodeValid = async (codeToCheck) => {
  const postCodeDetails = await getPostCodeDetails(codeToCheck);
  return postCodeDetails.status !== 404;
};

export const getNewGeoLocation = (latitude, longitude, longChangeKm, latChangeKm) => {
  // east = +longKm, west = -longKm
  // north = +latKm, south = - latKm
  const earthRadiusInKm = 6378;
  const pi = Math.PI;

  const newLatitude = latitude + (latChangeKm / earthRadiusInKm) * (180 / pi);
  const newLongitude = longitude + (longChangeKm / earthRadiusInKm) * (180 / pi) / Math.cos(latitude * pi / 180);

  return { latitude: newLatitude, longitude: newLongitude };
};

export const getClosePostCode = async (lat, long) => {
  const locationList = [];

  await postcodes.geo(lat, long, {
    limit: 20,
    radius: 1000,
    wideSearch: true,
  }).then((data) => {
    if (data.status === 200) {
      for (let i = 0; i < data.result.length; i += 1) {
        const info = {
          postCode: data.result[i].postcode,
          longitude: data.result[i].longitude,
          latitude: data.result[i].latitude,
          town: data.result[i].admin_ward,
          country: data.result[i].country,
        };
        locationList.push(info);
      }
    }
  });

  if (locationList.length > 0) {
    return locationList[locationList.length - 1];
  }
  return locationList;
};

export const distanceBetween = ([latA, lonA], [latB, lonB]) => {
  const toRadian = (angle) => (Math.PI / 180) * angle;
  const distance = (a, b) => (Math.PI / 180) * (a - b);
  const earthRadiusInMiles = 3958.8;

  const dLat = distance(latB, latA);
  const dLon = distance(lonB, lonA);

  const radianLatA = toRadian(latA);
  const radianLatB = toRadian(latB);

  // Haversine Formula
  const a = (Math.sin(dLat / 2) ** 2)
    + (Math.sin(dLon / 2) ** 2)
    * Math.cos(radianLatA) * Math.cos(radianLatB);

  const c = 2 * Math.asin(Math.sqrt(a));

  return round(earthRadiusInMiles * c, 2);
};
