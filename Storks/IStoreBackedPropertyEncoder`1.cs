// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

namespace Storks
{
    /// <summary>
    /// Provides methods for decoding and encoding the given type between the store representation and CLR representation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStoreBackedPropertyEncoder<T>
    {
        /// <summary>
        /// Decodes the bytes into a concrete <c>T</c>
        /// </summary>
        /// <param name="data">The byte data to decode</param>
        /// <returns>An object of type T with the data from <paramref name="data"/></returns>
        T Decode(byte[] data);

        /// <summary>
        /// Encodes the object into a <see cref="T:byte[]"/> for storage
        /// </summary>
        /// <param name="data">The data to encode</param>
        /// <returns>A <see cref="T:byte[]"/> representing the data</returns>
        byte[] Encode(T data);
    }
}