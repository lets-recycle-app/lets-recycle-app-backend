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

Execute any SQL statement by passing a valid string parameter to db.runSql() e.g.
```javascript
db.runSql('show tables');
```

Multiple SQL statements can be defined in a single string variable. 
The sql statements will be executed asynchronously in order e.g.


```javascript
const clearUsers = `

delete from users;

alter table users auto_increment=1;

insert into users (permission, forename, surname) 
values
('driver', 'Alice', 'Brown'),
('driver', 'Bob', 'Newman');

`;

db.runSql(clearUsers);
```
