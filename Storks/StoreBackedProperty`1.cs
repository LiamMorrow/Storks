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
    /// <typeparam name="T"></typeparam>
    public class StoreBackedProperty<T>
    {
        private StoreBackedProperty()
            :this(Guid.NewGuid().ToString())
        {
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

        /// <summary>
        /// A unique identifier for the data in storage
        /// </summary>
        public string Id { get; }
    }
}
