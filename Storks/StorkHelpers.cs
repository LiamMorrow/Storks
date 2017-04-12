// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Storks.Encoders;

namespace Storks
{
    /// <summary>
    /// A collection of methods for useful functions when working with Storks
    /// </summary>
    public static class StorksHelpers
    {
        /// <summary>
        /// When called, all Json that is converted without custom settings will deserialize <see cref="StoreBackedProperty{T}"/>s and pull their data
        /// </summary>
        /// <param name="controller">The controller to use to retrieve data</param>
        public static void AutoBindLoadedJson(this IStoreBackedPropertyController controller)
        {
            JsonConvert.DefaultSettings = GetDefaultSettings(controller);
        }

        /// <summary>
        /// Registers encoders for many standard types This does not need to be called on <see cref="StoreBackedPropertyController"/>
        /// </summary>
        /// <param name="controller">The controller to register the encoders to</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="controller"/> is null</exception>
        public static void RegisterDefaultEncoders(this IStoreBackedPropertyController controller)
        {
            Throw.IfNull(() => controller);
            controller.RegisterEncoder(new StringStoreBackedPropertyEncoder());
            controller.RegisterEncoder(new ByteArrayStoreBackedPropertyEncoder());
        }

        private static Func<JsonSerializerSettings> GetDefaultSettings(IStoreBackedPropertyController controller)
        {
            return () =>
           {
               var serializerSettings = new JsonSerializerSettings();
               serializerSettings.Converters = serializerSettings.Converters ?? new List<JsonConverter>();
               serializerSettings.Converters.Add(new StoreBackedPropertyJsonConverter(controller));
               return serializerSettings;
           };
        }
    }
}