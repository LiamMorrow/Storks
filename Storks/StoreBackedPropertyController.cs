// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Storks.Encoders;

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
        /// Used to detect multiple calls to <see cref="Dispose()"/> and to throw an error if methods are accessed after disposal
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Initializes the default implementation of <see cref="IStoreBackedPropertyController"/>
        /// </summary>
        /// <param name="dataCommunicator">The <see cref="DataCommunicator"/> to use for storage and retrieval operations</param>
        public StoreBackedPropertyController(IStoreBackedPropertyDataCommunicator dataCommunicator)
        {
            Throw.IfNull(() => dataCommunicator);
            this.RegisterDefaultEncoders();
            DataCommunicator = dataCommunicator;
        }

        /// <summary>
        /// Used to communicate with the store for storing / retrieving data
        /// </summary>
        public virtual IStoreBackedPropertyDataCommunicator DataCommunicator { get; set; }

        /// <summary>
        /// When set to true, we will use the <see cref="BsonStoreBackedPropertyEncoder{T}"/> if there is no resolver set for the given type
        /// </summary>
        public bool FallbackToBsonEncoder { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether this object has been disposed via the <see cref="Dispose()"/> method
        /// </summary>
        protected bool IsDisposed => _isDisposed;

        /// <summary>
        /// Disposes the current object
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in the virtual Dispose(bool disposing) method.
            Dispose(true);
        }

        /// <summary>
        /// Returns the resolver for <typeparamref name="T"/>.
        /// If there is no resolver and <see cref="FallbackToBsonEncoder"/> is set, a <see cref="BsonStoreBackedPropertyEncoder{T}"/> is used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A resolver for type <typeparamref name="T"/></returns>
        public virtual IStoreBackedPropertyEncoder<T> GetEncoder<T>()
        {
            ThrowIfDisposed();
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
        public virtual async Task<T> GetValueAsync<T>(StoreBackedProperty<T> property)
        {
            ThrowIfDisposed();
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

        /// <summary>
        /// Registers an encoder to be used for serialization/deserialization of <see cref="StoreBackedProperty{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="encoder">The encoder to set.  If set to null, removes the encoder for type <typeparamref name="T"/></param>
        public virtual void RegisterEncoder<T>(IStoreBackedPropertyEncoder<T> encoder)
        {
            ThrowIfDisposed();
            _encoders.AddOrUpdate(typeof(T), encoder, (t, o) => encoder);
        }

        /// <summary>
        /// Stores the value into the store and returns a <see cref="StoreBackedProperty{T}" /> with the Id for retrieval
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The property to store</param>
        /// <returns>The link to retrieve he object from the store</returns>
        /// <exception cref="InvalidOperationException">
        /// When there is no encoder set for <typeparamref name="T"/> and <see cref="FallbackToBsonEncoder"/> is false
        /// </exception>
        public virtual async Task<StoreBackedProperty<T>> StoreValueAsync<T>(T value)
        {
            ThrowIfDisposed();
            ThrowIfNoDataCommunicator();
            var property = new StoreBackedProperty<T>(Guid.NewGuid().ToString());
            var encoder = GetEncoder<T>();
            var bytes = encoder.Encode(value);
            await DataCommunicator.StoreDataAsync(property.Id, bytes).ConfigureAwait(false);
            return property;
        }

        /// <summary>
        /// The implementation of the IDisposable pattern
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed objects
                }
                DataCommunicator = null;
                _encoders.Clear();
                _isDisposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("StoreBackedPropertyController");
            }
        }

        private void ThrowIfNoDataCommunicator()
        {
            if (DataCommunicator == null)
            {
                throw new InvalidOperationException("No DataRetriever set");
            }
        }
    }
}