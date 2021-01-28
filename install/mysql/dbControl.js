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
  let connectSecret = '';
  let db;
  let store;

  const connect = (data) => new Promise((completed) => {
    store = data.store;

    getSecretObject(data.region, data.dbInstance)
      .then((awsSecret) => { connectSecret = JSON.parse(awsSecret.toString()); })
      .then(() => { db = mysql.createConnection(connectSecret); })
      .then(() => { completed('completed: database connect'); })
      .catch((error) => { completed(`error: database connect ${error}`); });
  });

  const sql = (sqlText, id = 'none') => new Promise((completed) => {
    // run a single valid sql statement

    // spit multiple sql statements into individual statements
    // and store the statements in an array avoiding empty lines.

    if (typeof sqlText !== 'string') {
      console.log('sql error: received non string text ', sqlText);
      completed(null);
    }

    const sqlArray = sqlText.split(';').map((s) => s.replace(/(\r\n|\n|\r)/gm, ''))
      .filter((line) => line.length > 0);

    sqlArray.forEach((singleSql) => {
      const results = [];
      db.query(singleSql, (error, mySqlResult) => {
        if (error) {
          console.log(error);
          completed(`error: sql ${sqlText.slice(0, 25)}`);
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

            if (id) {
            // output is a table select so convert output to a table json object

              store.put(id, results);
            }
            completed(`completed: sql ${sqlText.slice(0, 25)}`);
          } catch (e) {
          // output is not a table select so just store the text under the id
            if (id) {
              store.put(id, mySqlResult);
            }
            completed(`completed: sql ${sqlText.slice(0, 25)}`);
          }
        }
      });
    });
  });

  return (
    {
      connect,
      close: () => db.end(),
      sql,
    }
  );
};
