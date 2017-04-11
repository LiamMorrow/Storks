using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Storks.Resolvers
{
    /// <summary>
    /// A standard implementation of <see cref="IStoreBackedPropertyEncoder{T}"/> for storing POCOs as BSON data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BsonStoreBackedPropertyEncoder<T> : IStoreBackedPropertyEncoder<T>
    {
        /// <summary>
        /// Decodes the data using a <see cref="Newtonsoft.Json.Bson.BsonDataReader"/>
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
            using (var ms = new MemoryStream(data))
            {
                using (var writer = new BsonDataReader(ms))
                {
                    var serializer = new JsonSerializer();
                    return serializer.Deserialize<T>(writer);
                }
            }
        }

        /// <summary>
        /// Encodes the data using a <see cref="Newtonsoft.Json.Bson.BsonDataWriter"/>
        /// </summary>
        /// <param name="data">The object to be serialized</param>
        /// <returns>A byte array representing the object given by <paramref name="data"/></returns>
        public virtual byte[] Encode(T data)
        {
            if (EqualityComparer<T>.Default.Equals(data, default(T)))
            {
                return new byte[0];
            }
            using (var ms = new MemoryStream())
            {
                using (var writer = new BsonDataWriter(ms))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, data);
                    return ms.ToArray();
                }
            }
        }
    }
}
