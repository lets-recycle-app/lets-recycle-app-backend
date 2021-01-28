import postcodes from 'node-postcodes.io';
import {
  getPostCodeDetails,
  isPostCodeValid,
  distanceBetween,
  getNewGeoLocation,
  getClosePostCode,
  getRandomInt,

} from './dbUtils.js';

const showDetails = async (location) => {
  console.log(`${location.postCode} [${location.latitude}, ${location.longitude}], ${location.town}`);
};

const loop = async (startCode) => {
  console.log('start ', startCode);

  let lookupCode = startCode;
  for (let i = 1; i <= 14; i += 1) {
    const newDetails = await getPostCodeDetails(lookupCode);
    console.log('Cur Code', newDetails.postCode);
    console.log('Cur Det', '[ ', newDetails.latitude, ' , ', newDetails.longitude, ']');
    const newGeo = await getNewGeoLocation(newDetails.latitude, newDetails.longitude, 1, 1);
    console.log('Cur Det', '[ ', newGeo.latitude, ' , ', newGeo.longitude, ']');
    const newCodeDetails = await getClosePostCode(newGeo.latitude, newGeo.longitude);
    console.log('New Code', newCodeDetails.postCode);
    lookupCode = newCodeDetails.postCode;
  /*
  getPostCodeDetails(startCode)
    .then((newDetails) => {
      getNewGeoLocation(newDetails.latitude, newDetails.longitude, 100, 0);
      console.log(newDetails);
      changeCode = newDetails.postCode;
      console.log('Here', newDetails);
    })
    .catch((error) => {
      changeCode = 'XXX XXX';
      console.log(error);
    });
  */
  }
};

loop('N21 3JS');
