using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Storks
{
    /// <summary>
    /// Provides an implementation of <see cref="IStoreBackedPropertyDataCommunicator"/> that stores data in memory
    /// </summary>
    public class InMemoryDataCommunicator : IStoreBackedPropertyDataCommunicator
    {
        private readonly ConcurrentDictionary<string, byte[]> _memoryStore = new ConcurrentDictionary<string, byte[]>();

        
        /// <summary>
        /// Retrieves the data from the store, using the unique ID given
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <returns>A task describing the state of the retrieve operation</returns>
        /// <exception cref="ArgumentNullException">When id is null or empty</exception>
        public Task<byte[]> GetDataAsync(string id)
        {
            Throw.IfNullOrEmpty(() => id);
            if (_memoryStore.TryGetValue(id, out var stored) && stored != null)
            {
                var copy = new byte[stored.Length];
                Array.Copy(stored, copy, stored.Length);
                return Task.FromResult(copy);
            }
            return Task.FromResult<byte[]>(null);
        }

        /// <summary>
        /// Stores the given data with a unique id for later retrieval
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <param name="data">The data to be stored</param>
        /// <returns>A task describing the state of the store operation</returns>
        /// <exception cref="ArgumentNullException">When id is null or empty</exception>
        public Task StoreDataAsync(string id, byte[] data)
        {
            Throw.IfNullOrEmpty(() => id);
            if (data == null)
            {
                _memoryStore.TryRemove(id, out var dummy);
                return Task.FromResult(true);
            }
            _memoryStore.AddOrUpdate(id, data, (key,oldData) => data);
            return Task.FromResult(true);
        }
    }
}
