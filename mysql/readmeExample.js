import { asyncList, asyncStore } from './asyncList.js';
import { dbControl } from './dbControl.js';
import { showTable } from './dbUtils.js';

const createUsers = `

drop table if exists users;

create table users (
  id int not null auto_increment,
  forename varchar(30) not null,
  surname varchar(30) not null,
  department varchar(20) not null,
  primary key (id)
);

alter table users auto_increment=1;

insert into users (forename, surname, department) 
values
('Alice', 'Brown', 'Admin'),
('Bob', 'Newman', 'Finance');
`;

const store = asyncStore();
const db = dbControl();

(async () => {
  await db.connect({ store, region: 'eu-west-2', dbInstance: 'prod-mysql' });
  await db.sql(createUsers);
  await db.sql('select * from users;', 'users')
    .then(() => { db.close(); });

  showTable(store.get('users')[0]);
  console.log('#####\n');
  console.log(store.get('users')[0]);
})();
