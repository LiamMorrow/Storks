using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Storks
{

    /// <summary>
    /// Provides an implementation of <see cref="IStoreBackedPropertyDataCommunicator"/> that uses provided delegates for read/write ops
    /// </summary>
    public class DelegatedDataCommunicator : IStoreBackedPropertyDataCommunicator
    {
        private readonly Func<string,byte[],Task> _storeDataAsyncDelegate;
        private readonly Func<string,Task<byte[]>> _retrieveDataAsyncDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatedDataCommunicator"/> class using asynchronous delegates
        /// </summary>
        /// <param name="storeDataAsync">A task that executes a store operation using the id and given bytes</param>
        /// <param name="getDataAsync">A task that retrieves data by the id</param>
        public DelegatedDataCommunicator(Func<string,byte[],Task> storeDataAsync, Func<string,Task<byte[]>> getDataAsync)
        {
            _storeDataAsyncDelegate = storeDataAsync;
            _retrieveDataAsyncDelegate = getDataAsync;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatedDataCommunicator"/> class using synchronous delegates
        /// </summary>
        /// <param name="storeData">An action that executes a store operation using the id and given bytes</param>
        /// <param name="getData">A function that retrieves data by the id</param>
        public DelegatedDataCommunicator(Action<string,byte[]> storeData, Func<string,byte[]> getData)
        {
            _storeDataAsyncDelegate = (string id, byte[] data) =>
            {
                storeData(id, data);
                return Task.FromResult<int>(1);
            };

            _retrieveDataAsyncDelegate = (string id) =>
            {
                return Task.FromResult(getData(id));
            };
        }

        /// <summary>
        /// Retrieves the data from the store, using the unique ID given using the delegate that was provided
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <returns>A task describing the state of the retrieve operation</returns>
        public Task<byte[]> GetDataAsync(string id)
        {
            return _retrieveDataAsyncDelegate(id);
        }


        /// <summary>
        /// Stores the given data with a unique id for later retrieval using the delegate that was provided
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <param name="data">The data to be stored</param>
        /// <returns>A task describing the state of the store operation</returns>
        public Task StoreDataAsync(string id, byte[] data)
        {
            return _storeDataAsyncDelegate(id, data);
        }
    }
}
