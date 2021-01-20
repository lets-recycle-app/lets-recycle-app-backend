import faker from 'faker';

// mock data population

const randomName = faker.name.findName();
const randomEmail = faker.internet.email(); // Kassandra.Haley@erich.biz
//const randomCard = faker.helpers.createCard(); // random contact card containing many properties


console.log(randomEmail);
