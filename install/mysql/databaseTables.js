export const createRoutes = `
drop table if exists routes;

create table routes (
    depotId int not null,                   /* depot Id from table depots */
    driverId int not null,                  /* driver Id from table drivers */
    routeDate date not null,                /* route date */
    routeSeqNo int not null,                /* delivery or recycle seq no. in route 1,2,3,... */
    distance decimal(12,4) not null,        /* distance to next stop */
    addressId int not null,                 /* address Id from table addresses */
    addressPostcode varchar(8) not null,    /* address postcode */
    latitude  decimal(12,9) not null,       /* latitude associated with the postcode */
    longitude decimal(12,9) not null,       /* longitude associated with the postcode */
    routeAction varchar(10) not null,       /* delivery or recycle */
    itemType varchar(30) not null,          /* fridge, freezer etc. */
    status varchar(10) not null,            /* pending, completed, failed */
    refNo  varchar(50) not null,            /* reference no supplied to customer */
    primary key (depotId, driverId, routeDate, routeSeqNo)
);
`;

export const createDepots = `

drop table if exists depots;

create table depots (
  depotId int not null auto_increment,      /* depotId auto created */
  depotName varchar(50) not null,           /* depot town name */   
  postcode varchar(8) not null,             /* depot post code */
  fleetSize int not null,                   /* number of drivers attached to depot */
  primary key (depotId)
);

alter table depots auto_increment=1;
`;

export const createDrivers = `
drop table if exists drivers;

create table drivers (
    driverId int not null auto_increment,   /* driverId auto created */
    depotId int not null,                   /* allocated depotId for driver */
    driverName varchar(50) not null,        /* free format driver name */
    truckSize int not null,                 /* cubic capacity of truck */
    userName varchar(50) not null,          /* app sign-in user name */
    apiKey  varchar(50) null,               /* app sign-in api key */
    primary key (driverId)
);

alter table drivers auto_increment=1;
`;

export const createAdmins = `
drop table if exists admins;

create table admins (
    adminId int not null auto_increment,    /* adminId auto created */
    adminName varchar(50) not null,         /* free format admin name */
    userName varchar(50) not null,          /* app sign-in user name */
    apiKey  varchar(50) null,               /* app sign-in api key */
    primary key (adminId)
);

alter table admins auto_increment=1;
`;

export const createAddresses = `
drop table if exists addresses;

create table addresses (
    addressId int not null auto_increment,  /* driverId auto created */
    postcode varchar(8) not null,          /* a valid postcode */
    customerName varchar(50) not null,      /* free format customer name */
    customerEmail varchar(100) not null,    /* customer email address */
    locationType varchar(30) not null,      /* "private" or "public" address type */  
    houseNo varchar(10) not null,           /* house or building number */
    street varchar(50) not null,            /* street name */
    townAddress varchar(50) null,           /* free format address town/city */
    notes varchar(200) null,                /* free format notes field */
    primary key (addressId)             
);

alter table addresses auto_increment=1;
`;

export const createPostCodes = `

create table if not exists postcodes (
  postcodeId int not null auto_increment,   /* postcodeId auto created */
  postcode varchar(8) not null,             /* a uk valid postcode */
  latitude  decimal(12,9) not null,           /* latitude associated with the postcode */
  longitude decimal(12,9) not null,         /* longitude associated with the postcode */
  primary key (postcodeId)
);
`;

export const createPostCodesIndex = `
alter table postcodes auto_increment=1;
create index postcode_idx on postcodes (postcode);
create index postcode_idx2 on postcodes (latitude,longitude);
`;
