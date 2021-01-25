export const createRoutes = `
drop table if exists routes;

create table routes (
    depotId int not null,                   /* depot Id from table depots */
    driverId int not null,                  /* driver Id from table drivers */
    routeDate date not null,                /* route date */
    routeSeqNo int not null,                /* delivery or recycle seq no. in route 1,2,3,... */
    addressId int not null,                 /* address Id from table addresses */
    addressPostCode varchar(10) not null,   /* address postcode */
    routeAction varchar(10) not null,       /* deliver or recycle */
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
  postCode varchar(10) not null,            /* depot post code */
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
    postcode varchar(10) not null,          /* a valid postcode */
    customerName varchar(50) not null,      /* free format customer name */
    customerEmail varchar(100) not null,    /* customer email address */
    locationType varchar(10) not null,      /* "private" or "public" address type */  
    fullAddress varchar(100) not null,      /* free format address details */
    houseNo varchar(10) not null,           /* house or building number */
    street varchar(50) not null,            /* street name */
    townAddress varchar(50) null,           /* free format address town/city */
    notes varchar(200) null,                /* free format notes field */
    primary key (addressId)                 
);

alter table addresses auto_increment=1;
`;
