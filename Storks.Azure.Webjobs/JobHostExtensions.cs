
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Storks.Azure.Webjobs
{
    public static class JobHostExtensions
    {
        public static void UseStoreBackedPropertyBinder(this JobHostConfiguration config, IStoreBackedPropertyController controller)
        {
            JsonConvert.DefaultSettings = JobHostExtensions.GetDefaultSettings(controller);
        }

        private static Func<JsonSerializerSettings> GetDefaultSettings(IStoreBackedPropertyController controller)
        {
            return () =>
           {
               var serializerSettings = new JsonSerializerSettings();
               serializerSettings.Converters = serializerSettings.Converters ?? (IList<JsonConverter>)new List<JsonConverter>();
               serializerSettings.Converters.Add((JsonConverter)new StorksPropertyJsonConverter(controller));
               return serializerSettings;
           };
        }
    }
}
