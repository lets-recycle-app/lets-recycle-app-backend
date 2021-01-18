export const createCollRequest = `

drop table if exists collection_request;

create table collection_request (
  id int not null auto_increment,
  appliance varchar(30) not null,
  locationType varchar(30) not null,
  name varchar(100) not null,
  email varchar(100) not null,
  houseNo varchar(10) not null,
  street varchar(100) not null,
  town varchar(50) not null,
  postcode varchar(10) not null,
  notes text not null,
  longitude decimal(20,15) not null,
  latitude decimal(20,15) not null,
  datetimeCreated datetime not null,
  datetimeCompleted datetime DEFAULT null,
  assignedDate date DEFAULT null,
  driverId int(11) not null,
  waitingList tinyint(1) not null,
  completed tinyint(1) not null,
  errandType varchar(30) not null,
  primary key (id)
);

alter table collection_request auto_increment=1000;

insert into collection_request 
(appliance, locationType, name, email, houseNo, street, 
town, postcode, notes, longitude, latitude, datetimeCreated, 
datetimeCompleted, assignedDate, driverId, waitingList, 
completed, errandType) values
('Washer', 'private property', 'Dolor Sit Amet', 
'aaa@aa.aa', '45', 'Some St', 'Some Town', 'M32 0JG', 'aorh owirpwi sirpw poasrpow apiopfwi aporp paow sf.', 
'0.000000000000000', '0.000000000000000', '2021-01-15 11:47:55', null, null, 0, 0, 0, '');
`;

export const createUsers = `

drop table if exists users;

create table users (
  id int not null auto_increment,
  permission varchar(20) not null,
  forename varchar(30) not null,
  surname varchar(30) not null,
  email varchar(100) not null,
  password varchar(250) not null,
  dateCreated datetime not null,
  dateDeleted datetime DEFAULT null,
  primary key (id)
);

alter table users auto_increment=1;

insert into users (permission, forename, surname, email, password, dateCreated, dateDeleted) 
values
('admin', 'Dan', 'Irani', 'ddd@dd.dd', 'loremipsum', '2021-01-15 11:08:29', NULL),
('admin', 'Dorota', 'Lewinska', 'ccc@cc.cc', 'loremipsum', '2021-01-15 11:11:41', NULL),
('driver', 'Alice', 'Brown', 'aaa@aa.aa', 'loremipsum', '2021-01-15 11:12:48', NULL),
('driver', 'Bob', 'Newman', 'bbb@bb.bb', 'loremipsum', '2021-01-15 11:15:45', NULL);
`;

export const createDepots = `

drop table if exists depots;

create table depots (
  id int not null auto_increment,
  name varchar(20) not null,
  postCode varchar(30) not null,
  fleetSize varchar(30) null,
  primary key (id)
);

alter table depots auto_increment=1;

insert into depots (name, postCode, fleetSize) 
values

('Horwich', 'HO1 8XJ', 26),
('Stockport', 'M18 7TQ', 13),
('Liverpool', 'L1 1DA', 15),
('Dumfirmline', 'KY11 3AE', 11),
('Watford', 'WD17 1AP', 20),
('Milton Keynes', 'MK10 1SZ', 23),
('Crewe', 'CW1 2BS', 14),
('Cardiff', 'CF10 1BE', 8),
('Southampton', 'SO14 0AH', 9),
('Wolverhampton', 'WV1 1HB', 16),
('Brighton', 'BN41 1HS', 6),
('Southend', 'SS2 4DY', 8),
('Middlesbrough', 'TS3 6AF', 23),
('Blackpool', 'FY3 8DF', 11),
('Derby', 'DE4 2HE', 25),
('Leeds', 'LS2 8JS', 18),
('Oldham', 'OL1 3EG', 5),
('Reading', 'RG1 3YL', 13),
('Blackburn', 'BB2 1NA', 19),
('York', 'YO10 3FQ', 9);

`;