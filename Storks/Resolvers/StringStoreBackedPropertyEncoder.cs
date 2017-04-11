using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Storks.Resolvers
{
    /// <summary>
    /// A standard implementation of <see cref="IStoreBackedPropertyEncoder{T}"/> for storing strings as a byte array
    /// </summary>
    public class StringStoreBackedPropertyEncoder : IStoreBackedPropertyEncoder<string>
    {
        /// <summary>
        /// The <see cref="System.Text.Encoding"/> to use to convert <see cref="string"/> to and from <see cref="T:byte[]"/>
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Constructs a new <see cref="StringStoreBackedPropertyEncoder"/> with the <see cref="Encoding"/> set to <see cref="Encoding.Unicode"/>
        /// </summary>
        public StringStoreBackedPropertyEncoder()
        {
            Encoding = Encoding.Unicode;
        }

        /// <summary>
        /// Decodes the data using <see cref="Encoding.GetString(byte[])"/> using the encoding specified by <see cref="Encoding"/>
        /// </summary>
        /// <param name="data">The binary data from the store</param>
        /// <returns>A decoded <see cref="string"/> from <paramref name="data"/></returns>
        public string Decode(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            var encoding = Encoding ?? Encoding.Unicode;
            return encoding.GetString(data);
        }

        /// <summary>
        /// Encodes the data using <see cref="Encoding.GetBytes(string)"/> using the encoding specified by <see cref="Encoding"/>
        /// </summary>
        /// <param name="data">The object to be serialized</param>
        /// <returns>A byte array representing the <see cref="string"/> in <paramref name="data"/></returns>
        public byte[] Encode(string data)
        {
            var encoding = Encoding ?? Encoding.Unicode;
            if(data == null)
            {
                return new byte[0];
            }
            return encoding.GetBytes(data);
        }
    }
}
