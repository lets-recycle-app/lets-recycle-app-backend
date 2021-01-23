export const asyncList = () => {
  const procList = [];
  const showLog = false;

  const dataStore = () => {
    const dataObject = {};

    return {
      fetch: (id) => dataObject[id],
      put: (id, textArray) => {
        dataObject[id] = textArray;
      },
      getStore: () => dataObject,
    };
  };

  const procOutput = dataStore();

  const add = (funcToCall, dataObject = {}) => {
    // store the procOutput function globally for read/write access
    procOutput.put('procOutput', procOutput);

    // add a placeholder for this id's data output
    if (dataObject.id) {
      procOutput.put(dataObject.id, []);
    }

    procList.push({
      execFunction: funcToCall,
      execParams: Object.assign(dataObject, { ...procOutput.getStore() }),
    });
  };

  const run = async () => {
    const log = (result) => {
      if (showLog) {
        console.log('=>', result);
      }
    };

    await procList.reduce(
      (activePromise, process) => activePromise.then(
        () => process.execFunction(process.execParams).then(log),
      ),
      Promise.resolve(null),
    );
  };

  return {
    add,
    run,
  };
};
