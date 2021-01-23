import mysql from 'mysql';
import { asyncList } from './asyncList.js';

export const sleep = (data) => new Promise((completed) => {
  const endOfTimeOut = () => {
    completed(`sleep ${data.timeOut} completed`);
  };

  setTimeout(endOfTimeOut, data.timeOut);
});

const subWebFetch = (data) => new Promise((completed) => {
  const b = asyncList();
  b.add(sleep, { timeOut: 1555 });
  b.add(sleep, { timeOut: 444 });
  b.add(sleep, { timeOut: 333 });
  b.add(sleep, { timeOut: 222 });
  b.add(sleep, { timeOut: 111 });
  b.run()
    .then(() => { completed('subweb completed'); });
});

const a = asyncList();
const db = 'mysql 7.22';
const secret = { key: 2323, pass: 'myPass' };
const array = { rows: ['one', 'two', 'three'] };
a.store({ database: db });
a.store(secret);
a.store(array);

a.add(sleep, { timeOut: 3000 });
a.add(sleep, { timeOut: 2000 });
a.add(sleep, { timeOut: 1000 });
a.add(subWebFetch, { misc: 'test' });
a.add(sleep, { timeOut: 1500 });

a.run();
