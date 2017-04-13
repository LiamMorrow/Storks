// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Storks
{
    /// <summary>
    /// Provides an implementation of <see cref="IStoreBackedPropertyDataCommunicator"/> that stores data in the local FileSystem
    /// </summary>
    public class LocalFileDataCommunicator : IStoreBackedPropertyDataCommunicator
    {
        /// <summary>
        /// Creates a new instance of <see cref="LocalFileDataCommunicator"/> setting it's directory for storing files as <paramref name="masterDirectory"/>
        /// </summary>
        /// <param name="masterDirectory">The folder to store files into</param>
        public LocalFileDataCommunicator(string masterDirectory)
        {
            Throw.IfNullOrEmpty(() => masterDirectory);
            if (!Directory.Exists(masterDirectory))
            {
                Directory.CreateDirectory(masterDirectory);
            }

            MasterDirectory = masterDirectory;
        }

        /// <summary>
        /// The Path to the directory where data should be stored
        /// </summary>
        public string MasterDirectory { get; }

        private string GetPathForFile(string file)
        {
            return Path.Combine(MasterDirectory, file);
        }

        /// <summary>
        /// Retrieves the data from the store, using the unique ID given
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <returns>A task describing the state of the retrieve operation</returns>
        /// <exception cref="ArgumentNullException">When id is null or empty</exception>
        public async Task<byte[]> GetDataAsync(string id)
        {
            Throw.IfNullOrEmpty(() => id);
            var filePath = GetPathForFile(id);
            if (File.Exists(filePath))
            {
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    var buff = new byte[file.Length];
                    await file.ReadAsync(buff, 0, (int)file.Length).ConfigureAwait(false);
                    return buff;
                }
            }
            return null;
        }

        /// <summary>
        /// Stores the given data with a unique id for later retrieval
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <param name="data">The data to be stored</param>
        /// <returns>A task describing the state of the store operation</returns>
        /// <exception cref="ArgumentNullException">When id is null or empty</exception>
        public async Task StoreDataAsync(string id, byte[] data)
        {
            Throw.IfNullOrEmpty(() => id);
            var filePath = GetPathForFile(id);
            if (File.Exists(filePath))
            {
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.Write, 4096, true))
                {
                    await file.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                }
            }
        }
    }
}