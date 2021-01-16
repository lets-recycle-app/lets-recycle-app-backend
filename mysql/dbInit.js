import { connectDB, closeDB, execDB } from './dbControl.js';
import { userSelect, tableSelect } from './dbSqls.js';

(async () => {
  const db = await connectDB();
  await execDB(db, userSelect);
  await execDB(db, tableSelect);
  await closeDB(db);
})();
