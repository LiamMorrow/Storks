using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Storks.Resolvers;

namespace Storks
{
    /// <summary>
    /// A default implementation of <see cref="IStoreBackedPropertyController"/> using a <see cref="IStoreBackedPropertyDataCommunicator"/> 
    /// for interaction with the store
    /// </summary>
    public class StoreBackedPropertyController : IStoreBackedPropertyController
    {
        private readonly ConcurrentDictionary<Type, object> _encoders = new ConcurrentDictionary<Type, object>();
        /// <summary>
        /// Used to communicate with the store for storing / retrieving data
        /// </summary>
        public IStoreBackedPropertyDataCommunicator DataCommunicator { get; set; }

        /// <summary>
        /// When set to true, we will use the <see cref="BsonStoreBackedPropertyEncoder{T}"/> if there is no resolver set for the given type
        /// </summary>
        public bool FallbackToBsonEncoder { get; set; } = true;

        /// <summary>
        /// Returns the resolver for <typeparamref name="T"/>. 
        /// If there is no resolver and <see cref="FallbackToBsonEncoder"/> is set, a <see cref="BsonStoreBackedPropertyEncoder{T}"/> is used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A resolver for type <typeparamref name="T"/></returns>
        public IStoreBackedPropertyEncoder<T> GetEncoder<T>()
        {
            if (_encoders.TryGetValue(typeof(T), out var resolver)
                && resolver != null)
            {
                return (IStoreBackedPropertyEncoder<T>)resolver;
            }
            if (FallbackToBsonEncoder)
            {
                return new BsonStoreBackedPropertyEncoder<T>();
            }
            throw new InvalidOperationException("No Resolver for type: " + typeof(T).ToString());
        }

        /// <summary>
        /// Retrieves the value from the store using the <see cref="DataCommunicator"/> and the appropriate <see cref="IStoreBackedPropertyEncoder{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of value to retrieve</typeparam>
        /// <param name="property">The property that is being pulled.  The <see cref="StoreBackedProperty{T}.Id"/> should be set properly</param>
        /// <returns>The deserialized property from the store</returns>
        public async Task<T> GetValueAsync<T>(StoreBackedProperty<T> property)
        {
            ThrowIfNoDataCommunicator();
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            var encoder = GetEncoder<T>();
            var data = await DataCommunicator.GetDataAsync(property.Id)
                .ConfigureAwait(false);
            return encoder.Decode(data);
        }

        private void ThrowIfNoDataCommunicator()
        {
            if (DataCommunicator == null)
            {
                throw new InvalidOperationException("No DataRetriever set");
            }
        }

        /// <summary>
        /// Registers an encoder to be used for serialization/deserialization of <see cref="StoreBackedProperty{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="encoder">The encoder to set.  If set to null, removes the encoder for type <typeparamref name="T"/></param>
        public void RegisterEncoder<T>(IStoreBackedPropertyEncoder<T> encoder)
        {
            _encoders.AddOrUpdate(typeof(T), encoder, (t, o) => encoder);
        }

        /// <summary>
        /// Stores the value into the store and returns a <see cref="StoreBackedProperty{T}" /> with the Id for retrieval
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The property to store</param>
        /// <returns>The link to retrieve he object from the store</returns>
        public async Task<StoreBackedProperty<T>> StoreValueAsync<T>(T value)
        {
            ThrowIfNoDataCommunicator();
            var property = new StoreBackedProperty<T>(Guid.NewGuid().ToString());
            var encoder = GetEncoder<T>();
            var bytes = encoder.Encode(value);
            await DataCommunicator.StoreDataAsync(property.Id, bytes).ConfigureAwait(false);
            return property;
        }
    }
}