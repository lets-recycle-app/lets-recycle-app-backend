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

  const connect = (data) => new Promise((completed) => {
    (getSecretObject(data.region, data.dbInstance))
      .then((awsSecret) => { connectSecret = JSON.parse(awsSecret.toString()); })
      .then(() => { db = mysql.createConnection(connectSecret); })
      .then(() => { completed('completed: database connect'); })
      .catch((error) => { completed(`error: database connect ${error}`); });
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
        console.log(error);
        completed(`error: sql ${inputParams.sql.slice(0, 25)}`);
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
          }
          completed(`completed: sql ${inputParams.sql.slice(0, 25)}`);
        } catch (e) {
          // output is not a table select so just store the text under the id
          if (inputParams.id) {
            inputParams.procOutput.put(inputParams.id, mySqlResult);
          }
          completed(`completed: sql ${inputParams.sql.slice(0, 25)}`);
        }
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
    // pass the parent process output object to the child queue.

    const queue = asyncList(inputParams.procOutput);

    sqlArray.forEach((singleSql) => {
      queue.add(runSingleSql, { sql: singleSql, id: inputParams.id, procOutput: inputParams.procOutput });
    });

    queue.run()
      .then(() => { completed('completed: async sql process list'); })
      .catch((error) => { completed(`error: async sql process list ${error}`); });
  });

  return (
    {
      connect,
      close: () => db.end(),
      sql,
      showTable,
    }
  );
};
