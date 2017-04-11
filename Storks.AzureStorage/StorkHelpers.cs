using Storks.Encoders;

namespace Storks.AzureStorage
{
    /// <summary>
    /// A collection of methods for useful functions when working with Storks
    /// </summary>
    public static class StorksHelpers
    {
        /// <summary>
        /// Registers encoders for many standard types
        /// </summary>
        /// <param name="controller">The controller to register the encoders to</param>
        /// <param name="connectionString">The connection string for connecting to Azure Blob storage</param>
        /// <param name="blobContainer">The container to use for storing data</param>
        /// <exception cref="System.ArgumentNullException">
        /// if either <paramref name="connectionString"/>, <paramref name="blobContainer"/>, <paramref name="controller"/> is null
        /// </exception>
        public static void UseAzureBlobStorage(this IStoreBackedPropertyController controller, string connectionString,string blobContainer)
        {
            Throw.IfNull(() => controller);
            controller.DataCommunicator = new AzureBlobStorageCommunicator(connectionString, blobContainer);
        }
    }
}