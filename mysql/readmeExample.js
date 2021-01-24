import { asyncList } from './asyncList.js';
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

const db = dbControl();
const a = asyncList();

a.add(db.connect, { region: 'eu-west-2', dbInstance: 'prod-mysql' });
a.add(db.sql, { sql: createUsers });
a.add(db.sql, { sql: 'select * from users;', id: 'users' });

a.run().then(() => {
  db.close();
  showTable(a.fetch('users'));
  console.log('#####\n');
  console.log(a.fetch('users'));
});
