import mysql from 'mysql';
import { getSecretObject } from './awsSecret.js';

export const connectDB = () => mysql.createConnection(
  getSecretObject('prod-mysql'),
);

export const closeDB = (db) => db.end();

export const execDB = (db, sqlStatement) => {
  db.query(sqlStatement,
    (error, results) => {
      if (error) {
        throw error;
      }
      results.forEach((x) => console.log(x));
    });
};
