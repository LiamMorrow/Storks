// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storks
{
    /// <summary>
    /// Provides an implementation of <see cref="IStoreBackedPropertyDataCommunicator"/> that stores data in memory
    /// </summary>
    public class InMemoryDataCommunicator : IStoreBackedPropertyDataCommunicator
    {
        private readonly IDictionary<string, byte[]> _memoryStore;

        /// <summary>
        /// Creates a new instance of <see cref="InMemoryDataCommunicator"/> with its own concurrent memory store
        /// </summary>
        public InMemoryDataCommunicator() : this(new ConcurrentDictionary<string, byte[]>())
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="InMemoryDataCommunicator"/> with the given <paramref name="memoryStore"/>
        /// </summary>
        /// <param name="memoryStore">The backing store to use for data operations</param>
        public InMemoryDataCommunicator(IDictionary<string, byte[]> memoryStore)
        {
            Throw.IfNull(() => memoryStore);
            _memoryStore = memoryStore;
        }

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
            _memoryStore[id] = data;
            return Task.FromResult(true);
        }
    }
}