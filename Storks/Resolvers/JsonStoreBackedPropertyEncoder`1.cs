using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Storks.Resolvers
{
    /// <summary>
    /// A standard implementation of <see cref="IStoreBackedPropertyEncoder{T}"/> for storing POCOs as JSON data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonStoreBackedPropertyEncoder<T> : IStoreBackedPropertyEncoder<T>
    {
        /// <summary>
        /// Settings for serialization or deserialization.  Set to null for default settings
        /// </summary>
        public JsonSerializerSettings JsonSettings { get; set; }

        /// <summary>
        /// Decodes the data using <see cref="JsonConvert"/> with the settings given in <see cref="JsonSettings"/> or default settings if that is null
        /// </summary>
        /// <param name="data">The binary data from the store</param>
        /// <returns>An object of type T with properties set from <paramref name="data"/></returns>
        public virtual T Decode(byte[] data)
        {
            if (data == null)
            {
                return default(T);
            }
            if (data.Length == 0)
            {
                return default(T);
            }
            var jsonStr = Encoding.Unicode.GetString(data);
            if (JsonSettings != null)
            {
                return JsonConvert.DeserializeObject<T>(jsonStr, JsonSettings);
            }
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }

        /// <summary>
        /// Encodes the data using <see cref="JsonConvert"/> with the settings given in <see cref="JsonSettings"/> or default settings if that is null
        /// </summary>
        /// <param name="data">The object to be serialized</param>
        /// <returns>A byte array representing the object given by <paramref name="data"/></returns>
        public virtual byte[] Encode(T data)
        {
            if (EqualityComparer<T>.Default.Equals(data, default(T)))
            {
                return new byte[0];
            }
            string jsonStr;
            if (JsonSettings != null)
            {
                jsonStr = JsonConvert.SerializeObject(data, JsonSettings);
            }
            else
            {
                jsonStr = JsonConvert.SerializeObject(data);
            }
            return Encoding.Unicode.GetBytes(jsonStr);
        }
    }
}
