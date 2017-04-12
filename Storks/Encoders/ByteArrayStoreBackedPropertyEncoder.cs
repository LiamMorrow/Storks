// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

namespace Storks.Encoders
{
    /// <summary>
    /// The encoder for byte data. This class does nothing in reality, but it used for compatibility with <see cref="IStoreBackedPropertyController"/>
    /// </summary>
    public class ByteArrayStoreBackedPropertyEncoder : IStoreBackedPropertyEncoder<byte[]>
    {
        /// <summary>
        /// Returns the same data that was given
        /// </summary>
        /// <param name="data">The byte data to decode</param>
        /// <returns>The same byte array as given</returns>
        public byte[] Decode(byte[] data)
        {
            return data;
        }

        /// <summary>
        /// Returns the same data as given
        /// </summary>
        /// <param name="data">The data to encode</param>
        /// <returns>A <see cref="T:byte[]" /> representing the data</returns>
        public byte[] Encode(byte[] data)
        {
            return data;
        }
    }
}