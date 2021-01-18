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

  const execSingleSql = (sql) => {
    if (sql.length > 0) {
      db.query(sql, (error, results) => {
        if (error) {
          throw error;
        }

        if (results.length > 0) {
          results.forEach((x) => console.log(x));
        }
      });
    }
  };

  const executeSQL = (fullSql) => {
    // receive an array of sql strings each containing
    // one or more sql statements separated by semi-colon.

    // remove new all newline characters from each individual sql

    const sqlArray = fullSql.split(';').map((s) => s.replace(/(\r\n|\n|\r)/gm, ''));

    (async () => {
      const initialPromise = Promise.resolve(null);

      await sqlArray.reduce(
        (p, spec) => p.then(() => execSingleSql(spec)),
        initialPromise,
      );

      db.end();
    })();
  };

  const runSql = (sqlStatement) => {
    (getSecretObject(regionName, instanceName))
      .then((awsSecret) => { connectSecret = JSON.parse(awsSecret.toString()); })
      .then(() => { db = mysql.createConnection(connectSecret); })
      .then(() => { executeSQL(sqlStatement); })
      .catch((error) => { console.log('Fail:', error); });
  };

  return (
    {
      setRegion: (value) => { regionName = value; },
      setInstance: (value) => { instanceName = value; },
      runSql: (value) => runSql(value),
    }
  );
};
