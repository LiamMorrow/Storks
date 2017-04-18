// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

namespace Storks.AzureStorage
{
    /// <summary>
    /// A collection of methods for useful functions when working with Storks
    /// </summary>
    public static class StorksHelpers
    {
        /// <summary>
        /// Sets the DataCommunicator of the controller to be a <see cref="AzureBlobStorageCommunicator"/> allowing blob storage to be the backing Store
        /// </summary>
        /// <param name="controller">The controller to set the communicator for</param>
        /// <param name="connectionString">The connection string for connecting to Azure Blob storage</param>
        /// <exception cref="System.ArgumentNullException">
        /// if either <paramref name="connectionString"/>, <paramref name="controller"/> is null
        /// </exception>
        /// <returns>The <see cref="AzureBlobStorageCommunicator"/> to pass additional configuration options</returns>
        public static AzureBlobStorageCommunicator UseAzureBlobStorage(this IStoreBackedPropertyController controller, string connectionString)
        {
            Throw.IfNull(() => controller);
            var communicator = new AzureBlobStorageCommunicator(connectionString);

            controller.DataCommunicator = communicator;
            return communicator;
        }
    }
}