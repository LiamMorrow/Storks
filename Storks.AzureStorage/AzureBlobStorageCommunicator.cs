// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Storks.AzureStorage
{
    /// <summary>
    /// Provides an implementation of <see cref="IStoreBackedPropertyDataCommunicator"/> for interacting with Azure storage
    /// </summary>
    public class AzureBlobStorageCommunicator : IStoreBackedPropertyDataCommunicator
    {
        /// <summary>
        /// The prefix of all containers that storks will create
        /// </summary>
        public const string ContainerPrefix = "storks-cache";

        /// <summary>
        /// Initializes a new AzureBlobStorageCommunicator
        /// </summary>
        /// <param name="connectionString">The connection string for connecting to Azure Blob storage</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="connectionString"/> is null</exception>
        public AzureBlobStorageCommunicator(string connectionString)
        {
            Throw.IfNullOrEmpty(() => connectionString);
            ConnectionString = connectionString;
        }

        /// <summary>
        /// The minimum amount of time for an item to be cached, before being queued for deletion defaults to 1 day
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// The Format to store the dates as. Can't use ISO8601 because it produces invalid chars
        /// </summary>
        private const string DateFormat = "yyyyMMddTHHmmss";

        /// <summary>
        /// Gets or sets a value that determines whether Storks will automatically clear caches older than <see cref="CacheDuration"/>
        /// Defaults to true
        /// </summary>
        public bool PeriodicallyClearCaches { get; set; } = true;

        /// <summary>
        /// The connection string for connecting to Azure Blob storage
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the azure storage container names that are being used
        /// </summary>
        /// <returns>A Tuple containing the previous, current, and next names that will be used for containers</returns>
        public (string PreviousName, string CurrentName, string NextName) GetContainerNames()
        {
            string getName(DateTime t)
            {
                return ContainerPrefix + t.ToString(DateFormat, CultureInfo.InvariantCulture);
            }
            var time = DateTime.UtcNow;
            var currentWindow = time.RoundDown(CacheDuration);
            var previousWindow = currentWindow.Subtract(CacheDuration);
            var nextWindow = currentWindow.Add(CacheDuration);
            return (getName(previousWindow),
                getName(currentWindow),
                getName(nextWindow));
        }

        /// <summary>
        /// Retrieves the data from the store, using the unique ID given
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <returns>A task describing the state of the retrieve operation</returns>
        /// <exception cref="System.ArgumentNullException">if <paramref name="id"/> is null</exception>
        public async Task<byte[]> GetDataAsync(string id)
        {
            Throw.IfNullOrEmpty(() => id);
            var blob = await GetBlobReference(id, false).ConfigureAwait(false);
            var blobExists = await blob.ExistsAsync().ConfigureAwait(false);

            if (!blobExists)
            {
                // Attempt to see if it is in the previous cache, just in case
                blob = await GetBlobReference(id, true).ConfigureAwait(false);
                blobExists = await blob.ExistsAsync().ConfigureAwait(false);

                if (!blobExists)
                {
                    return null;
                }
            }

            using (var ms = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(ms).ConfigureAwait(false);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Stores the given data with a unique id for later retrieval
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <param name="data">The data to be stored</param>
        /// <returns>A task describing the state of the store operation</returns>
        /// <exception cref="System.ArgumentNullException">if <paramref name="id"/> is null</exception>
        public async Task StoreDataAsync(string id, byte[] data)
        {
            Throw.IfNullOrEmpty(() => id);
            data = data ?? new byte[0];
            var blob = await GetBlobReference(id, false).ConfigureAwait(false);
            await blob.UploadFromByteArrayAsync(data, 0, data.Length).ConfigureAwait(false);
        }

        private async Task ClearOldContainers(CloudBlobClient client)
        {
            var token = new BlobContinuationToken();
            var containers = await client.ListContainersSegmentedAsync(ContainerPrefix, token)
                .ConfigureAwait(false);
            var validContainerNames = GetContainerNames();

            if (containers?.Results == null)
            {
                return;
            }

            // loop through all containers
            var results = containers.Results.ToList();
            foreach (var container in results)
            {
                if (container.Name.StartsWith(ContainerPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (validContainerNames.NextName != container.Name
                       && validContainerNames.CurrentName != container.Name
                       && validContainerNames.PreviousName != container.Name)
                    {
                        // Delete the container if it doesn't match the current container or padding windows
                        await container.DeleteAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<CloudBlockBlob> GetBlobReference(string id, bool usePreviousContainerCache)
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            if (PeriodicallyClearCaches)
            {
                await ClearOldContainers(blobClient).ConfigureAwait(false);
            }
            var containerNames = GetContainerNames();
            var containerName = containerNames.CurrentName;
            // Retrieve reference to the container
            if (usePreviousContainerCache)
            {
                containerName = containerNames.PreviousName;
            }
            var container = blobClient.GetContainerReference(containerName);

            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            // Retrieve reference to the blob
            return container.GetBlockBlobReference(id);
        }
    }
}