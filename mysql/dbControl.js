import AWS from 'aws-sdk';
import mysql from 'mysql';
import { asyncList } from './asyncList.js';

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
  let connectSecret = '';
  let db;

  const connect = (data) => new Promise((resolve) => {
    (getSecretObject(data.region, data.dbInstance))
      .then((awsSecret) => { connectSecret = JSON.parse(awsSecret.toString()); })
      .then(() => { db = mysql.createConnection(connectSecret); })
      .then(() => { resolve('database connected.'); });
  });

  const close = () => new Promise((resolve) => {
    db.end();
    resolve('database closed.');
  });

  const showTable = (rowData) => {
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

  const runSingleSql = (inputParams) => new Promise((completed) => {
    // run a single valid sql statement

    const results = [];
    db.query(inputParams.sql, (error, mySqlResult) => {
      if (error) {
        close().then(completed);
      } else {
        try {
          // try to convert raw sql output
          // to JSON column data

          mySqlResult.forEach((row) => {
            const rowObject = {};
            Object.keys(row).forEach((key) => {
              rowObject[key] = row[key];
            });
            results.push(rowObject);
          });

          if (inputParams.id) {
            // output is a table select so convert output to a table json object
            inputParams.procOutput.put(inputParams.id, results);
            console.log('HEREA', inputParams.procOutput.fetch(inputParams.id));
          }
        } catch (e) {
          // output is not a table select so just store the text under the id

          if (inputParams.id) {
            inputParams.procOutput.put(inputParams.id, mySqlResult);
          }
        }

        completed(results);
      }
    });
  });

  const sql = (inputParams) => new Promise((completed) => {
    // spit multiple sql statements into individual statements
    // and store the statements in an array avoiding empty lines.

    const sqlArray = inputParams.sql.split(';').map((s) => s.replace(/(\r\n|\n|\r)/gm, ''))
      .filter((line) => line.length > 0);

    // the data parameter will contain an object with keys
    // {sql: sqlStatement, storeFn: func.get/set, [id: storageId]

    // create a child queue of single sql statements and run asynchronously.
    const queue = asyncList();

    sqlArray.forEach((singleSql) => {
      Object.assign(inputParams, { sql: singleSql });
      queue.add(runSingleSql, inputParams);
    });
    queue.run()
      .then(() => { console.log('All Over', inputParams.procOutput.getStore()); })
      .then(() => { completed('sql batch completed.'); });
  });

  return (
    {
      connect,
      close,
      sql,
      showTable,
    }
  );
};
