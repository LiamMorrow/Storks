// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Storks
{
    /// <summary>
    /// A <see cref="JsonConverter"/> that will automatically retrieve data when deserializing <see cref="StoreBackedProperty{T}"/>s
    /// </summary>
    public class StoreBackedPropertyJsonConverter : JsonConverter
    {
        private readonly static ConcurrentDictionary<Type, Creator> _typeCreatorCache = new ConcurrentDictionary<Type, Creator>();
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
        /// Gets a value indicating whether this <see cref="JsonConverter" /> can write JSON.
        /// </summary>
        /// <value><c>true</c> if this <see cref="JsonConverter" /> can write JSON; otherwise, <c>false</c>.</value>
        public override bool CanWrite => false;

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

            // Pull the creator from the cache or generate a new creator on first run
            var typeCreator = _typeCreatorCache.GetOrAdd(objectType, GetTypeCreator);
            var storeBackedProperty = typeCreator.CreateWithId(id);

            // Using dynamic here to simplify the use of a generic Task.Result.
            dynamic task = (Task)typeof(IStoreBackedPropertyController)
#if !NETSTANDARD1_3
                .GetMethod("GetValueAsync")
#else
                .GetTypeInfo()
                .GetDeclaredMethod("GetValueAsync")
#endif
                .MakeGenericMethod(storeBackedPropertyType)
                .Invoke(_controller, new[] { storeBackedProperty });
            var result = task.Result;
            return typeCreator.CreateWithValue(id, result);
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
        /// Creates the delegates for creating new StoreBackedProperties so we don't have to use Activator.CreateInstance
        /// </summary>
        /// <param name="objectType">The type of object to create.  Should be a StoreBackedProperty</param>
        /// <returns></returns>
        private Creator GetTypeCreator(Type objectType)
        {
            Throw.IfNull(() => objectType);
            if (!CanConvert(objectType))
            {
                throw new ArgumentException(nameof(objectType), "Type must be a StoreBackedProperty");
            }

            var underlyingType = objectType.GenericTypeArguments[0];

            // Get the constructor for creating a StoreBackedProperty with just an id
            var justIdConstructor = objectType
#if !NETSTANDARD1_3
                .GetConstructor(new[] { typeof(string) });
#else
                .GetTypeInfo()
                .DeclaredConstructors
                .First(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1;
                });
#endif

            // Get the constructor for creating a StoreBackedProperty with a value
            var idAndValueConstructor = objectType
#if !NETSTANDARD1_3
                .GetConstructor(new[] { typeof(string) });
#else
                .GetTypeInfo()
                .DeclaredConstructors
                .First(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 2 && parameters[1].ParameterType == underlyingType;
                });
#endif
            // Create the method parameters for a Func<string,object>
            var idParam = Expression.Parameter(typeof(string));
            var valueParam = Expression.Parameter(typeof(object));

            // Create the expression for unboxing the valueParam to its proper type i.e. (string)valueParam
            var unBoxedValueParam = Expression.Convert(valueParam, underlyingType);

            // Create the expression for: new StoreBackedProperty<T>(string id)
            var createWithId = Expression.New(justIdConstructor, idParam);

            // Create the expression for: new StoreBackedProperty<T>(string id, T value)
            var crateWithValue = Expression.New(idAndValueConstructor, idParam, unBoxedValueParam);

            // Box the results of these calls into objects for strongly typed function calls. This allows us to use Invoke rather than DynamicInvoke
            var boxedCreateWithId = Expression.Convert(createWithId, typeof(object));
            var boxedCreateWithIdAndValue = Expression.Convert(crateWithValue, typeof(object));

            return new Creator
            {
                CreateWithId = Expression.Lambda<Func<string, object>>(boxedCreateWithId, idParam).Compile(),
                CreateWithValue = Expression.Lambda<Func<string, object, object>>(boxedCreateWithIdAndValue, idParam, valueParam).Compile()
            };
        }

        private class Creator
        {
            public Func<string, object> CreateWithId { get; set; }
            public Func<string, object, object> CreateWithValue { get; set; }
        }
    }
}