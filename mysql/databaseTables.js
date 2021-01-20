
export const createDepots = `

drop table if exists depots;

create table depots (
  id int not null auto_increment,
  depotName varchar(50) not null,
  postCode varchar(10) not null,
  fleetSize int not null,
  primary key (id)
);

alter table depots auto_increment=1;

insert into depots (depotName, postCode, fleetSize) 
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

export const createAll = `

drop table if exists routes;

create table routes (
    depotId int not null,                   /* depot Id from table depots */
    driverId int not null,                  /* driver Id from table drivers */
    routeDate date not null,                /* route date */
    addressId int not null,                 /* address Id from table addresses */
    routeSeqNo int not null,                /* delivery or recycle seq no. in route 1,2,3,... */
    routeAction varchar(10) not null,       /* deliver or recycle */
    itemType varchar(30) not null,          /* fridge, freezer etc. */
    status varchar(10) not null,            /* pending, completed, failed */
    primary key (depotId, driverId, routeDate)
);

drop table if exists drivers;

create table drivers (
    driverId int not null auto_increment,   /* driverId auto created */
    driverName varchar(50) not null,        /* free format driver name */
    truckSize int not null,                 /* cubic capacity of truck */
    primary key (driverId)
);
    
alter table drivers auto_increment=1;

drop table if exists addresses;

create table addresses (
    addressId int not null auto_increment,  /* driverId auto created */
    postcode varchar(10) not null,          /* a valid postcode */
    customerName varchar(50) not null,      /* free format customer name */
    customerEmail varchar(100) not null,    /* customer email address */
    locationType varchar(10) not null,      /* "private" or "public" address type */  
    fullAddress varchar(100) not null,      /* free format address details */
    townAddress varchar(50) null,           /* free format address town/city */
    notes varchar(200) null,                /* free format notes field */
    primary key (addressId)                 
);
    
alter table addresses auto_increment=1;
`;
