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
  let region = 'unknown';
  let dbName = 'none';
  let connectSecret = '';
  let db;

  const executeSQL = (sql) => {
    let sqlStatement = sql;

    if (!sqlStatement) {
      sqlStatement = 'show variables like "%version%";';
    }

    db.query(sqlStatement,
      (error, results) => {
        if (error) {
          throw error;
        }
        results.forEach((x) => console.log(x));
      });
  };

  const runSql = (sqlStatement) => {
    (getSecretObject(region, dbName))
      .then((awsSecret) => { connectSecret = JSON.parse(awsSecret.toString()); })
      .then(() => { db = mysql.createConnection(connectSecret); })
      .then(() => { executeSQL(sqlStatement); })
      .then(() => { db.end(); })
      .catch((error) => { console.log('Fail:', error); });
  };

  return (
    {
      setRegion: (value) => { region = value; },
      setName: (value) => { dbName = value; },
      runSql: (value) => runSql(value),
    }
  );
};
