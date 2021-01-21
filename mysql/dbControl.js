import AWS from 'aws-sdk';
import mysql from 'mysql';

export const getSecretObject = (regionCode, secretName) => new Promise((resolve, reject) => {
  const client = new AWS.SecretsManager({
    region: regionCode,
  });

  client.getSecretValue({ SecretId: secretName }, (error, data) => {
    if (error) {
      reject(error.code);
    } else {
      resolve(data.SecretString);
    }
  });
});

export const dbControl = () => {
  let regionName = 'unknown';
  let instanceName = 'none';
  let connectSecret = '';
  let db;
  const preQueue = [];
  const postQueue = [];
  const resultsArray = [];

  const dbConnect = () => new Promise((resolve) => {
    (getSecretObject(regionName, instanceName))
      .then((awsSecret) => { connectSecret = JSON.parse(awsSecret.toString()); })
      .then(() => { db = mysql.createConnection(connectSecret); })
      .then(() => { resolve(null); });
  });

  const dbClose = () => new Promise((resolve) => {
    db.end();
    resolve('closed');
  });

  const dbExecSql = (sql, id) => new Promise((resolve, reject) => {
    if (sql.length > 0) {
      // only process sql statement if it has a length
      db.query(sql, (error, mySqlresult) => {
        // running mySQL statement
        if (error) {
          // sql statement failed, so close database
          dbClose();
          reject(new Error('sql failed'));
        } else if (id) {
          // if an id has been sent with the sql statement
          // store the sql output in the resultsArray

          let resultEntry = resultsArray.find((item) => item.id === id);

          if (!resultEntry) {
            // this is the first time this id has been
            // used so create an object store in the resultsArray

            resultEntry = { id, data: [] };
            resultsArray.push(resultEntry);
          }

          // place each output row returned from mySql
          // into the data array for this sql statement id

          mySqlresult.forEach((row) => {
            resultEntry.data.push(row);
          });
        }
        resolve(null);
      });
    }
  });

  const addQueue = (data, id) => {
    preQueue.push({ data, id });
  };

  const processQueue = () => new Promise((resolve) => {
    // run all sql statements and procesing functions
    // as a single threaded process

    (async () => {
      preQueue.forEach((queueItem) => {
        if (typeof queueItem.data === 'string') {
          // create separate queue tasks for any sql strings that
          // contain more than one statements in them separated by
          // a semi-colon.

          // remove new all newline characters from each individual sql
          // and place each statement into an individual array;

          const itemArray = queueItem.data.split(';').map((s) => s.replace(/(\r\n|\n|\r)/gm, ''));

          itemArray.forEach((singleItem) => {
            if (singleItem.length) {
              postQueue.push({ data: singleItem, id: queueItem.id });
            }
          });
        } else {
          // do not modified functions, just add to the final queue
          postQueue.push({ data: queueItem.data, id: queueItem.id });
        }
      });

      // add database connect as the first task in the process queue
      postQueue.unshift({ type: 'func', data: dbConnect });

      // add database close as the last task in the process queue
      postQueue.push({ type: 'func', data: dbClose });

      const firstPromise = Promise.resolve(null);

      await postQueue.reduce(
        (p, queueItem) => p.then(() => {
          if (typeof queueItem.data === 'string') {
            return (dbExecSql(queueItem.data, queueItem.id));
          }

          // execute queueItem as a function()
          return (queueItem.data());
        }),
        firstPromise,
      );
      resolve(null);
    }
    )();
  });

  const fetchResults = (id) => {
    const entryObject = resultsArray.find((x) => x.id === id);
    const returnObject = [];
    if (entryObject) {
      entryObject.data.forEach((row) => {
        const rowObject = {};
        Object.keys(row).forEach((key) => {
          rowObject[key] = row[key];
        });
        returnObject.push(rowObject);
      });
    }

    return returnObject;
  };

  const showResults = (id) => {
    const result = fetchResults(id);

    if (result.length > 0) {
      let screenOutput = '';
      result.forEach((row) => {
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

  return (
    {
      setRegion: (value) => { regionName = value; },
      setInstance: (value) => { instanceName = value; },
      fetchResults,
      showResults,
      processQueue,
      addQueue,
    }
  );
};
