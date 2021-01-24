export const asyncList = (previousStore = null) => {
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

  let procOutput = dataStore();

  if (previousStore) {
    // allow the sub queues to inherit the parent data
    procOutput = previousStore;
  }

  const add = (funcToCall, dataObject = {}) => {
    // store the procOutput function for read/write access

    procOutput.put('procOutput', procOutput);

    // add a placeholder for this id's data output
    if (dataObject.id) {
      procOutput.put(dataObject.id, []);
    }

    // add process and the parameter store to the process queue
    procList.push({
      execFunction: funcToCall,
      execParams: Object.assign(dataObject, { ...procOutput.getStore() }),
    });
  };

  const run = () => new Promise((completed) => {
    const log = (result) => {
      if (showLog) {
        console.log('=>', result);
      }
    };

    (async () => {
      // reduce will provide a method for executing
      // processes asynchronously by placing a terminating
      // promise of the running process into the accumulator.

      await procList.reduce(
        (activePromise, process) => activePromise.then(
          () => process.execFunction(process.execParams).then(log),
        ),
        Promise.resolve(null),
      ).then(() => { completed('completed: last process in list'); });
    })();
  });

  return {
    add,
    run,
  };
};
