using System.IO;
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
        /// Initializes a new AzureBlobStorageCommunicator
        /// </summary>
        /// <param name="connectionString">The connection string for connecting to Azure Blob storage</param>
        /// <param name="blobContainer">The container to use for storing data</param>
        /// <exception cref="System.ArgumentNullException">if either <paramref name="connectionString"/>, <paramref name="blobContainer"/> is null</exception>
        public AzureBlobStorageCommunicator(string connectionString, string blobContainer)
        {
            Throw.IfNullOrEmpty(() => connectionString);
            Throw.IfNullOrEmpty(() => blobContainer);
            ConnectionString = connectionString;
            BlobContainer = blobContainer;
        }

        /// <summary>
        /// The container to use for storing data
        /// </summary>
        public string BlobContainer { get; }

        /// <summary>
        /// The connection string for connecting to Azure Blob storage
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Retrieves the data from the store, using the unique ID given
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <returns>A task describing the state of the retrieve operation</returns>
        /// <exception cref="System.ArgumentNullException">if <paramref name="id"/> is null</exception>
        public async Task<byte[]> GetDataAsync(string id)
        {
            Throw.IfNullOrEmpty(() => id);
            var blob = await GetBlobReference(id).ConfigureAwait(false);
            var blobExists = await blob.ExistsAsync().ConfigureAwait(false);

            if (!blobExists)
            {
                return null;
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
            var blob = await GetBlobReference(id).ConfigureAwait(false);
            await blob.UploadFromByteArrayAsync(data, 0, data.Length).ConfigureAwait(false);
        }

        private async Task<CloudBlockBlob> GetBlobReference(string id)
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to the container
            var container = blobClient.GetContainerReference(BlobContainer);

            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            // Retrieve reference to the blob
            return container.GetBlockBlobReference(id);
        }
    }
}