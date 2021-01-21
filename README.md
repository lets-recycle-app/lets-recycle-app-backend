# MySQL databaseSetup.js

```javascript
npm databaseSetup
````

To install/reinstall MySQL tables and static data for a database hosted on AWS.

First, create an 'AWS Secret' with a key name that matches the database instance name.
Set the secret value to be a json object with connection settings as shown in the example below.
```javascript
{
    "host": "prod-instance.iwolqkjmdnhwf.eu-west-2.rds.amazonaws.com",
    "user": "admin",
    "password": "uNhKDj&H54%K",
    "database": "rockets"
}
```

Prior to executing any SQL statements, initialise dbControl() with the correct AWS region 
and MySQL instance name e.g.

```javascript
const db = dbControl();
db.setRegion('eu-west-2');
db.setInstance('prod-instance');
```

Add SQL statement text to a single threaded processing queue 
in the order of execution required.

Execute any SQL statement by passing a valid string parameter to db.addQueue() e.g.
```javascript

db.addQueue('show tables');
db.addQueue(clearUsers);

db.processQueue().then(() => {
  console.log('Installation completed.');
});
```

Multiple SQL statements can be defined in a single string variable. 
The sql statements will be executed asynchronously in order e.g.


```javascript
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

db.addQueue(clearUsers);
db.addQueue(fetchUsers, 'userOutput');

db.processQueue()
  .then(() => {
    db.showResults('userOutput');
    console.log('#####\n');
    console.log(db.fetchResults('userOutput'));
  });



```
The above SQL queue will execute with the following output
```javascript
{ 1 , Alice , Brown , Admin }
{ 2 , Bob , Newman , Finance }

#####

  [
  { id: 1, forename: 'Alice', surname: 'Brown', department: 'A
    dmin' },
{ id: 2, forename: 'Bob', surname: 'Newman', department: 'Fi
  nance' }
]
```