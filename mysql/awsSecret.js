// import { secret } from 'aws-sdk';

export const getSecretObject = (name) => {
  let secretObject = {};

  if (name === 'prod-mysql') {
    secretObject = {
      host: 'xxxx.amazonaws.com',
      user: 'xxxx',
      password: 'xxxx',
      database: 'xxxx',
    };
  }

  return secretObject;
};

export default getSecretObject();
