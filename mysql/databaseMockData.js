import { asyncList } from './asyncList.js';
import { dbControl } from './dbControl.js';

const clearUsers = `

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

const fetchUsers = 'select * from users;';
const fetchDepots = 'select * from depots;';

const db = dbControl();
const a = asyncList();

const createRandom = (data) => new Promise((completed) => {
  console.log('Create Random ', data.procOutput.getStore());
  completed(null);
});

a.add(db.connect, { region: 'eu-west-2', dbInstance: 'prod-mysql' });
a.add(db.sql, { sql: clearUsers, id: 'clearUsers' });
a.add(db.sql, { sql: fetchUsers, id: 'users' });
a.add(db.sql, { sql: fetchDepots, id: 'depots' });
a.add(createRandom, { id: 'random' });
a.add(db.close);
a.run().then(() => {
  console.log('Installation completed.');
});
