using System;
using System.Threading.Tasks;

namespace Storks
{
    /// <summary>
    /// An interface for interacting with the backing store to set and retrieve properties
    /// </summary>
    public interface IStoreBackedPropertyController : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="IStoreBackedPropertyEncoder{T}"/> for the type given by <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The encoder for <typeparamref name="T"/></returns>
        IStoreBackedPropertyEncoder<T> GetEncoder<T>();

        /// <summary>
        /// Used to communicate with the store for storing / retrieving data
        /// </summary>
        IStoreBackedPropertyDataCommunicator DataCommunicator { get; set; }

        /// <summary>
        /// Retrieves the value from the store for the property using its Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property">The property to retrieve.  The Id must be consistent in retrieval and storage operations</param>
        /// <returns>The deserialized object from the store</returns>
        Task<T> GetValueAsync<T>(StoreBackedProperty<T> property);

        /// <summary>
        /// Stores the value into the store and returns a <see cref="StoreBackedProperty{T}"/> with the Id for retrieval
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The property to store</param>
        /// <returns>The link to retrieve he object from the store</returns>
        Task<StoreBackedProperty<T>> StoreValueAsync<T>(T value);

        /// <summary>
        /// Registers an encoder to be used for serialization/deserialization of <see cref="StoreBackedProperty{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="encoder">The encoder to set.  If set to null, removes the encoder for type <typeparamref name="T"/></param>
        void RegisterEncoder<T>(IStoreBackedPropertyEncoder<T> encoder);
    }
}