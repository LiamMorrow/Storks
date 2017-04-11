using System.Threading.Tasks;

namespace Storks
{
    /// <summary>
    /// An interface for the communication layer between the <see cref="IStoreBackedPropertyController"/> and the store
    /// </summary>
    public interface IStoreBackedPropertyDataCommunicator
    {
        /// <summary>
        /// Retrieves the data from the store, using the unique ID given
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <returns>A task describing the state of the retrieve operation</returns>
        Task<byte[]> GetDataAsync(string id);

        /// <summary>
        /// Stores the given data with a unique id for later retrieval
        /// </summary>
        /// <param name="id">A unique id to reference the data</param>
        /// <param name="data">The data to be stored</param>
        /// <returns>A task describing the state of the store operation</returns>
        Task StoreDataAsync(string id, byte[] data);
    }
}
