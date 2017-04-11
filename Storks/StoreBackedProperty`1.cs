using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Storks
{
    /// <summary>
    /// Represents a property value that needs to be retrieved from storage
    /// </summary>
    public interface IStoreBackedProperty
    {
        /// <summary>
        /// The type of the property to deserialize into
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// A unique identifier for the data in storage
        /// </summary>
        string Id { get; }
    }

    /// <summary>
    /// Represents a property value that needs to be retrieved from storage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StoreBackedProperty<T> : IStoreBackedProperty
    {
        private StoreBackedProperty()
            : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Initializes a new StoreBackedProperty with a given value
        /// </summary>
        /// <param name="id">The unique id of the property for reference in storage</param>
        /// <param name="value">The loaded value of the store backed property.</param>
        public StoreBackedProperty(string id, T value) : this(id)
        {
            Value = value;
        }

        /// <summary>
        /// The loaded value of the store backed property.
        /// This can be used to avoid creating temporary variables
        /// </summary>
        [JsonIgnore]
        public T Value { get; }

        /// <summary>
        /// The default constructor for a <see cref="StoreBackedProperty{T}"/>
        /// </summary>
        /// <param name="id">The unique id of the property for reference in storage</param>
        [JsonConstructor]
        public StoreBackedProperty(string id)
        {
            Id = id;
        }

        /// <summary>
        /// A unique identifier for the data in storage
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The type of the property to deserialize into
        /// </summary>
        public Type PropertyType => typeof(T);
    }
}
