// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Storks
{
    /// <summary>
    /// Represents a property value that needs to be retrieved from storage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StoreBackedProperty<T>
    {
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
        /// The default constructor for a <see cref="StoreBackedProperty{T}"/>
        /// </summary>
        /// <param name="id">The unique id of the property for reference in storage</param>
        [JsonConstructor]
        public StoreBackedProperty(string id)
        {
            Id = id;
        }

        private StoreBackedProperty()
                            : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// A unique identifier for the data in storage
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The type of the property to deserialize into
        /// </summary>
        public Type PropertyType => typeof(T);

        /// <summary>
        /// The loaded value of the store backed property.
        /// This can be used to avoid creating temporary variables
        /// </summary>
        [JsonIgnore]
        public T Value { get; }
    }
}