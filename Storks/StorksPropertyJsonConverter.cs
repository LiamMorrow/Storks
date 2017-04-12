using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Storks.Encoders;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Reflection;

namespace Storks
{
    /// <summary>
    /// A <see cref="JsonConverter"/> that will automatically retrieve data when deserializing <see cref="StoreBackedProperty{T}"/>s
    /// </summary>
    public class StoreBackedPropertyJsonConverter : JsonConverter
    {
        private readonly IStoreBackedPropertyController _controller;

        /// <summary>
        /// Initializes a new instance of <see cref="StoreBackedPropertyJsonConverter"/> with the controller used for retrieval operations
        /// </summary>
        /// <param name="controller">The controller to use for retrieval operations</param>
        public StoreBackedPropertyJsonConverter(IStoreBackedPropertyController controller)
        {
            this._controller = controller;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType
#if !NETSTANDARD1_3
                .IsGenericType
#else
                .IsConstructedGenericType
#endif
                && objectType.GetGenericTypeDefinition() == typeof(Storks.StoreBackedProperty<>);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobject = JObject.Load(reader);
            var storeBackedPropertyType = objectType.GenericTypeArguments[0];

            // Check if the JSON is valid i.e. has and Id property
            if (jobject == null || !jobject.HasValues || !jobject.TryGetValue("Id", out var jtoken))
            {
                return null;
            }

            var id = jtoken.Value<string>();

            // TODO use expression compilation and a caching strategy to move away from the DLR

            var storeBackedProperty = Activator.CreateInstance(objectType, id);

            // Using dynamic here to simplify the use of a generic Task.Result.
            dynamic task = typeof(IStoreBackedPropertyController)
#if !NETSTANDARD1_3
                .GetMethod("GetValueAsync")
#else
                .GetTypeInfo()
                .GetDeclaredMethod("GetValueAsync")
#endif
                .MakeGenericMethod(storeBackedPropertyType)
                .Invoke(_controller, new[] { storeBackedProperty });
            var result = task.Result;
            return Activator.CreateInstance(objectType, id, result);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("Writing is not supported");
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter" /> can write JSON.
        /// </summary>
        /// <value><c>true</c> if this <see cref="JsonConverter" /> can write JSON; otherwise, <c>false</c>.</value>
        public override bool CanWrite => false;
    }
}
