import { dbControl } from './dbControl.js';

const db = dbControl();
db.setRegion('eu-west-2');
db.setInstance('prod-mysql');

export const sql1 = `
select * from depots;
`;

export const sql2 = `
select * from users;
show tables;
`;
export const sql3 = 'select 3 from dual;';
export const sql4 = 'select 4 from dual;';

const postProcess = () => {
  db.showResults('depots');
  db.showResults('users');
};

const interim = () => {
  console.log('Calling interim');
};

db.addQueue(sql1, 'depots');
db.addQueue(interim);
db.addQueue(sql1, 'depots');
db.addQueue(sql2, 'users');

db.processQueue()
  .then(() => { postProcess(); });
